using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomStudentGenerator.Models
{
    internal class Class
    {
        public string Name { get; set; } = string.Empty;

        public ObservableCollection<Student> Students { get; } = new ObservableCollection<Student>();
        public List<Student> RecentlyAskedStudents = new List<Student>();
    }
}