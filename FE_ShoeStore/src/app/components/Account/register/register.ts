import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AccountService } from '../../../services/Account/account';
import { Account } from '../../../models/account.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.html'
})
export class RegisterComponent {
  regData: Account = { 
    userId: '00000000-0000-0000-0000-000000000000',
    username: '', password: '', email: '', role: 'Customer', phone: '', address: ''
  };

  confirmPassword = '';
  // Quản lý lỗi hiển thị đỏ dưới input
  errors = { username: '', password: '', email: '', phone: '', confirmPassword: '', address: '' };

  // Trạng thái hiển thị mật khẩu
  showP = false;
  showCP = false;

  constructor(private accountService: AccountService, private router: Router) {}

  onRegister() {
    this.errors = { username: '', password: '', confirmPassword: '', email: '', phone: '', address: '' };
    let isValid = true;

    if (!this.regData.username) { this.errors.username = 'Vui lòng nhập tên đăng nhập'; isValid = false; }
    if (!this.regData.password) { this.errors.password = 'Vui lòng nhập mật khẩu'; isValid = false; }
    if (!this.regData.email) { this.errors.email = 'Vui lòng nhập Email'; isValid = false; }
    if (!this.regData.phone) { this.errors.phone = 'Vui lòng nhập số điện thoại'; isValid = false; }
    if (!this.regData.address) { this.errors.address = 'Vui lòng nhập địa chỉ'; isValid = false; }

    // Kiểm tra định dạng số điện thoại
    const phoneRegex = /^[0-9]{10}$/;
    if (this.regData.phone && !phoneRegex.test(this.regData.phone)) {
      this.errors.phone = 'Số điện thoại phải bao gồm đúng 10 chữ số';
      isValid = false;
    }
    // Kiểm tra độ dài mật khẩu
    if (this.regData.password && this.regData.password.length < 6) {
      this.errors.password = 'Mật khẩu phải có ít nhất 6 ký tự';
      isValid = false;
    }
    // Kiểm tra khớp mật khẩu
    if (this.regData.password !== this.confirmPassword) {
      this.errors.confirmPassword = 'Xác nhận mật khẩu không khớp';
      isValid = false;
    }
    // Kiểm tra định dạng email @gmail.com
    const gmailRegex = /^[a-zA-Z0-9._%+-]+@gmail\.com$/;
    if (this.regData.email && !gmailRegex.test(this.regData.email)) {
      this.errors.email = 'Email phải đúng định dạng @gmail.com';
      isValid = false;
    }

    if (!isValid) return;

    this.accountService.register(this.regData).subscribe({
      next: () => {
        Swal.fire('Thành công', 'Tài khoản đã được tạo thành công!', 'success');
        this.router.navigate(['/account/login']);
      },
      error: (err) => {
        const msg = err.error.message;
        if (msg.includes('Tên đăng nhập')) this.errors.username = msg;
        else if (msg.includes('Email')) this.errors.email = msg;
        else if (msg.includes('Số điện thoại')) this.errors.phone = msg;
        else Swal.fire('Lỗi', msg || 'Đăng ký thất bại', 'error');
      }
    });
  }
}