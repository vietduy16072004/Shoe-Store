import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { DiscountEventService } from '../../../services/Discount/discount-event';
import { DiscountEventDetail } from '../../../models/discount-event.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-discount-event-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './discount-event-detail.html'
})
export class DiscountEventDetailComponent implements OnInit {
  detail?: DiscountEventDetail; // Đối tượng chứa dữ liệu chi tiết

  constructor(
    private route: ActivatedRoute,
    private discountService: DiscountEventService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Lấy ID từ URL (ví dụ: /admin/discount-event-detail/4)
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadEventDetail(+id);
    }
  }

  loadEventDetail(id: number) {
    this.discountService.getEventDetail(id).subscribe({
      next: (res) => {
        this.detail = res; // Gán dữ liệu trả về từ API
        this.cdr.detectChanges(); // Ép UI cập nhật lại
      },
      error: (err) => {
        console.error('Lỗi tải chi tiết sự kiện:', err);
        Swal.fire('Lỗi', 'Không thể tải thông tin chi tiết sự kiện', 'error');
      }
    });
  }
}