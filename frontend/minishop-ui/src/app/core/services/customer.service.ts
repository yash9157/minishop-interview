import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Customer, PagedResult } from '../../models';
import { API_BASE_URL } from '../api.constants';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_BASE_URL}/customers`;

  getPage(page = 1, pageSize = 10, search = '') {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .set('search', search);
    return this.http.get<PagedResult<Customer>>(this.url, { params });
  }

  save(value: Omit<Customer, 'id'>, id?: number) {
    return id
      ? this.http.put<Customer>(`${this.url}/${id}`, value)
      : this.http.post<Customer>(this.url, value);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
