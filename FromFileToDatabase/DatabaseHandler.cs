using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace FromFileToDatabase
{
    public class DatabaseHandler
    {
        string tableName = Program.GetConfigValue("table");
        SqlConnection sqlConn;
        SqlCommand sqlCmd = new SqlCommand();

        public DatabaseHandler(string databaseName)
        {
            string serverConnStr = Program.GetConfigValue("connectionString");
            sqlConn = new SqlConnection(serverConnStr);
            sqlConn.Open();

            InitializeDb(databaseName);
        }

        public void InitializeDb(string databaseName)
        {
            CreateDbIfNotExists(databaseName);

            string dbConnStr = string.Format(@"USE {0}", databaseName);
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
                        )", tableName);

            UsingCommand(cmdString);
        }

        public void WriteDataToDb(List<WordCount> inputData)
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
            var cmdString = string.Format(@"SELECT * FROM {0} WHERE word = '{1}'", tableName, word);
            sqlCmd = new SqlCommand(cmdString, sqlConn);
            return sqlCmd.ExecuteReader();
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
            var cmdString = string.Format(@"UPDATE {0} SET count = {1}
                                            WHERE word = '{2}'", tableName, value, word);

            UsingCommand(cmdString);
        }

        private void InsertData(string word, int value)
        {
            var cmdString = string.Format(@"INSERT {0}(word, count)
                                            VALUES ('{1}', {2})", tableName, word, value);

            UsingCommand(cmdString);
        }

        private void UsingCommand(string cmdString)
        {
            sqlCmd = new SqlCommand(cmdString, sqlConn);
            sqlCmd.ExecuteNonQuery();
        }
    }
}
