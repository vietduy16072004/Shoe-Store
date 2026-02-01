using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Enum;
using Shoe.Models;
using Shoe.ViewModels;

namespace Shoe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductDetailController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _storagePath = "wwwroot/images/DB9";

        public ProductDetailController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================== 1. LẤY DANH SÁCH CHI TIẾT THEO SẢN PHẨM ==================
        [HttpGet("ByProduct/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductDetails!)
                    .ThenInclude(pd => pd.Size)
                .Include(p => p.ProductDetails!)
                    .ThenInclude(pd => pd.Variant)
                .Include(p => p.ProductDetails!)
                    .ThenInclude(pd => pd.ProductImages)
                .FirstOrDefaultAsync(p => p.Product_Id == productId);

            if (product == null) return NotFound(new { success = false, message = "Không tìm thấy sản phẩm" });

            decimal finalPrice = product.Price * (100 - product.Discount) / 100;
            // Map sang ViewModel như bản gốc để Angular nhận dữ liệu sạch
            var result = new
            {
                product_Name = product.Product_Name,
                price = product.Price,
                discount = product.Discount,
                finalPrice = finalPrice,
                description = product.Description,
                brand_Name = product.Brand?.Brand_Name,
                category_Name = product.Category?.Category_Name,
                details = product.ProductDetails?.Select(pd => new {
                    pd.ProductDetail_Id,
                    sizeName = pd.Size?.Size_Name,
                    variantName = pd.Variant?.Variants_Name,
                    pd.Quantity,
                    // ĐIỀU CHỈNH TẠI ĐÂY: Chỉ lấy các trường dữ liệu phẳng của ảnh
                    images = pd.ProductImages?.OrderBy(i => i.DisplayOrder).Select(i => new {
                        i.ProductImage_Id,
                        i.ImagePath,    // Thuộc tính từ ProductImage.cs
                        i.DisplayOrder  // Thuộc tính từ ProductImage.cs
                    })
                })
            };

            return Ok(new { success = true, data = result });
        }

        // ================== 2. THÊM CHI TIẾT MỚI (POST) ==================
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductDetailViewModel vm)
        {
            const int MAX_IMAGES = 3;

            if (vm.Quantity < 0) return BadRequest(new { success = false, message = "Số lượng không được âm" });

            // Kiểm tra trùng lặp cấu hình
            var isDuplicate = await _context.ProductDetails
                .AnyAsync(pd => pd.Product_Id == vm.Product_Id && pd.Size_Id == vm.Size_Id && pd.Variants_Id == vm.Variants_Id);

            if (isDuplicate) return BadRequest(new { success = false, message = "Chi tiết sản phẩm này đã tồn tại." });

            var detail = new ProductDetail
            {
                Product_Id = vm.Product_Id,
                Size_Id = vm.Size_Id,
                Variants_Id = vm.Variants_Id,
                Quantity = vm.Quantity
            };

            _context.ProductDetails.Add(detail);
            await _context.SaveChangesAsync();

            // Xử lý upload tối đa 3 ảnh
            if (vm.ImageFiles != null && vm.ImageFiles.Any())
            {
                if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
                int order = 1;
                foreach (var file in vm.ImageFiles.Take(MAX_IMAGES))
                {
                    var fileName = $"{detail.ProductDetail_Id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(_storagePath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductDetail_Id = detail.ProductDetail_Id,
                        ImagePath = fileName,
                        DisplayOrder = order++
                    });
                }
                await _context.SaveChangesAsync();
            }

            await CheckAndUpdateProductStatus(vm.Product_Id);
            return Ok(new { success = true, message = "Thêm mới thành công!" });
        }

        // ================== 3. Update productdetail ==================

        // ================== LẤY MỘT CHI TIẾT CỤ THỂ (Dùng cho trang Edit) ==================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var detail = await _context.ProductDetails
                .Include(pd => pd.Product)
                .Include(pd => pd.Size)
                .Include(pd => pd.Variant)
                .Include(pd => pd.ProductImages)
                .FirstOrDefaultAsync(pd => pd.ProductDetail_Id == id);

            if (detail == null) return NotFound();

            var result = new
            {
                detail.ProductDetail_Id,
                detail.Product_Id,
                product_Name = detail.Product?.Product_Name,
                detail.Size_Id,
                detail.Variants_Id,
                detail.Quantity,
                images = detail.ProductImages?.OrderBy(i => i.DisplayOrder)
            };

            return Ok(new { success = true, data = result });
        }


        [HttpGet("GetOptions")]
        public async Task<IActionResult> GetOptions()
        {
            // Lấy dữ liệu từ đúng bảng của ProductDetail
            var sizes = await _context.Sizes.ToListAsync();
            var variants = await _context.Variants.ToListAsync();

            return Ok(new { success = true, sizes, variants });
        }

        // ================== THAY THẾ MỘT ẢNH CỤ THỂ ==================
        [HttpPost("ReplaceImage")]
        public async Task<IActionResult> ReplaceImage([FromForm] int imageId, [FromForm] IFormFile newFile)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null || newFile == null) return NotFound();

            // 1. Xóa ảnh cũ vật lý
            var oldPath = Path.Combine(_storagePath, image.ImagePath);
            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);

            // 2. Lưu ảnh mới
            var fileName = $"{image.ProductDetail_Id}_{Guid.NewGuid()}{Path.GetExtension(newFile.FileName)}";
            var filePath = Path.Combine(_storagePath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await newFile.CopyToAsync(stream);
            }

            // 3. Cập nhật Database
            image.ImagePath = fileName;
            _context.ProductImages.Update(image);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, newUrl = $"/images/DB9/{fileName}", imageId });
        }

        // ================== THÊM ẢNH MỚI VÀO CẤU HÌNH CÓ SẴN ==================
        [HttpPost("UpdateImages")]
        public async Task<IActionResult> UpdateImages([FromForm] int ProductDetail_Id, [FromForm] List<IFormFile> ImageFiles)
        {
            const int MAX_TOTAL_IMAGES = 3;
            var currentCount = await _context.ProductImages.CountAsync(i => i.ProductDetail_Id == ProductDetail_Id);

            if (currentCount >= MAX_TOTAL_IMAGES)
                return BadRequest(new { success = false, message = "Đã đạt giới hạn tối đa 3 ảnh" });

            if (ImageFiles != null && ImageFiles.Any())
            {
                int availableSlots = MAX_TOTAL_IMAGES - currentCount;
                int order = currentCount + 1;

                foreach (var file in ImageFiles.Take(availableSlots))
                {
                    var fileName = $"{ProductDetail_Id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(_storagePath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductDetail_Id = ProductDetail_Id,
                        ImagePath = fileName,
                        DisplayOrder = order++
                    });
                }
                await _context.SaveChangesAsync();
            }
            return Ok(new { success = true });
        }

        // ================== XÓA ẢNH (Dùng cho giao diện chỉnh sửa) ==================
        [HttpDelete("DeleteImage/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null) return NotFound();

            var filePath = Path.Combine(_storagePath, image.ImagePath);
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDetail detail)
        {
            // Kiểm tra ID khớp nhau để đảm bảo an toàn
            if (id != detail.ProductDetail_Id)
                return BadRequest(new { success = false, message = "ID không khớp" });

            var existingDetail = await _context.ProductDetails.FindAsync(id);
            if (existingDetail == null) return NotFound();

            // Cập nhật các trường thông tin
            existingDetail.Size_Id = detail.Size_Id;
            existingDetail.Variants_Id = detail.Variants_Id;
            existingDetail.Quantity = detail.Quantity;

            _context.ProductDetails.Update(existingDetail);
            await _context.SaveChangesAsync();

            // Cập nhật trạng thái sản phẩm (Selling/OutOfStock) dựa trên số lượng mới
            await CheckAndUpdateProductStatus(existingDetail.Product_Id);

            return Ok(new { success = true, message = "Cập nhật thành công!" });
        }

        // ================== 4. XÓA CẤU HÌNH (DELETE) ==================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var detail = await _context.ProductDetails.Include(pd => pd.ProductImages).FirstOrDefaultAsync(pd => pd.ProductDetail_Id == id);
            if (detail == null) return NotFound();

            int productId = detail.Product_Id;
            // Xóa ảnh vật lý
            foreach (var img in detail.ProductImages ?? new List<ProductImage>())
            {
                var path = Path.Combine(_storagePath, img.ImagePath);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            _context.ProductDetails.Remove(detail);
            await _context.SaveChangesAsync();
            await CheckAndUpdateProductStatus(productId);

            return Ok(new { success = true, message = "Xóa thành công" });
        }

        // Logic cập nhật tự động Selling/OutOfStock giữ nguyên
        private async Task CheckAndUpdateProductStatus(int productId)
        {
            var product = await _context.Products.Include(p => p.ProductDetails).FirstOrDefaultAsync(p => p.Product_Id == productId);
            if (product == null) return;

            bool hasStock = product.ProductDetails != null && product.ProductDetails.Any(pd => pd.Quantity > 0);

            if (!hasStock && product.Status == StatusProduct.Selling)
            {
                product.Status = StatusProduct.OutOfStock;
            }
            else if (hasStock && product.Status == StatusProduct.OutOfStock)
            {
                product.Status = StatusProduct.Selling;
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}