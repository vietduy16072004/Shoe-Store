using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Shoe.Enum;
using Shoe.Models;

namespace Shoe.ViewModels
{
    public class CategoryViewModel
    {
        public int Category_Id { get; set; }

        [Display(Name = "Danh mục")]
        public string? Category_Name { get; set; }
        public int ProductCount { get; set; }
        public List<ProductViewModel>? Products { get; set; }
    }
}
