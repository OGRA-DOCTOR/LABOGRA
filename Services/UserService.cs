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
        private static UserService? _instance;
        private static readonly object _lock = new object();

        // Singleton pattern for static-like access
        public static UserService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new UserService();
                        }
                    }
                }
                return _instance;
            }
        }

        // Static methods for backward compatibility
        public static List<User> LoadUsers() => Instance.GetLoadUsers();
        public static void SaveUsers(List<User> users) => Instance.GetSaveUsers(users);
        public static User? ValidateUser(string username, string password) => Instance.GetValidateUser(username, password);
        public static string HashPassword(string password) => Instance.GetHashPassword(password);
        public static void AddUser(User user) => Instance.GetAddUser(user);
        public static void UpdateUser(User user) => Instance.GetUpdateUser(user);
        public static void DeleteUser(string username) => Instance.GetDeleteUser(username);
        public static bool UserExists(string username) => Instance.GetUserExists(username);

        // Private constructor for Singleton
        private UserService()
        {
            _filePath = Path.Combine("Database", "Data", "users.xml");
        }

        // Public constructor for dependency injection
        public UserService(string? customPath = null)
        {
            _filePath = customPath ?? Path.Combine("Database", "Data", "users.xml");
        }

        // Instance methods with Get prefix to avoid confusion
        public List<User> GetLoadUsers()
        {
            if (!File.Exists(_filePath))
            {
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
                // Log error if needed
                Console.WriteLine($"Error loading users: {ex.Message}");
                return new List<User>();
            }
        }

        public void GetSaveUsers(List<User> users)
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

                string directory = Path.GetDirectoryName(_filePath) ?? "";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                doc.Save(_filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users: {ex.Message}");
                throw;
            }
        }

        public User? GetValidateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            string passwordHash = GetHashPassword(password);
            var users = GetLoadUsers();
            return users.FirstOrDefault(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) &&
                u.PasswordHash == passwordHash);
        }

        public string GetHashPassword(string password)
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

        public void GetAddUser(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("Invalid user data");
            }

            var users = GetLoadUsers();

            if (users.Any(u => string.Equals(u.Username, user.Username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"User '{user.Username}' already exists");
            }

            if (!string.IsNullOrEmpty(user.PasswordHash) && !IsBase64String(user.PasswordHash))
            {
                user.PasswordHash = GetHashPassword(user.PasswordHash);
            }

            users.Add(user);
            GetSaveUsers(users);
        }

        public void GetUpdateUser(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("Invalid user data");
            }

            var users = GetLoadUsers();
            var existingUser = users.FirstOrDefault(u =>
                string.Equals(u.Username, user.Username, StringComparison.OrdinalIgnoreCase));

            if (existingUser != null)
            {
                existingUser.Role = user.Role;

                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    existingUser.PasswordHash = IsBase64String(user.PasswordHash)
                        ? user.PasswordHash
                        : GetHashPassword(user.PasswordHash);
                }

                GetSaveUsers(users);
            }
            else
            {
                throw new InvalidOperationException($"User '{user.Username}' not found");
            }
        }

        public void GetDeleteUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be empty");
            }

            var users = GetLoadUsers();
            int removedCount = users.RemoveAll(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (removedCount > 0)
            {
                GetSaveUsers(users);
            }
        }

        public bool GetUserExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            var users = GetLoadUsers();
            return users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsBase64String(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            try
            {
                Convert.FromBase64String(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Interface implementation (for dependency injection scenarios)
        List<User> IUserService.LoadUsers() => GetLoadUsers();
        void IUserService.SaveUsers(List<User> users) => GetSaveUsers(users);
        User? IUserService.ValidateUser(string username, string password) => GetValidateUser(username, password);
        string IUserService.HashPassword(string password) => GetHashPassword(password);
        void IUserService.AddUser(User user) => GetAddUser(user);
        void IUserService.UpdateUser(User user) => GetUpdateUser(user);
        void IUserService.DeleteUser(string username) => GetDeleteUser(username);
        bool IUserService.UserExists(string username) => GetUserExists(username);
    }
}