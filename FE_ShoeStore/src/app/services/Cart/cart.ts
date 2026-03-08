import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CartItem } from '../../models/cart.model';

@Injectable({ providedIn: 'root' })
export class CartService {
  private apiUrl = 'https://localhost:7168/api/Cart';

  constructor(private http: HttpClient) {}

  getCart(): Observable<CartItem[]> {
    return this.http.get<CartItem[]>(this.apiUrl, { withCredentials: true });
  }

  addToCart(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/add`, data, { withCredentials: true });
  }

  updateQuantity(cartId: number, quantity: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/update-quantity`, { cartId, quantity }, { withCredentials: true });
  }

  deleteItem(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, { withCredentials: true });
  }
  
  updateOptions(data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/update-options`, data, { withCredentials: true });
  }
}