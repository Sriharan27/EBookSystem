using EBookSystem.Models;

namespace EBookSystem.Services.Interfaces
{
    public interface IUsersRepository
    {
        Task<IEnumerable<Users>> GetAllUsers();
        Task<Users> GetUserByIdAsync(int id);
        Task<Users> GetUserByEmailAsync(string Email);
        Task<int> GetUsersCountAsync();
        bool AddUser(Users user);
        bool UpdateUser(Users user);
        bool DeleteUser(Users user);
        bool Save();
    }
}
