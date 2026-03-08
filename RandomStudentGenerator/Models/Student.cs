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
    }
}