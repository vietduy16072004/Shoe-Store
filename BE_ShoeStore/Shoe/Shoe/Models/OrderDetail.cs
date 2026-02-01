namespace Shoe.Models
{
    public class OrderDetail
    {
        public int OrderDetail_Id { get; set; }
        public long Bill_Id { get; set; }
        public int ProductDetail_Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { set; get; }
        public Order? Order { get; set; }
        public ProductDetail? ProductDetails { get; set; }
    }
}
