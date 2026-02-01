import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Account } from '../../models/account.model';
import { SocialLoginRequest } from '../../models/SocialLoginRequest.model';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private apiUrl = 'https://localhost:7168/api/Account'; //

  constructor(private http: HttpClient) {}

  login(model: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, model, { withCredentials: true });
  }

  loginGoogle(model: SocialLoginRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/login-google`, model, { withCredentials: true });
  }

  register(model: Account): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, model, { withCredentials: true });
  }

  getProfile(): Observable<Account> {
    return this.http.get<Account>(`${this.apiUrl}/profile`, { withCredentials: true });
  }

  editProfile(model: Account): Observable<any> {
    return this.http.post(`${this.apiUrl}/edit-profile`, model, { withCredentials: true });
  }

  updatePassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/update-password`, data, { withCredentials: true });
  }

  logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/logout`, {}, { withCredentials: true });
  }
}