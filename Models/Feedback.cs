using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBookSystem.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }

        [ForeignKey("BookId")]
        public Books Book { get; set; }
        public DateTime EnteredDate { get; set; } = DateTime.Now;
    }
}
