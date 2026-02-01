using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shoe.Models
{
    public class EventDetail
    {
        [Key]
        public int Id { get; set; }

        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public DiscountEvent DiscountEvent { get; set; }

        public int ProductID { get; set; }
        [ForeignKey("ProductID")]
        public Product Product { get; set; }

        public bool Status { get; set; } = true; // Trạng thái kích hoạt trong sự kiện này
    }
}