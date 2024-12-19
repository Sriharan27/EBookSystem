using EBookSystem.Models;

namespace EBookSystem.Services.Interfaces
{
    public interface IBooksRepository
    {
        Task<IEnumerable<Books>> GetAllBooks();
        Task<Books> GetBookByIdAsync(int id);
        Task<int> GetBooksCountAsync();
        bool AddBook(Books book);
        bool UpdateBook(Books book);
        bool DeleteBook(Books book);
        bool Save();
    }
}
