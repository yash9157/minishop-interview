import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { AccessRequest, Dashboard, PagedResult, Permission, Role, TargetSystem, User } from '../models';
import { API_BASE_URL } from './api.constants';

@Injectable({ providedIn: 'root' })
export class AccessApiService {
  private readonly http = inject(HttpClient);

  dashboard() {
    return this.http.get<Dashboard>(`${API_BASE_URL}/dashboard`);
  }
  users() {
    return this.http.get<PagedResult<User>>(`${API_BASE_URL}/users?page=1&pageSize=100`);
  }
  createUser(value: { fullName: string; email: string; password: string; managerId?: string | null }) {
    return this.http.post<User>(`${API_BASE_URL}/users`, value, {
      headers: { 'Idempotency-Key': crypto.randomUUID() },
    });
  }
  updateUser(id: string, value: { fullName: string; managerId?: string | null; isActive: boolean }) {
    return this.http.put<User>(`${API_BASE_URL}/users/${id}`, value);
  }
  deleteUser(id: string) {
    return this.http.delete<void>(`${API_BASE_URL}/users/${id}`);
  }
  assignRole(userId: string, roleId: string) {
    return this.http.post<void>(`${API_BASE_URL}/users/${userId}/roles`, { roleId });
  }
  removeRole(userId: string, roleId: string) {
    return this.http.delete<void>(`${API_BASE_URL}/users/${userId}/roles/${roleId}`);
  }
  effectivePermissions(userId: string) {
    return this.http.get<string[]>(`${API_BASE_URL}/users/${userId}/permissions`);
  }
  roles() {
    return this.http.get<Role[]>(`${API_BASE_URL}/roles`);
  }
  createRole(value: { name: string; isRequestable: boolean }) {
    return this.http.post<Role>(`${API_BASE_URL}/roles`, value);
  }
  updateRole(id: string, value: { name: string; isRequestable: boolean }) {
    return this.http.put<Role>(`${API_BASE_URL}/roles/${id}`, value);
  }
  deleteRole(id: string) {
    return this.http.delete<void>(`${API_BASE_URL}/roles/${id}`);
  }
  setRolePermissions(roleId: string, permissionIds: number[]) {
    return this.http.put<void>(`${API_BASE_URL}/roles/${roleId}/permissions`, { permissionIds });
  }
  permissions() {
    return this.http.get<Permission[]>(`${API_BASE_URL}/permissions`);
  }
  createPermission(value: { code: string; name: string }) {
    return this.http.post<Permission>(`${API_BASE_URL}/permissions`, value);
  }
  updatePermission(id: number, value: { code: string; name: string }) {
    return this.http.put<Permission>(`${API_BASE_URL}/permissions/${id}`, value);
  }
  deletePermission(id: number) {
    return this.http.delete<void>(`${API_BASE_URL}/permissions/${id}`);
  }
  systems() {
    return this.http.get<TargetSystem[]>(`${API_BASE_URL}/target-systems`);
  }
  myRequests() {
    return this.http.get<PagedResult<AccessRequest>>(
      `${API_BASE_URL}/access-requests/mine?page=1&pageSize=100`);
  }
  pendingApprovals() {
    return this.http.get<PagedResult<AccessRequest>>(
      `${API_BASE_URL}/access-requests/pending-approvals?page=1&pageSize=100`);
  }
  requests(status?: string) {
    const params = status ? new HttpParams().set('status', status) : undefined;
    const paging = (params ?? new HttpParams()).set('page', 1).set('pageSize', 100);
    return this.http.get<PagedResult<AccessRequest>>(`${API_BASE_URL}/access-requests`, {
      params: paging,
    });
  }
  auditLogs(page = 1, pageSize = 20) {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<import('../models').AuditLog>>(
      `${API_BASE_URL}/audit-logs`, { params });
  }
  createRequest(value: { targetSystemId: number; requestedRoleId: string; businessJustification: string }) {
    return this.http.post<AccessRequest>(`${API_BASE_URL}/access-requests`, value);
  }
  submitRequest(id: number) {
    return this.http.post<AccessRequest>(`${API_BASE_URL}/access-requests/${id}/submit`, {});
  }
  decide(id: number, action: 'approve' | 'reject', remarks: string) {
    return this.http.post<AccessRequest>(
      `${API_BASE_URL}/access-requests/${id}/${action}`, { remarks });
  }
  provision(id: number) {
    return this.http.post<AccessRequest>(`${API_BASE_URL}/access-requests/${id}/provision`, {});
  }
}
