import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category } from '../../models/category.model';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private apiUrl = 'https://localhost:7168/api/category'; // Khớp port 7168 Duy đang dùng 

  constructor(private http: HttpClient) {}

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(this.apiUrl, { withCredentials: true });
  }

  createCategory(category: Category): Observable<any> {
    return this.http.post(this.apiUrl, category, { withCredentials: true });
  }

  updateCategory(id: number, category: Category): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, category, { withCredentials: true });
  }

  deleteCategory(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, { withCredentials: true });
  }
}