import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DiscountEventService } from '../../../services/Discount/discount-event';
import { DiscountEventList, DiscountEventForm, DiscountEventDetail } from '../../../models/discount-event.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-discount-events',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './discount-events.html'
})
export class DiscountEventsComponent implements OnInit {
  events: DiscountEventList[] = [];
  productList: any[] = []; // Danh sách sản phẩm để chọn trong Modal
  
  isModalOpen = false;
  isEditMode = false;
  currentEvent: DiscountEventForm = this.getEmptyForm();

  constructor(
    private discountService: DiscountEventService, 
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadEvents();
    this.loadProductsLookup();
  }

  // Khởi tạo form trống
  getEmptyForm(): DiscountEventForm {
    return {
      id: 0,
      eventName: '',
      description: '',
      discountValue: 0,
      discountType: 1, // Mặc định là %
      startDate: new Date().toISOString().split('T')[0],
      endDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
      isActive: true,
      selectedProductIds: []
    };
  }

  // Lấy danh sách sự kiện hiển thị ở bảng
  loadEvents() {
    this.discountService.getEvents().subscribe({
      next: (data) => {
        this.events = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Lỗi tải sự kiện:', err)
    });
  }

  // Lấy danh sách ID và Tên sản phẩm cho Checkbox
  loadProductsLookup() {
    this.discountService.getProductsLookup().subscribe(data => {
      this.productList = data;
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentEvent = this.getEmptyForm();
    this.isModalOpen = true;
  }

  // Logic mở Modal Sửa: Lấy chi tiết để biết các sản phẩm đã chọn
  openEditModal(event: DiscountEventList) {
    this.isEditMode = true;
    this.discountService.getEventDetail(event.id).subscribe({
      next: (res: DiscountEventDetail) => {
        this.currentEvent = {
          id: res.id,
          eventName: res.eventName,
          description: res.description,
          // Chuyển đổi chuỗi hiển thị (VD: "20%") về số nguyên để nhập liệu
          discountValue: parseFloat(res.discountDisplay.replace(/[^0-9]/g, '')),
          discountType: res.discountDisplay.includes('%') ? 1 : 2,
          startDate: new Date(res.startDate).toISOString().split('T')[0],
          endDate: new Date(res.endDate).toISOString().split('T')[0],
          isActive: res.isActive,
          selectedProductIds: res.products.map(p => p.productId)
        };
        this.isModalOpen = true;
        this.cdr.detectChanges();
      },
      error: () => Swal.fire('Lỗi', 'Không thể lấy thông tin chi tiết sự kiện', 'error')
    });
  }

  // Logic xử lý Checkbox sản phẩm
  toggleProduct(productId: number) {
    const index = this.currentEvent.selectedProductIds.indexOf(productId);
    if (index > -1) {
      this.currentEvent.selectedProductIds.splice(index, 1); // Bỏ chọn nếu đã tồn tại
    } else {
      this.currentEvent.selectedProductIds.push(productId); // Thêm mới vào mảng
    }
  }

  // Lưu dữ liệu (Thêm hoặc Sửa)
  onSave() {
    if (!this.currentEvent.eventName) {
      Swal.fire('Cảnh báo', 'Vui lòng nhập tên sự kiện', 'warning');
      return;
    }

    const request = this.isEditMode 
      ? this.discountService.updateEvent(this.currentEvent.id, this.currentEvent)
      : this.discountService.createEvent(this.currentEvent);

    request.subscribe({
      next: (res) => {
        Swal.fire('Thành công', res.message, 'success');
        this.closeModal();
        this.loadEvents();
      },
      error: (err) => Swal.fire('Lỗi', err.error.message || 'Thao tác thất bại', 'error')
    });
  }

  // Xóa sự kiện với SweetAlert2
  onDelete(id: number) {
    Swal.fire({
      title: 'Xác nhận xóa sự kiện?',
      text: "Sản phẩm sẽ không còn được áp dụng mức giảm giá này!",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#4f46e5',
      cancelButtonColor: '#ef4444',
      confirmButtonText: 'Đúng, xóa ngay!',
      cancelButtonText: 'Hủy bỏ'
    }).then((result) => {
      if (result.isConfirmed) {
        this.discountService.deleteEvent(id).subscribe({
          next: (res) => {
            Swal.fire('Đã xóa', res.message, 'success');
            this.loadEvents();
          },
          error: (err) => Swal.fire('Lỗi', 'Không thể xóa sự kiện này', 'error')
        });
      }
    });
  }

  closeModal() {
    this.isModalOpen = false;
    this.isEditMode = false;
  }
}