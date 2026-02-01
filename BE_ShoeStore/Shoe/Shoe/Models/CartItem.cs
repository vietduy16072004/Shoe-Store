namespace Shoe.Models
{
    public class CartItem
    {
        public int Cart_Id { set; get; }
        public int ProductDetail_Id { set; get; }
        public int Quantity { set; get; }
        public decimal Price { set; get; }
        public DateTime DateCreated { get; set; }
        public ProductDetail? ProductDetail { get; set; }
        public Guid UserId { set; get; }
        public User? AppUser { get; set; }
    }

}
