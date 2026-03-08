using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Models;
using Shoe.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Shoe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/category
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .OrderByDescending(c => c.Category_Id)
                .Select(c => new CategoryViewModel
                {
                    Category_Id = c.Category_Id,
                    Category_Name = c.Category_Name,
                    ProductCount = c.Products != null ? c.Products.Count : 0
                }).ToListAsync();

            return Ok(categories);
        }

        // GET: api/category/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound(new { success = false, message = "Không tìm thấy loại giày!" });

            return Ok(new CategoryViewModel { Category_Id = category.Category_Id, Category_Name = category.Category_Name });
        }

        // POST: api/category
        [HttpPost]
        public async Task<IActionResult> Create(CategoryViewModel vm)
        {
            if (await _context.Categories.AnyAsync(c => c.Category_Name == vm.Category_Name))
                return BadRequest(new { success = false, message = $"Loại giày '{vm.Category_Name}' đã tồn tại!" });

            var category = new Category { Category_Name = vm.Category_Name };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Thêm loại giày thành công!" });
        }

        // PUT: api/category/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoryViewModel vm)
        {
            if (id != vm.Category_Id) return BadRequest();

            bool isDuplicate = await _context.Categories.AnyAsync(c => c.Category_Name == vm.Category_Name && c.Category_Id != id);
            if (isDuplicate) return BadRequest(new { success = false, message = "Tên loại giày đã tồn tại!" });

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.Category_Name = vm.Category_Name;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cập nhật thành công!" });
        }

        // DELETE: api/category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Category_Id == id);
            if (category == null) return NotFound();

            if (category.Products != null && category.Products.Any())
                return BadRequest(new { success = false, message = "Không thể xóa loại giày đang có sản phẩm!" });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Xóa thành công!" });
        }
    }
}