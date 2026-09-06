import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationService } from 'primeng/api';
import { AccessApiService } from '../../core/access-api.service';
import { Role, User } from '../../models';

@Component({
  selector: 'app-users',
  imports: [ReactiveFormsModule],
  templateUrl: './users.page.html',
})
export class UsersPage implements OnInit {
  private readonly api = inject(AccessApiService);
  private readonly confirmation = inject(ConfirmationService);
  readonly users = signal<User[]>([]);
  readonly managerOptions = signal<User[]>([]);
  readonly roles = signal<Role[]>([]);
  readonly permissions = signal<string[]>([]);
  readonly permissionsFor = signal('');
  readonly totalCount = signal(0);
  readonly error = signal('');
  page = 1;
  readonly pageSize = 10;
  private idempotencyKey = crypto.randomUUID();
  readonly form = new FormGroup({
    fullName: new FormControl('', { nonNullable: true, validators: Validators.required }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(8)] }),
    managerId: new FormControl<string | null>(null),
  });
  readonly editForm = new FormGroup({
    fullName: new FormControl('', { nonNullable: true, validators: Validators.required }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    managerId: new FormControl<string | null>(null),
    isActive: new FormControl(true, { nonNullable: true }),
  });
  readonly editingId = signal<string | null>(null);
  selectedRoles: Record<string, string> = {};

  ngOnInit(): void {
    this.form.valueChanges.subscribe(() => this.idempotencyKey = crypto.randomUUID());
    this.load();
    this.api.users(1, 100).subscribe((x) => this.managerOptions.set(x.items));
    this.api.roles().subscribe((x) => this.roles.set(x.items));
  }
  load(): void {
    this.api.users(this.page, this.pageSize).subscribe((x) => {
      this.users.set(x.items);
      this.totalCount.set(x.totalCount);
    });
  }
  create(): void {
    if (this.form.invalid) return;
    this.api.createUser(this.form.getRawValue(), this.idempotencyKey).subscribe({
      next: () => {
        this.idempotencyKey = crypto.randomUUID();
        this.form.reset();
        this.load();
        this.api.users(1, 100).subscribe((x) => this.managerOptions.set(x.items));
      },
      error: (e) => this.error.set(e.error?.detail ?? 'Unable to create user.'),
    });
  }
  assign(user: User): void {
    const roleId = this.selectedRoles[user.id];
    if (!roleId) return;
    this.api.assignRole(user.id, roleId).subscribe({ next: () => this.load(),
      error: (e) => this.error.set(e.error?.detail ?? 'Unable to assign role.') });
  }
  removeRole(user: User, roleName: string): void {
    const role = this.roles().find((item) => item.name === roleName);
    if (!role) return;
    this.api.removeRole(user.id, role.id).subscribe({
      next: () => this.load(),
      error: (e) => this.error.set(e.error?.detail ?? 'Unable to remove role.'),
    });
  }
  showPermissions(user: User): void {
    this.api.effectivePermissions(user.id).subscribe((x) => {
      this.permissions.set(x);
      this.permissionsFor.set(user.fullName);
    });
  }
  remove(user: User): void {
    this.confirmation.confirm({
      message: `Deactivate ${user.fullName}?`,
      header: 'Confirm deactivation',
      accept: () => this.api.deleteUser(user.id).subscribe(() => this.load()),
    });
  }
  edit(user: User): void {
    this.editingId.set(user.id);
    this.editForm.setValue({
      fullName: user.fullName,
      email: user.email,
      managerId: user.managerId ?? null,
      isActive: user.isActive,
    });
  }
  saveEdit(): void {
    const id = this.editingId();
    if (!id || this.editForm.invalid) return;
    this.api.updateUser(id, this.editForm.getRawValue()).subscribe({
      next: () => { this.editingId.set(null); this.load(); },
      error: (e) => this.error.set(e.error?.detail ?? 'Unable to update user.'),
    });
  }

  changePage(value: number): void {
    this.page = value;
    this.load();
  }
}
