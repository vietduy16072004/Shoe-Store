import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ProductDetailService {
  private apiUrl = 'https://localhost:7168/api/ProductDetail';

  constructor(private http: HttpClient) {}

  // Lấy danh sách theo ProductId 
  getByProduct(productId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/ByProduct/${productId}`);
  }

  // Thêm mới cấu hình (Sử dụng FormData để upload ảnh) 
  create(formData: FormData): Observable<any> {
    return this.http.post(this.apiUrl, formData);
  }

  // Xóa cấu hình 
  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  //Update cấu hình (Sử dụng FormData để upload ảnh)
  // Lấy 1 cấu hình cụ thể để sửa
  getById(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  // Cập nhật thông tin (Size, Màu, SL)
  update(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, data);
  }

  // Xóa ảnh
  deleteImage(imageId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/DeleteImage/${imageId}`);
  }

  replaceImage(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/ReplaceImage`, formData);
  }

  updateImages(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/UpdateImages`, formData);
  }

  // services/product-detail.ts
  getOptions(): Observable<any> {
    return this.http.get(`${this.apiUrl}/GetOptions`);
  }
}