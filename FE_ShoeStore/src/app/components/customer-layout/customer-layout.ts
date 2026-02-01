import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HomeService } from '../../services/Home/home';

@Component({
  selector: 'app-customer-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './customer-layout.html'
})
export class CustomerLayoutComponent implements OnInit {
  private backendUrl = 'https://localhost:7168';
  
  username: string | null = null;
  isSearchOpen = false;
  searchQuery = '';
  searchResults: any[] = [];

  constructor(private router: Router, private homeService: HomeService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.username = localStorage.getItem('username'); // Kiểm tra trạng thái đăng nhập
  }

  openSearchModal() { 
    this.isSearchOpen = true; 
    this.searchQuery = '';
    this.searchResults = [];
  }

  onSearch() {
    const term = this.searchQuery.trim();
    
    // Nếu xóa trắng ô search thì xóa kết quả ngay
    if (!term) {
      this.searchResults = [];
      this.cdr.detectChanges();
      return;
    }

    // Gọi API tìm kiếm linh hoạt (BE đã dùng EF.Functions.Like)
    this.homeService.searchQuick(term).subscribe({
      next: (res) => {
        this.searchResults = res.map((p: any) => ({
          ...p,
          imageUrl: p.imageUrl 
            ? this.backendUrl + p.imageUrl.replace('~/', '/') 
            : this.backendUrl + '/images/placeholder.png'
        }));
        this.cdr.detectChanges(); // Ép hiển thị kết quả lên giao diện ngay
      },
      error: () => {
        this.searchResults = [];
        this.cdr.detectChanges();
      }
    });
  }

  onLogout() {
    localStorage.clear();
    this.username = null;
    this.router.navigate(['/home']);
  }
}