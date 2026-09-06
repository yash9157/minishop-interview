import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { AuthResponse } from '../models';

import { API_BASE_URL } from './api.constants';

const SESSION_KEY = 'mini-access-management-auth';
const AUTH_URL = `${API_BASE_URL}/auth`;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly state = signal<AuthResponse | null>(this.read());
  readonly user = computed(() => this.state()?.user ?? null);
  readonly token = computed(() => this.state()?.accessToken ?? null);
  readonly isAdmin = computed(() => this.user()?.roles.includes('Admin') ?? false);
  readonly canApprove = computed(
    () =>
      this.user()?.roles.some((role) => role === 'Manager' || role === 'SecurityAdmin') ?? false,
  );
  readonly canProvision = computed(
    () => this.user()?.roles.some((role) => role === 'Admin' || role === 'Provisioner') ?? false,
  );

  login(value: { email: string; password: string }) {
    return this.http
      .post<AuthResponse>(`${AUTH_URL}/login`, value)
      .pipe(tap((response) => this.save(response)));
  }

  homeUrl(): string {
    if (this.isAdmin()) return '/dashboard';
    if (this.canApprove()) return '/approvals';
    if (this.canProvision()) return '/provisioning';
    return '/requests';
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
