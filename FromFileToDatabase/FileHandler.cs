using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FromFileToDatabase
{
    class FileHandler
    {
        public static List<LoadedWord> LoadFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Отсутствует файл по заданному пути.");
                return null;
            }

            var loadedData = File.ReadAllText(path);

            var pattern = @"(\S{3,20})";
            var selectedData = Regex.Matches(loadedData, pattern);

            var groupedData = selectedData.GroupBy(x => x.ToString())
                              .Where(x => x.Count() > 3)
                              .Select(x => new LoadedWord{ Name = x.Key.ToString(), Count = x.Count() });

            var result = new List<LoadedWord>();
            result.AddRange(groupedData);

            return result;
        }
    }
}
