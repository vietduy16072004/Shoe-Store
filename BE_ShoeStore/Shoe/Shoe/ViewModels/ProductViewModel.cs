using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Shoe.Enum;
using Shoe.Models;
using System.Text.Json.Serialization; // Thêm thư viện này

namespace Shoe.ViewModels
{
    public class ProductViewModel
    {
        // Sử dụng JsonPropertyName để đảm bảo tên thuộc tính trong JSON 
        // khớp chính xác với những gì Angular mong đợi
        [JsonPropertyName("product_Id")]
        public int Product_Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [JsonPropertyName("product_Name")]
        public string Product_Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá bán là bắt buộc")]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("discount")]
        public int Discount { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("status")]
        public StatusProduct Status { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        // Bỏ qua ImageFile khi gửi dữ liệu xuống Frontend (vì chỉ dùng để upload)
        [JsonIgnore]
        public IFormFile? ImageFile { get; set; }

        // ====== Category ======
        [JsonPropertyName("category_Id")]
        public int Category_Id { get; set; }

        [JsonPropertyName("category_Name")]
        public string? Category_Name { get; set; }

        [JsonIgnore] // Không cần gửi list dropdown xuống JSON chính
        public IEnumerable<Category>? CategoryList { get; set; }

        // ====== Brand ======
        [JsonPropertyName("brand_Id")]
        public int Brand_Id { get; set; }

        [JsonPropertyName("brand_Name")]
        public string? Brand_Name { get; set; }

        [JsonIgnore]
        public IEnumerable<Brand>? BrandList { get; set; }

        // ====== Giá sau giảm (QUAN TRỌNG) ======
        [JsonPropertyName("finalPrice")]
        public decimal FinalPrice { get; set; }

        [JsonPropertyName("productDetails")]
        public List<ProductDetail>? ProductDetails { get; set; }
    }
}