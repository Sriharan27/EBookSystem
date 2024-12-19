using EBookSystem.Data;
using EBookSystem.Models;
using EBookSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EBookSystem.Services.Repository
{
    public class BooksRepository : IBooksRepository
    {
        private readonly ApplicationDbContext _context;
        public BooksRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool AddBook(Books book)
        {
            _context.Add(book);
            return Save();
        }
        public bool UpdateBook(Books book)
        {
            _context.Update(book);
            return Save();
        }
        public bool DeleteBook(Books book)
        {
            _context.Remove(book);
            return Save();
        }

        public async Task<IEnumerable<Books>> GetAllBooks()
        {
            return await _context.Books.Include(b => b.Genre).ToListAsync();
        }

        public async Task<Books> GetBookByIdAsync(int id)
        {
            return await _context.Books.Include(b => b.Genre).FirstOrDefaultAsync(b => b.BookId == id);
        }
        public async Task<int> GetBooksCountAsync()
        {
            return await _context.Books.CountAsync();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}
