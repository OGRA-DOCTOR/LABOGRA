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
    public static class UserService
    {
        private static readonly string filePath = Path.Combine("Database", "Data", "users.xml");

        public static List<User> LoadUsers()
        {
            if (!File.Exists(filePath))
            {
                return new List<User>();
            }

            try
            {
                XDocument doc = XDocument.Load(filePath);
                return doc.Root?.Elements("User")
                    .Select(x => new User
                    {
                        Username = x.Element("Username")?.Value ?? string.Empty,
                        PasswordHash = x.Element("Password")?.Value ?? string.Empty,
                        Role = x.Element("Role")?.Value ?? string.Empty
                    }).ToList() ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        public static void SaveUsers(List<User> users)
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

            string directory = Path.GetDirectoryName(filePath) ?? "";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            doc.Save(filePath);
        }

        public static User? ValidateUser(string username, string password)
        {
            string passwordHash = HashPassword(password);
            var users = LoadUsers();
            return users.FirstOrDefault(u => u.Username == username && u.PasswordHash == passwordHash);
        }

        public static string HashPassword(string password)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
