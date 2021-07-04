using System;
using System.Configuration;
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
            var path = GetConfigValue("filePath");

            var dataFromFile = ReadFile(path);
            if (string.IsNullOrEmpty(dataFromFile)) return;

            var pattern = @"\S+";
            var minLength = Convert.ToInt32(GetConfigValue("minLength"));
            var maxLength = Convert.ToInt32(GetConfigValue("maxLength"));
            var filteredData = FilterData(pattern, dataFromFile, minLength, maxLength);

            var wordCounts = GetWordCounts(filteredData);

            var dbName = GetConfigValue("db");
            var dbHandler = new DatabaseHandler(dbName);
            dbHandler.WriteDataToDb(wordCounts);
        }

        private static string ReadFile(string filePath)
        {
            return CheckFileExistence(filePath) ? File.ReadAllText(filePath) : string.Empty;
        }

        private static bool CheckFileExistence(string filePath)
        {
            if (File.Exists(filePath)) return true;

            Console.WriteLine("Отсутствует файл по заданному пути.");
            return false;
        }

        private static IEnumerable<Match> FilterData(string pattern, string data, int minLength, int maxLength)
        {
            return Regex.Matches(data, pattern).Where(x => x.Length >= minLength && x.Length <= maxLength);
        }

        private static List<WordCount> GetWordCounts(IEnumerable<Match> words)
        {
            var minCount = Convert.ToInt32(GetConfigValue("minCount"));
            var filteredWords = FilterDataByMinCount(words, minCount);
            var result = ConvertToWordCounts(filteredWords);
            return result;
        }

        private static IEnumerable<IGrouping<string, Match>> FilterDataByMinCount(IEnumerable<Match> filteredFile, int minCount)
        { 
            return filteredFile.GroupBy(x => x.ToString()).Where(x => x.Count() >= minCount);
        }

        private static List<WordCount> ConvertToWordCounts(IEnumerable<IGrouping<string, Match>> data)
        { 
            return data.Select(x => new WordCount { Name = x.Key.ToString(), Count = x.Count() }).ToList<WordCount>();
        }

        internal static string GetConfigValue(string key)
        {
            return ConfigurationManager.AppSettings.Get(key);
        }
    }

    public class WordCount
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
