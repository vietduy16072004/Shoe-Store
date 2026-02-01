import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class HomeService {
  private apiUrl = 'https://localhost:7168/api/Home'; // URL API của HomeController

  constructor(private http: HttpClient) {}

  // Lấy dữ liệu sản phẩm kèm bộ lọc
  getHomeData(filters: any): Observable<any> {
    let params = new HttpParams();
    if (filters.search) params = params.set('search', filters.search);
    if (filters.brandId) params = params.set('brandId', filters.brandId);
    if (filters.categoryId) params = params.set('categoryId', filters.categoryId);
    if (filters.minPrice) params = params.set('minPrice', filters.minPrice);
    if (filters.maxPrice) params = params.set('maxPrice', filters.maxPrice);
    params = params.set('page', filters.page || 1);

    return this.http.get<any>(this.apiUrl, { params });
  }

  // API Tìm kiếm nhanh cho Search Modal
  searchQuick(term: string): Observable<any[]> {
    return this.http.get<any[]>(`https://localhost:7168/api/Product/Search?term=${term}`);
  }
}