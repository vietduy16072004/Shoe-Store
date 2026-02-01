import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../services/Product/product';
import { Product } from '../../models/product.model';
import { Brand } from '../../models/brand.model';
import { Category } from '../../models/category.model';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css'
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  brandList: Brand[] = [];       // Dữ liệu động từ Backend
  categoryList: Category[] = []; // Dữ liệu động từ Backend
  currentPage: number = 1;
  totalPages: number = 1;
  pages: number[] = [];

  loading: boolean = true;
  isModalOpen: boolean = false;
  currentProduct: any = {}; // Đối tượng dùng để ràng buộc với Form
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  totalCount: number = 0;

  constructor(private productService: ProductService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadInitialData();
  }

  // Tải dữ liệu ban đầu cho các Combobox và danh sách sản phẩm
  loadInitialData(): void {
    this.productService.getBrands().subscribe(res => {
      this.brandList = res;
      this.cdr.detectChanges();
    });
    this.productService.getCategories().subscribe(res => {
      this.categoryList = res;
      this.cdr.detectChanges();
    });
    this.loadProducts();
  }

  loadProducts(page: number = 1): void {
    this.currentPage = page;
    this.loading = true;

    this.productService.getProducts(page).subscribe({
      next: (res) => {
        if (res?.success) {
          this.products = res.data;
          // Lưu thông tin phân trang và tổng số lượng sản phẩm
          this.totalPages = res.pagination.totalPages;
          this.totalCount = res.pagination.totalCount;
          this.generatePageArray();
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

  // Tạo mảng số trang để hiển thị ở HTML
  generatePageArray(): void {
    this.pages = Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  // Hàm xử lý khi nhấn chuyển trang
  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.loadProducts(page);
    }
  }

  // Hàm mở Modal xử lý cả Thêm và Sửa
  openModal(item: Product | null = null) {
    if (item) {
      // CHẾ ĐỘ SỬA: Ép kiểu dữ liệu về Number để Combobox nhận diện được giá trị
      this.currentProduct = {
        ...item,
        brand_Id: Number(item.brand_Id),
        category_Id: Number(item.category_Id),
        status: Number(item.status)
      };
      this.imagePreview = 'https://localhost:7168' + item.imageUrl;
    } else {
      // CHẾ ĐỘ THÊM: Khởi tạo giá trị mặc định
      this.currentProduct = {
        product_Id: 0,
        product_Name: '',
        price: 0,
        discount: 0,
        description: '',
        status: 0, // 0: Selling
        category_Id: this.categoryList.length > 0 ? this.categoryList[0].category_Id : 0,
        brand_Id: this.brandList.length > 0 ? this.brandList[0].brand_Id : 0
      };
      this.imagePreview = null;
    }
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  saveProduct() {
    const formData = new FormData();
    // Đảm bảo tên thuộc tính (Key) khớp chính xác với ProductViewModel.cs ở Backend
    formData.append('Product_Name', this.currentProduct.product_Name);
    formData.append('Price', this.currentProduct.price.toString());
    formData.append('Discount', this.currentProduct.discount.toString());
    formData.append('Description', this.currentProduct.description || '');
    formData.append('Status', this.currentProduct.status.toString());
    formData.append('Category_Id', this.currentProduct.category_Id.toString());
    formData.append('Brand_Id', this.currentProduct.brand_Id.toString());

    if (this.selectedFile) {
      formData.append('ImageFile', this.selectedFile);
    }

    const action = this.currentProduct.product_Id > 0
      ? this.productService.updateProduct(this.currentProduct.product_Id, formData)
      : this.productService.createProduct(formData);

    action.subscribe({
      next: () => {
        this.isModalOpen = false;
        this.selectedFile = null; // Reset file sau khi lưu thành công
        this.loadProducts();
      },
      error: (err) => console.error('Lỗi khi lưu sản phẩm:', err)
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result as string;
        this.cdr.detectChanges();
      };
      reader.readAsDataURL(file);
    }
  }

  onDelete(id: number) {
    if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này?')) {
      this.productService.deleteProduct(id).subscribe({
        next: (res: any) => {
          // Khi xóa thành công (status 200)
          if (res.success) {
            alert(res.message || 'Xóa thành công');
            this.loadProducts();
          }
        },
        error: (err) => {
          // Xử lý khi Backend trả về lỗi (ví dụ status 400 do vẫn còn ProductDetails)
          // err.error.message chính là câu thông báo chúng ta viết ở Backend
          const errorMessage = err.error?.message || 'Có lỗi xảy ra khi xóa sản phẩm';
          alert(errorMessage);
          console.error('Lỗi xóa sản phẩm:', err);
        }
      });
    }
  }

  closeModal() {
    this.isModalOpen = false;
    this.selectedFile = null;
    this.imagePreview = null;
  }
}