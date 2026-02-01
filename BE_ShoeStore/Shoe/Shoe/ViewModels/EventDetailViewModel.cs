using System;

namespace Shoe.ViewModels
{
    public class EventDetailListViewModel
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }

        // Hiển thị chuỗi format (VD: "20%" hoặc "50,000đ")
        public string DiscountDisplay { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        // Đếm xem có bao nhiêu sản phẩm trong sự kiện này
        public int ProductCount { get; set; }

        // Kiểm tra xem sự kiện có đang diễn ra không (dựa vào ngày tháng)
        public string StatusLabel { get; set; }
    }
}