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

        public void CreateNewClass()
        {
            Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "RandomStudentGenerator"));
            File.AppendAllText(path, "*" + Name + "*" + Environment.NewLine);
            File.AppendAllText(path, "!" + Name + "!" + Environment.NewLine);
            Trace.WriteLine(path);
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

        public void EditStudent(Student studentInfo, Student newStudentInfo, string className)
        {
            int lineIndex = -1;
            int startIndex = -1;
            int endIndex = -1;
            var classInfo = new Class { Name = className };
            if (!File.Exists(path))
                Trace.WriteLine("File not found: " + path);
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                lineIndex++;
                if (line.Contains($"*{className}*"))
                    startIndex = lineIndex;
                if (line.Contains($"!{className}!"))
                    endIndex = lineIndex;
            }
            for (int i = startIndex + 1; i < endIndex; i++)
            {
                if (lines[i].Contains($"{studentInfo.Name} {studentInfo.Lastname}"))
                {
                    lines[i] = $"{newStudentInfo.Name} {newStudentInfo.Lastname} {newStudentInfo.IsPresent}";
                    File.WriteAllLines(path, lines);
                    break;
                }
            }
            Students.Where(s => s.Name == studentInfo.Name && s.Lastname == studentInfo.Lastname).ToList().ForEach(s =>
            {
                s.Name = newStudentInfo.Name;
                s.Lastname = newStudentInfo.Lastname;
                s.IsPresent = newStudentInfo.IsPresent;
            });
        }
        public void DeleteStudent(Student studentInfo, string className)
        {
            int lineIndex = -1;
            int startIndex = -1;
            int endIndex = -1;
            var classInfo = new Class { Name = className };
            if (!File.Exists(path))
                Trace.WriteLine("File not found: " + path);
            var lines = File.ReadAllLines(path).ToList();
            foreach (var line in lines)
            {
                lineIndex++;
                if (line.Contains($"*{className}*"))
                    startIndex = lineIndex;
                if (line.Contains($"!{className}!"))
                    endIndex = lineIndex;
            }
            for (int i = startIndex + 1; i < endIndex; i++)
            {
                if (lines[i].Contains($"{studentInfo.Name} {studentInfo.Lastname}"))
                {
                    lines.RemoveAt(i);
                    File.WriteAllLines(path, lines);
                    break;
                }
            }
            Students.Where(s => s.Name == studentInfo.Name && s.Lastname == studentInfo.Lastname).ToList().ForEach(s =>
            {
                Students.Remove(s);
            });
        }

        public void AddToRecentlyAskedStudents(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));
            RecentlyAskedStudents.Add(student);
            if (RecentlyAskedStudents.Count > 3)
                RecentlyAskedStudents.RemoveAt(0);
            SaveRecentlyAskedStudents();
            foreach (var r in RecentlyAskedStudents)
            {
                Trace.WriteLine(r.Name + r.Lastname);
            }
        }

        public void SaveRecentlyAskedStudents()
        {
            string newLine = "%";
            var lines = File.ReadAllLines(path).ToList();
            int insertIndex = lines.FindIndex(line => line.Contains($"!{Name}!"));
            if (insertIndex == -1)
            {
                throw new Exception("Nie znaleziono końca klasy w pliku.");
            }
            for (int i = 0; i < RecentlyAskedStudents.Count; i++)
            {
                if (i != 0)
                {
                    newLine += "#";
                }
                newLine += $"{RecentlyAskedStudents[i].Name} {RecentlyAskedStudents[i].Lastname}";
            }
            newLine += "%";
            if (lines[insertIndex - 1].Contains("%"))
            {
                lines[insertIndex - 1] = newLine;
            }
            else
            {
                lines.Insert(insertIndex, newLine);
            }
            File.WriteAllLines(path, lines);
        }
        public void LoadRecentlyAskedStudents()
        {
            var lines = File.ReadAllLines(path).ToList();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Count(x => x == '%') == 2 && lines[i + 1] == $"!{Name}!")
                {
                    for (int j = 0; j < lines[i].Count(x => x == '#') + 1; j++)
                    {
                        var recentStudent = new Student()
                        {
                            Name = lines[i].Split('#')[j].Split(' ')[0].Trim('%'),
                            Lastname = lines[i].Split('#')[j].Split(' ')[1].Trim('%'),
                            IsPresent = true,
                            ParentClass = this
                        };
                        RecentlyAskedStudents.Add(recentStudent);
                    }
                }
            }
        }

        public void DeleteClass()
        {
            var lines = File.ReadAllLines(path).ToList();
            int startIndex = lines.FindIndex(line => line.Contains($"*{Name}*"));
            int endIndex = lines.FindIndex(line => line.Contains($"!{Name}!"));
            if (startIndex == -1 || endIndex == -1)
            {
                throw new Exception("Nie znaleziono klasy w pliku.");
            }
            lines.RemoveRange(startIndex, endIndex - startIndex + 1);
            File.WriteAllLines(path, lines);
        }

        public void ExportClass(string exportPath)
        {
            var lines = new List<string>
            {
                $"*{Name}*"
            };
            foreach (var student in Students)
            {
                lines.Add($"{student.Name} {student.Lastname} {student.IsPresent}");
            }
            lines.Add($"!{Name}!");
            File.WriteAllLines(exportPath, lines);
        }
    }
}