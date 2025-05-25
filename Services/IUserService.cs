using LABOGRA.Models;
namespace LABOGRA.Services
{
    public interface IUserService
    {
        List<User> LoadUsers();
        void SaveUsers(List<User> users);
        User? ValidateUser(string username, string password);
        string HashPassword(string password);
        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteUser(string username);
        bool UserExists(string username);
    }
}