using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Models;
using Shoe.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Shoe.Controllers
{
    public class SizeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SizeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================== INDEX ==================
        public async Task<IActionResult> Index()
        {
            var sizes = await _context.Sizes.OrderBy(s => s.Size_Name).ToListAsync();

            var vmList = sizes.Select(s => new SizeViewModel
            {
                Size_Id = s.Size_Id,
                Size_Name = s.Size_Name
            }).ToList();

            return View(vmList);
        }

        // ================== CREATE (GET) ==================
        [HttpGet]
        public IActionResult Create()
        {
            // Trả về PartialView để AJAX load vào Modal
            return PartialView("_CreateSizeModal", new SizeViewModel());
        }

        // ================== CREATE (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SizeViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            // 1. Kiểm tra trùng tên
            if (await _context.Sizes.AnyAsync(s => s.Size_Name == vm.Size_Name))
            {
                return Json(new { success = false, message = $"Size '{vm.Size_Name}' đã tồn tại!" });
            }

            try
            {
                var size = new Size
                {
                    Size_Name = vm.Size_Name
                };

                _context.Sizes.Add(size);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm size mới thành công!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ================== EDIT (GET) ==================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var size = await _context.Sizes.FindAsync(id);
            if (size == null) return NotFound();

            var vm = new SizeViewModel
            {
                Size_Id = size.Size_Id,
                Size_Name = size.Size_Name
            };

            // Trả về PartialView để AJAX load vào Modal
            return PartialView("_EditSizeModal", vm);
        }

        // ================== EDIT (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SizeViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            // 1. Kiểm tra trùng tên (Trùng tên NHƯNG khác ID)
            bool isDuplicate = await _context.Sizes
                .AnyAsync(s => s.Size_Name == vm.Size_Name && s.Size_Id != vm.Size_Id);

            if (isDuplicate)
            {
                return Json(new { success = false, message = $"Size '{vm.Size_Name}' đã tồn tại!" });
            }

            var size = await _context.Sizes.FindAsync(vm.Size_Id);
            if (size == null) return Json(new { success = false, message = "Không tìm thấy Size!" });

            try
            {
                size.Size_Name = vm.Size_Name;

                _context.Sizes.Update(size);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật size thành công!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ================== DELETE (POST) ==================
        // Delete giữ nguyên Redirect vì thường nút xóa nằm ngay ở Index, 
        // nhưng nếu muốn cũng có thể chuyển sang JSON. Ở đây giữ nguyên như Brand mẫu.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var size = await _context.Sizes.FindAsync(id);
            if (size == null) return NotFound();

            // Kiểm tra ràng buộc dữ liệu (Nếu cần)
            bool inUse = await _context.ProductDetails.AnyAsync(pd => pd.Size_Id == id);
            if (inUse)
            {
                TempData["Error"] = $"Khong the xoa Size '{size.Size_Name}' vi dang co san pham su dung.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Sizes.Remove(size);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xoa size thanh cong!";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Loi khi xoa: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}