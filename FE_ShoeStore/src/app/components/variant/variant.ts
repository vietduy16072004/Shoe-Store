import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VariantService } from '../../services/Variant/variant';
import { Variant } from '../../models/variant.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-variant',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './variant.html'
})
export class VariantComponent implements OnInit {
  variants: Variant[] = [];
  isModalOpen = false;
  isEditMode = false;
  currentVariant: Variant = this.getEmptyVariant();

  constructor(private variantService: VariantService, private cdr: ChangeDetectorRef) {}

  ngOnInit() { 
    this.loadVariants(); // Tải ngay dữ liệu khi vào trang
  }

  getEmptyVariant(): Variant {
    return { variants_Id: 0, variants_Name: '' };
  }

  loadVariants() {
    this.variantService.getVariants().subscribe(data => {
      this.variants = data;
      this.cdr.detectChanges(); // Cập nhật UI ngay lập tức
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentVariant = this.getEmptyVariant();
    this.isModalOpen = true;
  }

  openEditModal(v: Variant) {
    this.isEditMode = true;
    this.currentVariant = { ...v };
    this.isModalOpen = true;
  }

  onSave() {
    if (this.isEditMode) {
      this.variantService.updateVariant(this.currentVariant.variants_Id, this.currentVariant).subscribe({
        next: (res) => {
          Swal.fire('Thành công', res.message, 'success');
          this.closeModal();
          this.loadVariants();
        },
        error: (err) => Swal.fire('Lỗi', err.error.message || 'Cập nhật thất bại', 'error')
      });
    } else {
      this.variantService.createVariant(this.currentVariant).subscribe({
        next: (res) => {
          Swal.fire('Thành công', res.message, 'success');
          this.closeModal();
          this.loadVariants();
        },
        error: (err) => Swal.fire('Lỗi', err.error.message || 'Thêm mới thất bại', 'error')
      });
    }
  }

  onDelete(id: number) {
    Swal.fire({
      title: 'Xác nhận xóa màu này?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#4f46e5',
      confirmButtonText: 'Đúng, xóa nó!'
    }).then(result => {
      if (result.isConfirmed) {
        this.variantService.deleteVariant(id).subscribe({
          next: (res) => {
            Swal.fire('Đã xóa', res.message, 'success');
            this.loadVariants();
          },
          error: (err) => Swal.fire('Thất bại', err.error.message || 'Không thể xóa', 'error')
        });
      }
    });
  }

  closeModal() { this.isModalOpen = false; }
}