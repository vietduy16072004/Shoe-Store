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
    public class BrandController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BrandController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Brand
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BrandViewModel>>> GetBrands()
        {
            var brands = await _context.Brands
                .Include(b => b.Products)
                .OrderByDescending(b => b.Brand_Id)
                .Select(b => new BrandViewModel
                {
                    Brand_Id = b.Brand_Id,
                    Brand_Name = b.Brand_Name,
                    ProductCount = b.Products != null ? b.Products.Count : 0
                }).ToListAsync();

            return Ok(brands);
        }

        // GET: api/Brand/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BrandViewModel>> GetBrand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();

            return Ok(new BrandViewModel { Brand_Id = brand.Brand_Id, Brand_Name = brand.Brand_Name });
        }

        // POST: api/Brand
        [HttpPost]
        public async Task<IActionResult> Create(BrandViewModel vm)
        {
            if (await _context.Brands.AnyAsync(b => b.Brand_Name == vm.Brand_Name))
                return BadRequest("Thương hiệu đã tồn tại!");

            var brand = new Brand { Brand_Name = vm.Brand_Name };
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBrand), new { id = brand.Brand_Id }, brand);
        }

        // PUT: api/Brand/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BrandViewModel vm)
        {
            if (id != vm.Brand_Id) return BadRequest();

            bool isDuplicate = await _context.Brands.AnyAsync(b => b.Brand_Name == vm.Brand_Name && b.Brand_Id != id);
            if (isDuplicate) return BadRequest("Tên thương hiệu đã tồn tại!");

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();

            brand.Brand_Name = vm.Brand_Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Brand/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands.Include(b => b.Products).FirstOrDefaultAsync(b => b.Brand_Id == id);
            if (brand == null) return NotFound();

            if (brand.Products != null && brand.Products.Any())
                return BadRequest("Không thể xóa thương hiệu đang có sản phẩm.");

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}