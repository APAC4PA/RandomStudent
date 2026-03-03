using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomStudentGenerator.Models
{
    partial class School : ObservableObject
    {
        [ObservableProperty]
        private int luckyNumber;
        
        string path = Path.Combine(FileSystem.AppDataDirectory, "RandomStudentGenerator", "classes.txt");

        public void SaveLuckyNumber()
        {
            var lines = File.ReadAllLines(path).ToList();
            if (lines[0].Count(x => x == '^') == 2)
            {
                lines[0] = "^" + LuckyNumber.ToString() + "^";
            }
            else
            {
                lines.Insert(0 ,"^" + LuckyNumber.ToString() + "^");
            }
            File.WriteAllLines(path, lines);
        }
        public void GetLuckyNumber()
        {
            if (!File.Exists(path))
                return;
            var lines = File.ReadAllLines(path).ToList();
            if (lines.Count == 0)
                return;
            var lastLine = lines[lines.Count - 1];
            if (lastLine.Count(x => x == '^') == 2)
            {
                var numberStr = lastLine.Split('^')[1];
                if (int.TryParse(numberStr, out int number))
                {
                    luckyNumber = number;
                }
            }
        }
    }
}
