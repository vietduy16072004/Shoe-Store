import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DiscountEventService } from '../../../services/Discount/discount-event';
import { DiscountEventDetail } from '../../../models/discount-event.model';
import { AddToCartComponent } from '../add-to-cart/add-to-cart';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-page-discount-event',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, AddToCartComponent],
  templateUrl: './page-discount-event.html'
})
export class PageDiscountEventComponent implements OnInit {
  private backendUrl = 'https://localhost:7168';
  
  // Danh sách sự kiện đầy đủ kèm sản phẩm
  eventsWithProducts: any[] = [];
  isLoading = true;
  selectedEventId: string = 'all'; // Cho bộ lọc nhanh
  selectedProduct: any = null;

  constructor(
    private discountService: DiscountEventService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadAllEventData();
  }

  loadAllEventData() {
    this.isLoading = true;
    // 1. Lấy danh sách ID các sự kiện đang diễn ra
    this.discountService.getEvents().subscribe(list => {
      const activeEvents = list.filter(e => e.statusLabel === 'Đang diễn ra');
      
      if (activeEvents.length === 0) {
        this.isLoading = false;
        return;
      }

      // 2. Dùng forkJoin để gọi API chi tiết của tất cả sự kiện cùng lúc
      const detailRequests = activeEvents.map(e => this.discountService.getEventDetail(e.id));
      
      forkJoin(detailRequests).subscribe(details => {
        this.eventsWithProducts = details.map(res => ({
          ...res,
          products: res.products.map(p => ({
            ...p,
            product_Id: p.productId,
            product_Name: p.productName,
            imageUrl: p.imageUrl ? this.backendUrl + p.imageUrl : this.backendUrl + '/images/placeholder.png',
            finalPrice: this.calculatePrice(p.originalPrice, res.discountDisplay)
          }))
        }));
        this.isLoading = false;
        this.cdr.detectChanges();
      });
    });
  }

  calculatePrice(oldPrice: number, discount: string): number {
    if (discount.includes('%')) {
      return oldPrice * (1 - parseFloat(discount) / 100);
    }
    return oldPrice - parseFloat(discount.replace(/[^0-9]/g, ''));
  }

  // Cuộn đến vị trí sự kiện khi nhấn bộ lọc
  scrollToEvent(eventId: string) {
    this.selectedEventId = eventId;
    if (eventId !== 'all') {
      const element = document.getElementById('event-' + eventId);
      if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    } else {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  openAddToCart(product: any) { this.selectedProduct = product; }
}