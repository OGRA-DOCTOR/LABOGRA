using LABOGRA.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace LABOGRA.Services
{
    public class UserService : IUserService
    {
        private readonly string _filePath;

        public UserService()
        {
            // Get the base directory where the application is running
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Combine the base directory with the relative path to users.xml
            // This assumes that your Services/Database/Data structure will be present
            // in the output directory relative to the executable.
            _filePath = Path.Combine(baseDirectory, "Services", "Database", "Data", "users.xml");

            EnsureDataDirectoryExists();
        }

        private void EnsureDataDirectoryExists()
        {
            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    // Log or handle the exception if directory creation fails
                    Console.WriteLine($"Error creating directory {_filePath}: {ex.Message}");
                    // Depending on the severity, you might want to re-throw or handle differently
                }
            }
        }

        public List<User> LoadUsers()
        {
            if (!File.Exists(_filePath))
            {
                SaveUsers(new List<User>());
                return new List<User>();
            }

            try
            {
                XDocument doc = XDocument.Load(_filePath);
                return doc.Root?.Elements("User")
                    .Select(x => new User
                    {
                        Username = x.Element("Username")?.Value ?? string.Empty,
                        PasswordHash = x.Element("Password")?.Value ?? string.Empty,
                        Role = x.Element("Role")?.Value ?? string.Empty
                    }).ToList() ?? new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users from {_filePath}: {ex.Message}");
                return new List<User>();
            }
        }

        public void SaveUsers(List<User> users)
        {
            try
            {
                XDocument doc = new XDocument(
                    new XElement("Users",
                        users.Select(u =>
                            new XElement("User",
                                new XElement("Username", u.Username),
                                new XElement("Password", u.PasswordHash),
                                new XElement("Role", u.Role)
                            )
                        )
                    )
                );

                EnsureDataDirectoryExists();
                doc.Save(_filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users to {_filePath}: {ex.Message}");
                throw;
            }
        }

        public User? ValidateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            string passwordHash = HashPassword(password);
            var users = LoadUsers();
            return users.FirstOrDefault(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) &&
                u.PasswordHash == passwordHash);
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return string.Empty;
            }

            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public void AddUser(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("User object is null or username is empty.", nameof(user));
            }

            var users = LoadUsers();

            if (users.Any(u => string.Equals(u.Username, user.Username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"User '{user.Username}' already exists.");
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new ArgumentException("Password cannot be empty for a new user.", nameof(user.PasswordHash));
            }

            if (!IsBase64String(user.PasswordHash))
            {
                user.PasswordHash = HashPassword(user.PasswordHash);
            }

            users.Add(user);
            SaveUsers(users);
        }

        public void UpdateUser(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("User object is null or username is empty.", nameof(user));
            }

            var users = LoadUsers();
            var existingUser = users.FirstOrDefault(u =>
                string.Equals(u.Username, user.Username, StringComparison.OrdinalIgnoreCase));

            if (existingUser != null)
            {
                existingUser.Role = user.Role;

                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    if (!IsBase64String(user.PasswordHash))
                    {
                        existingUser.PasswordHash = HashPassword(user.PasswordHash);
                    }
                    else
                    {
                        existingUser.PasswordHash = user.PasswordHash;
                    }
                }
                SaveUsers(users);
            }
            else
            {
                throw new InvalidOperationException($"User '{user.Username}' not found for update.");
            }
        }

        public void DeleteUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be empty.", nameof(username));
            }

            var users = LoadUsers();
            int removedCount = users.RemoveAll(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (removedCount > 0)
            {
                SaveUsers(users);
            }
        }

        public bool UserExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }
            var users = LoadUsers();
            return users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsBase64String(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();
            return (s.Length % 4 == 0) && System.Text.RegularExpressions.Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", System.Text.RegularExpressions.RegexOptions.None);
        }
    }
}