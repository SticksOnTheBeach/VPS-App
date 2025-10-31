using System;
using System.IO;
using Microsoft.Data.Sqlite;
using BCrypt.Net; // ✅ pour HashPassword & Verify

namespace VPSControl.Services
{
    public class Database
    {
        private readonly string _dbPath;

        public Database()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "VPSControl"
            );
            Directory.CreateDirectory(folder);
            _dbPath = Path.Combine(folder, "vpscontrol.db");
        }

        public void Init()
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                email TEXT NOT NULL UNIQUE,
                password TEXT NOT NULL
            );";
            cmd.ExecuteNonQuery();
        }

        public bool AddUser(string email, string passwordPlain)
        {
            // Hashage sécurisé
            var pass = BCrypt.Net.BCrypt.HashPassword(passwordPlain); // ✅ syntaxe fully-qualified

            try
            {
                using var conn = new SqliteConnection($"Data Source={_dbPath}");
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO users (email, password) VALUES ($e, $p)";
                cmd.Parameters.AddWithValue("$e", email);
                cmd.Parameters.AddWithValue("$p", pass);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqliteException)
            {
                return false;
            }
        }

        public bool CheckUser(string email, string passwordPlain)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT password FROM users WHERE email = $e LIMIT 1";
            cmd.Parameters.AddWithValue("$e", email);
            var r = cmd.ExecuteReader();
            if (r.Read())
            {
                var hash = r.GetString(0);
                return BCrypt.Net.BCrypt.Verify(passwordPlain, hash); // ✅ syntaxe fully-qualified
            }
            return false;
        }
    }
}
