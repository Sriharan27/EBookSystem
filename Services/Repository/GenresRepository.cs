using EBookSystem.Data;
using EBookSystem.Models;
using EBookSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EBookSystem.Services.Repository
{
    public class GenresRepository : IGenresRepository
    {
        private readonly ApplicationDbContext _context;
        public GenresRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool AddGenre(Genres genre)
        {
            _context.Add(genre);
            return Save();
        }
        public bool UpdateGenre(Genres genre)
        {
            _context.Update(genre);
            return Save();
        }
        public bool DeleteGenre(Genres genre)
        {
            _context.Remove(genre);
            return Save();
        }

        public async Task<IEnumerable<Genres>> GetAllGenres()
        {
            return await _context.Genres.ToListAsync();
        }

        public async Task<Genres> GetGenreByIdAsync(int id)
        {
            return await _context.Genres.FirstOrDefaultAsync(g => g.GenreId == id);
        }
        public async Task<int> GetGenresCountAsync()
        {
            return await _context.Genres.CountAsync();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}
