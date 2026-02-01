import { Component, OnInit, ChangeDetectorRef } from '@angular/core'; // Thêm ChangeDetectorRef để ép cập nhật giao diện
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../../services/Account/account';
import { Account } from '../../../models/account.model';
import { RouterModule, Router } from '@angular/router';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './profile.html'
})
export class ProfileComponent implements OnInit {
  // Khởi tạo đối tượng ban đầu tránh lỗi undefined
  profile: Account = { userId: '', username: '', email: '', role: '', password: '', phone: '', address: '' };
  
  // Quản lý chế độ Xem/Sửa
  isEditMode: boolean = false;
  
  // Quản lý trạng thái Modal đổi mật khẩu
  isPasswordModalOpen: boolean = false;
  
  // Dữ liệu và trạng thái hiển thị mật khẩu
  passData = { oldPassword: '', newPassword: '', confirmPassword: '' };
  showOld = false; showNew = false; showConfirm = false;

  constructor(
    private accountService: AccountService, 
    private router: Router,
    private cdr: ChangeDetectorRef // Inject ChangeDetectorRef để xử lý luồng giao diện
  ) {}

  ngOnInit() { 
    this.loadProfile(); 
  }

  // Tải dữ liệu hồ sơ từ API
  loadProfile() {
    this.accountService.getProfile().subscribe({
      next: (data) => {
        this.profile = data;
        this.cdr.detectChanges(); // Khắc phục lỗi "nhấn lần 2 mới hiện dữ liệu"
      },
      error: () => this.router.navigate(['/account/login'])
    });
  }

  // Chuyển đổi giữa chế độ Xem và Sửa
  toggleEdit() {
    if (this.isEditMode) {
      this.onUpdateProfile();
    } else {
      this.isEditMode = true;
    }
  }

  errors = { email: '', phone: '', address: '' };

  // Cập nhật thông tin hồ sơ
  onUpdateProfile() {
    // Reset lỗi trước khi kiểm tra
    this.errors = { email: '', phone: '', address: '' };
    let isValid = true;

    // 1. Kiểm tra không để trống
    if (!this.profile.email) { this.errors.email = 'Email không được để trống'; isValid = false; }
    if (!this.profile.phone) { this.errors.phone = 'Số điện thoại không được để trống'; isValid = false; }
    if (!this.profile.address) { this.errors.address = 'Địa chỉ không được để trống'; isValid = false; }

    // 2. Kiểm tra ràng buộc số điện thoại phải là 10 số
    const phoneRegex = /^[0-9]{10}$/;
    if (this.profile.phone && !phoneRegex.test(this.profile.phone)) {
      this.errors.phone = 'Số điện thoại phải bao gồm đúng 10 chữ số';
      isValid = false;
    }

    if (!isValid) {
      this.cdr.detectChanges(); // Cập nhật để hiện chữ đỏ ngay
      return;
    }

    this.accountService.editProfile(this.profile).subscribe({
      next: () => {
        this.isEditMode = false;
        this.cdr.detectChanges();
        Swal.fire({ icon: 'success', title: 'Thành công', text: 'Hồ sơ đã được cập nhật!', timer: 1500, showConfirmButton: false });
      },
      error: (err) => {
        const msg = err.error.message;
        // Hiển thị lỗi trùng số điện thoại hoặc email từ Backend trả về
        if (msg.includes('Số điện thoại')) {
          this.errors.phone = msg;
        } else if (msg.includes('Email')) {
          this.errors.email = msg;
        } else {
          Swal.fire('Lỗi', msg || 'Không thể cập nhật', 'error');
        }
        this.cdr.detectChanges();
      }
    });
  }

  // --- QUẢN LÝ MODAL ĐỔI MẬT KHẨU ---
  openPasswordModal() { 
    this.isPasswordModalOpen = true; 
  }

  closePasswordModal() { 
    this.isPasswordModalOpen = false;
    // Reset dữ liệu trong form mật khẩu khi đóng
    this.passData = { oldPassword: '', newPassword: '', confirmPassword: '' };
    this.cdr.detectChanges();
  }

  onChangePassword() {
    // Kiểm tra khớp mật khẩu mới
    if (this.passData.newPassword !== this.passData.confirmPassword) {
      Swal.fire('Lỗi', 'Mật khẩu xác nhận không trùng khớp!', 'error');
      return;
    }

    if (this.passData.newPassword.length < 6) {
      Swal.fire('Chú ý', 'Mật khẩu mới phải có ít nhất 6 ký tự', 'warning');
      return;
    }

    this.accountService.updatePassword(this.passData).subscribe({
      next: () => {
        // Đóng Modal ngay khi thành công
        this.isPasswordModalOpen = false;
        this.cdr.detectChanges(); // Ép thoát Modal về lại giao diện Profile chính

        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Mật khẩu đã được thay đổi thành công!',
          timer: 1500,
          showConfirmButton: false
        });
        
        // Reset dữ liệu form
        this.passData = { oldPassword: '', newPassword: '', confirmPassword: '' };
      },
      error: (err) => {
        Swal.fire('Thất bại', err.error.message || 'Mật khẩu cũ không chính xác', 'error');
      }
    });
  }

  // Đăng xuất tài khoản
  onLogout() {
    this.accountService.logout().subscribe({
      next: () => {
        localStorage.clear();
        this.router.navigate(['/account/login']);
      }
    });
  }
}