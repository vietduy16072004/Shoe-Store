using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Shoe.Models;
using Shoe.Enum;

namespace Shoe.ViewModels
{
    public class ProductDetailViewModel
    {
        public int ProductDetail_Id { get; set; }
        // ====== Product ======
        [Required(ErrorMessage = "Sản phẩm là bắt buộc")]
        [Display(Name = "Sản phẩm")]
        public int Product_Id { get; set; }
        public string? Product_Name { get; set; }
        public IEnumerable<Product>? ProductList { get; set; }

        // Giá bán và giảm giá (lấy từ Product)
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        [Display(Name = "Giảm giá (%)")]
        public int Discount { get; set; }

        // ====== Size ======
        [Required(ErrorMessage = "Kích thước là bắt buộc")]
        [Display(Name = "Kích thước")]
        public int Size_Id { get; set; }
        public string? Size_Name { get; set; }
        public IEnumerable<Size>? SizeList { get; set; }

        // ====== Variant (Color) ======
        [Required(ErrorMessage = "Màu sắc là bắt buộc")]
        [Display(Name = "Màu sắc")]
        public int Variants_Id { get; set; }
        public string? Variants_Name { get; set; }
        public IEnumerable<Variant>? VariantList { get; set; }

        [Display(Name = "Số lượng tồn")]
        public int Quantity { get; set; }


        [Display(Name = "Chọn ảnh chi tiết (PNG/JPG)")]
        public List<IFormFile>? ImageFiles { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
    }
}
