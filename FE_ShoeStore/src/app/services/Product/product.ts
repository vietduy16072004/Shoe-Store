import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from '../../models/product.model';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  // URL này phải khớp với cấu trúc trong Program.cs và appsettings.json
  private apiUrl = 'https://localhost:7168/api/Product'; 

  constructor(private http: HttpClient) { }

  // Gọi API GetProducts (Index cũ)
  getProducts(page: number = 1): Observable<any> {
    // Truyền tham số page vào URL để Backend nhận được
    return this.http.get<any>(`${this.apiUrl}?page=${page}`);
  }

  // Gọi API Search
  searchProducts(term: string): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/Search?term=${term}`);
  }

  createProduct(formData: FormData): Observable<any> {
    return this.http.post(this.apiUrl, formData);
  }

  updateProduct(id: number, formData: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, formData);
  }

  deleteProduct(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  getBrands(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/Brands`); // Gọi đúng https://localhost:7168/api/Product/Brands
  }

  getCategories(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/Categories`); // Gọi đúng https://localhost:7168/api/Product/Categories
  }

  
}
