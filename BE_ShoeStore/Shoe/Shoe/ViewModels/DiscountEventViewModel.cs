using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần cái này cho SelectList
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shoe.ViewModels
{
    public class DiscountEventViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tên sự kiện")]
        [Required(ErrorMessage = "Vui lòng nhập tên sự kiện")]
        public string EventName { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Giá trị giảm")]
        [Required(ErrorMessage = "Vui lòng nhập giá trị giảm")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị giảm phải lớn hơn 0")]
        public double DiscountValue { get; set; }

        [Display(Name = "Loại giảm giá")]
        public int DiscountType { get; set; } = 1; // 1: %, 2: VNĐ

        [Display(Name = "Ngày bắt đầu")]
        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày kết thúc")]
        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        // --- PHẦN LIÊN KẾT CHỌN SẢN PHẨM ---

        public List<int> SelectedProductIds { get; set; } = new List<int>();
        
        [ValidateNever]
        public MultiSelectList ProductList { get; set; }
    }
}