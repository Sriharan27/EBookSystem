using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBookSystem.Models
{
    public class OrderLineItems
    {
        [Key]
        public int OrderLineItemId { get; set; }
        public int OrderId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        [ForeignKey("OrderId")]
        public Orders Order { get; set; }

        [ForeignKey("BookId")]
        public Books Book { get; set; }
    }
}
