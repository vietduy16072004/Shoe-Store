import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Size } from '../../models/size.model';

@Injectable({ providedIn: 'root' })
export class SizeService {
  private apiUrl = 'https://localhost:7168/api/size'; 

  constructor(private http: HttpClient) {}

  getSizes(): Observable<Size[]> {
    return this.http.get<Size[]>(this.apiUrl, { withCredentials: true });
  }

  createSize(size: Size): Observable<any> {
    return this.http.post(this.apiUrl, size, { withCredentials: true });
  }

  updateSize(id: number, size: Size): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, size, { withCredentials: true });
  }

  deleteSize(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, { withCredentials: true });
  }
}