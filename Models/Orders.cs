using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBookSystem.Models
{
    public class Orders
    {
        [Key]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status {  get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }
    }
}
