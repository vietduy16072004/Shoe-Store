using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Enum;
using Shoe.Models;
using Shoe.Services;
using Shoe.ViewModels;

namespace Shoe.Controllers
{
    [Route("api/[controller]")] // Route API
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PriceService _priceService;
        private const int PageSize = 9;

        public HomeController(ApplicationDbContext context, PriceService priceService)
        {
            _context = context;
            _priceService = priceService;
        }

        [HttpGet] // Trả về JSON cho Angular
        public async Task<IActionResult> GetHomeData(
            string? search, int? brandId, int? categoryId,
            decimal? minPrice, decimal? maxPrice, int page = 1)
        {
            if (page < 1) page = 1;

            // Logic lọc sản phẩm (Giữ nguyên từ code cũ)
            var query = _context.Products
                .Where(p => p.Status == StatusProduct.Selling)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => EF.Functions.Like(p.Product_Name, $"%{search}%"));

            if (brandId.HasValue) query = query.Where(p => p.Brand_Id == brandId.Value);
            if (categoryId.HasValue) query = query.Where(p => p.Category_Id == categoryId.Value);

            var allProductsRaw = await query.ToListAsync();

            // Tính toán giá và map sang ViewModel
            var vmList = allProductsRaw.Select(p => {
                decimal bestPrice = _priceService.CalculateBestPrice(p.Product_Id);
                int realDiscount = (p.Price > 0) ? (int)((p.Price - bestPrice) / p.Price * 100) : 0;

                return new ProductViewModel
                {
                    Product_Id = p.Product_Id,
                    Product_Name = p.Product_Name,
                    Price = p.Price,
                    Discount = realDiscount,
                    FinalPrice = bestPrice,
                    ImageUrl = p.ImageUrl,
                    Brand_Name = p.Brand?.Brand_Name,
                    Category_Name = p.Category?.Category_Name
                };
            }).ToList();

            // Lọc theo giá sau khi tính toán
            if (minPrice.HasValue && minPrice.Value > 0)
                vmList = vmList.Where(p => p.FinalPrice >= minPrice.Value).ToList();

            decimal maxDefault = 5000000;
            if (maxPrice.HasValue && maxPrice.Value < maxDefault)
                vmList = vmList.Where(p => p.FinalPrice <= maxPrice.Value).ToList();

            var totalCount = vmList.Count();
            var pagedProducts = vmList.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            return Ok(new
            {
                products = pagedProducts,
                brands = await _context.Brands.ToListAsync(),
                categories = await _context.Categories.ToListAsync(),
                totalCount = totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)PageSize)
            });
        }
    }
}