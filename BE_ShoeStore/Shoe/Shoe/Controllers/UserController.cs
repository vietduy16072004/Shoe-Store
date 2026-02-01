using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Models;
using Shoe.ViewModels;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shoe.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LẤY TẤT CẢ TÀI KHOẢN (Đã đổi tên và chuyển sang API)
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    Role = u.Role,
                    Provider = u.Provider, // Bổ sung thông tin nguồn đăng nhập
                    ProviderId = u.ProviderId
                }).ToListAsync();

            return Ok(users); // Trả về List JSON kèm mã trạng thái 200 (Ok)
        }

        // 2. TẠO TÀI KHOẢN MỚI (Admin tạo trực tiếp)
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] UserViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ!" });

            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                return BadRequest(new { success = false, message = "Tên đăng nhập đã tồn tại!" });

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                return BadRequest(new { success = false, message = "Email đã được sử dụng!" });

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = model.Username,
                Password = model.Password, // Mật khẩu do Admin gán ban đầu
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                Role = model.Role ?? "Customer"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Thêm tài khoản thành công!" });
        }

        // 3. CHỈNH SỬA TÀI KHOẢN (Hỗ trợ Admin reset mật khẩu)
        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromBody] UserViewModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
                return NotFound(new { success = false, message = "Không tìm thấy người dùng!" });

            // Kiểm tra trùng lặp email/username của người khác
            if (await _context.Users.AnyAsync(u => u.Username == model.Username && u.UserId != model.UserId))
                return BadRequest(new { success = false, message = "Tên đăng nhập đã tồn tại!" });

            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != model.UserId))
                return BadRequest(new { success = false, message = "Email đã được sử dụng!" });

            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Address = model.Address;

            // Nếu Admin có nhập mật khẩu mới thì tiến hành cập nhật
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = model.Password;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cập nhật tài khoản thành công!" });
        }

        // 4. XÓA TÀI KHOẢN (Gồm kiểm tra ràng buộc dữ liệu)
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "Không tìm thấy người dùng!" });

            // Kiểm tra xem user có đơn hàng hoặc giỏ hàng không
            bool hasOrders = await _context.Orders.AnyAsync(o => o.UserId == id);
            if (hasOrders)
                return BadRequest(new { success = false, message = "Không thể xóa: Tài khoản này đã có lịch sử đơn hàng!" });

            bool hasCartItems = await _context.CartItems.AnyAsync(c => c.UserId == id);
            if (hasCartItems)
                return BadRequest(new { success = false, message = "Không thể xóa: Tài khoản đang có sản phẩm trong giỏ hàng!" });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Xóa tài khoản thành công!" });
        }
    }
}
