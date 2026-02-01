using System;
using System.ComponentModel.DataAnnotations;

namespace Shoe.ViewModels
{
    public class UserViewModel
    {
        [Key]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email phải có định dạng hợp lệ và kết thúc bằng @gmail.com")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số")]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò người dùng")]
        public string Role { get; set; } = "Customer";
        public string? Provider { get; set; }
        public string? ProviderId { get; set; }
    }
}
