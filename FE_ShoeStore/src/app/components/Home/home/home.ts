import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router'; // THÊM ĐỂ DÙNG routerLink TRONG HTML
import { HomeService } from '../../../services/Home/home';
import { AddToCartComponent } from '../add-to-cart/add-to-cart';

@Component({
  selector: 'app-home',
  standalone: true,
  // Đã thêm RouterModule để hết lỗi routerLink
  imports: [CommonModule, FormsModule, RouterModule, AddToCartComponent], 
  templateUrl: './home.html'
})
export class HomeComponent implements OnInit {
  
  private backendUrl = 'https://localhost:7168';
  
  products: any[] = [];
  brands: any[] = [];
  categories: any[] = [];
  
  filters = { search: '', brandId: null, categoryId: null, minPrice: 0, maxPrice: 5000000, page: 1 };
  
  selectedProduct: any = null;

  constructor(private homeService: HomeService, private cdr: ChangeDetectorRef) {}

  ngOnInit() { 
    // Tự động load sản phẩm ngay khi chạy chương trình
    this.loadData(); 
  }

  loadData() {
    this.homeService.getHomeData(this.filters).subscribe(res => {
      this.products = res.products.map((p: any) => ({
        ...p,
        imageUrl: p.imageUrl ? this.backendUrl + p.imageUrl.replace('~/', '/') : this.backendUrl + '/images/placeholder.png'
      }));
      this.brands = res.brands;
      this.categories = res.categories;
      this.cdr.detectChanges(); 
    });
  }

  // Hàm này được gọi ngay khi người dùng nhấn chọn Select box 
  applyFilter() { 
    this.filters.page = 1; 
    this.loadData(); 
  }

  openAddToCart(product: any) { 
    this.selectedProduct = product; 
  }
}