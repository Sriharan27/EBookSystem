using EBookSystem.Data;
using EBookSystem.Models;
using EBookSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EBookSystem.Services.Repository
{
    public class UsersRepository : IUsersRepository
    {
        private readonly ApplicationDbContext _context;
        public UsersRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool AddUser(Users user)
        {
            _context.Add(user);
            return Save();
        }
        public bool UpdateUser(Users user)
        {
            _context.Update(user);
            return Save();
        }
        public bool DeleteUser(Users user)
        {
            _context.Remove(user);
            return Save();
        }

        public async Task<IEnumerable<Users>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<Users> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<Users> GetUserByEmailAsync(string Email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
        }
        public async Task<int> GetUsersCountAsync()
        {
            return await _context.Users.Where(u => u.Role == "Customer").CountAsync();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}
