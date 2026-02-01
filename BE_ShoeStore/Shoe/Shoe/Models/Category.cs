namespace Shoe.Models
{
    public class Category
    {
        public int Category_Id { get; set; }
        public string? Category_Name { get; set; }
        public List<Product>? Products { get; set; }
    }

}
