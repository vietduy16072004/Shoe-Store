// components/product-detail/edit-detail/edit-detail.ts
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductDetailService } from '../../../services/ProducDetail/product-detail';
import { ProductService } from '../../../services/Product/product';

@Component({
  selector: 'app-edit-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './edit-detail.html'
})
export class EditDetailComponent implements OnInit {
  detailId!: number;
  editingData: any = {};
  sizeList: any[] = [];
  variantList: any[] = [];
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private detailService: ProductDetailService,
    private productService: ProductService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.detailId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadInitialData();
  }

  // edit-detail.ts
  loadInitialData() {
    this.loading = true;
    // 1. Lấy danh sách Size và Màu sắc từ API mới
    this.detailService.getOptions().subscribe((res: any) => {
      if (res.success) {
        this.sizeList = res.sizes; 
        this.variantList = res.variants;
      }
      
      // 2. Lấy dữ liệu của bản ghi đang sửa
      this.detailService.getById(this.detailId).subscribe((res: any) => {
        if (res.success) {
          this.editingData = res.data;
        }
        this.loading = false;
        this.cdr.detectChanges();
      });
    });
  }

  // Quản lý ảnh: Xóa
  onDeleteImage(imageId: number) {
    if (confirm('Bạn có chắc muốn xóa ảnh này?')) {
      this.detailService.deleteImage(imageId).subscribe(() => {
        this.editingData.images = this.editingData.images.filter((img: any) => img.productImage_Id !== imageId);
        this.cdr.detectChanges();
      });
    }
  }
  // Quản lý ảnh: Thay thế
onReplaceImage(event: any, imageId: number) {
    const file = event.target.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('imageId', imageId.toString());
    formData.append('newFile', file);

    this.detailService.replaceImage(formData).subscribe({
      next: (res: any) => {
        if (res.success) {
          // Tìm và cập nhật lại ảnh trong mảng local để UI đổi ngay
          const imgIndex = this.editingData.images.findIndex((i: any) => i.productImage_Id === imageId);
          if (imgIndex > -1) {
            // Thêm timestamp để ép trình duyệt load lại ảnh mới 
            this.editingData.images[imgIndex].imagePath = res.newUrl.split('/').pop() + '?t=' + new Date().getTime();
          }
          alert('Thay thế ảnh thành công!');
          this.cdr.detectChanges();
        }
      }
    });
  }

onAddImage(event: any) {
    const files = event.target.files;
    if (files.length > 0) {
      const formData = new FormData();
      formData.append('ProductDetail_Id', this.detailId.toString());
      Array.from(files).forEach((f: any) => formData.append('ImageFiles', f));

      this.detailService.updateImages(formData).subscribe({
        next: (res: any) => {
          // Sau khi thêm, tải lại toàn bộ dữ liệu để lấy danh sách ảnh mới từ Server
          this.loadInitialData(); 
          alert('Thêm ảnh thành công!');
        },
        error: (err) => alert(err.error?.message || 'Lỗi thêm ảnh')
      });
    }
  }

  onUpdateInfo() {
    this.detailService.update(this.detailId, this.editingData).subscribe({
      next: (res: any) => {
        // Khi Backend trả về success: true
        alert('Cập nhật thành công!');
        // Điều hướng về trang chi tiết sản phẩm dựa trên product_Id
        // Đảm bảo editingData.product_Id tồn tại (lấy từ GetById)
        this.router.navigate(['/product-detail', this.editingData.product_Id]);
      },
      error: (err: any) => {
        // Hiển thị lỗi nếu Backend trả về lỗi (404, 500, 405...)
        console.error('Lỗi khi cập nhật:', err);
        alert('Lỗi: ' + (err.error?.message || 'Không thể cập nhật cấu hình'));
      }
    });
  }
}