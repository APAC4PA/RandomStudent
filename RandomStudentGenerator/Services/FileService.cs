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
    }
}
