import { Component, AfterViewInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { AccountService } from '../../../services/Account/account'; 
import { SocialLoginRequest } from '../../../models/SocialLoginRequest.model';
import Swal from 'sweetalert2';

declare var google: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.html'
})
export class LoginComponent implements AfterViewInit {
  loginData = { Username: '', Password: '' };
  errors = { Username: '', Password: '' };

  // Biến trạng thái hiển thị mật khẩu
  showPassword = false;

  constructor(private accountService: AccountService, private router: Router) {}

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  ngAfterViewInit() {
    // Khởi tạo Google với Client ID từ appsettings.json
    google.accounts.id.initialize({
      client_id: '801102873395-3agcn77uklkg962ckbmdgc256s0d9bg9.apps.googleusercontent.com',
      callback: (res: any) => this.handleGoogleResponse(res)
    });

    google.accounts.id.renderButton(
      document.getElementById('buttonDiv'),
      { theme: 'outline', size: 'large', shape: 'pill', width: 360 }
    );
  }

  handleGoogleResponse(response: any) {
    const payload = JSON.parse(atob(response.credential.split('.')[1]));
    const socialReq: SocialLoginRequest = {
      email: payload.email,
      name: payload.name,
      provider: 'Google',
      providerId: payload.sub
    };

    this.accountService.loginGoogle(socialReq).subscribe({
      next: (res) => {
        if(res.success) {
          localStorage.setItem('username', res.username);
          localStorage.setItem('userRole', res.role);
          this.router.navigate(['/product-list']);
        }
      },
      error: () => Swal.fire('Lỗi', 'Đăng nhập Google thất bại!', 'error')
    });
  }

  onLogin() {
    // Reset lỗi trước khi kiểm tra
    this.errors = { Username: '', Password: '' };

    if (!this.loginData.Username) this.errors.Username = 'Tên đăng nhập không được để trống';
    if (!this.loginData.Password) this.errors.Password = 'Mật khẩu không được để trống';

    if (this.errors.Username || this.errors.Password) return;

    this.accountService.login(this.loginData).subscribe({
      next: (res) => {
        if (res.success) {
          localStorage.setItem('username', res.username);
          localStorage.setItem('userRole', res.role);
          this.router.navigate([res.role === 'Admin' ? '/admin/product-list' : '/home']);
        }
      },
      error: (err) => Swal.fire('Thất bại', err.error?.message || 'Sai tài khoản hoặc mật khẩu!', 'error')
    });
  }
}