using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Models;
using Shoe.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shoe.Controllers
{
    public class DiscountEventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscountEventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================== INDEX ==================
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách sự kiện và đếm số lượng sản phẩm trong mỗi sự kiện
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
                    ProductCount = e.EventDetails.Count(), // Đếm số sản phẩm tham gia
                    StatusLabel = (e.IsActive && e.StartDate <= DateTime.Now && e.EndDate >= DateTime.Now)
                                   ? "Đang diễn ra"
                                   : (!e.IsActive ? "Đã tắt" : (e.EndDate < DateTime.Now ? "Đã kết thúc" : "Sắp diễn ra"))
                })
                .ToListAsync();

            return View(events);
        }

        // ================== CREATE (GET) ==================
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new DiscountEventViewModel();

            // Load danh sách sản phẩm để hiển thị Dropdown chọn nhiều (MultiSelect)
            // Chỉ lấy Tên và ID để nhẹ dữ liệu
            var products = _context.Products
                .Select(p => new { p.Product_Id, DisplayName = $"{p.Product_Name} - {p.Price:N0}đ" })
                .ToList();

            vm.ProductList = new MultiSelectList(products, "Product_Id", "DisplayName");

            return View(vm);
        }

        // ================== CREATE (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiscountEventViewModel vm)
        {
            // --- [THÊM MỚI] VALIDATION LOGIC NGHIỆP VỤ ---
            // Nếu chọn Loại 1 (Phần trăm) mà nhập > 100 thì báo lỗi ngay
            if (vm.DiscountType == 1 && vm.DiscountValue > 100)
            {
                ModelState.AddModelError("DiscountValue", "Loại giảm giá là Phần trăm (%) thì giá trị tối đa là 100.");
            }

            // Kiểm tra số âm (đề phòng)
            if (vm.DiscountValue < 0)
            {
                ModelState.AddModelError("DiscountValue", "Giá trị giảm không được là số âm.");
            }
            // ----------------------------------------------

            if (ModelState.IsValid)
            {
                // 1. Lưu thông tin sự kiện
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

                // 2. Lưu danh sách sản phẩm
                if (vm.SelectedProductIds != null && vm.SelectedProductIds.Any())
                {
                    var details = new List<EventDetail>();
                    foreach (var prodId in vm.SelectedProductIds)
                    {
                        details.Add(new EventDetail
                        {
                            EventId = discountEvent.Id,
                            ProductID = prodId,
                            Status = true
                        });
                    }
                    _context.EventDetails.AddRange(details);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Tao chuong trinh giam gia thanh cong!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, load lại danh sách sản phẩm
            var products = _context.Products
                .Select(p => new { p.Product_Id, DisplayName = $"{p.Product_Name} - {p.Price:N0}đ" })
                .ToList();
            vm.ProductList = new MultiSelectList(products, "Product_Id", "DisplayName", vm.SelectedProductIds);

            return View(vm);
        }

        // ================== EDIT (GET) ==================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var discountEvent = await _context.DiscountEvents
                .Include(e => e.EventDetails) // Load kèm danh sách sản phẩm đã chọn
                .FirstOrDefaultAsync(e => e.Id == id);

            if (discountEvent == null) return NotFound();

            var vm = new DiscountEventViewModel
            {
                Id = discountEvent.Id,
                EventName = discountEvent.EventName,
                Description = discountEvent.Description,
                DiscountValue = discountEvent.DiscountValue,
                DiscountType = discountEvent.DiscountType,
                StartDate = discountEvent.StartDate,
                EndDate = discountEvent.EndDate,
                IsActive = discountEvent.IsActive,

                // Lấy danh sách ID sản phẩm ĐANG tham gia sự kiện này
                SelectedProductIds = discountEvent.EventDetails.Select(ed => ed.ProductID).ToList()
            };

            // Load lại danh sách tất cả sản phẩm
            var products = _context.Products
                .Select(p => new { p.Product_Id, DisplayName = $"{p.Product_Name} - {p.Price:N0}đ" })
                .ToList();

            // Truyền SelectedProductIds vào MultiSelectList để nó tự động "Tick" chọn những cái cũ
            vm.ProductList = new MultiSelectList(products, "Product_Id", "DisplayName", vm.SelectedProductIds);

            return View(vm);
        }

        // ================== EDIT (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DiscountEventViewModel vm)
        {
            // --- [THÊM MỚI] VALIDATION LOGIC NGHIỆP VỤ ---
            if (vm.DiscountType == 1 && vm.DiscountValue > 100)
            {
                ModelState.AddModelError("DiscountValue", "Loại giảm giá là Phần trăm (%) thì giá trị tối đa là 100.");
            }

            if (vm.DiscountValue < 0)
            {
                ModelState.AddModelError("DiscountValue", "Giá trị giảm không được là số âm.");
            }
            // ----------------------------------------------

            if (ModelState.IsValid)
            {
                var discountEvent = await _context.DiscountEvents.FindAsync(vm.Id);
                if (discountEvent == null) return NotFound();

                // 1. Cập nhật thông tin sự kiện
                discountEvent.EventName = vm.EventName;
                discountEvent.Description = vm.Description;
                discountEvent.DiscountValue = vm.DiscountValue;
                discountEvent.DiscountType = vm.DiscountType;
                discountEvent.StartDate = vm.StartDate;
                discountEvent.EndDate = vm.EndDate;
                discountEvent.IsActive = vm.IsActive;

                _context.DiscountEvents.Update(discountEvent);

                // 2. Cập nhật danh sách sản phẩm (Xóa cũ -> Thêm mới)
                var oldDetails = _context.EventDetails.Where(ed => ed.EventId == vm.Id);
                _context.EventDetails.RemoveRange(oldDetails);

                if (vm.SelectedProductIds != null && vm.SelectedProductIds.Any())
                {
                    var newDetails = new List<EventDetail>();
                    foreach (var prodId in vm.SelectedProductIds)
                    {
                        newDetails.Add(new EventDetail
                        {
                            EventId = vm.Id,
                            ProductID = prodId,
                            Status = true
                        });
                    }
                    _context.EventDetails.AddRange(newDetails);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cap nhat chuong trinh su kien thanh cong!";
                return RedirectToAction(nameof(Index));
            }

            // Reload nếu lỗi
            var products = _context.Products
               .Select(p => new { p.Product_Id, DisplayName = $"{p.Product_Name} - {p.Price:N0}đ" })
               .ToList();
            vm.ProductList = new MultiSelectList(products, "Product_Id", "DisplayName", vm.SelectedProductIds);

            return View(vm);
        }

        // ================== DELETE ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var discountEvent = await _context.DiscountEvents.FindAsync(id);
            if (discountEvent == null) return NotFound();

            // Cascade delete đã được cấu hình trong DbContext nên chỉ cần xóa Event là Detail tự bay màu
            _context.DiscountEvents.Remove(discountEvent);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xoa su kien giam gia thanh cong!";
            return RedirectToAction(nameof(Index));
        }


        // ================== DETAILS ==================
        public async Task<IActionResult> Details(int id)
        {
            var discountEvent = await _context.DiscountEvents
                .Include(e => e.EventDetails)
                    .ThenInclude(ed => ed.Product) // Lấy thông tin sản phẩm
                        .ThenInclude(p => p.Category) // Lấy tên loại giày
                .FirstOrDefaultAsync(m => m.Id == id);

            if (discountEvent == null)
            {
                return NotFound();
            }

            // Map dữ liệu sang ViewModel
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

                // Map danh sách sản phẩm
                Products = discountEvent.EventDetails.Select(ed => new ProductInEventViewModel
                {
                    ProductId = ed.Product.Product_Id,
                    ProductName = ed.Product.Product_Name,
                    OriginalPrice = ed.Product.Price,
                    ImageUrl = ed.Product.ImageUrl,
                    CategoryName = ed.Product.Category?.Category_Name
                }).ToList()
            };

            return View(vm);
        }
    }
}