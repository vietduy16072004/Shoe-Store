using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shoe.Controllers;
using Shoe.Data;
using Shoe.Models;
using Shoe.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shoe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountEventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public DiscountEventsController(ApplicationDbContext context) => _context = context;

        // GET: api/DiscountEvents
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.DiscountEvents
                .Include(e => e.EventDetails)
                .OrderByDescending(e => e.StartDate)
                .Select(e => new EventDetailListViewModel
                {
                    Id = e.Id,
                    EventName = e.EventName,
                    Description = e.Description,
                    DiscountDisplay = e.DiscountType == 1 ? $"{e.DiscountValue}%" : $"{e.DiscountValue:N0}đ",
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsActive = e.IsActive,
                    ProductCount = e.EventDetails.Count(),
                    StatusLabel = (e.IsActive && e.StartDate <= DateTime.Now && e.EndDate >= DateTime.Now)
                                   ? "Đang diễn ra"
                                   : (!e.IsActive ? "Đã tắt" : (e.EndDate < DateTime.Now ? "Đã kết thúc" : "Sắp diễn ra"))
                }).ToListAsync();
            return Ok(events);
        }

        // GET: api/DiscountEvents/products-lookup (Dùng cho dropdown chọn sản phẩm)
        [HttpGet("products-lookup")]
        public async Task<IActionResult> GetProductsLookup()
        {
            var products = await _context.Products
                .Select(p => new { p.Product_Id, p.Product_Name, p.Price })
                .ToListAsync();
            return Ok(products);
        }

        // POST: api/DiscountEvents
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DiscountEventViewModel vm)
        {
            if (vm.DiscountType == 1 && vm.DiscountValue > 100)
                return BadRequest(new { message = "Phần trăm giảm giá tối đa là 100%" });

            var discountEvent = new DiscountEvent
            {
                EventName = vm.EventName,
                Description = vm.Description,
                DiscountValue = vm.DiscountValue,
                DiscountType = vm.DiscountType,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                IsActive = vm.IsActive
            };

            _context.DiscountEvents.Add(discountEvent);
            await _context.SaveChangesAsync();

            if (vm.SelectedProductIds?.Any() == true)
            {
                var details = vm.SelectedProductIds.Select(prodId => new EventDetail
                {
                    EventId = discountEvent.Id,
                    ProductID = prodId,
                    Status = true
                });
                _context.EventDetails.AddRange(details);
                await _context.SaveChangesAsync();
            }
            return Ok(new { success = true, message = "Tạo sự kiện thành công!" });
        }

        // Thêm API Update cho Angular
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] DiscountEventViewModel vm)
        {
            var discountEvent = await _context.DiscountEvents.Include(e => e.EventDetails).FirstOrDefaultAsync(e => e.Id == id);
            if (discountEvent == null) return NotFound(new { message = "Không tìm thấy sự kiện!" });

            // 1. Cập nhật thông tin cơ bản
            discountEvent.EventName = vm.EventName;
            discountEvent.Description = vm.Description;
            discountEvent.DiscountValue = vm.DiscountValue;
            discountEvent.DiscountType = vm.DiscountType;
            discountEvent.StartDate = vm.StartDate;
            discountEvent.EndDate = vm.EndDate;
            discountEvent.IsActive = vm.IsActive;

            // 2. Cập nhật danh sách sản phẩm (Xóa cũ - Thêm mới)
            _context.EventDetails.RemoveRange(discountEvent.EventDetails);
            if (vm.SelectedProductIds?.Any() == true)
            {
                var newDetails = vm.SelectedProductIds.Select(prodId => new EventDetail
                {
                    EventId = id,
                    ProductID = prodId,
                    Status = true
                });
                _context.EventDetails.AddRange(newDetails);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Cập nhật sự kiện thành công!" });
        }

        // DELETE: api/DiscountEvents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var discountEvent = await _context.DiscountEvents.FindAsync(id);
            if (discountEvent == null) return NotFound();

            _context.DiscountEvents.Remove(discountEvent);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Xóa sự kiện thành công!" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventDetail(int id)
        {
            var discountEvent = await _context.DiscountEvents
                .Include(e => e.EventDetails)
                    .ThenInclude(ed => ed.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (discountEvent == null) return NotFound(new { message = "Không tìm thấy sự kiện!" });

            // Map dữ liệu sang ViewModel tương tự cấu trúc bạn đã gửi
            var vm = new DiscountEventDetailViewModel
            {
                Id = discountEvent.Id,
                EventName = discountEvent.EventName,
                Description = discountEvent.Description,
                DiscountDisplay = discountEvent.DiscountType == 1
                                  ? $"{discountEvent.DiscountValue}%"
                                  : $"{discountEvent.DiscountValue:N0}đ",
                StartDate = discountEvent.StartDate,
                EndDate = discountEvent.EndDate,
                IsActive = discountEvent.IsActive,
                StatusLabel = (discountEvent.IsActive && discountEvent.StartDate <= DateTime.Now && discountEvent.EndDate >= DateTime.Now)
                               ? "Đang diễn ra"
                               : (!discountEvent.IsActive ? "Đã tắt" : (discountEvent.EndDate < DateTime.Now ? "Đã kết thúc" : "Sắp diễn ra")),

                Products = discountEvent.EventDetails.Select(ed => new ProductInEventViewModel
                {
                    ProductId = ed.Product.Product_Id,
                    ProductName = ed.Product.Product_Name,
                    OriginalPrice = ed.Product.Price,
                    ImageUrl = ed.Product.ImageUrl,
                    CategoryName = ed.Product.Category?.Category_Name
                }).ToList()
            };

            return Ok(vm);
        }
    }
}