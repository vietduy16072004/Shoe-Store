namespace Shoe.Models
{
    public class Brand
    {
        public int Brand_Id { get; set; }
        public string? Brand_Name { get; set; }  
        public List<Product>? Products { get; set; }
    }

}
