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
    public class VariantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VariantController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/variant
        [HttpGet]
        public async Task<IActionResult> GetVariants()
        {
            var variants = await _context.Variants
                .OrderByDescending(v => v.Variants_Id)
                .Select(v => new VariantViewModel
                {
                    Variants_Id = v.Variants_Id,
                    Variants_Name = v.Variants_Name
                }).ToListAsync();

            return Ok(variants);
        }

        // POST: api/variant
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VariantViewModel vm)
        {
            if (await _context.Variants.AnyAsync(v => v.Variants_Name == vm.Variants_Name))
                return BadRequest(new { success = false, message = $"Màu sắc '{vm.Variants_Name}' đã tồn tại!" });

            var variant = new Variant { Variants_Name = vm.Variants_Name };
            _context.Variants.Add(variant);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Thêm màu sắc thành công!" });
        }

        // PUT: api/variant/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VariantViewModel vm)
        {
            if (id != vm.Variants_Id) return BadRequest();

            bool isDuplicate = await _context.Variants.AnyAsync(v => v.Variants_Name == vm.Variants_Name && v.Variants_Id != id);
            if (isDuplicate) return BadRequest(new { success = false, message = "Tên màu sắc đã tồn tại!" });

            var variant = await _context.Variants.FindAsync(id);
            if (variant == null) return NotFound();

            variant.Variants_Name = vm.Variants_Name;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cập nhật thành công!" });
        }

        // DELETE: api/variant/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var variant = await _context.Variants.FindAsync(id);
            if (variant == null) return NotFound();

            // Kiểm tra ràng buộc (nếu cần)
            bool inUse = await _context.ProductDetails.AnyAsync(pd => pd.Variants_Id == id);
            if (inUse)
                return BadRequest(new { success = false, message = "Không thể xóa màu sắc đang được sử dụng!" });

            _context.Variants.Remove(variant);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Xóa màu sắc thành công!" });
        }
    }
}