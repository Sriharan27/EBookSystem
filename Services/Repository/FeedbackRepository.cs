using EBookSystem.Data;
using EBookSystem.Models;
using EBookSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EBookSystem.Services.Repository
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly ApplicationDbContext _context;
        public FeedbackRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool AddFeedback(Feedback feedback)
        {
            _context.Add(feedback);
            return Save();
        }
        public bool UpdateFeedback(Feedback feedback)
        {
            _context.Update(feedback);
            return Save();
        }
        public bool DeleteFeedback(Feedback feedback)
        {
            _context.Remove(feedback);
            return Save();
        }

        public async Task<IEnumerable<Feedback>> GetAllFeedbacks()
        {
            return await _context.Feedbacks.Include(f => f.User).Include(f => f.Book).Include(f => f.Book.Genre).ToListAsync();
        }

        public async Task<Feedback> GetFeedbackByIdAsync(int id)
        {
            return await _context.Feedbacks.Include(f => f.User).Include(f => f.Book).FirstOrDefaultAsync(f => f.FeedbackId == id);
        }   
        public async Task<int> GetFeedbacksCountAsync()
        {
            return await _context.Feedbacks.CountAsync();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}

