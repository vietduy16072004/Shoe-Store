using System;
using System.Collections.Generic;

namespace Shoe.ViewModels
{
    // ViewModel này dùng riêng cho trang XEM CHI TIẾT (Details)
    public class DiscountEventDetailViewModel
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string DiscountDisplay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string StatusLabel { get; set; }

        // ĐIỂM KHÁC BIỆT: Chứa danh sách sản phẩm cụ thể để hiển thị ra bảng
        public List<ProductInEventViewModel> Products { get; set; } = new List<ProductInEventViewModel>();
    }

    // Class con để hứng dữ liệu từng sản phẩm trong sự kiện
    public class ProductInEventViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal OriginalPrice { get; set; }
        public string ImageUrl { get; set; }
        public string CategoryName { get; set; }
    }
}