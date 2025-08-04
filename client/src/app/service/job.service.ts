import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class JobService {
  constructor(private http: HttpClient) { }

  addJob(data: any, token: string): Observable<any> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    // return this.http.post(`${environment.apiUrl}/addJob`, data, { headers });
    return this.http.post(`${environment.apiUrl}/Jobs`, data, { headers });

  }

  getJobSummaries(employerId: string, token: string, page: number = 1) {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    const params = { page: page.toString() };

    // return this.http.get<any>(
    //   `${environment.apiUrl}/employer/${employerId}/jobs-summary`,
    //   { headers, params }
    // );
    return this.http.get<any>(
      `${environment.apiUrl}/Jobs/summary/${employerId}`,
      { headers, params }
    );
  }

  getJobById(
    jobId: string,
    token: string
  ): Observable<{ job: any; applicants: any[] }> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    // return this.http.get<{ job: any; applicants: any[] }>(
    //   `${environment.apiUrl}/getJobById/${jobId}`,
    //   { headers }
    // );
    return this.http.get<{ job: any; applicants: any[] }>(
      `${environment.apiUrl}/Jobs/${jobId}`,
      { headers }
    );
  }

  getJobs(employerId: string, token: string, page: number = 1): Observable<any> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    const params = { page: page.toString() };

    // return this.http.get<any>(`${environment.apiUrl}/getJobs/${employerId}`, {
    //   headers,
    //   params,
    // });
    return this.http.get<any>(`${environment.apiUrl}/Jobs/employer/${employerId}`, {
      headers,
      params,
    });
  }

  updateJob(jobId: string, payload: any, token: string): Observable<any> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    // return this.http.put(`${environment.apiUrl}/updateJob/${jobId}`, payload, {
    //   headers,
    // });
    return this.http.put(`${environment.apiUrl}/Jobs/${jobId}`, payload, {
      headers,
    });
  }

  deleteJob(jobId: string, token: string): Observable<any> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    // return this.http.delete(`${environment.apiUrl}/deleteJob/${jobId}`, {
    //   headers,
    // });
    return this.http.delete(`${environment.apiUrl}/Jobs/${jobId}`, {
      headers,
    });
  }

  appliedJobs(userId: string, page: number = 1, limit: number = 5): Observable<any> {
   // return this.http.get(`${environment.apiUrl}/appliedJobs/${userId}?page=${page}&limit=${limit}`);
    return this.http.get(`${environment.apiUrl}/Applications/user/${userId}?page=${page}&limit=${limit}`);

  }

  searchJobs(params: Record<string, any> = {}) {
    let httpParams = new HttpParams();
    Object.keys(params).forEach((key) => {
      const value = params[key];
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, value);
      }
    });

    // return this.http.get(`${environment.apiUrl}/searchJobs`, {
    //   params: httpParams,
    // });
    return this.http.get(`${environment.apiUrl}/Jobs/searchJobs`, {
      params: httpParams,
    });
  }
}
