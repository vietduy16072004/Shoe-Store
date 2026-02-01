using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Models;
using Shoe.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Shoe.Controllers
{
    public class VariantController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VariantController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================== INDEX ==================
        public async Task<IActionResult> Index()
        {
            var variants = await _context.Variants
                .OrderByDescending(v => v.Variants_Id)
                .ToListAsync();

            var vmList = variants.Select(v => new VariantViewModel
            {
                Variants_Id = v.Variants_Id,
                Variants_Name = v.Variants_Name
            }).ToList();

            return View(vmList);
        }

        // ================== CREATE (GET) ==================
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateVariantModal", new VariantViewModel());
        }

        // ================== CREATE (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VariantViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            // 1. Kiểm tra trùng tên
            if (await _context.Variants.AnyAsync(v => v.Variants_Name == vm.Variants_Name))
            {
                return Json(new { success = false, message = $"Màu/Biến thể '{vm.Variants_Name}' đã tồn tại!" });
            }

            try
            {
                var variant = new Variant { Variants_Name = vm.Variants_Name };
                _context.Variants.Add(variant);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm biến thể thành công!" });
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
            var variant = await _context.Variants.FindAsync(id);
            if (variant == null) return NotFound();

            var vm = new VariantViewModel
            {
                Variants_Id = variant.Variants_Id,
                Variants_Name = variant.Variants_Name
            };
            return PartialView("_EditVariantModal", vm);
        }

        // ================== EDIT (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VariantViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            // 1. Kiểm tra trùng tên (Trùng tên NHƯNG khác ID)
            bool isDuplicate = await _context.Variants.AnyAsync(v => v.Variants_Name == vm.Variants_Name && v.Variants_Id != vm.Variants_Id);

            if (isDuplicate)
            {
                return Json(new { success = false, message = $"Màu/Biến thể '{vm.Variants_Name}' đã tồn tại!" });
            }

            var variant = await _context.Variants.FindAsync(vm.Variants_Id);
            if (variant == null)
                return Json(new { success = false, message = "Không tìm thấy biến thể!" });

            try
            {
                variant.Variants_Name = vm.Variants_Name;
                _context.Variants.Update(variant);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật biến thể thành công!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ================== DELETE (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var variant = await _context.Variants.FindAsync(id);
            if (variant == null) return NotFound();

            // Có thể thêm kiểm tra xem biến thể có đang được dùng trong ProductDetail nào không ở đây nếu cần

            try
            {
                _context.Variants.Remove(variant);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xoa bien the thanh cong!";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Loi khi xoa: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}