using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Models;
using Shoe.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shoe.Controllers
{
    [Route("api/[controller]")]
    [ApiController] // Sử dụng ApiController để tự động xử lý ModelState và trả về JSON
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PriceService _priceService;

        public CartController(ApplicationDbContext context, PriceService priceService)
        {
            _context = context;
            _priceService = priceService;
        }

        // Hàm hỗ trợ lấy UserId từ Session (Đồng bộ với hệ thống đăng nhập của bạn)
        private Guid? GetUserId()
        {
            var idStr = HttpContext.Session.GetString("UserId");
            return string.IsNullOrEmpty(idStr) ? null : Guid.Parse(idStr);
        }

        // ===================== 1. LẤY DANH SÁCH GIỎ HÀNG (GET) =====================
        // Tích hợp PriceService để luôn trả về mức giá ưu đãi nhất hiện tại
        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { message = "Vui lòng đăng nhập" });

            var items = await _context.CartItems
                .Include(c => c.ProductDetail!).ThenInclude(pd => pd.Product)
                .Include(c => c.ProductDetail!).ThenInclude(pd => pd.Size)
                .Include(c => c.ProductDetail!).ThenInclude(pd => pd.Variant)
                .Where(c => c.UserId == userId.Value)
                .ToListAsync();

            var result = items.Select(c => {
                decimal currentUnitPrice = _priceService.CalculateBestPrice(c.ProductDetail.Product_Id);
                return new
                {
                    cart_Id = c.Cart_Id,
                    productDetail_Id = c.ProductDetail_Id,
                    productId = c.ProductDetail.Product_Id, // BẮT BUỘC PHẢI CÓ DÒNG NÀY
                    productName = c.ProductDetail.Product.Product_Name,
                    imageUrl = c.ProductDetail.Product.ImageUrl,
                    sizeName = c.ProductDetail.Size.Size_Name,
                    variantName = c.ProductDetail.Variant.Variants_Name,
                    quantity = c.Quantity,
                    unitPrice = currentUnitPrice,
                    totalPrice = currentUnitPrice * c.Quantity,
                    stock = c.ProductDetail.Quantity
                };
            });

            return Ok(result);
        }

        // ===================== 2. THÊM SẢN PHẨM VÀO GIỎ (POST) =====================
        // Xử lý logic thêm mới hoặc cộng dồn số lượng nếu sản phẩm đã có trong giỏ
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest req)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { message = "Vui lòng đăng nhập" });

            // Tìm cấu hình chi tiết sản phẩm (Size + Màu)
            var detail = await _context.ProductDetails
                .FirstOrDefaultAsync(pd => pd.Product_Id == req.ProductId &&
                                         pd.Size_Id == req.SizeId &&
                                         pd.Variants_Id == req.VariantId);

            if (detail == null) return BadRequest(new { message = "Tùy chọn sản phẩm không tồn tại" });
            if (detail.Quantity < req.Quantity) return BadRequest(new { message = "Số lượng trong kho không đủ" });

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.ProductDetail_Id == detail.ProductDetail_Id);

            if (existingItem != null)
            {
                // Nếu đã có trong giỏ thì cộng dồn số lượng
                existingItem.Quantity += req.Quantity;
                if (existingItem.Quantity > detail.Quantity) existingItem.Quantity = detail.Quantity;

                decimal unitPrice = _priceService.CalculateBestPrice(detail.Product_Id);
                existingItem.Price = unitPrice * existingItem.Quantity;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                // Nếu chưa có thì tạo mới CartItem
                decimal unitPrice = _priceService.CalculateBestPrice(detail.Product_Id);
                var newItem = new CartItem
                {
                    UserId = userId.Value,
                    ProductDetail_Id = detail.ProductDetail_Id,
                    Quantity = req.Quantity,
                    Price = unitPrice * req.Quantity,
                    DateCreated = DateTime.Now
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng thành công!" });
        }

        // ===================== 3. CẬP NHẬT SỐ LƯỢNG (PUT) =====================
        // API dùng để lưu thay đổi khi người dùng nhấn nút +/- ở trang giỏ hàng
        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQtyRequest req)
        {
            var item = await _context.CartItems
                .Include(c => c.ProductDetail)
                .FirstOrDefaultAsync(c => c.Cart_Id == req.CartId);

            if (item == null) return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ" });

            // Kiểm tra tồn kho thực tế trước khi lưu
            if (req.Quantity > item.ProductDetail.Quantity)
                return BadRequest(new { message = "Vượt quá số lượng tồn kho" });

            item.Quantity = req.Quantity;

            // Tính lại tổng giá trị item dựa trên giá khuyến mãi mới nhất
            decimal currentUnitPrice = _priceService.CalculateBestPrice(item.ProductDetail.Product_Id);
            item.Price = currentUnitPrice * req.Quantity;

            _context.CartItems.Update(item);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, newTotalPrice = item.Price });
        }

        // ===================== 4. XÓA SẢN PHẨM (DELETE) =====================
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item == null) return NotFound();

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã xóa sản phẩm khỏi giỏ hàng" });
        }

        // ===================== LẤY TÙY CHỌN SẢN PHẨM (Cho Quick Add) =====================
        [HttpGet("GetProductOptions")]
        public async Task<IActionResult> GetProductOptions(int productId)
        {
            var productDetails = await _context.ProductDetails
                .Include(pd => pd.Size)
                .Include(pd => pd.Variant)
                .Where(pd => pd.Product_Id == productId)
                .Select(pd => new {
                    productDetail_Id = pd.ProductDetail_Id,
                    size_Id = pd.Size_Id,
                    size_Name = pd.Size!.Size_Name,
                    variants_Id = pd.Variants_Id,
                    variants_Name = pd.Variant!.Variants_Name,
                    stock = pd.Quantity
                })
                .ToListAsync();

            return Ok(new { productDetails });
        }

        // ===================== CẬP NHẬT CẤU HÌNH (Size/Màu) =====================
        [HttpPut("update-options")]
        public async Task<IActionResult> UpdateOptions([FromBody] UpdateOptionsRequest req)
        {
            var cartItem = await _context.CartItems.FindAsync(req.CartId);
            if (cartItem == null) return NotFound();

            // Tìm ProductDetail_Id mới dựa trên Size và Variant khách vừa chọn lại
            var newDetail = await _context.ProductDetails
                .FirstOrDefaultAsync(pd => pd.Product_Id == req.ProductId &&
                                         pd.Size_Id == req.SizeId &&
                                         pd.Variants_Id == req.VariantId);

            if (newDetail == null) return BadRequest(new { message = "Cấu hình này hiện không có sẵn" });

            // Cập nhật ID chi tiết mới và tính lại giá theo PriceService
            cartItem.ProductDetail_Id = newDetail.ProductDetail_Id;
            decimal unitPrice = _priceService.CalculateBestPrice(newDetail.Product_Id);
            cartItem.Price = unitPrice * cartItem.Quantity;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
        public class UpdateOptionsRequest
        {
            public int CartId { get; set; }
            public int ProductId { get; set; }
            public int SizeId { get; set; }
            public int VariantId { get; set; }
        }
    }


    // Các lớp DTO để nhận dữ liệu từ Angular
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateQtyRequest
    {
        public int CartId { get; set; }
        public int Quantity { get; set; }
    }
}