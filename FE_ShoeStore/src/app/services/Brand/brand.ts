import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Brand } from '../../models/brand.model';

@Injectable({ providedIn: 'root' })
export class BrandService {
  // Sử dụng đúng port 7168 từ hình ảnh API của bạn
  private apiUrl = 'https://localhost:7168/api/brand'; 

  constructor(private http: HttpClient) {}

  getBrands(): Observable<Brand[]> {
    // Thêm withCredentials để đảm bảo quyền truy cập nếu cần
    return this.http.get<Brand[]>(this.apiUrl, { withCredentials: true });
  }

  // Các phương thức khác giữ nguyên, gọi trực tiếp vào apiUrl theo BrandController
  createBrand(brand: Brand): Observable<any> {
    return this.http.post(this.apiUrl, brand, { withCredentials: true });
  }

  updateBrand(id: number, brand: Brand): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, brand, { withCredentials: true });
  }

  deleteBrand(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, { withCredentials: true });
  }
}