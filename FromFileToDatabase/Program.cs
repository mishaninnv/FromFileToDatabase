using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace FromFileToDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"C:\Users\misha\Desktop\newdocument.txt";

            var file = ReadFile(path);
            if (string.IsNullOrEmpty(file)) return;

            var pattern = @"\S+";
            var filteredData = FilterData(pattern, file, 3, 20);

            var wordCounts = GetWordCounts(filteredData);

            var dbHandler = new DatabaseHandler("DataBaseTest");
            dbHandler.WriteDataToDb(wordCounts);
        }

        private static string ReadFile(string filePath)
        {
            return CheckFile(filePath) ? File.ReadAllText(filePath) : string.Empty;
        }

        private static bool CheckFile(string filePath)
        {
            if (File.Exists(filePath)) return true;

            Console.WriteLine("Отсутствует файл по заданному пути.");
            return false;
        }

        private static IEnumerable<Match> FilterData(string pattern, string file, int minLength, int maxLength)
        {
            return Regex.Matches(file, pattern).Where(x => x.Length >= minLength && x.Length <= maxLength);
        }

        private static List<WordCount> GetWordCounts(IEnumerable<Match> filteredFile)
        {
            var selectedByCount = SelectionByCount(filteredFile);
            var wordCounts = ConvertToWordCounts(selectedByCount);
            return wordCounts;
        }

        private static IEnumerable<IGrouping<string, Match>> SelectionByCount(IEnumerable<Match> filteredFile)
        { 
            return filteredFile.GroupBy(x => x.ToString()).Where(x => x.Count() > 3);
        }

        private static List<WordCount> ConvertToWordCounts(IEnumerable<IGrouping<string, Match>> selectedByCount)
        { 
            return selectedByCount.Select(x => new WordCount { Name = x.Key.ToString(), Count = x.Count() }).ToList<WordCount>();
        }
    }

    public class WordCount
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
