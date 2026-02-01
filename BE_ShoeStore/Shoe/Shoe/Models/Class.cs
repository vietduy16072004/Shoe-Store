using System.ComponentModel.DataAnnotations;

namespace Shoe.Models
{
    public class Carrier
    {
        [Key]
        public int CarrierId { get; set; }
        public string CarrierName { get; set; } = null!; // VD: Giao Hàng Nhanh
        public string ApiBaseUrl { get; set; } = null!; // VD: https://online-gateway.ghn.vn/shiip/public-api/v2/
        public string Token { get; set; } = null!; // Token bạn gửi
        public string? ShopId { get; set; } // GHN thường cần ShopId để định danh kho gửi
        public bool IsActive { get; set; }
    }
}