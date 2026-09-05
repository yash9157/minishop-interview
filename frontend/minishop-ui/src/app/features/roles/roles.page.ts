import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccessApiService } from '../../core/access-api.service';
import { Permission, Role } from '../../models';

@Component({ selector: 'app-roles', imports: [ReactiveFormsModule], templateUrl: './roles.page.html' })
export class RolesPage implements OnInit {
  private readonly api = inject(AccessApiService);
  readonly roles = signal<Role[]>([]);
  readonly permissions = signal<Permission[]>([]);
  readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: Validators.required }),
    isRequestable: new FormControl(true, { nonNullable: true }),
  });
  readonly permissionForm = new FormGroup({
    code: new FormControl('', { nonNullable: true, validators: Validators.required }),
    name: new FormControl('', { nonNullable: true, validators: Validators.required }),
  });
  selected: Record<string, number[]> = {};
  ngOnInit(): void { this.load(); }
  load(): void {
    this.api.roles().subscribe((roles) => {
      this.roles.set(roles);
      for (const role of roles) this.selected[role.id] = [...role.permissionIds];
    });
    this.api.permissions().subscribe((x) => this.permissions.set(x));
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
    const name = prompt('Role name', role.name)?.trim();
    if (name) this.api.updateRole(role.id, { name, isRequestable: role.isRequestable })
      .subscribe(() => this.load());
  }
  remove(role: Role): void {
    if (confirm(`Delete ${role.name}?`))
      this.api.deleteRole(role.id).subscribe(() => this.load());
  }
  createPermission(): void {
    if (this.permissionForm.invalid) return;
    this.api.createPermission(this.permissionForm.getRawValue()).subscribe(() => {
      this.permissionForm.reset();
      this.load();
    });
  }
  editPermission(permission: Permission): void {
    const name = prompt('Permission name', permission.name)?.trim();
    if (name) this.api.updatePermission(permission.id, { code: permission.code, name })
      .subscribe(() => this.load());
  }
  removePermission(permission: Permission): void {
    if (confirm(`Delete ${permission.code}?`))
      this.api.deletePermission(permission.id).subscribe(() => this.load());
  }
}
