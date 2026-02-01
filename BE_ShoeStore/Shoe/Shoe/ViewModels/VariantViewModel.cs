using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Shoe.Enum;
using Shoe.Models;

namespace Shoe.ViewModels
{
    public class VariantViewModel
    {
        public int Variants_Id { get; set; }

        [Display(Name = "Tên biến thể (VD: Màu sắc)")]
        public string? Variants_Name { get; set; }

        public List<ProductDetailViewModel>? ProductDetails { get; set; }
    }
}
