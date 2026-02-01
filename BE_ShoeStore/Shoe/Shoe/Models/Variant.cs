using System.Drawing;

namespace Shoe.Models
{
    public class Variant
    {
        public int Variants_Id { get; set; }
        public string? Variants_Name { get; set; }
        public List<ProductDetail>? ProductDetails { get; set; }
    }
}
