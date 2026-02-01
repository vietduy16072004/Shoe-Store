namespace Shoe.Models
{
    public class Size
    {
        public int Size_Id { get; set; }
        public string? Size_Name { get; set; }
        public List<ProductDetail>? ProductDetails { get; set; }
    }
}
