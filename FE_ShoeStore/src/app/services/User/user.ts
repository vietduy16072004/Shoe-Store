import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private apiUrl = 'https://localhost:7168/api/User'; //

  constructor(private http: HttpClient) {}

  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/get-all`, { withCredentials: true });
  }

  createUser(model: User): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, model, { withCredentials: true });
  }

  editUser(model: User): Observable<any> {
    return this.http.post(`${this.apiUrl}/edit`, model, { withCredentials: true });
  }

  deleteUser(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/delete/${id}`, {}, { withCredentials: true });
  }
}