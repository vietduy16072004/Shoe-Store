using Shoe.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shoe.Models
{
    public class User
    {
        public Guid UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? Address { get; set; }

        public string Role { get; set; } = "Customer";

        // === THUỘC TÍNH BỔ SUNG CHO SOCIAL LOGIN ===
        public string? Provider { get; set; }   // Ví dụ: "Google", "Facebook"
        public string? ProviderId { get; set; } // ID duy nhất từ nền tảng Social trả về

        // Quan hệ 1 User -> N Orders
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
