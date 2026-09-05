import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { OrderDetails, OrderStatus, OrderSummary, PagedResult, SaveOrder } from '../../models';
import { API_BASE_URL } from '../api.constants';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_BASE_URL}/orders`;

  getPage(page = 1, pageSize = 10, search = '', status?: OrderStatus) {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .set('search', search);
    if (status) params = params.set('status', status);
    return this.http.get<PagedResult<OrderSummary>>(this.url, { params });
  }

  get(id: number) {
    return this.http.get<OrderDetails>(`${this.url}/${id}`);
  }

  save(value: SaveOrder, id?: number) {
    return id
      ? this.http.put<OrderDetails>(`${this.url}/${id}`, value)
      : this.http.post<OrderDetails>(this.url, value);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
