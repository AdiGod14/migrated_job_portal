import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class EmployerService {
  constructor(private http: HttpClient) {}
  private testapiUrl = 'http://localhost:5086/api';


  getEmployerData(employerId: string, token: string): Observable<any> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    // return this.http.get<any>(
    //   `${environment.apiUrl}/getEmployerData/${employerId}`,
    //   { headers }
    // );

      return this.http.get<any>(
      `${this.testapiUrl}/UsersAndEmployers/getEmployer/${employerId}`,
      { headers }
    );
  }

  updateEmployer(
    employerId: string,
    data: any,
    token: string
  ): Observable<any> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    // return this.http.put(
    //   `${environment.apiUrl}/updateEmployer/${employerId}`,
    //   data,
    //   { headers }
    // );
        return this.http.patch(
      `${this.testapiUrl}/UsersAndEmployers/updateEmployer/${employerId}`,
      data,
      { headers }
    );
  }

  deleteEmployer(employerId: string, token: string): Observable<any> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    // return this.http.delete(
    //   `${environment.apiUrl}/deleteEmployer/${employerId}`,
    //   { headers }
    // );
        return this.http.delete(
      `${this.testapiUrl}/deleteEmployer/${employerId}`,
      { headers }
    );
  }

    getApplicationStatusSummary(employerId: string, token: string) {
        return this.http.get<any>(`${this.testapiUrl}/Analytics/employer/status/${employerId}`, {
        headers: { Authorization: `Bearer ${token}` }
  });
}


}