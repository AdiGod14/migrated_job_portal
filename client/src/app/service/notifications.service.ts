import { Injectable, Injector } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class NotificationsService {
  constructor(private http: HttpClient) { }
  private testapiUrl = 'http://localhost:5086/api';

  getNotifications(userId: string, token: string, page: number = 1) {
    // return this.http.get(`${environment.apiUrl}/notifications/${userId}?page=${page}`, {
    //   headers: { Authorization: `Bearer ${token}` },
    // });
    return this.http.get(`${this.testapiUrl}/Notifications/${userId}?page=${page}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
  }

  clearNotifications(userId: string, token: string): Observable<any> {
    // return this.http.delete(`${environment.apiUrl}/notifications/clear/${userId}`, {
    //   headers: new HttpHeaders({ Authorization: `Bearer ${token}` }),
    // });
    return this.http.delete(`${this.testapiUrl}/Notifications/${userId}`, {
      headers: new HttpHeaders({ Authorization: `Bearer ${token}` }),
    });
  }
}
