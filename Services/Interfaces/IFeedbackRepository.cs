using EBookSystem.Models;

namespace EBookSystem.Services.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<IEnumerable<Feedback>> GetAllFeedbacks();
        Task<Feedback> GetFeedbackByIdAsync(int id);
        Task<int> GetFeedbacksCountAsync();
        bool AddFeedback(Feedback feedback);
        bool UpdateFeedback(Feedback feedback);
        bool DeleteFeedback(Feedback feedback);
        bool Save();
    }
}
