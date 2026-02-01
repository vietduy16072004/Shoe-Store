using System.Text;
using System.Text.Json;
using Shoe.Models; // Để dùng đối tượng Order

namespace Shoe.Services
{
    public class GhnService
    {
        private readonly HttpClient _httpClient;
        private readonly string _ghnToken;
        private readonly string _ghnShopId;
        private const string BASE_URL = "https://online-gateway.ghn.vn/shiip/public-api/";

        public GhnService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _ghnToken = configuration["GHN:Token"]!;
            _ghnShopId = configuration["GHN:ShopId"]!;
            _httpClient.DefaultRequestHeaders.Add("token", _ghnToken);
        }

        // --- CÁC HÀM LẤY ĐỊA CHỈ ---
        public async Task<string> GetProvinces()
        {
            try
            {
                var response = await _httpClient.GetAsync(BASE_URL + "master-data/province");
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex) { return $"{{\"code\": 500, \"message\": \"{ex.Message}\"}}"; }
        }

        public async Task<string> GetDistricts(int provinceId)
        {
            var payload = new { province_id = provinceId };
            return await PostRequest("master-data/district", payload);
        }

        public async Task<string> GetWards(int districtId)
        {
            var payload = new { district_id = districtId };
            return await PostRequest("master-data/ward", payload);
        }

        // --- TÍNH PHÍ VẬN CHUYỂN ---
        public async Task<decimal> CalculateShippingFee(int toDistrictId, string toWardCode, int weightInGram = 500)
        {
            var payload = new
            {
                service_type_id = 2,
                to_district_id = toDistrictId,
                to_ward_code = toWardCode,
                height = 15,
                length = 30,
                width = 20,
                weight = weightInGram,
                insurance_value = 0
            };

            var request = new HttpRequestMessage(HttpMethod.Post, BASE_URL + "v2/shipping-order/fee");
            request.Headers.Add("ShopId", _ghnShopId);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);
                var jsonStr = await response.Content.ReadAsStringAsync();

                if (jsonStr.Contains("\"total\":"))
                {
                    var part = jsonStr.Split("\"total\":")[1];
                    var moneyPart = part.Split(',')[0].Split('}')[0];
                    if (decimal.TryParse(moneyPart, out decimal fee)) return fee;
                }
            }
            catch { }
            return 0;
        }

        // --- TẠO ĐƠN HÀNG BÊN GHN ---
        public async Task<string> CreateShippingOrder(Order order, List<OrderDetail> details)
        {
            // 1. Tạo danh sách sản phẩm
            var items = new List<object>();
            foreach (var item in details)
            {
                items.Add(new
                {
                    name = item.ProductDetails?.Product?.Product_Name ?? "Giày",
                    quantity = item.Quantity,
                    code = item.ProductDetail_Id.ToString(),
                    price = (int)item.Price,
                    weight = 500 // Giả định 500g
                });
            }

            // 2. Tính tiền thu hộ (COD)
            // Nếu đã thanh toán VNPAY (có mã GD) thì COD = 0, ngược lại thu đủ.
            int codAmount = !string.IsNullOrEmpty(order.VnpayTxnRef) ? 0 : (int)order.Totalprice;

            // 3. Payload tạo đơn
            var payload = new
            {
                to_name = order.UserName,
                to_phone = order.PhoneNumber,
                to_address = order.Address,
                to_ward_code = order.WardCode,
                to_district_id = order.DistrictId,

                cod_amount = codAmount,

                weight = 500 * (details.Count > 0 ? details.Count : 1),
                length = 30,
                width = 20,
                height = 15,

                service_type_id = 2,
                payment_type_id = 1, // 1: Shop trả ship, 2: Khách trả ship

                required_note = "CHOXEMHANGKHONGTHU",
                items = items,
                note = order.Note ?? "Giao hàng giờ hành chính"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, BASE_URL + "v2/shipping-order/create");
            request.Headers.Add("ShopId", _ghnShopId);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"{{\"code\": 500, \"message\": \"Lỗi Exception: {ex.Message}\"}}";
            }
        }

        private async Task<string> PostRequest(string endpoint, object payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BASE_URL + endpoint, content);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"{{\"code\": 500, \"message\": \"Lỗi: {ex.Message}\"}}";
            }
        }
    }
}