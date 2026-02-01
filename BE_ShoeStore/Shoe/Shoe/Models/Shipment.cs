using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shoe.Models
{
    public class Shipment
    {
        [Key]
        public long ShipmentId { get; set; } // Primary Key

        public long Bill_Id { get; set; } // Foreign Key to Order
        [ForeignKey("Bill_Id")]
        public Order Order { get; set; } = null!;

        public int CarrierId { get; set; } // Foreign Key to Carrier
        [ForeignKey("CarrierId")]
        public Carrier Carrier { get; set; } = null!;

        public string? TrackingCode { get; set; } // Mã vận đơn GHN trả về
        public DateTime? ExpectedDeliveryTime { get; set; } // Thời gian giao dự kiến
        public decimal ActualShippingFee { get; set; } // Phí ship thực tế trả cho GHN

        // Navigation Log
        public List<HistoryShipping>? ShippingLogs { get; set; }
    }
}