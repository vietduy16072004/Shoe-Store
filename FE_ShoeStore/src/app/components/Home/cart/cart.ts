import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CartService } from '../../../services/Cart/cart';
import { CartItem } from '../../../models/cart.model';
import { AddToCartComponent } from '../add-to-cart/add-to-cart';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, AddToCartComponent],
  templateUrl: './cart.html'
})
export class CartComponent implements OnInit {
  private backendUrl = 'https://localhost:7168';
  
  items: CartItem[] = [];
  totalAmount = 0;
  isLoading = true;

  editingProduct: any = null;

  constructor(
    private cartService: CartService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadCart();
  }

  // Tải dữ liệu giỏ hàng từ API
  loadCart() {
    this.isLoading = true;
    this.cartService.getCart().subscribe({
      next: (res) => {
        this.items = res.map(item => ({
          ...item,
          // Xử lý đường dẫn ảnh để hiển thị đúng từ folder Images/DB9
          imageUrl: item.imageUrl ? this.backendUrl + item.imageUrl.replace('~/', '/') : ''
        }));
        this.calculateTotal();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openEditOptions(item: CartItem) {
    this.editingProduct = {
      product_Id: item.productId, // Cần truyền ID sản phẩm cha để lấy list size/màu
      product_Name: item.productName,
      cart_Id: item.cart_Id, // Truyền thêm CartId để biết là đang EDIT
      mode: 'edit' // Đánh dấu chế độ chỉnh sửa
    };
  }

  // Hàm thay đổi số lượng và LƯU trực tiếp vào Database
  changeQuantity(item: CartItem, delta: number) {
    const newQty = item.quantity + delta;

    // 1. Kiểm tra giới hạn tối thiểu
    if (newQty < 1) return;

    // 2. Kiểm tra tồn kho thực tế từ ProductDetail
    if (newQty > item.stock) {
      Swal.fire({
        title: 'Thông báo',
        text: `Rất tiếc, sản phẩm này chỉ còn ${item.stock} đôi trong kho.`,
        icon: 'warning',
        confirmButtonColor: '#4f46e5'
      });
      return;
    }

    // 3. Gọi API để lưu thay đổi vào Database
    this.cartService.updateQuantity(item.cart_Id, newQty).subscribe({
      next: (res) => {
        if (res.success) {
          item.quantity = newQty;
          // Cập nhật lại tổng tiền của item dựa trên đơn giá mới nhất từ PriceService
          item.totalPrice = item.unitPrice * newQty;
          this.calculateTotal();
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        Swal.fire('Lỗi', err.error?.message || 'Không thể cập nhật số lượng', 'error');
      }
    });
  }

  // Xóa sản phẩm khỏi giỏ hàng
  removeItem(id: number) {
    Swal.fire({
      title: 'Xác nhận xóa?',
      text: "Bạn có chắc muốn bỏ sản phẩm này khỏi giỏ hàng?",
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#ef4444',
      cancelButtonColor: '#64748b',
      confirmButtonText: 'Xóa ngay',
      cancelButtonText: 'Hủy'
    }).then((result) => {
      if (result.isConfirmed) {
        this.cartService.deleteItem(id).subscribe(() => {
          this.items = this.items.filter(i => i.cart_Id !== id);
          this.calculateTotal();
          this.cdr.detectChanges();
        });
      }
    });
  }

  calculateTotal() {
    this.totalAmount = this.items.reduce((sum, item) => sum + item.totalPrice, 0);
  }
}