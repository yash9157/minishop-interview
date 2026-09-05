import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Category } from '../../models';
import { API_BASE_URL } from '../api.constants';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_BASE_URL}/categories`;

  getAll() {
    return this.http.get<Category[]>(this.url);
  }

  save(value: Omit<Category, 'id'>, id?: number) {
    return id
      ? this.http.put<Category>(`${this.url}/${id}`, value)
      : this.http.post<Category>(this.url, value);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
