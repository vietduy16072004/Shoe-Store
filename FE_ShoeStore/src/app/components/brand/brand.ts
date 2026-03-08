import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BrandService } from '../../services/Brand/brand';
import { Brand } from '../../models/brand.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-brand',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './brand.html'
})
export class BrandComponent implements OnInit {
  brands: Brand[] = [];
  
  // Quản lý Modal & Search (tương tự user.ts)
  isModalOpen = false;
  isEditMode = false;
  currentBrand: Brand = this.getEmptyBrand();

  constructor(private brandService: BrandService, private cdr: ChangeDetectorRef) {}

  ngOnInit() { 
    this.loadBrands(); // Tự động chạy khi vừa vào trang
  }

  getEmptyBrand(): Brand {
    return { brand_Id: 0, brand_Name: '', productCount: 0 };
  }

  loadBrands() {
    this.brandService.getBrands().subscribe(data => {
      this.brands = data;
      this.cdr.detectChanges(); // Ép Angular cập nhật UI ngay lập tức
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentBrand = this.getEmptyBrand();
    this.isModalOpen = true;
  }

  openEditModal(brand: Brand) {
    this.isEditMode = true;
    this.currentBrand = { ...brand };
    this.isModalOpen = true;
  }

  onSave() {
    if (this.isEditMode) {
      this.brandService.updateBrand(this.currentBrand.brand_Id, this.currentBrand).subscribe({
        next: () => {
          Swal.fire('Thành công', 'Cập nhật thương hiệu thành công', 'success');
          this.closeModal();
          this.loadBrands();
        },
        error: (err) => Swal.fire('Lỗi', err.error || 'Cập nhật thất bại', 'error')
      });
    } else {
      this.brandService.createBrand(this.currentBrand).subscribe({
        next: () => {
          Swal.fire('Thành công', 'Thêm thương hiệu mới thành công', 'success');
          this.closeModal();
          this.loadBrands();
        },
        error: (err) => Swal.fire('Lỗi', err.error || 'Thêm mới thất bại', 'error')
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
      cancelButtonColor: '#ef4444',
      confirmButtonText: 'Đúng, xóa nó!'
    }).then(result => {
      if (result.isConfirmed) {
        this.brandService.deleteBrand(id).subscribe({
          next: () => {
            Swal.fire('Đã xóa', 'Xóa thương hiệu thành công', 'success');
            this.loadBrands();
          },
          error: (err) => Swal.fire('Thất bại', err.error || 'Không thể xóa', 'error')
        });
      }
    });
  }

  closeModal() { this.isModalOpen = false; }
}