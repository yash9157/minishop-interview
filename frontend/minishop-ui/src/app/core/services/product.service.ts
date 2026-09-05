import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { PagedResult, Product } from '../../models';
import { API_BASE_URL } from '../api.constants';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_BASE_URL}/products`;

  getPage(page = 1, pageSize = 10, search = '', isActive?: boolean) {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .set('search', search);
    if (isActive !== undefined) params = params.set('isActive', isActive);
    return this.http.get<PagedResult<Product>>(this.url, { params });
  }

  save(value: Omit<Product, 'id' | 'categoryName'>, id?: number) {
    return id
      ? this.http.put<Product>(`${this.url}/${id}`, value)
      : this.http.post<Product>(this.url, value);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
