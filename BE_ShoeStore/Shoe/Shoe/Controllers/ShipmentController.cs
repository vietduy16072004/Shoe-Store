using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Models;

namespace Shoe.Controllers
{
    public class ShipmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShipmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách tất cả vận đơn
        public async Task<IActionResult> Index()
        {
            var shipments = await _context.Shipments
                .Include(s => s.Order).ThenInclude(o => o.AppUser) // Lấy thông tin người mua
                .Include(s => s.Carrier) // Lấy tên nhà vận chuyển (GHN)
                .OrderByDescending(s => s.ShipmentId)
                .ToListAsync();

            return View(shipments);
        }

        // 2. Xem chi tiết hành trình (Tracking)
        [Authorize]
        public async Task<IActionResult> Detail(long id)
        {
            var shipment = await _context.Shipments
                .Include(s => s.Order)
                .Include(s => s.Carrier)
                .Include(s => s.ShippingLogs) // Lấy lịch sử log
                .FirstOrDefaultAsync(s => s.ShipmentId == id);

            if (shipment == null) return NotFound();

            return View(shipment);
        }
    }
}