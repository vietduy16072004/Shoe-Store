import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DiscountEventList, DiscountEventForm, DiscountEventDetail} from '../../models/discount-event.model';

@Injectable({ providedIn: 'root' })
export class DiscountEventService {
  private apiUrl = 'https://localhost:7168/api/DiscountEvents';

  constructor(private http: HttpClient) {}

  getEvents(): Observable<DiscountEventList[]> {
    return this.http.get<DiscountEventList[]>(this.apiUrl, { withCredentials: true });
  }

  getProductsLookup(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/products-lookup`, { withCredentials: true });
  }

  createEvent(event: DiscountEventForm): Observable<any> {
    return this.http.post(this.apiUrl, event, { withCredentials: true });
  }

  // Thêm vào trong class DiscountEventService
  updateEvent(id: number, event: DiscountEventForm): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, event, { withCredentials: true });
  }

  deleteEvent(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, { withCredentials: true });
  }

  getEventDetail(id: number): Observable<DiscountEventDetail> {
    return this.http.get<DiscountEventDetail>(`${this.apiUrl}/${id}`, { withCredentials: true });
  }
}