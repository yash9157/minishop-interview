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
  readonly roles = signal<Role[]>([]);
  readonly permissions = signal<string[]>([]);
  readonly error = signal('');
  readonly form = new FormGroup({
    fullName: new FormControl('', { nonNullable: true, validators: Validators.required }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(8)] }),
    managerId: new FormControl<string | null>(null),
  });
  readonly editForm = new FormGroup({
    fullName: new FormControl('', { nonNullable: true, validators: Validators.required }),
    managerId: new FormControl<string | null>(null),
    isActive: new FormControl(true, { nonNullable: true }),
  });
  readonly editingId = signal<string | null>(null);
  selectedRoles: Record<string, string> = {};

  ngOnInit(): void { this.load(); }
  load(): void {
    this.api.users().subscribe((x) => this.users.set(x.items));
    this.api.roles().subscribe((x) => this.roles.set(x));
  }
  create(): void {
    if (this.form.invalid) return;
    this.api.createUser(this.form.getRawValue()).subscribe({
      next: () => { this.form.reset(); this.load(); },
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
    this.api.effectivePermissions(user.id).subscribe((x) => this.permissions.set(x));
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
}
