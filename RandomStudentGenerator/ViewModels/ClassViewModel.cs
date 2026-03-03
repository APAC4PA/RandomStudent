using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RandomStudentGenerator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RandomStudentGenerator.ViewModels
{
    internal class ClassViewModel : ObservableObject, IQueryAttributable
    {
        int luckyNumber;
        public ICommand AddStudentCommand { get; }
        public ICommand ChangePresenceCommand { get; }
        public ICommand EditStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand PickStudentCommand { get; }

        private Models.Class _class;

        public Models.Class Class
        {
            get => _class;
            set
            {
                _class = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Students));
            }
        }

        public ObservableCollection<Models.Student> Students => Class.Students;
        public ClassViewModel()
        {
            Class = new Class();
            AddStudentCommand = new AsyncRelayCommand(AddStudent);
            ChangePresenceCommand = new AsyncRelayCommand<Student>(ChangeStudentPresence);
            EditStudentCommand = new AsyncRelayCommand<Student>(EditStudent);
            DeleteStudentCommand = new AsyncRelayCommand<Student>(DeleteStudent);
            PickStudentCommand = new AsyncRelayCommand(PickRandomStudent);
        }
        public ClassViewModel(Class c)
        {
            Class = c;
        }
        void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("load", out var value))
            {
                string listName = value as string;

                var loaded = Class.Load(listName);

                Class = loaded;
                Class.LoadRecentlyAskedStudents(); 
            }
            if (query.TryGetValue("luckyNumber", out var luckyNumberValue))
            {
                string luckyNumberS = luckyNumberValue as string;
                luckyNumber = int.Parse(luckyNumberS);
            }
        }

        private async Task AddStudent()
        {
            var page = App.Current?.MainPage;
            if (page == null) return;
            string studentName = await page.DisplayPromptAsync("New student", "Enter name of the student: ");
            if (string.IsNullOrWhiteSpace(studentName))
                return;
            if (!studentName.Contains(' ') || studentName.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                await page.DisplayAlert("Invalid name", "Please enter valid name and lastname for student", "OK");
                return;
            }
            string name = studentName.Split(' ')[0];
            string lastname = studentName.Split(' ')[1];
            var newStudent = new Models.Student { Name = name, Lastname = lastname };
            Class.AddStudent(newStudent);
        }

        private async Task ChangeStudentPresence(Student student)
        {
            if (student == null) return;
            student.ChangePresence();
            OnPropertyChanged(nameof(Students));
        }
        private async Task EditStudent(Student student)
        {
            var page = App.Current?.MainPage;
            string newStudentInfo = await page.DisplayPromptAsync("Edit student", "Enter new name and lastname of the student: ", initialValue: $"{student.Name} {student.Lastname}");
            if (string.IsNullOrWhiteSpace(newStudentInfo))
                return;
            if (!newStudentInfo.Contains(' '))
            {
                await page.DisplayAlert("Invalid name", "Please enter both name and lastname of the student.", "OK");
                return;
            }
            var editedStudent = new Student
            {
                Name = newStudentInfo.Split(' ')[0],
                Lastname = newStudentInfo.Split(' ')[1],
                IsPresent = student.IsPresent
            };
            Class.EditStudent(student, editedStudent, Class.Name);
        }
        private async Task DeleteStudent(Student student)
        {
            var page = App.Current?.MainPage;
            bool confirm = await page.DisplayAlert("Delete student", $"Are you sure you want to delete {student.Name} {student.Lastname}?", "Yes", "No");
            if (confirm)
                Class.DeleteStudent(student, Class.Name);
        }

        private async Task PickRandomStudent()
        {
            var page = App.Current?.MainPage;
            var random = new Random();
            var randomStudent = new Student();
            if (page == null) return;
            var avaiableStudents = Class.Students
                .Where((s, index) => s.IsPresent && index+1 != luckyNumber)
                .ToList();
            foreach(var student in avaiableStudents)
            {
                Trace.WriteLine($"Available student: {student.Name} {student.Lastname}");
            }
            if (avaiableStudents.Count == 0)
            {
                await page.DisplayAlert("No students present", "There are no students marked as present. Please mark at least one student as present to pick a random student.", "OK");
                return;
            }
            do
            {
                if(Class.RecentlyAskedStudents.Count == avaiableStudents.Count && Class.RecentlyAskedStudents.Contains(randomStudent))
                { 
                    await page.DisplayAlert("All students asked", "All present students have been recently asked", "OK");
                    return;
                }
                randomStudent = avaiableStudents[random.Next(avaiableStudents.Count)];
                Trace.WriteLine($"Randomly picked student: {randomStudent.Name} {randomStudent.Lastname}");
            }
            while (Class.RecentlyAskedStudents.Contains(randomStudent));
            Class.AddToRecentlyAskedStudents(randomStudent);
            await page.DisplayAlert("Random student", $"The randomly picked student is: {randomStudent.Name} {randomStudent.Lastname}", "OK");
        }
    }
}
