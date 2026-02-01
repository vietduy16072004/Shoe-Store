import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../services/User/user';
import { User } from '../../models/user.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user.html'
})
export class UsersComponent implements OnInit {
  users: User[] = [];
  filteredUsers: User[] = [];
  searchTerm: string = '';
  
  // Quản lý Modal
  isModalOpen = false;
  isEditMode = false;
  currentUser: User = this.getEmptyUser();

  constructor(private userService: UserService, private cdr: ChangeDetectorRef) {}

  ngOnInit() { this.loadUsers(); }

  getEmptyUser(): User {
    return { userId: '', username: '', email: '', role: 'Customer', phone: '', address: '', password: '' };
  }

  loadUsers() {
    this.userService.getAllUsers().subscribe(data => {
      this.users = data;
      this.applySearch();
      this.cdr.detectChanges();
    });
  }

  applySearch() {
    this.filteredUsers = this.users.filter(u => 
      u.username.toLowerCase().includes(this.searchTerm.toLowerCase()) || 
      u.email.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentUser = this.getEmptyUser();
    this.isModalOpen = true;
  }

  openEditModal(user: User) {
    this.isEditMode = true;
    this.currentUser = { ...user, password: '' }; // Không hiện mật khẩu cũ
    this.isModalOpen = true;
  }

  onSave() {
    if (this.isEditMode) {
      this.userService.editUser(this.currentUser).subscribe({
        next: (res) => {
          Swal.fire('Thành công', res.message, 'success');
          this.closeModal();
          this.loadUsers();
        },
        error: (err) => Swal.fire('Lỗi', err.error.message, 'error')
      });
    } else {
      this.userService.createUser(this.currentUser).subscribe({
        next: (res) => {
          Swal.fire('Thành công', res.message, 'success');
          this.closeModal();
          this.loadUsers();
        },
        error: (err) => Swal.fire('Lỗi', err.error.message, 'error')
      });
    }
  }

  onDelete(id: string) {
    Swal.fire({
      title: 'Xác nhận xóa?',
      text: "Bạn không thể hoàn tác hành động này!",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#4f46e5',
      cancelButtonColor: '#ef4444',
      confirmButtonText: 'Đúng, xóa nó!'
    }).then(result => { // ĐÃ SỬA: Thay .subscribe thành .then
      if (result.isConfirmed) {
        this.userService.deleteUser(id).subscribe({
          next: (res) => {
            Swal.fire('Đã xóa', res.message, 'success');
            this.loadUsers();
          },
          error: (err) => Swal.fire('Thất bại', err.error.message, 'error')
        });
      }
    });
  }

  closeModal() { this.isModalOpen = false; }
}