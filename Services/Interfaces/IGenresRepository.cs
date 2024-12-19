using EBookSystem.Models;

namespace EBookSystem.Services.Interfaces
{
    public interface IGenresRepository
    {
        Task<IEnumerable<Genres>> GetAllGenres();
        Task<Genres> GetGenreByIdAsync(int id);
        Task<int> GetGenresCountAsync();
        bool AddGenre(Genres genre);
        bool UpdateGenre(Genres genre);
        bool DeleteGenre(Genres genre);
        bool Save();
    }
}
