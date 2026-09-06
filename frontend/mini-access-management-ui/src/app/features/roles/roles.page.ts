import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { AccessApiService } from '../../core/access-api.service';
import { Permission, Role } from '../../models';

@Component({
  selector: 'app-roles',
  imports: [ReactiveFormsModule, DialogModule],
  templateUrl: './roles.page.html',
})
export class RolesPage implements OnInit {
  private readonly api = inject(AccessApiService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly messages = inject(MessageService);
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
  readonly createRoleOpen = signal(false);
  readonly createPermissionOpen = signal(false);
  readonly saving = signal(false);
  selected: Record<string, number[]> = {};
  ngOnInit(): void {
    this.load();
  }
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
  openCreateRole(): void {
    this.form.reset({ name: '', isRequestable: true });
    this.createRoleOpen.set(true);
  }
  openCreatePermission(): void {
    this.permissionForm.reset({ code: '', name: '' });
    this.createPermissionOpen.set(true);
  }
  create(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.api.createRole(this.form.getRawValue()).subscribe({
      next: () => {
        this.saving.set(false);
        this.createRoleOpen.set(false);
        this.load();
        this.messages.add({
          severity: 'success',
          summary: 'Role created',
          detail: 'The new role is available in the matrix.',
        });
      },
      error: (e) => this.showError(e, 'Unable to create role.'),
    });
  }
  toggle(roleId: string, permissionId: number, checked: boolean): void {
    const ids = this.selected[roleId] ?? [];
    this.selected[roleId] = checked
      ? [...new Set([...ids, permissionId])]
      : ids.filter((x) => x !== permissionId);
  }
  save(role: Role): void {
    this.api.setRolePermissions(role.id, this.selected[role.id] ?? []).subscribe({
      next: () => {
        this.load();
        this.messages.add({
          severity: 'success',
          summary: 'Permissions saved',
          detail: `${role.name} permissions were updated.`,
        });
      },
      error: (e) => this.showError(e, 'Unable to update role permissions.'),
    });
  }
  edit(role: Role): void {
    this.editingRoleId.set(role.id);
    this.roleEditForm.setValue({ name: role.name, isRequestable: role.isRequestable });
  }
  saveRoleEdit(): void {
    const id = this.editingRoleId();
    if (!id || this.roleEditForm.invalid) return;
    this.saving.set(true);
    this.api.updateRole(id, this.roleEditForm.getRawValue()).subscribe({
      next: () => {
        this.saving.set(false);
        this.editingRoleId.set(null);
        this.load();
        this.messages.add({
          severity: 'success',
          summary: 'Changes saved',
          detail: 'The role was updated.',
        });
      },
      error: (e) => this.showError(e, 'Unable to update role.'),
    });
  }
  closeRoleEdit(): void {
    this.editingRoleId.set(null);
  }
  remove(role: Role): void {
    this.confirmation.confirm({
      message: `Delete ${role.name}?`,
      header: 'Confirm role deletion',
      accept: () =>
        this.api.deleteRole(role.id).subscribe({
          next: () => {
            this.load();
            this.messages.add({
              severity: 'success',
              summary: 'Role deleted',
              detail: `${role.name} was removed.`,
            });
          },
          error: (e) => this.showError(e, 'Unable to delete role.'),
        }),
    });
  }
  createPermission(): void {
    if (this.permissionForm.invalid) return;
    this.saving.set(true);
    this.api.createPermission(this.permissionForm.getRawValue()).subscribe({
      next: () => {
        this.saving.set(false);
        this.createPermissionOpen.set(false);
        this.load();
        this.messages.add({
          severity: 'success',
          summary: 'Permission created',
          detail: 'The permission is available in the matrix.',
        });
      },
      error: (e) => this.showError(e, 'Unable to create permission.'),
    });
  }
  editPermission(permission: Permission): void {
    this.editingPermissionId.set(permission.id);
    this.permissionEditForm.setValue({ code: permission.code, name: permission.name });
  }
  savePermissionEdit(): void {
    const id = this.editingPermissionId();
    if (!id || this.permissionEditForm.invalid) return;
    this.saving.set(true);
    this.api.updatePermission(id, this.permissionEditForm.getRawValue()).subscribe({
      next: () => {
        this.saving.set(false);
        this.editingPermissionId.set(null);
        this.load();
        this.messages.add({
          severity: 'success',
          summary: 'Changes saved',
          detail: 'The permission was updated.',
        });
      },
      error: (e) => this.showError(e, 'Unable to update permission.'),
    });
  }
  closePermissionEdit(): void {
    this.editingPermissionId.set(null);
  }
  removePermission(permission: Permission): void {
    this.confirmation.confirm({
      message: `Delete ${permission.code}?`,
      header: 'Confirm permission deletion',
      accept: () =>
        this.api.deletePermission(permission.id).subscribe({
          next: () => {
            this.load();
            this.messages.add({
              severity: 'success',
              summary: 'Permission deleted',
              detail: `${permission.code} was removed.`,
            });
          },
          error: (e) => this.showError(e, 'Unable to delete permission.'),
        }),
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
  private showError(error: { error?: { detail?: string } }, fallback: string): void {
    this.saving.set(false);
    this.messages.add({
      severity: 'error',
      summary: 'Action failed',
      detail: error.error?.detail ?? fallback,
    });
  }
}
