import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-add-to-cart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './add-to-cart.html'
})
export class AddToCartComponent implements OnInit {
  @Input() product: any;
  @Output() close = new EventEmitter();

  options: any[] = [];
  uniqueSizes: any[] = [];
  uniqueVariants: any[] = [];
  
  selectedVariantId: number | null = null;
  selectedSizeId: number | null = null;
  quantity = 1;
  maxStock = 0;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    // Tải tùy chọn kích thước/màu sắc [cite: 67]
    this.http.get<any>(`https://localhost:7168/api/Cart/GetProductOptions?productId=${this.product.product_Id}`).subscribe(res => {
      this.options = res.productDetails;
      this.uniqueVariants = [...new Map(this.options.map(o => [o.variants_Id, o])).values()];
      this.uniqueSizes = [...new Map(this.options.map(o => [o.size_Id, o])).values()];
    });
  }

  selectSize(sizeId: number) {
    this.selectedSizeId = sizeId;
    const match = this.options.find(o => o.variants_Id == this.selectedVariantId && o.size_Id == sizeId);
    this.maxStock = match ? match.stock : 0; // Kiểm tra tồn kho [cite: 96, 104]
  }

  confirmAdd() {
    if (!this.selectedSizeId) {
      Swal.fire('Chú ý', 'Vui lòng chọn Size!', 'warning');
      return; // Trả về void thay vì trả về kết quả của Swal.fire
    }

    // Logic gọi API AddToCart ở đây...
    Swal.fire('Thành công', 'Đã thêm sản phẩm vào giỏ hàng!', 'success');
    this.close.emit();
  }
}