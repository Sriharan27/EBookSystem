using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBookSystem.Models
{
    public class Books
    {
        [Key]
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public int GenreId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime EnteredDate{ get; set; } = DateTime.Now;
        public byte[] BookImage { get; set; }

        [ForeignKey("GenreId")]
        public Genres Genre { get; set; }
    }
}
