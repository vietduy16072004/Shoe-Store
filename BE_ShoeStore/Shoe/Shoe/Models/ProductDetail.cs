using Shoe.Enum;

namespace Shoe.Models
{
    public class ProductDetail
    {
        public int ProductDetail_Id { get; set; }
        public int Product_Id { get; set; }
        public Product? Product { get; set; }
        public int Size_Id { get; set; }
        public Size? Size { get; set; }
        public int Variants_Id { get; set; }
        public Variant? Variant { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
        public int Quantity { get; set; }

        public List<CartItem>? Carts { get; set; }

    }
}
