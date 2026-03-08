using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Shoe.Data;
using Shoe.Services;
using System.Text.Json.Serialization;
using VNPAY.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    // QUAN TRỌNG: Dòng này giúp bỏ qua các vòng lặp quan hệ trong Database
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

    // Tùy chọn: Giúp JSON trả về gọn hơn bằng cách không hiện các trường null
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddDistributedMemoryCache();

// 1. Thêm dịch vụ CORS trước builder.Build()
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAngular", policy => {
        policy.WithOrigins("http://localhost:4200") // Port của Angular
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Quan trọng nếu bạn dùng MySessions
    });
});

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(2);

    // THÊM 2 DÒNG NÀY ĐỂ FIX LỖI TRẮNG TRANG / REDIRECT
    options.Cookie.SameSite = SameSiteMode.None; // Cho phép gửi Cookie giữa các Port khác nhau
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Bắt buộc vì bạn đang dùng HTTPS (Port 7168)
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None; // Hỗ trợ chạy khác Port (4200 vs 7168)
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        // ĐÂY LÀ ĐIỂM MẤU CHỐT: Trả về lỗi thay vì Redirect
        options.Events.OnRedirectToLogin = context => {
            context.Response.StatusCode = 401; // Unauthorized
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context => {
            context.Response.StatusCode = 403; // Forbidden
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var vnpayConfig = builder.Configuration.GetSection("VNPAY");

builder.Services.AddVnpayClient(config =>
{
    config.TmnCode = vnpayConfig["TmnCode"]!;
    config.HashSecret = vnpayConfig["HashSecret"]!;
    config.CallbackUrl = vnpayConfig["CallbackUrl"]!;
    config.BaseUrl = vnpayConfig["BaseUrl"]!; // Tùy chọn
    config.Version = vnpayConfig["Version"]!; // Tùy chọn
    config.OrderType = vnpayConfig["OrderType"]!; // Tùy chọn
});

// Đăng ký Service tính giá
builder.Services.AddScoped<Shoe.Services.PriceService>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<GhnService>();

var app = builder.Build();

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "images", "DB9")),
    RequestPath = "/detailimages"
});

app.UseRouting();
// 3. Sử dụng CORS ngay sau UseRouting
app.UseCors("AllowAngular");

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();