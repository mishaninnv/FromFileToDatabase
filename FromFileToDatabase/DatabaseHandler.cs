using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace FromFileToDatabase
{
    public class DatabaseHandler
    {
        public static void WriteToDatabase(string databaseName, List<LoadedWord> inputData)
        {
            string connString = @"Server=localhost\SQLEXPRESS01;Trusted_Connection=True;";

            var conn = new SqlConnection(connString);
            var isExistDb = CheckDatabaseExists(conn, databaseName);

            if (!isExistDb)
            {
                var isCreatedDb = CreateDatabase(conn, databaseName);
                if (isCreatedDb) conn.ConnectionString = string.Format(@"Server=localhost\SQLEXPRESS01;Database={0};Trusted_Connection=True;", databaseName);
            }
            else conn.ConnectionString = string.Format(@"Server=localhost\SQLEXPRESS01;Database={0};Trusted_Connection=True;", databaseName);

            CreateTable(conn);
            WriteToTable(conn, inputData);
        }

        private static bool CheckDatabaseExists(SqlConnection tmpConn, string databaseName)
        {
            var result = false;
            var sqlCreateDB = string.Format("SELECT * FROM sys.databases WHERE Name = '{0}'", databaseName);
            using (var sqlCmd = new SqlCommand(sqlCreateDB, tmpConn))
            {
                tmpConn.Open();
                object resultObj = sqlCmd.ExecuteScalar();
                if (resultObj != null && resultObj.ToString().Equals(databaseName))
                {
                    result = true;
                }
            }
            tmpConn.Close();
            return result;
        }

        private static bool CreateDatabase(SqlConnection tmpConn, string databaseName)
        {
            var result = false;
            var cmdString = string.Format("CREATE DATABASE {0};", databaseName);

            try
            {
                using (var sqlCmd = new SqlCommand(cmdString, tmpConn))
                {
                    tmpConn.Open();
                    sqlCmd.ExecuteNonQuery();
                    result = true;
                }                    
            }
            catch
            {
                Console.WriteLine("Проверьте имя сервера и базы данных.");
            }

            tmpConn.Close();
            return result;
        }

        private static void CreateTable(SqlConnection tmpConn)
        {
            var cmdString =
                @"IF NOT (EXISTS ( SELECT * FROM DataBaseTest.INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_NAME = 'Words'))
                        CREATE TABLE Words (
                        word NVARCHAR(200) UNIQUE, 
                        count INT
                        )";
            using (var sqlCmd = new SqlCommand(cmdString, tmpConn))
            {
                tmpConn.Open();
                sqlCmd.ExecuteNonQuery();
            }
            tmpConn.Close();
        }

        private static void WriteToTable(SqlConnection tmpConn, List<LoadedWord> inputData)
        {
            foreach (var word in inputData)
            {
                var cmdString2 = string.Format(@"SELECT * FROM Words WHERE word = '{0}'", word.Name);

                using (var sqlCmd = new SqlCommand(cmdString2, tmpConn))
                {
                    tmpConn.Open();
                    var reader = sqlCmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        var currCount = reader.GetInt32(1) + word.Count;
                        reader.Close();

                        var cmdString = string.Format(@"UPDATE Words
                                                        SET count = {0}
                                                        WHERE word = '{1}'", currCount, word.Name);

                        sqlCmd.CommandText = cmdString;
                        sqlCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        reader.Close();

                        var cmdString = string.Format(@"INSERT Words(word, count)
                                                VALUES ('{0}', {1})", word.Name, word.Count);

                        sqlCmd.CommandText = cmdString;
                        sqlCmd.ExecuteNonQuery();
                    }
                    tmpConn.Close();
                }
            }            
        }
    }
}
