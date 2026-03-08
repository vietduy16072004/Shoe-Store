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
    public class SizeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SizeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/size
        [HttpGet]
        public async Task<IActionResult> GetSizes()
        {
            var sizes = await _context.Sizes
                .OrderBy(s => s.Size_Name)
                .Select(s => new SizeViewModel
                {
                    Size_Id = s.Size_Id,
                    Size_Name = s.Size_Name
                }).ToListAsync();

            return Ok(sizes);
        }

        // GET: api/size/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSize(int id)
        {
            var size = await _context.Sizes.FindAsync(id);
            if (size == null) return NotFound(new { success = false, message = "Không tìm thấy kích thước!" });

            return Ok(new SizeViewModel { Size_Id = size.Size_Id, Size_Name = size.Size_Name });
        }

        // POST: api/size
        [HttpPost]
        public async Task<IActionResult> Create(SizeViewModel vm)
        {
            if (await _context.Sizes.AnyAsync(s => s.Size_Name == vm.Size_Name))
                return BadRequest(new { success = false, message = $"Kích thước '{vm.Size_Name}' đã tồn tại!" });

            var size = new Size { Size_Name = vm.Size_Name };
            _context.Sizes.Add(size);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Thêm kích thước thành công!" });
        }

        // PUT: api/size/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SizeViewModel vm)
        {
            if (id != vm.Size_Id) return BadRequest();

            bool isDuplicate = await _context.Sizes.AnyAsync(s => s.Size_Name == vm.Size_Name && s.Size_Id != id);
            if (isDuplicate) return BadRequest(new { success = false, message = "Tên kích thước đã tồn tại!" });

            var size = await _context.Sizes.FindAsync(id);
            if (size == null) return NotFound();

            size.Size_Name = vm.Size_Name;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cập nhật thành công!" });
        }

        // DELETE: api/size/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var size = await _context.Sizes.FindAsync(id);
            if (size == null) return NotFound();

            // Kiểm tra ràng buộc với ProductDetails
            bool inUse = await _context.ProductDetails.AnyAsync(pd => pd.Size_Id == id);
            if (inUse)
                return BadRequest(new { success = false, message = "Không thể xóa: Kích thước này đang được sử dụng cho sản phẩm!" });

            _context.Sizes.Remove(size);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Xóa kích thước thành công!" });
        }
    }
}