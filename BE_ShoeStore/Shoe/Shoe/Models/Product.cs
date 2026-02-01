using Shoe.Enum;

namespace Shoe.Models
{
    public class Product
    {
        public int Product_Id { get; set; }
        public string Product_Name { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public StatusProduct Status { get; set; }
        public int Category_Id { get; set; }
        public Category? Category { get; set; }
        public int Brand_Id { get; set; }
        public Brand? Brand { get; set; }
        public List<ProductDetail>? ProductDetails { get; set; }
    }
}
