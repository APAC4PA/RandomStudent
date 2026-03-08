using RandomStudentGenerator.Models;
using System.Xml.Linq;

using RandomStudentGenerator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomStudentGenerator.Services
{
    class FileService
    {

        public static string path = Path.Combine(FileSystem.AppDataDirectory, "RandomStudentGenerator", "classes.txt");

        static public void CreateNewClass(Class c)
        {
            Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "RandomStudentGenerator"));
            File.AppendAllText(path, "*" + c.Name + "*" + Environment.NewLine);
            File.AppendAllText(path, "!" + c.Name + "!" + Environment.NewLine);
            Trace.WriteLine(path);
        }

        static public void EditStudent(Student studentInfo, Student newStudentInfo, Class c)
        {
            int lineIndex = -1;
            int startIndex = -1;
            int endIndex = -1;
            var classInfo = new Class { Name = c.Name };
            if (!File.Exists(path))
                Trace.WriteLine("File not found: " + path);
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                lineIndex++;
                if (line.Contains($"*{c.Name}*"))
                    startIndex = lineIndex;
                if (line.Contains($"!{c.Name}!"))
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
            c.Students.Where(s => s.Name == studentInfo.Name && s.Lastname == studentInfo.Lastname).ToList().ForEach(s =>
            {
                s.Name = newStudentInfo.Name;
                s.Lastname = newStudentInfo.Lastname;
                s.IsPresent = newStudentInfo.IsPresent;
            });
        }
        static public void DeleteStudent(Student studentInfo, Class c)
        {
            int lineIndex = -1;
            int startIndex = -1;
            int endIndex = -1;
            var classInfo = new Class { Name = c.Name };
            if (!File.Exists(path))
                Trace.WriteLine("File not found: " + path);
            var lines = File.ReadAllLines(path).ToList();
            foreach (var line in lines)
            {
                lineIndex++;
                if (line.Contains($"*{c.Name}*"))
                    startIndex = lineIndex;
                if (line.Contains($"!{c.Name}!"))
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
            c.Students.Where(s => s.Name == studentInfo.Name && s.Lastname == studentInfo.Lastname).ToList().ForEach(s =>
            {
                c.Students.Remove(s);
            });
        }
        static public void AddToRecentlyAskedStudents(Student student, Class c)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));
            c.RecentlyAskedStudents.Add(student);
            if (c.RecentlyAskedStudents.Count > 3)
                c.RecentlyAskedStudents.RemoveAt(0);
            SaveRecentlyAskedStudents(c);
            foreach (var r in c.RecentlyAskedStudents)
            {
                Trace.WriteLine(r.Name + r.Lastname);
            }
        }
        static public void SaveRecentlyAskedStudents(Class c)
        {
            string newLine = "%";
            var lines = File.ReadAllLines(path).ToList();
            int insertIndex = lines.FindIndex(line => line.Contains($"!{c.Name}!"));
            if (insertIndex == -1)
            {
                throw new Exception("Nie znaleziono końca klasy w pliku.");
            }
            for (int i = 0; i < c.RecentlyAskedStudents.Count; i++)
            {
                if (i != 0)
                {
                    newLine += "#";
                }
                newLine += $"{c.RecentlyAskedStudents[i].Name} {c.RecentlyAskedStudents[i].Lastname}";
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
        static public void LoadRecentlyAskedStudents(Class c)
        {
            var lines = File.ReadAllLines(path).ToList();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Count(x => x == '%') == 2 && lines[i + 1] == $"!{c.Name}!")
                {
                    for (int j = 0; j < lines[i].Count(x => x == '#') + 1; j++)
                    {
                        var recentStudent = new Student()
                        {
                            Name = lines[i].Split('#')[j].Split(' ')[0].Trim('%'),
                            Lastname = lines[i].Split('#')[j].Split(' ')[1].Trim('%'),
                            IsPresent = true
                        };
                        c.RecentlyAskedStudents.Add(recentStudent);
                    }
                }
            }
        }

        static public void DeleteClass(Class c)
        {
            var lines = File.ReadAllLines(path).ToList();
            int startIndex = lines.FindIndex(line => line.Contains($"*{c.Name}*"));
            int endIndex = lines.FindIndex(line => line.Contains($"!{c.Name}!"));
            if (startIndex == -1 || endIndex == -1)
            {
                throw new Exception("Nie znaleziono klasy w pliku.");
            }
            lines.RemoveRange(startIndex, endIndex - startIndex + 1);
            File.WriteAllLines(path, lines);
        }

        static public void ChangePresence(Student s)
        {
            s.IsPresent = !s.IsPresent;

            var lines = File.ReadAllLines(path).ToList();
            int insertIndex = lines.FindIndex(line => line.Contains($"{s.Name} {s.Lastname}"));

            if (insertIndex == -1)
                throw new Exception("Nie znaleziono studenta w pliku.");

            lines[insertIndex] = $"{s.Name} {s.Lastname} {s.IsPresent}";
            File.WriteAllLines(path, lines);
        }

        static public void SaveLuckyNumber(School sc)
        {
            var lines = File.ReadAllLines(path).ToList();
            if (lines[0].Count(x => x == '^') == 2)
            {
                lines[0] = "^" + sc.LuckyNumber.ToString() + "^";
            }
            else
            {
                lines.Insert(0, "^" + sc.LuckyNumber.ToString() + "^");
            }
            File.WriteAllLines(path, lines);
        }
        static public void GetLuckyNumber(School sc)
        {
            if (!File.Exists(path))
                return;
            var lines = File.ReadAllLines(path).ToList();
            if (lines.Count == 0)
                return;
            var firstLine = lines[0];
            if (firstLine.Count(x => x == '^') == 2)
            {
                var numberStr = firstLine.Split('^')[1];
                if (int.TryParse(numberStr, out int number))
                {
                    sc.LuckyNumber = number;
                }
            }
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

        static public void AddStudent(Student s, Class c)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            s.ParentClass = c;
            s.IsPresent = true;
            c.Students.Add(s);

            var lines = File.ReadAllLines(path).ToList();

            int insertIndex = lines.FindIndex(line => line.Contains($"!{s.ParentClass.Name}!"));
            if (insertIndex == -1)
            {
                throw new Exception("Nie znaleziono końca klasy w pliku.");
            }

            lines.Insert(insertIndex, $"{s.Name} {s.Lastname} {s.IsPresent}");
            File.WriteAllLines(path, lines);
        }
    }
}
