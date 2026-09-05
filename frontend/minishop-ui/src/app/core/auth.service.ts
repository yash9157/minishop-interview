import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { AuthResponse } from '../models';

import { API_BASE_URL } from './api.constants';

const SESSION_KEY = 'minishop-auth';
const AUTH_URL = `${API_BASE_URL}/auth`;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly state = signal<AuthResponse | null>(this.read());
  readonly user = computed(() => this.state()?.user ?? null);
  readonly token = computed(() => this.state()?.accessToken ?? null);
  readonly isAdmin = computed(() => this.user()?.roles.includes('Admin') ?? false);

  login(value: { email: string; password: string }) {
    return this.http
      .post<AuthResponse>(`${AUTH_URL}/login`, value)
      .pipe(tap((response) => this.save(response)));
  }

  register(value: { fullName: string; email: string; password: string }) {
    return this.http
      .post<AuthResponse>(`${AUTH_URL}/register`, value)
      .pipe(tap((response) => this.save(response)));
  }

  logout(): void {
    sessionStorage.removeItem(SESSION_KEY);
    this.state.set(null);
    void this.router.navigateByUrl('/login');
  }

  private save(value: AuthResponse): void {
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(value));
    this.state.set(value);
  }

  private read(): AuthResponse | null {
    try {
      const raw = sessionStorage.getItem(SESSION_KEY);
      if (!raw) return null;

      const value = JSON.parse(raw) as AuthResponse;
      if (new Date(value.expiresAtUtc) <= new Date()) {
        sessionStorage.removeItem(SESSION_KEY);
        return null;
      }

      return value;
    } catch {
      sessionStorage.removeItem(SESSION_KEY);
      return null;
    }
  }
}
