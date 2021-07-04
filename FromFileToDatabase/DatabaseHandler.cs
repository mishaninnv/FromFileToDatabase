using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace FromFileToDatabase
{
    public class DatabaseHandler
    {
        private static string TableName => Program.GetConfigValue("tableName");

        private readonly SqlConnection _sqlConn;
        private SqlCommand _sqlCmd = new SqlCommand();

        public DatabaseHandler(string databaseName)
        {
            var serverConnStr = Program.GetConfigValue("connectionString");
            _sqlConn = new SqlConnection(serverConnStr);
            _sqlConn.Open();

            InitializeDb(databaseName);
        }

        private void InitializeDb(string databaseName)
        {
            CreateDbIfNotExists(databaseName);

            var dbConnStr = $@"USE {databaseName}";
            UsingCommand(dbConnStr);

            CreateTable();
        }

        private void CreateDbIfNotExists(string databaseName)
        {
            var cmdString = string.Format(
            @"IF NOT (EXISTS ( SELECT * FROM sys.databases WHERE Name = '{0}'))
                        CREATE DATABASE {0};", databaseName);

            UsingCommand(cmdString);
        }

        private void CreateTable()
        {
            var cmdString = string.Format(
                @"IF NOT (EXISTS ( SELECT * FROM DataBaseTest.INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_NAME = '{0}'))
                        CREATE TABLE {0} (
                        word NVARCHAR(200) UNIQUE, 
                        count INT
                        )", TableName);

            UsingCommand(cmdString);
        }

        public void WriteDataToDb(IEnumerable<WordCount> inputData)
        {
            foreach (var word in inputData)
            {
                var reader = GetDbReader(word.Name);

                if (reader.HasRows)
                {
                    var newCount = GetNewCount(reader, word);
                    UpdateData(word.Name, newCount);
                }
                else
                {
                    reader.Close();
                    InsertData(word.Name, word.Count);
                }
            }
        }

        private SqlDataReader GetDbReader(string word)
        {
            var cmdString = $@"SELECT * FROM {TableName} WHERE word = '{word}'";
            _sqlCmd = new SqlCommand(cmdString, _sqlConn);
            return _sqlCmd.ExecuteReader();
        }

        private int GetNewCount(SqlDataReader reader, WordCount word)
        {
            reader.Read();
            var currCount = reader.GetInt32(1) + word.Count;
            reader.Close();

            return currCount;
        }

        private void UpdateData(string word, int value)
        {
            var cmdString = $@"UPDATE {TableName} SET count = {value}
                                            WHERE word = '{word}'";

            UsingCommand(cmdString);
        }

        private void InsertData(string word, int value)
        {
            var cmdString = $@"INSERT {TableName}(word, count)
                               VALUES ('{word}', {value})";

            UsingCommand(cmdString);
        }

        private void UsingCommand(string cmdString)
        {
            _sqlCmd = new SqlCommand(cmdString, _sqlConn);
            _sqlCmd.ExecuteNonQuery();
        }
    }
}
