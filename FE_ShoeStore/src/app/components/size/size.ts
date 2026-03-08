import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SizeService } from '../../services/Size/size';
import { Size } from '../../models/size.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-size',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './size.html'
})
export class SizeComponent implements OnInit {
  sizes: Size[] = [];
  isModalOpen = false;
  isEditMode = false;
  currentSize: Size = this.getEmptySize();

  constructor(private sizeService: SizeService, private cdr: ChangeDetectorRef) {}

  ngOnInit() { this.loadSizes(); }

  getEmptySize(): Size {
    return { size_Id: 0, size_Name: '' };
  }

  loadSizes() {
    this.sizeService.getSizes().subscribe(data => {
      this.sizes = data;
      this.cdr.detectChanges(); // Hiển thị dữ liệu ngay lập tức
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentSize = this.getEmptySize();
    this.isModalOpen = true;
  }

  openEditModal(size: Size) {
    this.isEditMode = true;
    this.currentSize = { ...size };
    this.isModalOpen = true;
  }

  onSave() {
    if (this.isEditMode) {
      this.sizeService.updateSize(this.currentSize.size_Id, this.currentSize).subscribe({
        next: (res) => {
          Swal.fire('Thành công', 'Cập nhật kích thước thành công', 'success');
          this.closeModal();
          this.loadSizes();
        },
        error: (err) => Swal.fire('Lỗi', err.error.message || 'Cập nhật thất bại', 'error')
      });
    } else {
      this.sizeService.createSize(this.currentSize).subscribe({
        next: (res) => {
          Swal.fire('Thành công', 'Thêm kích thước mới thành công', 'success');
          this.closeModal();
          this.loadSizes();
        },
        error: (err) => Swal.fire('Lỗi', err.error.message || 'Thêm mới thất bại', 'error')
      });
    }
  }

  onDelete(id: number) {
    Swal.fire({
      title: 'Xác nhận xóa?',
      text: "Bạn không thể hoàn tác hành động này!",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#4f46e5',
      confirmButtonText: 'Đúng, xóa nó!'
    }).then(result => {
      if (result.isConfirmed) {
        this.sizeService.deleteSize(id).subscribe({
          next: () => {
            Swal.fire('Đã xóa', 'Xóa kích thước thành công', 'success');
            this.loadSizes();
          },
          error: (err) => Swal.fire('Thất bại', err.error.message || 'Lỗi ràng buộc dữ liệu', 'error')
        });
      }
    });
  }

  closeModal() { this.isModalOpen = false; }
}