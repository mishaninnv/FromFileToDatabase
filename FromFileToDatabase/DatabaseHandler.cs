using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace FromFileToDatabase
{
    public class DatabaseHandler
    {
        SqlConnection sqlConn;
        SqlCommand sqlCmd = new SqlCommand();

        public DatabaseHandler(string databaseName)
        {
            string serverConnStr = @"Server=localhost\SQLEXPRESS01;Trusted_Connection=True;";
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
            var cmdString =
                @"IF NOT (EXISTS ( SELECT * FROM DataBaseTest.INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_NAME = 'Words'))
                        CREATE TABLE Words (
                        word NVARCHAR(200) UNIQUE, 
                        count INT
                        )";

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
            var cmdString = string.Format(@"SELECT * FROM Words WHERE word = '{0}'", word);
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

        private void UpdateData(string changeWord, int value)
        {
            var cmdString = string.Format(@"UPDATE Words SET count = {0}
                                            WHERE word = '{1}'", value, changeWord);

            UsingCommand(cmdString);
        }

        private void InsertData(string word, int value)
        {
            var cmdString = string.Format(@"INSERT Words(word, count)
                                            VALUES ('{0}', {1})", word, value);

            UsingCommand(cmdString);
        }

        private void UsingCommand(string cmdString)
        {
            sqlCmd = new SqlCommand(cmdString, sqlConn);
            sqlCmd.ExecuteNonQuery();
        }
    }
}
