using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;

namespace CreditBot
{
    public static class DataManager
    {
        private static string _dbName = "CreditBot.db";
        private static SQLiteConnection _sqlConn; 
        private static SQLiteConnection SqlConn
        {
            get
            {
                if(_sqlConn == null)
                    _sqlConn = new SQLiteConnection(string.Format("Data Source = {0}; Version = 3;", _dbName));

                return _sqlConn;
            }
        }

        public static void InitializeDatabase()
        {
            if (!File.Exists(_dbName))
                CreateDatabase();
        }

        public static User GetUser(string name)
        {
            try
            {
                SqlConn.Open();

                SQLiteCommand sqlCommand = new SQLiteCommand(SqlConn);
                sqlCommand.CommandText = string.Format("SELECT * FROM Users WHERE Name = '{0}'", name);

                using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
                {
                    reader.Read();

                    if (reader.HasRows)
                        return new User(reader["Name"].ToString(), int.Parse(reader["Value"].ToString()));
                    else
                        return null;
                }
            }
            finally
            {
                SqlConn.Close();
            }
        }

        internal static void SaveUserData(User userObj)
        {
            try
            {
                SqlConn.Open();

                SQLiteCommand sqlCommand = new SQLiteCommand(SqlConn);
                sqlCommand.CommandText = string.Format("SELECT * FROM Users WHERE Name = '{0}'", userObj.UserName);

                SQLiteDataReader reader = sqlCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Close();
                    sqlCommand.CommandText = string.Format("UPDATE Users SET Value = {0} WHERE Name = '{1}'", userObj.Value, userObj.UserName);
                    sqlCommand.ExecuteNonQuery();
                }
                else
                {
                    reader.Close();
                    sqlCommand.CommandText = string.Format("INSERT INTO Users (Name, Value) VALUES ('{0}', {1})", userObj.UserName, userObj.Value);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            finally
            {
                SqlConn.Close();
            }
        }

        private static void CreateDatabase()
        {
            try
            {
                SQLiteConnection.CreateFile(_dbName);

                SqlConn.Open();

                SQLiteCommand sqlCommand = new SQLiteCommand(SqlConn);
                sqlCommand.CommandText = "CREATE TABLE Users (Name TEXT, Value INTEGER)";

                sqlCommand.ExecuteNonQuery();
            }
            finally
            {
                SqlConn.Close();
            }
        }
    }
}
