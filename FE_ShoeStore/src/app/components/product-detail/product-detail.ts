// src/app/components/product-detail/product-detail.ts
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms'; // Cần thiết cho ngModel
import { ProductDetailService } from '../../services/ProducDetail/product-detail';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css'
})
export class ProductDetailComponent implements OnInit {
  productId!: number;
  productInfo: any = {};
  details: any[] = [];
  loading: boolean = true;

  // --- LOGIC MODAL THÊM MỚI ---
  isCreateModalOpen = false;
  newDetail: any = { size_Id: 0, variants_Id: 0, quantity: 1 }; 
  sizeList: any[] = [];
  variantList: any[] = [];
  selectedFiles: File[] = [];
  imagePreviews: string[] = [];

  constructor(
    private route: ActivatedRoute, 
    private router: Router,
    private detailService: ProductDetailService,
    private cdr: ChangeDetectorRef 
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.productId = Number(idParam);
      this.loadDetails();
    }
  }

  loadDetails() {
    this.loading = true;
    this.detailService.getByProduct(this.productId).subscribe({
      next: (res: any) => {
        if (res && res.success) {
          this.productInfo = res.data;
          this.details = res.data.details || [];
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Mở Modal và lấy danh sách Size/Màu từ API GetOptions
  openCreateModal() {
    this.detailService.getOptions().subscribe((res: any) => {
      if (res.success) {
        this.sizeList = res.sizes; 
        this.variantList = res.variants;
        this.isCreateModalOpen = true;
        this.cdr.detectChanges();
      }
    });
  }

  closeCreateModal() {
    this.isCreateModalOpen = false;
    this.newDetail = { size_Id: 0, variants_Id: 0, quantity: 1 };
    this.selectedFiles = [];
    this.imagePreviews = [];
  }

  // Xử lý chọn tối đa 3 ảnh
  onFilesSelected(event: any) {
    const files = Array.from(event.target.files as FileList).slice(0, 3);
    this.selectedFiles = files;
    this.imagePreviews = [];
    files.forEach(file => {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagePreviews.push(e.target.result);
        this.cdr.detectChanges();
      };
      reader.readAsDataURL(file);
    });
  }

  saveProductDetail() {
    if (this.newDetail.size_Id == 0 || this.newDetail.variants_Id == 0) {
      alert('Vui lòng chọn đầy đủ Size và Màu sắc');
      return;
    }

    const formData = new FormData();
    formData.append('Product_Id', this.productId.toString());
    formData.append('Size_Id', this.newDetail.size_Id.toString());
    formData.append('Variants_Id', this.newDetail.variants_Id.toString());
    formData.append('Quantity', this.newDetail.quantity.toString()); // Bổ sung số lượng
    
    // Gửi danh sách file ảnh lên Backend
    this.selectedFiles.forEach(file => formData.append('ImageFiles', file));

    this.detailService.create(formData).subscribe({
      next: (res: any) => {
        if (res.success) {
          alert('Thêm cấu hình thành công!');
          this.closeCreateModal();
          this.loadDetails();
        }
      },
      error: (err) => alert(err.error?.message || 'Lỗi khi lưu dữ liệu')
    });
  }

  onEdit(id: number) {
    this.router.navigate(['/product-detail/edit', id]);
  }
  
  onDelete(id: number) {
    if (confirm('Bạn có chắc muốn xóa cấu hình này?')) {
      this.detailService.delete(id).subscribe(() => this.loadDetails());
    }
  }
}