using Shoe.Enum;
using System.Transactions;

namespace Shoe.Models
{
    public class Order
    {
        public long Bill_Id { set; get; }
        public DateTime OrderDate { set; get; }
        public string? UserName { set; get; }
        public string? Address { set; get; }
        
        // --- BỔ SUNG CHO GHN ---
        public int ProvinceId { get; set; } // ID Tỉnh/TP
        public int DistrictId { get; set; } // ID Quận/Huyện (GHN cần cái này)
        public string? WardCode { get; set; } // Mã Phường/Xã (GHN cần cái này)

        public string? Email { set; get; }
        public string? PhoneNumber { set; get; }
        public decimal Totalprice { set; get; }
        public decimal ShippingFee { set; get; }
        public string? Note { set; get; }
        public Status Status { set; get; } //InProgress, Confirmed, Shipping, Success, Canceled
        public string? VnpayTxnRef { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
        public Shipment? Shipment { get; set; }
        public User? AppUser { set; get; }
        public Guid UserId { set; get; }
    }

}
