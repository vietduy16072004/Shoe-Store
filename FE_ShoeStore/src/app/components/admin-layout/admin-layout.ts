// components/admin-layout/admin-layout.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AccountService } from '../../services/Account/account';


@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css'
})
export class AdminLayoutComponent implements OnInit {
  username: string = 'Khách';

  constructor(private accountService: AccountService, private router: Router) {}

  ngOnInit() {
    // Lấy tên người dùng đã lưu khi đăng nhập thành công
    const savedName = localStorage.getItem('username');
    if (savedName) {
      this.username = savedName;
    }
  }

  // Hàm xử lý đăng xuất
  onLogout() {
    this.accountService.logout().subscribe({
      next: () => {
        localStorage.clear(); // Xóa sạch thông tin cũ
        this.router.navigate(['/account/login']); // Về trang login
      }
    });
  }
}