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
        public static string path = Path.Combine(FileSystem.AppDataDirectory, "RandomStudentGenerator", "classes.txt");
        public string Name { get; set; } = string.Empty;

        public ObservableCollection<Student> Students { get; } = new ObservableCollection<Student>();
        public List<Student> RecentlyAskedStudents = new List<Student>();

        public void AddStudent(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));

            student.ParentClass = this;
            student.IsPresent = true;
            Students.Add(student);

            var lines = File.ReadAllLines(path).ToList();

            int insertIndex = lines.FindIndex(line => line.Contains($"!{student.ParentClass.Name}!"));
            if (insertIndex == -1)
            {
                throw new Exception("Nie znaleziono końca klasy w pliku.");
            }

            lines.Insert(insertIndex, $"{student.Name} {student.Lastname} {student.IsPresent}");
            File.WriteAllLines(path, lines);
        }

        public static List<Class> LoadAll()
        {
            var classes = new List<Class>();
            if (!File.Exists(path))
                return classes;
            var lines = File.ReadAllLines(path);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Count(x => x == '*') == 2)
                {
                    classes.Add(new Class { Name = lines[i].Split('*')[1] });
                }
            }
            return classes;
        }
        public static Class Load(string name)
        {
            int lineIndex = -1;
            int startIndex = -1;
            int endIndex = -1;
            var classInfo = new Class { Name = name };
            if (!File.Exists(path))
                Trace.WriteLine("File not found: " + path);
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                lineIndex++;
                if (line.Contains($"*{name}*"))
                    startIndex = lineIndex;
                if (line.Contains($"!{name}!"))
                    endIndex = lineIndex;
            }
            for (int i = startIndex + 1; i < endIndex; i++)
            {
                if (!lines[i].Contains("#") && !lines[i].Contains("%"))
                {
                    var student = new Student()
                    {
                        Name = lines[i].Split(' ')[0],
                        Lastname = lines[i].Split(' ')[1],
                        IsPresent = bool.Parse(lines[i].Split(' ')[2])
                    };
                    classInfo.Students.Add(student);
                }
            }
            return classInfo;
        }
    }
}