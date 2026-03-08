import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../services/Product/product';
import { AccountService } from '../../../services/Account/account';
import { CartService } from '../../../services/Cart/cart';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-page-product-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './page-product-detail.html'
})
export class PageProductDetailComponent implements OnInit {
  private backendUrl = 'https://localhost:7168';
  
  product: any = null;
  mainImage: string = '';
  isLoggedIn: boolean = false; // Trạng thái này sẽ quyết định tất cả

  selectedSizeId: number | null = null;
  selectedVariantId: number | null = null;
  maxQuantity: number = 0;
  quantity: number = 1;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private accountService: AccountService,
    private cartService: CartService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    this.loadProduct(id);
    this.checkUserLogin(); // Xác thực ngay khi vào trang
  }

  // Kiểm tra đăng nhập thực tế từ Server thông qua Cookie
  checkUserLogin() {
    this.accountService.getProfile().subscribe({
      next: (profile) => {
        // Nếu Server trả về profile thành công (200 OK)
        this.isLoggedIn = true;
        this.cdr.detectChanges();
      },
      error: () => {
        // Nếu Server báo lỗi (401 Unauthorized)
        this.isLoggedIn = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadProduct(id: number) {
    this.productService.getProductDetail(id).subscribe({
      next: (res) => {
        if (res.success) {
          this.product = res.data;
          if (this.product.displayImages?.length > 0) {
            this.mainImage = this.product.displayImages[0];
          }
          this.cdr.detectChanges();
        }
      }
    });
  }

  setMainImage(img: string) {
    this.mainImage = img;
  }

  selectOption() {
    if (this.selectedSizeId && this.selectedVariantId) {
      const match = this.product.detailsLookup.find((d: any) => 
        d.variants_Id === this.selectedVariantId && d.size_Id === this.selectedSizeId
      );
      this.maxQuantity = match ? match.quantity : 0;
      this.quantity = this.maxQuantity > 0 ? 1 : 0;
    }
    this.cdr.detectChanges();
  }

  updateQuantity(delta: number) {
    const newVal = this.quantity + delta;
    if (newVal >= 1 && newVal <= this.maxQuantity) {
      this.quantity = newVal;
    }
  }

  // HÀM QUAN TRỌNG: Đã sửa để chặn thông báo "Thành công" nếu chưa login
  addToCart() {
    if (!this.isLoggedIn) {
      Swal.fire({
        title: 'Bạn chưa đăng nhập!',
        text: 'Vui lòng đăng nhập để có thể thêm sản phẩm vào giỏ hàng.',
        icon: 'info',
        showCancelButton: true,
        confirmButtonColor: '#4f46e5',
        confirmButtonText: 'Đến trang đăng nhập',
        cancelButtonText: 'Để sau'
      }).then((result) => {
        if (result.isConfirmed) {
          this.router.navigate(['/account/login']);
        }
      });
      return;
    }

    if (!this.selectedSizeId || !this.selectedVariantId) {
      Swal.fire('Thông báo', 'Vui lòng chọn Màu sắc và Kích thước!', 'warning');
      return;
    }

    // Chuẩn bị dữ liệu theo đúng AddToCartRequest của Backend 
    const cartData = {
      productId: this.product.product_Id,
      sizeId: this.selectedSizeId,
      variantId: this.selectedVariantId,
      quantity: this.quantity
    };

    // Gọi API lưu vào Database 
    this.cartService.addToCart(cartData).subscribe({
      next: (res) => {
        if (res.success) {
          Swal.fire({
            title: 'Thành công',
            text: 'Sản phẩm đã được thêm vào giỏ hàng của bạn!',
            icon: 'success',
            showCancelButton: true,
            confirmButtonColor: '#4f46e5',
            cancelButtonColor: '#64748b',
            confirmButtonText: 'Xem giỏ hàng',
            cancelButtonText: 'Tiếp tục mua sắm'
          }).then((result) => {
            if (result.isConfirmed) {
              this.router.navigate(['/cart']); // Chuyển đến trang giỏ hàng Duy vừa tạo
            }
          });
        }
      },
      error: (err) => {
        Swal.fire('Lỗi', err.error?.message || 'Không thể thêm vào giỏ hàng', 'error');
      }
    });
  }
}