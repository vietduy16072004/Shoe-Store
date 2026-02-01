namespace Shoe.Models
{
    public class ProductImage
    {
        public int ProductImage_Id { get; set; }

        // Đường dẫn ảnh
        public string ImagePath { get; set; } = string.Empty;

        // Mô tả ảnh nếu cần (ví dụ: "Giày nhìn từ bên hông")
        public string? Description { get; set; }

        // Thứ tự hiển thị
        public int DisplayOrder { get; set; } = 0;
        public int ProductDetail_Id { get; set; }
        public ProductDetail? ProductDetail { get; set; }
        public string ImageUrl => ImagePath;
    }
}
