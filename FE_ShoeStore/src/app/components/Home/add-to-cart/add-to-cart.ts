import { Component, Input, Output, EventEmitter, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http'; // ĐÚNG: Lấy từ common/http
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../services/Cart/cart';
import { AccountService } from '../../../services/Account/account';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-add-to-cart',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-to-cart.html'
})
export class AddToCartComponent implements OnInit {
  // Nhận dữ liệu từ CartComponent (bao gồm product_Id đã có từ API mới)
  @Input() product: any; 
  @Output() close = new EventEmitter();

  options: any[] = [];
  uniqueSizes: any[] = [];
  uniqueVariants: any[] = [];
  
  selectedVariantId: number | null = null;
  selectedSizeId: number | null = null;
  quantity = 1;
  maxStock = 0;
  isLoggedIn = false;

  constructor(
    private http: HttpClient,
    private cartService: CartService,
    private accountService: AccountService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.checkAuth();
    this.loadOptions();
  }

  checkAuth() {
    this.accountService.getProfile().subscribe({
      next: () => this.isLoggedIn = true,
      error: () => this.isLoggedIn = false
    });
  }

  loadOptions() {
    // productId lấy từ API GetCartItems chúng ta vừa sửa
    const pid = this.product.product_Id; 
    if (!pid) {
      console.error("Lỗi: Không tìm thấy ID sản phẩm để tải cấu hình!");
      return;
    }

    this.http.get<any>(`https://localhost:7168/api/Cart/GetProductOptions?productId=${pid}`).subscribe({
      next: (res) => {
        this.options = res.productDetails;
        this.uniqueVariants = [...new Map(this.options.map(o => [o.variants_Id, o])).values()];
        this.uniqueSizes = [...new Map(this.options.map(o => [o.size_Id, o])).values()];
        this.cdr.detectChanges();
      },
      error: () => {
        Swal.fire('Lỗi', 'Không thể tải cấu hình sản phẩm. Hãy kiểm tra Backend!', 'error');
      }
    });
  }

  selectSize(sizeId: number) {
    this.selectedSizeId = sizeId;
    const match = this.options.find(o => o.variants_Id == this.selectedVariantId && o.size_Id == sizeId);
    this.maxStock = match ? match.stock : 0;
  }

  confirmAdd() {
    if (!this.isLoggedIn) {
      this.close.emit();
      this.router.navigate(['/account/login']);
      return;
    }

    if (!this.selectedSizeId || !this.selectedVariantId) {
      Swal.fire('Chú ý', 'Vui lòng chọn Màu và Size!', 'warning');
      return;
    }

    if (this.product.mode === 'edit') {
      const updateData = {
        cartId: this.product.cart_Id,
        productId: this.product.product_Id,
        sizeId: this.selectedSizeId,
        variantId: this.selectedVariantId
      };

      this.cartService.updateOptions(updateData).subscribe({
        next: () => {
          Swal.fire('Thành công', 'Đã cập nhật cấu hình sản phẩm!', 'success');
          this.close.emit();
        },
        error: (err) => Swal.fire('Lỗi', err.error?.message || 'Cập nhật thất bại', 'error')
      });
    } else {
      const addData = {
        productId: this.product.product_Id,
        sizeId: this.selectedSizeId,
        variantId: this.selectedVariantId,
        quantity: this.quantity
      };

      this.cartService.addToCart(addData).subscribe({
        next: () => {
          Swal.fire('Thành công', 'Đã thêm vào giỏ hàng!', 'success');
          this.close.emit();
        }
      });
    }
  }
}