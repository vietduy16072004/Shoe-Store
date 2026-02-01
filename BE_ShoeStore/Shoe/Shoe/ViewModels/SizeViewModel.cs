using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Shoe.Enum;
using Shoe.Models;

namespace Shoe.ViewModels
{
    public class SizeViewModel
    {
        public int Size_Id { get; set; }

        [Display(Name = "Kích thước")]
        public string? Size_Name { get; set; }

        public List<ProductDetailViewModel>? ProductDetails { get; set; }
    }
}
