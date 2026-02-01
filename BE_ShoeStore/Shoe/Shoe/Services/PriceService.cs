using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using System;
using System.Linq;

namespace Shoe.Services
{
    public class PriceService
    {
        private readonly ApplicationDbContext _context;

        public PriceService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hàm trả về giá cuối cùng (đã trừ khuyến mãi sâu nhất)
        public decimal CalculateBestPrice(int productId)
        {
            // 1. Lấy thông tin sản phẩm
            var product = _context.Products.Find(productId);
            if (product == null) return 0;

            decimal originalPrice = product.Price;
            decimal maxDiscountAmount = 0; // Biến lưu số tiền giảm lớn nhất tìm được

            // --- TRƯỜNG HỢP A: Giảm giá trực tiếp trên sản phẩm (Product.Discount) ---
            // Giả sử Product.Discount là % (VD: 50%)
            if (product.Discount > 0)
            {
                decimal directDiscount = originalPrice * ((decimal)product.Discount / 100m);
                if (directDiscount > maxDiscountAmount)
                {
                    maxDiscountAmount = directDiscount;
                }
            }

            // --- TRƯỜNG HỢP B: Giảm giá từ Sự kiện (DiscountEvents) ---
            var activeEvents = _context.EventDetails
                .Include(ed => ed.DiscountEvent)
                .Where(ed => ed.ProductID == productId &&
                             ed.DiscountEvent.IsActive == true &&
                             ed.DiscountEvent.StartDate <= DateTime.Now &&
                             ed.DiscountEvent.EndDate >= DateTime.Now)
                .ToList();

            foreach (var detail in activeEvents)
            {
                var evt = detail.DiscountEvent;
                decimal currentEventDiscount = 0;

                if (evt.DiscountType == 1) // Giảm theo % (VD: 20%)
                {
                    currentEventDiscount = originalPrice * ((decimal)evt.DiscountValue / 100m);
                }
                else if (evt.DiscountType == 2) // Giảm tiền mặt (VD: 30.000đ)
                {
                    currentEventDiscount = (decimal)evt.DiscountValue;
                }

                // So sánh: Nếu mức giảm này ngon hơn mức hiện tại -> Cập nhật
                if (currentEventDiscount > maxDiscountAmount)
                {
                    maxDiscountAmount = currentEventDiscount;
                }
            }

            // 3. Tính giá cuối cùng
            decimal finalPrice = originalPrice - maxDiscountAmount;

            // Đảm bảo giá không bị âm
            return finalPrice < 0 ? 0 : finalPrice;
        }
    }
}