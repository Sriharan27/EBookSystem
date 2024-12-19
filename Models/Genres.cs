using System.ComponentModel.DataAnnotations;

namespace EBookSystem.Models
{
    public class Genres
    {
        [Key]
        public int GenreId { get; set; }
        public string Name { get; set; }
        public DateTime EnteredDate { get; set; } = DateTime.Now;
    }
}
