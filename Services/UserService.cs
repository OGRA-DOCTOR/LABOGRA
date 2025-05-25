using LABOGRA.Models;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.IO;
namespace LABOGRA.Services
{
    // تحويل من static class إلى instance class
    public class UserService : IUserService
    {
        private readonly string _filePath = Path.Combine("Database", "Data", "users.xml");
        public List<User> LoadUsers()
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
            catch
            {
                return new List<User>();
            }
        }
        public void SaveUsers(List<User> users)
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
                Directory.CreateDirectory(directory);
            doc.Save(_filePath);
        }
        public User? ValidateUser(string username, string password)
        {
            string passwordHash = HashPassword(password);
            var users = LoadUsers();
            return users.FirstOrDefault(u => u.Username == username && u.PasswordHash == passwordHash);
        }
        public string HashPassword(string password)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
        public void AddUser(User user)
        {
            var users = LoadUsers();

            if (!string.IsNullOrEmpty(user.PasswordHash) && !IsBase64String(user.PasswordHash))
            {
                user.PasswordHash = HashPassword(user.PasswordHash);
            }

            users.Add(user);
            SaveUsers(users);
        }
        public void UpdateUser(User user)
        {
            var users = LoadUsers();
            var existingUser = users.FirstOrDefault(u => u.Username == user.Username);

            if (existingUser != null)
            {
                existingUser.Role = user.Role;

                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    existingUser.PasswordHash = IsBase64String(user.PasswordHash)
                        ? user.PasswordHash
                        : HashPassword(user.PasswordHash);
                }

                SaveUsers(users);
            }
        }
        public void DeleteUser(string username)
        {
            var users = LoadUsers();
            users.RemoveAll(u => u.Username == username);
            SaveUsers(users);
        }
        public bool UserExists(string username)
        {
            var users = LoadUsers();
            return users.Any(u => u.Username == username);
        }
        private bool IsBase64String(string value)
        {
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
    }
}