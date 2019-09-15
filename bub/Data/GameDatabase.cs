using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace bub.Data
{
    public class GameDatabase : IDisposable
    {
        private static readonly string DataFileName = "Bubbler.db";

        private SQLiteConnection m_connection;

        public GameDatabase()
        {
            if (!File.Exists(DataFileName))
                SQLiteConnection.CreateFile(DataFileName);
            m_connection = new SQLiteConnection($"Data Source={DataFileName};Version=3;");
            m_connection.Open();

            using (var cmd = new SQLiteCommand(m_connection))
            {
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Users (id INTEGER PRIMARY KEY AUTOINCREMENT, userName TEXT)";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand(m_connection))
            {
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Results (id INTEGER PRIMARY KEY AUTOINCREMENT, userId INTEGER, score INTEGER, results_date TEXT)";
                cmd.ExecuteNonQuery();
            }
        }

        public int AddUser(string userName)
        {
            using (var cmd = new SQLiteCommand(m_connection))
            {
                cmd.CommandText = $"INSERT INTO Users (userName) VALUES ('{userName}')";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = new SQLiteCommand(m_connection))
            {
                cmd.CommandText = $"SELECT id FROM Users WHERE userName='{userName}'";
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        return reader.GetInt32(0);
            }
            return 0;
        }

        public void AddResult(User user, int score, DateTime when)
        {
            using (var cmd = new SQLiteCommand(m_connection))
            {
                cmd.CommandText = $"INSERT INTO Results (userId, score, results_date) VALUES ({user.Id}, {score}, '{when.ToString()}')";
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<User> Users
        {
            get
            {
                using (var cmd = new SQLiteCommand(m_connection))
                {
                    cmd.CommandText = "SELECT userName, id FROM Users";
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            yield return new User(reader.GetString(0), reader.GetInt32(1)); // "userName", "id"
                }
            }
        }

        public IEnumerable<GameResult> GameResults
        {
            get
            {
                using (var cmd = new SQLiteCommand(m_connection))
                {
                    cmd.CommandText = "SELECT u.userName, r.score, r.results_date FROM Users u, Results r WHERE u.id=r.userId ORDER by r.score DESC";
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            yield return new GameResult(reader.GetString(0), reader.GetInt32(1), reader.GetString(2)); // "userName", "score"
                }
            }
        }

        public void Dispose()
        {
            if (null != m_connection)
            {
                m_connection.Dispose();
                m_connection = null;
            }
        }
    }
}
