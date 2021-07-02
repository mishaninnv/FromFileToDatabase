namespace FromFileToDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"C:\Users\misha\Desktop\newdocument.txt";
            var wordList = FileHandler.LoadFile(path);

            if (wordList == null) return;

            DatabaseHandler.WriteToDatabase("DataBaseTest", wordList);     
        }
    }
}
