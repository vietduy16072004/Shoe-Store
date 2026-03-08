using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Enum;
using Shoe.Models;
using Shoe.Services;
using Shoe.ViewModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shoe.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController] // Tự động kiểm tra ModelState và hỗ trợ API
    public class ProductController : ControllerBase // Dùng ControllerBase cho API chuyên nghiệp
    {
        private readonly ApplicationDbContext _context;
        private readonly PriceService _priceService;
        private const int PageSize = 5; // Giữ nguyên phân trang như bản gốc
        private readonly string _webImageFolder = "wwwroot/images";

        public ProductController(ApplicationDbContext context, PriceService priceService)
        {
            _context = context;
            _priceService = priceService;
        }

        // ================== 1. LẤY DANH SÁCH (Thay thế Index) ==================
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string? searchName,
            [FromQuery] int? brandId,
            [FromQuery] int? categoryId,
            [FromQuery] StatusProduct? status,
            [FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();

            // Giữ nguyên logic lọc bài bản từ bản gốc
            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(p => EF.Functions.Like(p.Product_Name, $"%{searchName}%"));
            if (brandId.HasValue) query = query.Where(p => p.Brand_Id == brandId.Value);
            if (categoryId.HasValue) query = query.Where(p => p.Category_Id == categoryId.Value);
            if (status.HasValue) query = query.Where(p => p.Status == status.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var productsRaw = await query
                .OrderByDescending(p => p.Product_Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Ánh xạ sang ViewModel và tính toán giá khuyến mãi
            var vmList = productsRaw.Select(p =>
            {
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
                    Description = p.Description,
                    Status = p.Status,
                    Category_Id = p.Category_Id,
                    Brand_Id = p.Brand_Id,
                    Category_Name = p.Category?.Category_Name,
                    Brand_Name = p.Brand?.Brand_Name
                };
            }).ToList();

            return Ok(new
            {
                success = true,
                data = vmList,
                pagination = new
                {
                    totalCount,
                    totalPages,
                    currentPage = page,
                    pageSize = PageSize
                }
            });
        }


        [HttpGet("Brands")]
        public async Task<IActionResult> GetBrands()
        {
            // Lấy danh sách thương hiệu từ DB và trả về đúng định dạng JSON
            var brands = await _context.Brands
                .Select(b => new {
                    brand_Id = b.Brand_Id,
                    brand_Name = b.Brand_Name // Khớp với [JsonPropertyName("brand_name")]
                })
                .ToListAsync();
            return Ok(brands);
        }

        [HttpGet("Categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new {
                    category_Id = c.Category_Id,
                    category_Name = c.Category_Name
                })
                .ToListAsync();
            return Ok(categories);
        }

        // ================== 2. CHI TIẾT SẢN PHẨM ==================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductDetails!)
                    .ThenInclude(pd => pd.Variant)
                .Include(p => p.ProductDetails!)
                    .ThenInclude(pd => pd.Size)
                .Include(p => p.ProductDetails!)
                    .ThenInclude(pd => pd.ProductImages.OrderBy(pi => pi.DisplayOrder))
                .FirstOrDefaultAsync(p => p.Product_Id == id);

            if (product == null) return NotFound(new { success = false, message = "Không tìm thấy sản phẩm" });

            decimal bestPrice = _priceService.CalculateBestPrice(product.Product_Id);
            int realDiscount = (product.Price > 0) ? (int)((product.Price - bestPrice) / product.Price * 100) : 0;

            // Logic xử lý ưu tiên 5 ảnh hiển thị (Giữ nguyên logic phức tạp của bạn)
            const int MAX_THUMBNAILS = 5;
            var finalImages = new List<Shoe.Models.ProductImage>();
            var allSortedImages = product.ProductDetails
                ?.OrderBy(pd => pd.ProductDetail_Id)
                .SelectMany(pd => pd.ProductImages?.OrderBy(pi => pi.DisplayOrder) ?? Enumerable.Empty<Shoe.Models.ProductImage>())
                .ToList() ?? new List<Shoe.Models.ProductImage>();

            for (int order = 1; order <= MAX_THUMBNAILS; order++)
            {
                var imagesToAdd = allSortedImages
                    .Where(img => img.DisplayOrder == order && !finalImages.Contains(img))
                    .Take(MAX_THUMBNAILS - finalImages.Count)
                    .ToList();
                finalImages.AddRange(imagesToAdd);
                if (finalImages.Count >= MAX_THUMBNAILS) break;
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    productInfo = product,
                    bestPrice,
                    discount = realDiscount,
                    displayImages = finalImages.OrderBy(img => img.DisplayOrder),
                    availableSizes = product.ProductDetails?.Select(pd => pd.Size).DistinctBy(s => s?.Size_Id),
                    availableVariants = product.ProductDetails?.Select(pd => pd.Variant).DistinctBy(v => v?.Variants_Id)
                }
            });
        }

        // ================== 3. LẤY DỮ LIỆU ĐỂ TẠO/SỬA (Dùng cho dropdown Angular) ==================
        [HttpGet("GetFormData")]
        public async Task<IActionResult> GetFormData()
        {
            var categories = await _context.Categories.ToListAsync();
            var brands = await _context.Brands.ToListAsync();
            var statuses = System.Enum.GetValues(typeof(StatusProduct))
                .Cast<StatusProduct>()
                .Select(s => new { id = (int)s, name = s.ToString() });

            return Ok(new { categories, brands, statuses });
        }

        // ================== 4. THÊM MỚI (POST) ==================
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductViewModel vm)
        {
            try
            {
                var product = new Product
                {
                    Product_Name = vm.Product_Name,
                    Price = vm.Price,
                    Discount = vm.Discount,
                    Description = vm.Description,
                    Status = vm.Status,
                    Category_Id = vm.Category_Id,
                    Brand_Id = vm.Brand_Id
                };

                if (vm.ImageFile != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.ImageFile.FileName)}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await vm.ImageFile.CopyToAsync(stream);
                    }
                    product.ImageUrl = $"/images/{fileName}";
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ================== 5. CẬP NHẬT (PUT) ==================
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] ProductViewModel vm)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Product_Name = vm.Product_Name;
            product.Price = vm.Price;
            product.Discount = vm.Discount;
            product.Category_Id = vm.Category_Id;
            product.Brand_Id = vm.Brand_Id;
            product.Status = vm.Status;

            if (vm.ImageFile != null)
            {
                // 1. Tạo tên file duy nhất để tránh trùng lặp
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.ImageFile.FileName)}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                // 2. (Tùy chọn) Xóa ảnh cũ trên server nếu tồn tại
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                // 3. Lưu file ảnh mới vào thư mục /wwwroot/images
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(stream);
                }

                // 4. Cập nhật đường dẫn ảnh mới cho sản phẩm
                product.ImageUrl = $"/images/{fileName}";
            }

            _context.Update(product);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ================== 6. XÓA (DELETE) ==================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Lấy sản phẩm bao gồm cả danh sách ProductDetails để kiểm tra 
            var product = await _context.Products
                .Include(p => p.ProductDetails)
                .FirstOrDefaultAsync(p => p.Product_Id == id);

            if (product == null) return NotFound(new { success = false, message = "Không tìm thấy sản phẩm" });

            // KIỂM TRA ĐIỀU KIỆN XÓA: Nếu có bất kỳ productdetail nào thì không cho xóa
            if (product.ProductDetails != null && product.ProductDetails.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không được xóa sản phẩm này vì vẫn còn các cấu hình chi tiết (ProductDetails) đang tồn tại!"
                });
            }

            try
            {
                // Nếu không có chi tiết nào, tiến hành xóa sản phẩm 
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }

        // ================== SEARCH (API) ==================
        [HttpGet("Search")]
        public async Task<IActionResult> Search(string term)
        {
            // Trả về danh sách rỗng nếu không có từ khóa [cite: 192]
            if (string.IsNullOrWhiteSpace(term))
                return Ok(new List<object>());

            var results = await _context.Products
                .Where(p => EF.Functions.Like(p.Product_Name, $"%{term}%")) // Tìm kiếm linh hoạt với 1 chữ cái
                .Select(p => new
                {
                    product_Id = p.Product_Id,
                    product_Name = p.Product_Name,
                    price = p.Price,
                    // Xử lý đường dẫn ảnh để FE không bị lỗi hiển thị
                    imageUrl = p.ImageUrl != null ? p.ImageUrl.Replace("~/", "/") : "/images/placeholder.png"
                })
                .Take(10) // Giới hạn 10 kết quả cho Modal tìm kiếm nhanh [cite: 171]
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetProductDetail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductDetails!)
                    .ThenInclude(pd => pd.Variant)
                .Include(p => p.ProductDetails!)
                    .ThenInclude(pd => pd.Size)
                .Include(p => p.ProductDetails!)
                    .ThenInclude(pd => pd.ProductImages)
                .FirstOrDefaultAsync(p => p.Product_Id == id);

            if (product == null) return NotFound(new { success = false, message = "Không tìm thấy sản phẩm" });

            decimal bestPrice = _priceService.CalculateBestPrice(product.Product_Id);
            int realDiscount = (product.Price > 0) ? (int)((product.Price - bestPrice) / product.Price * 100) : 0;

            // --- XỬ LÝ ẢNH CHI TIẾT (Từ folder DB9 qua /detailimages/) ---
            const string DETAIL_VIRTUAL_PATH = "/detailimages/"; // Map từ images/DB9
            const int MAX_THUMBNAILS = 5;

            // Gom toàn bộ ảnh chi tiết từ các cấu hình và gắn prefix đúng
            var detailImages = product.ProductDetails
                ?.SelectMany(pd => pd.ProductImages?.OrderBy(pi => pi.DisplayOrder) ?? Enumerable.Empty<Shoe.Models.ProductImage>())
                .OrderBy(img => img.DisplayOrder)
                .Select(img => DETAIL_VIRTUAL_PATH + img.ImagePath.TrimStart('/'))
                .Distinct()
                .Take(MAX_THUMBNAILS)
                .ToList() ?? new List<string>();

            // Xử lý ảnh đại diện (Nếu không có ảnh chi tiết thì dùng cái này)
            string mainProductImg = !string.IsNullOrEmpty(product.ImageUrl)
                ? product.ImageUrl.Replace("~/", "/")
                : "/images/no-image.png";

            // Nếu gallery trống, đưa ảnh chính vào làm ảnh duy nhất
            if (!detailImages.Any()) detailImages.Add(mainProductImg);

            return Ok(new
            {
                success = true,
                data = new
                {
                    product_Id = product.Product_Id,
                    product_Name = product.Product_Name,
                    price = product.Price,
                    finalPrice = bestPrice,
                    discount = realDiscount,
                    description = product.Description,
                    status = product.Status,
                    brand_Name = product.Brand?.Brand_Name,
                    category_Name = product.Category?.Category_Name,
                    displayImages = detailImages, // Danh sách đường dẫn đã có folder đúng
                    mainImageUrl = detailImages.FirstOrDefault() ?? mainProductImg,
                    availableSizes = product.ProductDetails?.Select(pd => pd.Size).DistinctBy(s => s?.Size_Id).OrderBy(s => s?.Size_Name),
                    availableVariants = product.ProductDetails?.Select(pd => pd.Variant).DistinctBy(v => v?.Variants_Id),
                    detailsLookup = product.ProductDetails?.Select(pd => new {
                        pd.ProductDetail_Id,
                        pd.Size_Id,
                        pd.Variants_Id,
                        pd.Quantity
                    })
                }
            });
        }
    }
}
