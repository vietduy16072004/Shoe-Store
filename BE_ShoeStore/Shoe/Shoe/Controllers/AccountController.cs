using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe.Data;
using Shoe.Models;
using Shoe.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Shoe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context) { _context = context; }

        // --- ĐĂNG NHẬP THƯỜNG ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model) // Sử dụng LoginViewModel
        {
            // Chỉ kiểm tra Username và Password, không còn vướng Email/Phone
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

            if (user == null)
                return BadRequest(new { success = false, message = "Sai tài khoản hoặc mật khẩu!" });

            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("Role", user.Role);

            return Ok(new { success = true, username = user.Username, role = user.Role });
        }

        // --- ĐĂNG NHẬP GOOGLE ---
        [HttpPost("login-google")]
        public async Task<IActionResult> LoginGoogle([FromBody] SocialLoginRequest model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ProviderId == model.ProviderId);

            if (user == null)
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user != null)
                {
                    user.Provider = model.Provider;
                    user.ProviderId = model.ProviderId;
                    _context.Users.Update(user);
                }
                else
                {
                    user = new User
                    {
                        UserId = Guid.NewGuid(),
                        Username = model.Email.Split('@')[0],
                        Email = model.Email,
                        Provider = model.Provider,
                        ProviderId = model.ProviderId,
                        Role = "Customer",
                        Password = Guid.NewGuid().ToString() // Mật khẩu ảo
                    };
                    _context.Users.Add(user);
                }
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            return Ok(new { success = true, username = user.Username, role = user.Role });
        }

        // --- ĐĂNG KÝ ---
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model) // Sử dụng RegisterViewModel
        {
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại!" });

            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = model.Username,
                Password = model.Password,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                Role = "Customer"
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đăng ký thành công!" });
        }

        // --- XEM HỒ SƠ CÁ NHÂN ---
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var idStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(idStr)) return Unauthorized();

            var user = await _context.Users.FindAsync(Guid.Parse(idStr));
            if (user == null) return NotFound();

            // Map dữ liệu sang ProfileViewModel để trả về FE
            return Ok(new ProfileViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role
            });
        }

        // --- CHỈNH SỬA HỒ SƠ (Bổ sung theo yêu cầu) ---
        [HttpPost("edit-profile")]
        public async Task<IActionResult> EditProfile([FromBody] ProfileViewModel model) // Sử dụng ProfileViewModel
        {
            var idStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(idStr)) return Unauthorized();

            var user = await _context.Users.FindAsync(Guid.Parse(idStr));
            if (user == null) return NotFound();

            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != user.UserId))
                return BadRequest(new { message = "Email đã được sử dụng bởi tài khoản khác!" });

            if (await _context.Users.AnyAsync(u => u.Phone == model.Phone && u.UserId != user.UserId))
                return BadRequest(new { message = "Số điện thoại đã được sử dụng bởi tài khoản khác!" });

            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Address = model.Address;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Cập nhật hồ sơ thành công!" });
        }

        // --- CẬP NHẬT MẬT KHẨU ---
        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordViewModel model)
        {
            var idStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(idStr)) return Unauthorized();

            var user = await _context.Users.FindAsync(Guid.Parse(idStr));
            if (user.Password != model.OldPassword)
                return BadRequest(new { message = "Mật khẩu cũ không chính xác!" });

            user.Password = model.NewPassword;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Cập nhật mật khẩu thành công!" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { success = true });
        }
    }
}
