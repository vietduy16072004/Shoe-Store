using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shoe.Models
{
    public class HistoryShipping
    {
        [Key]
        public long Id { get; set; }

        public long ShipmentId { get; set; }
        [ForeignKey("ShipmentId")]
        public Shipment Shipment { get; set; } = null!;

        public string? Status { get; set; } // Trạng thái từ GHN (Picking, Delivering...)
        public string? Description { get; set; } // Mô tả chi tiết
        public string? Location { get; set; } // Bưu cục hiện tại
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}