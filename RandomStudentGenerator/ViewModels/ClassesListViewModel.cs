using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RandomStudentGenerator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using RandomStudentGenerator.Services;

namespace RandomStudentGenerator.ViewModels
{
    internal class ClassesListViewModel : ObservableObject
    {
        public ICommand NewClassCommand { get; }
        public ICommand SelectClass { get; }
        public ICommand SetLuckyNumberCommand { get; }
        public ICommand DeleteClassCommand { get; }

        public ObservableCollection<Models.Class> AllClasses { get; }
        private Models.Class? _selectedClass;
        public Models.Class? SelectedClass
        {
            get => _selectedClass;
            set => SetProperty(ref _selectedClass, value);
        }

        private Models.School _school;

        public Models.School School
        {
            get => _school;
            set
            {
                _school = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AllClasses));
            }
        }

        public ClassesListViewModel()
        {
            School = new Models.School();
            School.GetLuckyNumber();
            AllClasses = new ObservableCollection<Models.Class>(Models.Class.LoadAll());
            NewClassCommand = new AsyncRelayCommand(AddNewClass);
            SelectClass = new AsyncRelayCommand<Class?>(SelectClassAsync);
            SetLuckyNumberCommand = new AsyncRelayCommand(SetLuckyNumber);
            DeleteClassCommand = new AsyncRelayCommand<Class?>(DeleteClassAsync);
        }

        private async Task AddNewClass()
        {
            var page = App.Current?.MainPage;
            if (page == null) return;

            string className = await page.DisplayPromptAsync("New class", "Enter name of your new class: ");
            if (string.IsNullOrWhiteSpace(className))
                return;

            var newClass = new Models.Class { Name = className };
            FileService.CreateNewClass(newClass);

            AllClasses.Add(newClass);
        }

        private async Task SelectClassAsync(Class? list)
        {
            if (list == null) return;
            await Shell.Current.GoToAsync($"{nameof(Views.ClassPage)}?load={list.Name}&luckyNumber={School.LuckyNumber}");
        }

        private int CheckHigestStudentCount()
        {
            int maxCount = 0;
            foreach (var c in AllClasses)
            {
                var classInfo = Class.Load(c.Name);
                if (classInfo.Students.Count > maxCount)
                {
                    maxCount = classInfo.Students.Count;
                }
            }
            return maxCount;
        }

        private async Task SetLuckyNumber()
        {
            var page = App.Current?.MainPage;
            if (page == null) return;
            string decision = await page.DisplayActionSheet("Choose the method", "Cancel", null, "Generate random number", "Choose your own");
            int maxStudentCount = CheckHigestStudentCount();
            if (decision == "Generate random number")
            {
                var random = new Random();
                int luckyNbr = 0;
                if (maxStudentCount > 0)
                {
                    luckyNbr = random.Next(1, maxStudentCount + 1);
                }
                await page.DisplayAlert("Lucky number", $"The lucky number is: {luckyNbr}", "OK");
                School.LuckyNumber = luckyNbr;
            }
            else if (decision == "Choose your own")
            {
                string input = await page.DisplayPromptAsync("Lucky number", "Enter your lucky number");
                if (int.TryParse(input, out int luckyNbr))
                {
                    if (luckyNbr < 1)
                    {
                        await page.DisplayAlert("Invalid number", "Lucky number can't be less than 1", "OK");
                        return;
                    }
                    await page.DisplayAlert("Lucky number", $"Your lucky number is: {luckyNbr}", "OK");
                    School.LuckyNumber = luckyNbr;
                }
                else
                {
                    await page.DisplayAlert("Invalid input", "Please enter a valid number.", "OK");
                }
            }
            else return;
            School.SaveLuckyNumber();
        }

        private async Task DeleteClassAsync(Class? list)
        {
            if (list == null) return;
            var page = App.Current?.MainPage;
            if (page == null) return;
            bool confirm = await page.DisplayAlert("Delete class", $"Are you sure you want to delete the class '{list.Name}'?", "Yes", "No");
            if (confirm)
            {
                FileService.DeleteClass(list);
                AllClasses.Remove(list);
            }
        }
    }
}
