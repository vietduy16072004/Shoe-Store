using Microsoft.EntityFrameworkCore;
using Shoe.Models;

namespace Shoe.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ---------------------- DbSet cho các bảng ----------------------
        public DbSet<User> Users { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductDetail> ProductDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Variant> Variants { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<DiscountEvent> DiscountEvents { get; set; }
        public DbSet<EventDetail> EventDetails { get; set; }

        public DbSet<Carrier> Carriers { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<HistoryShipping> HistoryShippings { get; set; }

        // ---------------------- Cấu hình quan hệ ----------------------
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =================== USER ===================
            
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            // Bổ sung cấu hình cho Social Login
            modelBuilder.Entity<User>()
                .Property(u => u.Provider)
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.ProviderId)
                .HasMaxLength(255);

            // quan hệ Orders hiện tại
            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.AppUser)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =================== BRAND ===================
            modelBuilder.Entity<Brand>()
                .HasKey(b => b.Brand_Id);

            modelBuilder.Entity<Brand>()
                .Property(b => b.Brand_Name)
                .HasMaxLength(100);

            modelBuilder.Entity<Brand>()
                .HasMany(b => b.Products)
                .WithOne(p => p.Brand)
                .HasForeignKey(p => p.Brand_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // =================== CATEGORY ===================
            modelBuilder.Entity<Category>()
                .HasKey(c => c.Category_Id);

            modelBuilder.Entity<Category>()
                .Property(c => c.Category_Name)
                .HasMaxLength(100);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.Category_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // =================== PRODUCT DETAIL ===================
            modelBuilder.Entity<ProductDetail>()
                .HasKey(pd => pd.ProductDetail_Id);

            modelBuilder.Entity<ProductDetail>()
                .HasOne(pd => pd.Product)
                .WithMany(p => p.ProductDetails)
                .HasForeignKey(pd => pd.Product_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductDetail>()
                .HasOne(pd => pd.Variant)
                .WithMany(v => v.ProductDetails)
                .HasForeignKey(pd => pd.Variants_Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductDetail>()
                .HasOne(pd => pd.Size)
                .WithMany(s => s.ProductDetails)
                .HasForeignKey(pd => pd.Size_Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductDetail>()
                .Property(s => s.Quantity)
                .IsRequired();

            // =================== PRODUCT ===================
            modelBuilder.Entity<Product>()
                .HasKey(p => p.Product_Id);

            modelBuilder.Entity<Product>()
                .Property(p => p.Product_Name)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(pd => pd.Description)
                .HasMaxLength(500);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductDetails)
                .WithOne(s => s.Product)
                .HasForeignKey(s => s.Product_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(pd => pd.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(pd => pd.Brand_Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(pd => pd.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(pd => pd.Category_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // =================== VARIANT ===================
            modelBuilder.Entity<Variant>()
                .HasKey(v => v.Variants_Id);

            modelBuilder.Entity<Variant>()
                .Property(v => v.Variants_Name)
                .HasMaxLength(50);

            modelBuilder.Entity<Variant>()
                .HasMany(c => c.ProductDetails)
                .WithOne(p => p.Variant)
                .HasForeignKey(p => p.Variants_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // =================== SIZE ===================
            modelBuilder.Entity<Size>()
                .HasKey(s => s.Size_Id);

            modelBuilder.Entity<Size>()
                .Property(s => s.Size_Name)
                .HasMaxLength(20);

            modelBuilder.Entity<Size>()
                .HasMany(c => c.ProductDetails)
                .WithOne(p => p.Size)
                .HasForeignKey(p => p.Size_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // =================== CART ITEM ===================
            modelBuilder.Entity<CartItem>()
                .HasKey(ci => ci.Cart_Id);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ProductDetail)
                .WithMany(p => p.Carts)
                .HasForeignKey(ci => ci.ProductDetail_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.AppUser)
                .WithMany()
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =================== ORDER ===================
            modelBuilder.Entity<Order>()
                .HasKey(o => o.Bill_Id);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.AppUser)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.Bill_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // =================== ORDER DETAIL ===================
            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => od.OrderDetail_Id);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.ProductDetails)
                .WithMany()
                .HasForeignKey(od => od.ProductDetail_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // =================== ENUM CONFIG ===================
            modelBuilder.Entity<Product>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            // =================== PRODUCT IMAGE ===================
            modelBuilder.Entity<ProductImage>()
                .HasKey(pi => pi.ProductImage_Id);

            modelBuilder.Entity<ProductImage>()
                .Property(pi => pi.ImagePath)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<ProductImage>()
                .Property(pi => pi.Description)
                .HasMaxLength(255);

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.ProductDetail)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductDetail_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // =================== DISCOUNT EVENT ===================
            modelBuilder.Entity<DiscountEvent>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<DiscountEvent>()
                .Property(e => e.EventName)
                .IsRequired()
                .HasMaxLength(200);

            // =================== EVENT DETAIL ===================
            modelBuilder.Entity<EventDetail>()
                .HasKey(ed => ed.Id);

            // Cấu hình quan hệ với DiscountEvent
            modelBuilder.Entity<EventDetail>()
                .HasOne(ed => ed.DiscountEvent)
                .WithMany(e => e.EventDetails)
                .HasForeignKey(ed => ed.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ với Product
            modelBuilder.Entity<EventDetail>()
                .HasOne(ed => ed.Product)
                .WithMany() 
                .HasForeignKey(ed => ed.ProductID)
                .OnDelete(DeleteBehavior.Cascade);

            // =================== CARRIER (NHÀ VẬN CHUYỂN) ===================
            modelBuilder.Entity<Carrier>()
                .HasKey(c => c.CarrierId);

            // =================== SHIPMENT (VẬN ĐƠN) ===================
            modelBuilder.Entity<Shipment>()
                .HasKey(s => s.ShipmentId);

            // Quan hệ 1-1: Order - Shipment
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Order)
                .WithOne(o => o.Shipment)
                .HasForeignKey<Shipment>(s => s.Bill_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ 1-N: Carrier - Shipment
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Carrier)
                .WithMany() // Một Carrier có nhiều Shipment
                .HasForeignKey(s => s.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);

            // =================== HISTORY SHIPPING (LỊCH SỬ) ===================
            modelBuilder.Entity<HistoryShipping>()
                .HasKey(h => h.Id);

            // Quan hệ 1-N: Shipment - HistoryShipping
            modelBuilder.Entity<HistoryShipping>()
                .HasOne(h => h.Shipment)
                .WithMany(s => s.ShippingLogs)
                .HasForeignKey(h => h.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
