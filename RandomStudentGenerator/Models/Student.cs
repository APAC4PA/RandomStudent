using CommunityToolkit.Mvvm.ComponentModel;

namespace RandomStudentGenerator.Models
{
    internal partial class Student : ObservableObject
    {
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string lastname = string.Empty;

        [ObservableProperty]
        private bool isPresent = true;

        public Class ParentClass { get; set; }

        public void ChangePresence()
        {
            IsPresent = !IsPresent;

            var lines = File.ReadAllLines(Class.path).ToList();
            int insertIndex = lines.FindIndex(line => line.Contains($"{Name} {Lastname}"));

            if (insertIndex == -1)
                throw new Exception("Nie znaleziono studenta w pliku.");

            lines[insertIndex] = $"{Name} {Lastname} {IsPresent}";
            File.WriteAllLines(Class.path, lines);
        }
    }
}