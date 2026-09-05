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

  save(
    value: Pick<
      Product,
      'categoryId' | 'sku' | 'name' | 'price' | 'stockQuantity' | 'isActive'
    >,
    id?: number
  ) {
    return id
      ? this.http.put<Product>(`${this.url}/${id}`, value)
      : this.http.post<Product>(this.url, value);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  uploadImage(productId: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<void>(`${this.url}/${productId}/image`, formData);
  }

  getImage(productId: number) {
    return this.http.get(`${this.url}/${productId}/image`, { responseType: 'blob' });
  }

  deleteImage(productId: number) {
    return this.http.delete<void>(`${this.url}/${productId}/image`);
  }
}
