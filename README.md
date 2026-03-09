# 👟 SHOE STORE - FULLSTACK E-COMMERCE SYSTEM

Dự án **Shoe Store** là một hệ thống thương mại điện tử chuyên nghiệp được xây dựng trên nền tảng Full-stack với **ASP.NET Core Web API** và **Angular Standalone Components**. Hệ thống hỗ trợ trải nghiệm mua sắm mượt mà từ việc xem sản phẩm, quản lý giỏ hàng thông minh đến thanh toán trực tuyến.

## 📑 Mục lục

* [Tính năng nổi bật](#tinh-nang-noi-bat)
* [Công nghệ sử dụng](https://www.google.com/search?q=%23c%C3%B4ng-ngh%E1%BB%87-s%E1%BB%AD-d%E1%BB%A5ng)
* [Hướng dẫn cài đặt Backend](https://www.google.com/search?q=%23h%C6%B0%E1%BB%9Bng-d%E1%BA%ABn-c%C3%A0i-%C4%91%E1%BA%B7t-backend)
* [Hướng dẫn cài đặt Frontend](https://www.google.com/search?q=%23h%C6%B0%E1%BB%9Bng-d%E1%BA%ABn-c%C3%A0i-%C4%91%E1%BA%B7t-frontend)
* [Cấu hình hệ thống](#⚙️-cau-hinh-he-thong)

---

## ✨ Tính năng nổi bật

### 🔙 Backend (ASP.NET Core API)

* **Quản lý Sản phẩm & Biến thể**: Hỗ trợ nhiều kích cỡ (Size) và màu sắc (Variant) cho từng mẫu giày.
* **Hỗ trợ đặt hàng, giao hàng và quản lý đơn hàng**: Hỗ trợ cho việc đặt hàng đối với các sản phẩm đang hiện có trong giỏ hàng, lựa chọn nơi giao hàng rồi đưa ra giá ship hợp lý và quản lý đơn hàng cùng với tạo các vận đơn giao hàng.
* **Hệ thống Giá thông minh**: Tự động tính toán mức giá tốt nhất dựa trên các sự kiện giảm giá và chiết khấu trực tiếp (`PriceService`).
* **Xác thực Cookie-based**: Quản lý phiên đăng nhập bảo mật qua Cookie và Session.
* **Tích hợp Thanh toán**: Hỗ trợ cổng thanh toán **VNPAY**.
* **Tích hợp Vận chuyển**: Kết nối API **Giao Hàng Nhanh (GHN)** để tính toán vận chuyển thực tế.

### 🎨 Frontend (Angular)

* **Giao diện hiện đại**: Thiết kế theo phong cách Minimalism với **Tailwind CSS**, bo góc lớn và hiệu ứng chuyển động mượt mà.
* **Quick Add to Cart**: Thêm sản phẩm vào giỏ hàng nhanh chóng ngay từ trang chủ thông qua Modal.
* **Giỏ hàng thông minh**:
* Cập nhật số lượng và tính tiền thời gian thực.
* Tính năng **Chỉnh sửa cấu hình**: Cho phép đổi Size/Màu sắc ngay trong giỏ hàng mà không cần xóa sản phẩm.


* **Tìm kiếm linh hoạt**: Tìm kiếm sản phẩm theo tên với hiệu ứng gợi ý kết quả ngay lập tức trên Header.

---

## 🛠 Công nghệ sử dụng

| Thành phần | Công nghệ |
| --- | --- |
| **Backend** | .NET 8/9, EF Core, SQL Server |
| **Frontend** | Angular (Standalone), RxJS, Tailwind CSS |
| **Database** | SQL Server Management Studio (SSMS) |
| **Thư viện UI** | SweetAlert2 (Thông báo), FontAwesome (Icon) |

---

## 🚀 Hướng dẫn cài đặt

### 1. Cấu hình Backend (.NET API)

* **Bước 1**: Mở solution `Shoe.sln` bằng Visual Studio.
* **Bước 2**: Cấu hình cơ sở dữ liệu và các Token bảo mật (Xem phần [Cấu hình appsettings.json](https://www.google.com/search?q=%23c%E1%BA%A5u-h%C3%ACnh-h%E1%BB%87-th%E1%BB%91ng)).
* **Bước 3**: Mở *Package Manager Console* và chạy lệnh tạo database:
```bash
Update-Database

```


* **Bước 4**: Nhấn `F5` hoặc nút `Start` để chạy API tại `https://localhost:7168`.

### 2. Cấu hình Frontend (Angular)

Duy cần cài đặt **Node.js** trước khi thực hiện các lệnh sau:

* **Bước 1**: Di chuyển vào thư mục chứa code Frontend.
* **Bước 2**: Cài đặt các thư viện cần thiết:
```bash
npm install

```


* **Bước 3**: Chạy dự án ở môi trường phát triển:
```bash
ng serve --open

```


* **Bước 4**: Truy cập ứng dụng tại địa chỉ `http://localhost:4200`.

---

## ⚙️ Cấu hình hệ thống

Để đảm bảo tính bảo mật, các mã token và ID bí mật không được đẩy lên GitHub. Bạn cần tạo file **`appsettings.json`** trong thư mục gốc của dự án Backend với nội dung mẫu như sau:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=ShoeDB10;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_SECRET"
    }
  },
  "VNPAY": {
    "TmnCode": "YOUR_VNPAY_TMN_CODE",
    "HashSecret": "YOUR_VNPAY_HASH_SECRET",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "CallbackUrl": "https://localhost:7168/api/Vnpay/PaymentCallback"
  },
  "GHN": {
    "Token": "YOUR_GHN_TOKEN",
    "ShopId": "YOUR_GHN_SHOP_ID"
  },
  "AllowedHosts": "*"
}

```

> **Lưu ý**: Hãy thay thế các giá trị `YOUR_...` bằng thông tin thực tế từ tài khoản của bạn để các chức năng Thanh toán và Vận chuyển hoạt động chính xác.

