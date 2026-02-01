using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Models;
using Shoe.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Shoe.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================== INDEX ==================
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Products) // Vẫn cần Include để đếm
                .OrderByDescending(c => c.Category_Id)
                .ToListAsync();

            var vmList = categories.Select(c => new CategoryViewModel
            {
                Category_Id = c.Category_Id,
                Category_Name = c.Category_Name,

                ProductCount = c.Products?.Count ?? 0
            }).ToList();

            return View(vmList);
        }

        // ================== CREATE (GET) ==================
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateCategoryModal", new CategoryViewModel());
        }

        // ================== CREATE (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            // 1. Kiểm tra trùng tên
            if (await _context.Categories.AnyAsync(c => c.Category_Name == vm.Category_Name))
            {
                return Json(new { success = false, message = $"Danh mục '{vm.Category_Name}' đã tồn tại!" });
            }

            try
            {
                var category = new Category { Category_Name = vm.Category_Name };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm danh mục thành công!" });
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
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var vm = new CategoryViewModel
            {
                Category_Id = category.Category_Id,
                Category_Name = category.Category_Name
            };
            return PartialView("_EditCategoryModal", vm);
        }

        // ================== EDIT (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            // 1. Kiểm tra trùng tên (Trùng tên NHƯNG khác ID)
            bool isDuplicate = await _context.Categories.AnyAsync(c => c.Category_Name == vm.Category_Name && c.Category_Id != vm.Category_Id);

            if (isDuplicate)
            {
                return Json(new { success = false, message = $"Danh mục '{vm.Category_Name}' đã tồn tại!" });
            }

            var category = await _context.Categories.FindAsync(vm.Category_Id);
            if (category == null)
                return Json(new { success = false, message = "Không tìm thấy danh mục!" });

            try
            {
                category.Category_Name = vm.Category_Name;
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật danh mục thành công!" });
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
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Category_Id == id);

            if (category == null) return NotFound();

            if (category.Products != null && category.Products.Any())
            {
                TempData["Error"] = $"Khong the xoa '{category.Category_Name}' vi dang co {category.Products.Count} sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xoa danh muc thanh cong!";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Loi khi xoa: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}