using Microsoft.AspNetCore.Http; // Thêm namespace này để dùng Session
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
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PriceService _priceService;

        public CartController(ApplicationDbContext context, PriceService priceService)
        {
            _context = context;
            _priceService = priceService;
        }

        // ===================== INDEX =====================
        public async Task<IActionResult> Index()
        {
            var idStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(idStr))
                return RedirectToAction("Login", "Account");

            var userId = Guid.Parse(idStr);

            var cartItems = await _context.CartItems
                .Include(c => c.ProductDetail)!.ThenInclude(pd => pd.Product)
                .Include(c => c.ProductDetail)!.ThenInclude(pd => pd.Size)
                .Include(c => c.ProductDetail)!.ThenInclude(pd => pd.Variant)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            // Cập nhật lại giá cho tất cả sản phẩm theo khuyến mãi mới nhất từ Admin
            bool isPriceChanged = false;
            foreach (var item in cartItems)
            {
                if (item.ProductDetail != null)
                {
                    // Gọi PriceService để lấy đơn giá tốt nhất tại THỜI ĐIỂM HIỆN TẠI
                    decimal currentBestUnitPrice = _priceService.CalculateBestPrice(item.ProductDetail.Product_Id);

                    // Tính lại tổng giá cho số lượng đang có trong giỏ
                    decimal newTotalPrice = currentBestUnitPrice * item.Quantity;

                    // Nếu giá có sự thay đổi (Admin vừa sửa khuyến mãi), cập nhật lại vào Database
                    if (item.Price != newTotalPrice)
                    {
                        item.Price = newTotalPrice;
                        _context.CartItems.Update(item);
                        isPriceChanged = true;
                    }
                }
            }

            if (isPriceChanged)
            {
                await _context.SaveChangesAsync();
            }

            return View(cartItems);
        }

        // ===================== GET CART (AJAX) =====================
        [HttpGet]
        public async Task<IActionResult> GetCart(Guid userId)
        {
            var cartItems = await _context.CartItems
                .Include(c => c.ProductDetail)!.ThenInclude(pd => pd.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return Json(cartItems.Select(c => new
            {
                c.Cart_Id,
                ProductName = c.ProductDetail?.Product?.Product_Name,
                c.Quantity,
                c.Price
            }));
        }

        // ===================== GET PRODUCT OPTIONS (AJAX) =====================
        [HttpGet]
        public async Task<IActionResult> GetProductOptions(int productId)
        {
            var details = await _context.ProductDetails
                .Include(pd => pd.Size)
                .Include(pd => pd.Variant)
                .Where(pd => pd.Product_Id == productId)
                .Select(pd => new
                {
                    pd.ProductDetail_Id,
                    Size_Id = pd.Size != null ? (int?)pd.Size.Size_Id : null,
                    Size_Name = pd.Size != null ? pd.Size.Size_Name : null,
                    // [SỬA] Lấy Quantity trực tiếp từ ProductDetail
                    Stock = pd.Quantity,
                    Variants_Id = pd.Variant != null ? (int?)pd.Variant.Variants_Id : null,
                    Variants_Name = pd.Variant != null ? pd.Variant.Variants_Name : null
                })
                .ToListAsync();

            return Json(new { productDetails = details });
        }

        // ===================== ADD TO CART WITH OPTIONS (AJAX POST) =====================
        [HttpPost]
        public async Task<IActionResult> AddToCartWithOptions(int productId, int? selectedSizeId, int? selectedVariantId, int quantity)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return Json(new { success = false, message = "Bạn cần đăng nhập." });

            var userId = Guid.Parse(userIdStr);

            var productDetail = await _context.ProductDetails
                .Include(pd => pd.Product) // Cần Product để lấy ID tính giá
                .FirstOrDefaultAsync(pd =>
                    pd.Product_Id == productId &&
                    (selectedSizeId == null || pd.Size_Id == selectedSizeId) &&
                    (selectedVariantId == null || pd.Variants_Id == selectedVariantId)
                );

            if (productDetail == null)
                return Json(new { success = false, message = "Sản phẩm không hợp lệ." });

            if (quantity > productDetail.Quantity)
                return Json(new { success = false, message = $"Chỉ còn {productDetail.Quantity} sản phẩm." });

            // [QUAN TRỌNG] TÍNH GIÁ BÁN THỰC TẾ TẠI THỜI ĐIỂM MUA
            // Không dùng Product.Price hay Discount tĩnh nữa
            decimal unitPriceBest = _priceService.CalculateBestPrice(productDetail.Product_Id);

            var existing = await _context.CartItems
                .FirstOrDefaultAsync(c => c.ProductDetail_Id == productDetail.ProductDetail_Id && c.UserId == userId);

            if (existing != null)
            {
                if (existing.Quantity + quantity > productDetail.Quantity)
                    return Json(new { success = false, message = "Vượt quá tồn kho." });

                existing.Quantity += quantity;
                // Cập nhật lại giá cho toàn bộ số lượng (Giá mới nhất áp dụng cho cả cũ)
                existing.Price = unitPriceBest * existing.Quantity;
                _context.CartItems.Update(existing);
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    ProductDetail_Id = productDetail.ProductDetail_Id,
                    Quantity = quantity,
                    Price = unitPriceBest * quantity, // Đơn giá tốt nhất * Số lượng
                    UserId = userId,
                    DateCreated = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Đã thêm <b>{productDetail.Product.Product_Name}</b> vào giỏ!"
            });
        }

        // ===================== EDIT (GET) =====================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.CartItems
                .Include(c => c.ProductDetail)!.ThenInclude(pd => pd.Product)
                .Include(c => c.ProductDetail)!.ThenInclude(pd => pd.Size)
                .Include(c => c.ProductDetail)!.ThenInclude(pd => pd.Variant)
                .FirstOrDefaultAsync(c => c.Cart_Id == id);

            if (item == null) return NotFound();

            var productId = item.ProductDetail!.Product_Id;

            ViewBag.Sizes = await _context.ProductDetails
                .Include(pd => pd.Size)
                .Where(pd => pd.Product_Id == productId && pd.Size != null)
                .Select(pd => pd.Size!)
                .Distinct()
                .OrderBy(s => s.Size_Name)
                .ToListAsync();

            ViewBag.Variants = await _context.ProductDetails
                .Include(pd => pd.Variant)
                .Where(pd => pd.Product_Id == productId && pd.Variant != null)
                .Select(pd => pd.Variant!)
                .Distinct()
                .ToListAsync();

            ViewBag.ProductDetails = await _context.ProductDetails
                .Where(pd => pd.Product_Id == productId)
                .Select(pd => new
                {
                    pd.Size_Id,
                    pd.Variants_Id,
                    // [THÊM] Có thể truyền thêm stock xuống view nếu cần xử lý JS
                    Stock = pd.Quantity
                })
                .ToListAsync();

            return View(item);
        }

        // ===================== EDIT (POST) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int selectedSizeId, int selectedVariantId, int quantity)
        {
            var item = await _context.CartItems
                .Include(c => c.ProductDetail)!.ThenInclude(pd => pd.Product)
                .FirstOrDefaultAsync(c => c.Cart_Id == id);

            if (item == null) return NotFound();

            var productId = item.ProductDetail!.Product_Id;
            var newDetail = await _context.ProductDetails
                .FirstOrDefaultAsync(pd =>
                    pd.Product_Id == productId &&
                    pd.Size_Id == selectedSizeId &&
                    pd.Variants_Id == selectedVariantId);

            if (newDetail == null || quantity > newDetail.Quantity)
            {
                TempData["Error"] = "San pham khong du so luong.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            // [QUAN TRỌNG] Tính lại giá khi cập nhật giỏ
            decimal unitPriceBest = _priceService.CalculateBestPrice(productId);

            item.ProductDetail_Id = newDetail.ProductDetail_Id;
            item.Quantity = quantity;
            item.Price = unitPriceBest * quantity; // Cập nhật giá mới

            _context.CartItems.Update(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cap nhat gio hang thanh cong!";
            return RedirectToAction(nameof(Edit), new { id = item.Cart_Id });
        }

        // ===================== DELETE =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item == null) return NotFound();

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xoa san pham khoi gio hang thanh cong!";
            return RedirectToAction("Index", "Cart");
        }

        // ===================== SAVE CART =====================
        [HttpPost]
        public async Task<IActionResult> SaveCart(Guid userId, List<CartItem> cartItems)
        {
            var existing = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            _context.CartItems.RemoveRange(existing);

            foreach (var item in cartItems)
            {
                item.UserId = userId;
                item.DateCreated = DateTime.Now;
                _context.CartItems.Add(item);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Lưu giỏ hàng thành công." });
        }
    }
}