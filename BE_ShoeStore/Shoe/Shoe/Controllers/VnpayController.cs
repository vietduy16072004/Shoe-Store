using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shoe.Data;
using Shoe.Enum;
using Shoe.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using VNPAY;
using VNPAY.Models;
using VNPAY.Models.Enums;

namespace Shoe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VnpayController : ControllerBase
    {
        private readonly IVnpayClient _vnpayClient;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public VnpayController(IVnpayClient vnpayClient, IConfiguration configuration, ApplicationDbContext context)
        {
            _vnpayClient = vnpayClient;
            _configuration = configuration;
            _context = context;
        }

        // --- HÀNH ĐỘNG XỬ LÝ KẾT QUẢ TRẢ VỀ (GET) ---
        [HttpGet("PaymentCallback")]
        public async Task<IActionResult> PaymentCallback()
        {
            // Transaction vẫn cần thiết để đảm bảo cập nhật trạng thái là nguyên tử
            using var trans = await _context.Database.BeginTransactionAsync();

            // Khởi tạo các biến kết quả để truyền về trang History
            string redirectMessage = "Đơn hàng đã được xử lý.";
            bool isSuccessRedirect = false;

            try
            {
                // 1. Xử lý phản hồi và xác thực từ VNPAY (Kiểm tra Secure Hash)
                dynamic response = _vnpayClient.GetPaymentResult(Request);

                // 2. KIỂM TRA TÍNH TOÀN VẸN
                if (!response.IsSuccess)
                {
                    await trans.RollbackAsync();
                    redirectMessage = "Lỗi xác thực dữ liệu VNPAY. Giao dịch bị nghi ngờ.";
                    return RedirectToAction("History", "Order", new { success = false, message = redirectMessage });
                }

                // --- SECURE HASH ĐÃ HỢP LỆ ---
                string vnpResponseCode = response.vnp_ResponseCode;
                string vnpTxnRef = response.vnp_TxnRef;

                // 3. TÌM ĐƠN HÀNG DỰA TRÊN MÃ GIAO DỊCH
                if (string.IsNullOrEmpty(vnpTxnRef))
                {
                    await trans.RollbackAsync();
                    redirectMessage = "Không tìm thấy mã giao dịch VNPAY.";
                    return RedirectToAction("History", "Order", new { success = false, message = redirectMessage });
                }

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.VnpayTxnRef == vnpTxnRef);

                if (order == null)
                {
                    await trans.CommitAsync();
                    redirectMessage = $"Không tìm thấy đơn hàng với mã giao dịch: {vnpTxnRef}.";
                    return RedirectToAction("History", "Order", new { success = false, message = redirectMessage });
                }

                // 4. KIỂM TRA VÀ CẬP NHẬT TRẠNG THÁI (Chỉ cập nhật nếu đang Pending)
                if (order.Status != Status.Pending)
                {
                    await trans.CommitAsync();
                    redirectMessage = $"Giao dịch cho đơn hàng #{order.Bill_Id} đã được xử lý trước đó.";
                    return RedirectToAction("History", "Order", new { success = true, message = redirectMessage });
                }

                // 5. CẬP NHẬT TRẠNG THÁI (Phương án A)
                if (vnpResponseCode == "00")
                {
                    order.Status = Status.InProgress;
                    isSuccessRedirect = true;
                    redirectMessage = $"Thanh toán VNPAY thành công! Đơn hàng #{order.Bill_Id} đang được xử lý.";
                }
                else if (vnpResponseCode == "24")
                {
                    order.Status = Status.Canceled;
                    redirectMessage = $"Đơn hàng #{order.Bill_Id} đã bị hủy thanh toán bởi người dùng.";
                }
                else
                {
                    order.Status = Status.Failed;
                    redirectMessage = $"Thanh toán cho đơn hàng #{order.Bill_Id} thất bại. Mã lỗi VNPAY: {vnpResponseCode}.";
                }

                // 6. LƯU VÀ HOÀN TẤT
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                await trans.CommitAsync();

                return RedirectToAction("History", "Order", new { success = isSuccessRedirect, message = redirectMessage });
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                redirectMessage = "Lỗi xử lý hệ thống khi nhận kết quả thanh toán: " + ex.Message;
                return RedirectToAction("History", "Order", new { success = false, message = redirectMessage });
            }
        }
    }
}