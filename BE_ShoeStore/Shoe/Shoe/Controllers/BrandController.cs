using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Models;
using Shoe.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Shoe.Controllers
{
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrandController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================== INDEX ==================
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách thương hiệu
            var brands = await _context.Brands
                .Include(b => b.Products)
                .OrderByDescending(b => b.Brand_Id) // Sắp xếp mới nhất lên đầu
                .ToListAsync();

            var vmList = brands.Select(b => new BrandViewModel
            {
                Brand_Id = b.Brand_Id,
                Brand_Name = b.Brand_Name,
                // Đếm số lượng sản phẩm để hiển thị thống kê
                ProductCount = b.Products?.Count ?? 0
            }).ToList();

            return View(vmList);
        }

        // ================== CREATE (GET) ==================
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateBrandModal", new BrandViewModel());
        }

        // ================== CREATE (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            // 1. Kiểm tra trùng tên
            if (await _context.Brands.AnyAsync(b => b.Brand_Name == vm.Brand_Name))
            {
                return Json(new { success = false, message = $"Thương hiệu '{vm.Brand_Name}' đã tồn tại!" });
            }

            try
            {
                var brand = new Brand { Brand_Name = vm.Brand_Name };
                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm thương hiệu thành công!" });
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
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();

            var vm = new BrandViewModel { Brand_Id = brand.Brand_Id, Brand_Name = brand.Brand_Name };
            return PartialView("_EditBrandModal", vm);
        }

        // ================== EDIT (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            // 1. Kiểm tra trùng tên (Trùng tên NHƯNG khác ID)
            bool isDuplicate = await _context.Brands.AnyAsync(b => b.Brand_Name == vm.Brand_Name && b.Brand_Id != vm.Brand_Id);

            if (isDuplicate)
            {
                return Json(new { success = false, message = $"Thương hiệu '{vm.Brand_Name}' đã tồn tại ở mục khác!" });
            }

            var brand = await _context.Brands.FindAsync(vm.Brand_Id);
            if (brand == null) return Json(new { success = false, message = "Không tìm thấy thương hiệu!" });

            try
            {
                brand.Brand_Name = vm.Brand_Name;
                _context.Brands.Update(brand);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật thương hiệu thành công!" });
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
            var brand = await _context.Brands.Include(b => b.Products).FirstOrDefaultAsync(b => b.Brand_Id == id);
            if (brand == null) return NotFound();

            // Kiểm tra ràng buộc
            if (brand.Products != null && brand.Products.Any())
            {
                TempData["Error"] = $"Khong the xoa '{brand.Brand_Name}' vi dang co {brand.Products.Count} san pham.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xoa thuong hieu thanh cong!";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Loi khi xoa: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}