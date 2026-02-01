using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Enum;
using Shoe.Models;
using Shoe.Services;

namespace Shoe.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GhnService _ghnService; // Service

        public OrderController(ApplicationDbContext context, GhnService ghnService)
        {
            _context = context;
            _ghnService = ghnService;
        }

        // ========== INDEX (ADMIN) ==========
        public async Task<IActionResult> Index(string searchString, DateTime? fromDate, DateTime? toDate)
        {
            // 1. Khởi tạo truy vấn
            var query = _context.Orders
                .Include(o => o.AppUser)
                .AsQueryable();
            // 2. Lọc theo tên người dùng (Username hoặc Email)
            if (!string.IsNullOrEmpty(searchString))
            {
                // Chuyển về chữ thường để tìm kiếm không phân biệt hoa thường
                string search = searchString.ToLower();
                query = query.Where(o => o.AppUser.Username.ToLower().Contains(search)
                                      || (o.AppUser.Email != null && o.AppUser.Email.ToLower().Contains(search)));
            }

            // 3. Lọc theo ngày bắt đầu (From Date)
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            // 4. Lọc theo ngày kết thúc
            if (toDate.HasValue)
            {
                // Lấy đến cuối ngày của ngày được chọn (23:59:59)
                var endDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.OrderDate <= endDate);
            }

            // 5. Sắp xếp và lấy dữ liệu
            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            // 6. Lưu lại giá trị tìm kiếm để hiển thị lại trên View
            ViewData["CurrentFilter"] = searchString;
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

            return View(orders);
        }

        // ========== DETAIL ==========
        [Authorize]
        public async Task<IActionResult> Detail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.AppUser)
                .Include(o => o.OrderDetails)!
                    .ThenInclude(od => od.ProductDetails)!
                        .ThenInclude(pd => pd.Product)
                .Include(o => o.OrderDetails)!
                    .ThenInclude(od => od.ProductDetails)!
                        .ThenInclude(pd => pd.Size)
                .Include(o => o.OrderDetails)!
                    .ThenInclude(od => od.ProductDetails)!
                        .ThenInclude(pd => pd.Variant)
                .FirstOrDefaultAsync(o => o.Bill_Id == id);

            if (order == null) return NotFound();

            // Kiểm tra xem đơn hàng này đã có vận đơn chưa
            var shipment = await _context.Shipments
                .FirstOrDefaultAsync(s => s.Bill_Id == id);

            // Truyền ShipmentId sang View (nếu có)
            ViewData["ShipmentId"] = shipment?.ShipmentId;

            return View(order);
        }

        // ========== TẠO VẬN ĐƠN ==========
        [HttpPost]
        public async Task<IActionResult> PushToGHN(long id)
        {
            // 1. Lấy đơn hàng
            var order = await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.ProductDetails).ThenInclude(pd => pd.Product)
                .FirstOrDefaultAsync(o => o.Bill_Id == id);

            if (order == null) return NotFound();

            // 2. Gọi API GHN (Dùng hàm Create thật, không dùng Preview để lấy TrackingCode)
            string resultJson = await _ghnService.CreateShippingOrder(order, order.OrderDetails.ToList());

            using var doc = System.Text.Json.JsonDocument.Parse(resultJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("code", out var codeProp) && codeProp.GetInt32() == 200)
            {
                // 3. Lấy dữ liệu từ GHN trả về
                var data = root.GetProperty("data");
                string orderCode = data.GetProperty("order_code").GetString(); // Mã vận đơn (VD: L8CCMB)
                string expectedTimeStr = data.TryGetProperty("expected_delivery_time", out var timeProp) ? timeProp.GetString() : null;
                DateTime? expectedTime = !string.IsNullOrEmpty(expectedTimeStr) ? DateTime.Parse(expectedTimeStr) : DateTime.Now.AddDays(3);
                decimal totalFee = data.GetProperty("total_fee").GetDecimal(); // Phí thực tế GHN thu của Shop

                // 4. --- LƯU VÀO DATABASE (PHẦN MỚI) ---

                // A. Tìm ID của nhà vận chuyển GHN (Đã tạo ở Bước 1)
                var carrier = await _context.Carriers.FirstOrDefaultAsync(c => c.CarrierName == "Giao Hàng Nhanh");
                int carrierId = carrier != null ? carrier.CarrierId : 1; // Fallback nếu chưa tạo

                // B. Tạo Shipment (Vận đơn)
                var shipment = new Shipment
                {
                    Bill_Id = order.Bill_Id,
                    CarrierId = carrierId,
                    TrackingCode = orderCode, // Lưu mã vận đơn GHN
                    ExpectedDeliveryTime = expectedTime,
                    ActualShippingFee = totalFee // Phí thực tế Shop phải trả cho GHN
                };
                _context.Shipments.Add(shipment);
                await _context.SaveChangesAsync(); // Lưu để lấy ShipmentId

                // C. Ghi Log Lịch sử (HistoryShipping)
                var history = new HistoryShipping
                {
                    ShipmentId = shipment.ShipmentId,
                    Status = "ReadyToPick", // Trạng thái ban đầu của GHN
                    Description = "Đã tạo đơn hàng thành công trên hệ thống GHN",
                    Location = "Kho Shop",
                    Timestamp = DateTime.Now
                };
                _context.HistoryShippings.Add(history);

                // D. Cập nhật trạng thái đơn hàng
                order.Status = Status.Shipping;

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Tạo vận đơn thành công! Mã: {orderCode}";
            }
            else
            {
                string msg = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Lỗi không xác định";
                if (root.TryGetProperty("code_message_value", out var detailProp)) msg += $" ({detailProp.GetString()})";
                TempData["Error"] = $"Lỗi GHN: {msg}";
            }

            return RedirectToAction(nameof(Index));
        }

        // ========== UPDATE STATUS  ==========
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(long id, Status newStatus)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductDetails)
                .FirstOrDefaultAsync(o => o.Bill_Id == id);

            // Kiểm tra quy tắc chuyển trạng thái
            bool isAllowed = false;

            // Logic: InProgress -> Confirmed hoặc Canceled
            if (order.Status == Status.InProgress && (newStatus == Status.Confirmed || newStatus == Status.Canceled))
            {
                isAllowed = true;
            }
            // Logic: Confirmed -> Shipping
            else if (order.Status == Status.Confirmed && newStatus == Status.Shipping)
            {
                isAllowed = true;
            }
            // Logic: Shipping -> Success
            else if (order.Status == Status.Shipping && newStatus == Status.Success)
            {
                isAllowed = true;
            }

            // Nếu không thỏa mãn quy tắc trên thì báo lỗi
            if (!isAllowed)
            {
                TempData["Error"] = $"Lỗi: Không thể chuyển từ trạng thái {order.Status} sang {newStatus}!";
                return RedirectToAction(nameof(Index));
            }

            if (order == null)
            {
                TempData["Error"] = "Khong tim thay don hang!";
                return RedirectToAction(nameof(Index));
            }

            bool isValid = true;

            if (isValid)
            {
                order.Status = newStatus;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cap nhat trang thai thanh cong!";
            }

            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> History(DateTime? fromDate, DateTime? toDate)
        {
            var userId = GetCurrentUserId();

            // 1. Khởi tạo truy vấn cho User hiện tại
            var query = _context.Orders
                .Where(o => o.UserId == userId)
                .AsQueryable();

            // 2. Lọc theo ngày bắt đầu
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            // 3. Lọc theo ngày kết thúc
            if (toDate.HasValue)
            {
                var endDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.OrderDate <= endDate);
            }

            // 4. Lấy dữ liệu
            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            // 5. Lưu lại giá trị để hiển thị lại trên View
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetCurrentUserId();
            var order = await _context.Orders
                .Include(o => o.OrderDetails) // Include để lấy chi tiết
                    .ThenInclude(od => od.ProductDetails)
                .FirstOrDefaultAsync(o => o.Bill_Id == id && o.UserId == userId);

            if (order == null) return NotFound();

            if (order.Status is not (Status.InProgress or Status.Confirmed or Status.Shipping))
            {
                TempData["Error"] = "Khong the huy don hang o trang thai hien tai.";
                return RedirectToAction(nameof(History));
            }

            // --- CỘNG LẠI SỐ LƯỢNG KHO ---
            var affectedProductIds = new List<int>();
            if (order.OrderDetails != null)
            {
                foreach (var item in order.OrderDetails)
                {
                    if (item.ProductDetails != null)
                    {
                        // Hoàn số lượng về kho
                        item.ProductDetails.Quantity += item.Quantity;
                        _context.ProductDetails.Update(item.ProductDetails);

                        // Lưu lại ID sản phẩm để tí nữa kiểm tra trạng thái
                        if (!affectedProductIds.Contains(item.ProductDetails.Product_Id))
                        {
                            affectedProductIds.Add(item.ProductDetails.Product_Id);
                        }
                    }
                }
            }

            order.Status = Status.Canceled;
            await _context.SaveChangesAsync();

            foreach (var productId in affectedProductIds)
            {
                await CheckAndUpdateProductStatus(productId);
            }

            TempData["Success"] = "Don hang da duoc huy va hoan so luong ve ton kho.";
            return RedirectToAction(nameof(History));
        }

        // --- Hàm kiểm tra trạng thái sản phẩm ---
        private async Task CheckAndUpdateProductStatus(int productId)
        {
            // 1. Lấy tất cả chi tiết của sản phẩm đó (Số lượng thực tế trong DB)
            var details = await _context.ProductDetails
                .Where(pd => pd.Product_Id == productId)
                .AsNoTracking()
                .ToListAsync();

            // 2. Lấy sản phẩm cha
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            // 3. Tính toán: Có bất kỳ size/màu nào còn hàng không?
            bool hasStock = details.Any(pd => pd.Quantity > 0);

            bool isChanged = false;

            // Case A: Hết sạch hàng nhưng vẫn đang Selling -> Chuyển OutOfStock
            if (!hasStock && product.Status == StatusProduct.Selling)
            {
                product.Status = StatusProduct.OutOfStock;
                isChanged = true;
            }
            // Case B: Có hàng trở lại (do hủy đơn) nhưng đang OutOfStock -> Chuyển Selling
            else if (hasStock && product.Status == StatusProduct.OutOfStock)
            {
                product.Status = StatusProduct.Selling;
                isChanged = true;
            }

            if (isChanged)
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
        }

    }
}