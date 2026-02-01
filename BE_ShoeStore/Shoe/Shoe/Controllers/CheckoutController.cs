using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Models;
using Shoe.Enum;
using VNPAY;
using VNPAY.Models;
using VNPAY.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace Shoe.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IVnpayClient _vnpayClient;

        public CheckoutController(ApplicationDbContext context, IVnpayClient vnpayClient)
        {
            _context = context;
            _vnpayClient = vnpayClient;
        }

        // ========== INDEX (GET) ==========
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();

            var cartItems = await _context.CartItems
                .Include(c => c.ProductDetail).ThenInclude(pd => pd.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Gio hang trong!";
                return RedirectToAction("Index", "Cart");
            }

            ViewData["User"] = user;
            ViewData["CartItems"] = cartItems;

            // Tính tổng tiền hàng (chưa ship) để hiển thị tạm tính
            var totalPrice = cartItems.Sum(ci => ci.Price * ci.Quantity);
            ViewData["TotalPrice"] = totalPrice;

            return View();
        }

        // ========== INDEX (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Order order, string paymentMethod)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return RedirectToAction("Login", "Account");

            // 1. Lấy giỏ hàng
            var cartItems = await _context.CartItems
                .Include(c => c.ProductDetail).ThenInclude(pd => pd.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Khong co san pham trong gio hang!";
                return RedirectToAction("Index", "Cart");
            }

            // Tính tổng tiền HÀNG
            var productTotal = cartItems.Sum(ci => ci.Price * ci.Quantity);

            var affectedProductIds = new List<int>();

            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Kiểm tra và Trừ Tồn Kho (Giữ nguyên logic của bạn)
                foreach (var item in cartItems)
                {
                    if (item.ProductDetail.Quantity < item.Quantity)
                    {
                        await trans.RollbackAsync();
                        TempData["Error"] = $"San pham '{item.ProductDetail.Product.Product_Name}' khong du so luong.";
                        return RedirectToAction("Index", "Cart");
                    }
                    item.ProductDetail.Quantity -= item.Quantity;
                    _context.ProductDetails.Update(item.ProductDetail);

                    if (!affectedProductIds.Contains(item.ProductDetail.Product_Id))
                    {
                        affectedProductIds.Add(item.ProductDetail.Product_Id);
                    }
                }

                // 3. TẠO ĐƠN HÀNG
                order.UserId = userId;
                order.OrderDate = DateTime.Now;
                order.Status = Status.InProgress;

                // === [THAY ĐỔI QUAN TRỌNG] ===
                // order.ShippingFee đã được nhận từ View (do JS tính và gán vào input hidden)
                // Tổng tiền = Tiền hàng + Phí Ship
                order.Totalprice = productTotal + order.ShippingFee;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                foreach (var pid in affectedProductIds)
                {
                    await CheckAndUpdateProductStatus(pid);
                }
                // 4. LƯU ORDER DETAILS
                foreach (var item in cartItems)
                {
                    var detail = new OrderDetail
                    {
                        Bill_Id = order.Bill_Id,
                        ProductDetail_Id = item.ProductDetail_Id,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };
                    _context.OrderDetails.Add(detail);
                }

                // 5. XÓA GIỎ HÀNG
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                foreach (var pid in affectedProductIds)
                {
                    await CheckAndUpdateProductStatus(pid);
                }

                // 6. XỬ LÝ THANH TOÁN VNPAY
                if (paymentMethod == "VNPAY")
                {
                    // === [SỬA LẠI] ===
                    // Thanh toán số tiền cuối cùng (đã bao gồm Ship)
                    double amountForVnpay = (double)order.Totalprice;

                    var paymentRequest = new VnpayPaymentRequest
                    {
                        Description = $"Thanh toan don hang {order.Bill_Id}",
                        Money = amountForVnpay,
                        Language = DisplayLanguage.Vietnamese
                    };
                    PaymentUrlDetail urlDetail = _vnpayClient.CreatePaymentUrl(paymentRequest);

                    order.VnpayTxnRef = paymentRequest.PaymentId.ToString();
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();

                    await trans.CommitAsync();
                    return Redirect(urlDetail.Url);
                }

                // 7. XỬ LÝ COD
                await trans.CommitAsync();
                TempData["Success"] = $"Dat hang thanh cong! Don hang #{order.Bill_Id}. Phi ship: {order.ShippingFee:N0}đ";
                return RedirectToAction("History", "Order");
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                TempData["Error"] = "Loi khi dat hang: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var guid))
                return guid;
            if (HttpContext.Session.TryGetValue("UserId", out var bytes) && bytes.Length == 16)
                return new Guid(bytes);
            return Guid.Empty;
        }

        // --- Hàm kiểm tra trạng thái sản phẩm (Helper) ---
        private async Task CheckAndUpdateProductStatus(int productId)
        {
            // 1. Lấy tất cả chi tiết của sản phẩm đó trong DB (đã bao gồm số lượng vừa bị trừ)
            var details = await _context.ProductDetails
                .Where(pd => pd.Product_Id == productId)
                .AsNoTracking()
                .ToListAsync();

            // 2. Lấy sản phẩm cha
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            // 3. Tính toán: Có bất kỳ size/màu nào còn hàng (>0) không?
            bool hasStock = details.Any(pd => pd.Quantity > 0);

            // 4. Logic cập nhật
            // Nếu hết sạch hàng (hasStock = false) VÀ đang Selling -> Chuyển OutOfStock
            if (!hasStock && product.Status == StatusProduct.Selling)
            {
                product.Status = StatusProduct.OutOfStock;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            // Lưu ý: Ở trang Checkout (Mua hàng) thì chỉ có trường hợp từ Selling -> OutOfStock
            // Không bao giờ có trường hợp mua hàng mà lại biến thành còn hàng được, nên không cần check chiều ngược lại.
        }
    }
}