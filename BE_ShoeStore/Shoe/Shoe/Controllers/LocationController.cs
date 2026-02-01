using Microsoft.AspNetCore.Mvc;
using Shoe.Services; // Namespace chứa GhnService
using System.Threading.Tasks;

namespace Shoe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly GhnService _ghnService;

        public LocationController(GhnService ghnService)
        {
            _ghnService = ghnService;
        }

        [HttpGet("get-provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var json = await _ghnService.GetProvinces();
            // Trả về nguyên gốc những gì GHN gửi, bảo trình duyệt đây là JSON
            return Content(json, "application/json");
        }

        [HttpGet("get-districts/{provinceId}")]
        public async Task<IActionResult> GetDistricts(int provinceId)
        {
            var json = await _ghnService.GetDistricts(provinceId);
            return Content(json, "application/json");
        }

        [HttpGet("get-wards/{districtId}")]
        public async Task<IActionResult> GetWards(int districtId)
        {
            var json = await _ghnService.GetWards(districtId);
            return Content(json, "application/json");
        }

        [HttpPost("calculate-fee")]
        public async Task<IActionResult> CalculateFee([FromQuery] int districtId, [FromQuery] string wardCode)
        {
            decimal fee = await _ghnService.CalculateShippingFee(districtId, wardCode, 500);
            return Ok(new { fee = fee });
        }
    }
}