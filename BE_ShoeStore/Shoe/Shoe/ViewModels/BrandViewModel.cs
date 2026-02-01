using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Shoe.Enum;
using Shoe.Models;

namespace Shoe.ViewModels
{
    public class BrandViewModel
    {
        public int Brand_Id { get; set; }

        [Display(Name = "Thương hiệu")]
        public string? Brand_Name { get; set; }
        public int ProductCount { get; set; }
        public List<ProductViewModel>? Products { get; set; }
    }
}
