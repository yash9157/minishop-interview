import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationService } from 'primeng/api';
import { AccessApiService } from '../../core/access-api.service';
import { Permission, Role } from '../../models';

@Component({ selector: 'app-roles', imports: [ReactiveFormsModule], templateUrl: './roles.page.html' })
export class RolesPage implements OnInit {
  private readonly api = inject(AccessApiService);
  private readonly confirmation = inject(ConfirmationService);
  readonly roles = signal<Role[]>([]);
  readonly permissions = signal<Permission[]>([]);
  readonly roleTotal = signal(0);
  readonly permissionTotal = signal(0);
  rolePage = 1;
  permissionPage = 1;
  readonly pageSize = 10;
  readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: Validators.required }),
    isRequestable: new FormControl(true, { nonNullable: true }),
  });
  readonly permissionForm = new FormGroup({
    code: new FormControl('', { nonNullable: true, validators: Validators.required }),
    name: new FormControl('', { nonNullable: true, validators: Validators.required }),
  });
  readonly roleEditForm = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: Validators.required }),
    isRequestable: new FormControl(true, { nonNullable: true }),
  });
  readonly permissionEditForm = new FormGroup({
    code: new FormControl('', { nonNullable: true, validators: Validators.required }),
    name: new FormControl('', { nonNullable: true, validators: Validators.required }),
  });
  readonly editingRoleId = signal<string | null>(null);
  readonly editingPermissionId = signal<number | null>(null);
  selected: Record<string, number[]> = {};
  ngOnInit(): void { this.load(); }
  load(): void {
    this.api.roles(this.rolePage, this.pageSize).subscribe((result) => {
      this.roles.set(result.items);
      this.roleTotal.set(result.totalCount);
      for (const role of result.items) this.selected[role.id] = [...role.permissionIds];
    });
    this.api.permissions(this.permissionPage, this.pageSize).subscribe((result) => {
      this.permissions.set(result.items);
      this.permissionTotal.set(result.totalCount);
    });
  }
  create(): void {
    if (this.form.invalid) return;
    this.api.createRole(this.form.getRawValue()).subscribe(() => { this.form.reset({ isRequestable: true }); this.load(); });
  }
  toggle(roleId: string, permissionId: number, checked: boolean): void {
    const ids = this.selected[roleId] ?? [];
    this.selected[roleId] = checked ? [...new Set([...ids, permissionId])] : ids.filter((x) => x !== permissionId);
  }
  save(role: Role): void {
    this.api.setRolePermissions(role.id, this.selected[role.id] ?? []).subscribe(() => this.load());
  }
  edit(role: Role): void {
    this.editingRoleId.set(role.id);
    this.roleEditForm.setValue({ name: role.name, isRequestable: role.isRequestable });
  }
  saveRoleEdit(): void {
    const id = this.editingRoleId();
    if (!id || this.roleEditForm.invalid) return;
    this.api.updateRole(id, this.roleEditForm.getRawValue()).subscribe(() => {
      this.editingRoleId.set(null);
      this.load();
    });
  }
  remove(role: Role): void {
    this.confirmation.confirm({
      message: `Delete ${role.name}?`, header: 'Confirm role deletion',
      accept: () => this.api.deleteRole(role.id).subscribe(() => this.load()),
    });
  }
  createPermission(): void {
    if (this.permissionForm.invalid) return;
    this.api.createPermission(this.permissionForm.getRawValue()).subscribe(() => {
      this.permissionForm.reset();
      this.load();
    });
  }
  editPermission(permission: Permission): void {
    this.editingPermissionId.set(permission.id);
    this.permissionEditForm.setValue({ code: permission.code, name: permission.name });
  }
  savePermissionEdit(): void {
    const id = this.editingPermissionId();
    if (!id || this.permissionEditForm.invalid) return;
    this.api.updatePermission(id, this.permissionEditForm.getRawValue()).subscribe(() => {
      this.editingPermissionId.set(null);
      this.load();
    });
  }
  removePermission(permission: Permission): void {
    this.confirmation.confirm({
      message: `Delete ${permission.code}?`, header: 'Confirm permission deletion',
      accept: () => this.api.deletePermission(permission.id).subscribe(() => this.load()),
    });
  }
  changeRolePage(value: number): void {
    this.rolePage = value;
    this.load();
  }
  changePermissionPage(value: number): void {
    this.permissionPage = value;
    this.load();
  }
}
