import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Variant } from '../../models/variant.model';

@Injectable({ providedIn: 'root' })
export class VariantService {
  private apiUrl = 'https://localhost:7168/api/variant'; 

  constructor(private http: HttpClient) {}

  getVariants(): Observable<Variant[]> {
    return this.http.get<Variant[]>(this.apiUrl, { withCredentials: true });
  }

  createVariant(variant: Variant): Observable<any> {
    return this.http.post(this.apiUrl, variant, { withCredentials: true });
  }

  updateVariant(id: number, variant: Variant): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, variant, { withCredentials: true });
  }

  deleteVariant(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, { withCredentials: true });
  }
}