﻿using MySql.Data.MySqlClient;
using System;

namespace Server
{
    public class Database
    {
        private string connectionString = "server=localhost;user=root;database=phpmyadmin;port=3306";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // Method to register a new user
        public bool RegisterUser(string username, string password, string email)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO user (username, password, email) VALUES (@username, @password, @Email)";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@Email", email);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        return false;
                    }
                }
            }
        }
        public bool ValidateLogin(string username, string password)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM user WHERE username = @username AND password = @password";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
        // Phương thức để lấy thông tin người dùng
        public string? LoadUserInfo(string username)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT username, product_name FROM user WHERE username = @username";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string userInfo = $"Tên tài khoản: {reader["username"]}";
                            return userInfo; // Trả về thông tin người dùng
                        }
                    }
                }
            }
            return null; // Trả về null nếu không tìm thấy
        }
    }
}
