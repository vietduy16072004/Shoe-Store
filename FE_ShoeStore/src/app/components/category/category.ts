import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../../services/Category/category';
import { Category } from '../../models/category.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-category',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './category.html'
})
export class CategoryComponent implements OnInit {
  categories: Category[] = [];
  isModalOpen = false;
  isEditMode = false;
  currentCategory: Category = this.getEmptyCategory();

  constructor(private categoryService: CategoryService, private cdr: ChangeDetectorRef) {}

  ngOnInit() { this.loadCategories(); }

  getEmptyCategory(): Category {
    return { category_Id: 0, category_Name: '', productCount: 0 };
  }

  loadCategories() {
    this.categoryService.getCategories().subscribe(data => {
      this.categories = data;
      this.cdr.detectChanges(); // Đảm bảo data hiển thị liền khi nhấn vào layout 
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentCategory = this.getEmptyCategory();
    this.isModalOpen = true;
  }

  openEditModal(category: Category) {
    this.isEditMode = true;
    this.currentCategory = { ...category };
    this.isModalOpen = true;
  }

  onSave() {
    if (this.isEditMode) {
      this.categoryService.updateCategory(this.currentCategory.category_Id, this.currentCategory).subscribe({
        next: (res) => {
          Swal.fire('Thành công', res.message, 'success');
          this.closeModal();
          this.loadCategories();
        },
        error: (err) => Swal.fire('Lỗi', err.error.message, 'error')
      });
    } else {
      this.categoryService.createCategory(this.currentCategory).subscribe({
        next: (res) => {
          Swal.fire('Thành công', res.message, 'success');
          this.closeModal();
          this.loadCategories();
        },
        error: (err) => Swal.fire('Lỗi', err.error.message, 'error')
      });
    }
  }

  onDelete(id: number) {
    Swal.fire({
      title: 'Xác nhận xóa?',
      text: "Các sản phẩm thuộc loại này sẽ bị ảnh hưởng!",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#4f46e5',
      confirmButtonText: 'Đúng, xóa nó!'
    }).then(result => {
      if (result.isConfirmed) {
        this.categoryService.deleteCategory(id).subscribe({
          next: (res) => {
            Swal.fire('Đã xóa', res.message, 'success');
            this.loadCategories();
          },
          error: (err) => Swal.fire('Thất bại', err.error.message, 'error')
        });
      }
    });
  }

  closeModal() { this.isModalOpen = false; }
}