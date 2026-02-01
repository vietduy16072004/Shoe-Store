using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shoe.Models
{
    public class DiscountEvent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string EventName { get; set; }

        public string Description { get; set; }

        [Required]
        public double DiscountValue { get; set; } // Giá trị giảm (VD: 10% hoặc 50000đ)

        // 1 = Percentage (%), 2 = Fixed Amount (VNĐ)
        public int DiscountType { get; set; } = 1;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Quan hệ 1-N với EventDetail
        public ICollection<EventDetail> EventDetails { get; set; }
    }
}