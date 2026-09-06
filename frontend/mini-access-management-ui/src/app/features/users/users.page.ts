import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { AccessApiService } from '../../core/access-api.service';
import { Role, User } from '../../models';

@Component({
  selector: 'app-users',
  imports: [ReactiveFormsModule, DialogModule],
  templateUrl: './users.page.html',
})
export class UsersPage implements OnInit {
  private readonly api = inject(AccessApiService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly messages = inject(MessageService);
  readonly users = signal<User[]>([]);
  readonly managerOptions = signal<User[]>([]);
  readonly roles = signal<Role[]>([]);
  readonly permissions = signal<string[]>([]);
  readonly permissionsFor = signal('');
  readonly totalCount = signal(0);
  readonly createOpen = signal(false);
  readonly permissionsOpen = signal(false);
  readonly saving = signal(false);
  page = 1;
  readonly pageSize = 10;
  private idempotencyKey = crypto.randomUUID();
  readonly form = new FormGroup({
    fullName: new FormControl('', { nonNullable: true, validators: Validators.required }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(8)],
    }),
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
    this.form.valueChanges.subscribe(() => (this.idempotencyKey = crypto.randomUUID()));
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
  openCreate(): void {
    this.form.reset({ fullName: '', email: '', password: '', managerId: null });
    this.createOpen.set(true);
  }
  create(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.api.createUser(this.form.getRawValue(), this.idempotencyKey).subscribe({
      next: () => {
        this.saving.set(false);
        this.createOpen.set(false);
        this.idempotencyKey = crypto.randomUUID();
        this.form.reset();
        this.load();
        this.api.users(1, 100).subscribe((x) => this.managerOptions.set(x.items));
        this.messages.add({
          severity: 'success',
          summary: 'User created',
          detail: 'The user is ready for role assignment.',
        });
      },
      error: (e) => this.showError(e, 'Unable to create user.'),
    });
  }
  assign(user: User): void {
    const roleId = this.selectedRoles[user.id];
    if (!roleId) return;
    this.api.assignRole(user.id, roleId).subscribe({
      next: () => {
        this.selectedRoles[user.id] = '';
        this.load();
        this.messages.add({
          severity: 'success',
          summary: 'Role assigned',
          detail: `${user.fullName}'s roles were updated.`,
        });
      },
      error: (e) => this.showError(e, 'Unable to assign role.'),
    });
  }
  removeRole(user: User, roleName: string): void {
    const role = this.roles().find((item) => item.name === roleName);
    if (!role) return;
    this.confirmation.confirm({
      header: 'Remove role',
      message: `Remove ${roleName} from ${user.fullName}?`,
      accept: () =>
        this.api.removeRole(user.id, role.id).subscribe({
          next: () => {
            this.load();
            this.messages.add({
              severity: 'success',
              summary: 'Role removed',
              detail: `${roleName} was removed from ${user.fullName}.`,
            });
          },
          error: (e) => this.showError(e, 'Unable to remove role.'),
        }),
    });
  }
  showPermissions(user: User): void {
    this.api.effectivePermissions(user.id).subscribe({
      next: (x) => {
        this.permissions.set(x);
        this.permissionsFor.set(user.fullName);
        this.permissionsOpen.set(true);
      },
      error: (e) => this.showError(e, 'Unable to load permissions.'),
    });
  }
  remove(user: User): void {
    this.confirmation.confirm({
      message: `Deactivate ${user.fullName}?`,
      header: 'Confirm deactivation',
      accept: () =>
        this.api.deleteUser(user.id).subscribe({
          next: () => {
            this.load();
            this.messages.add({
              severity: 'success',
              summary: 'User deactivated',
              detail: `${user.fullName} can no longer sign in.`,
            });
          },
          error: (e) => this.showError(e, 'Unable to deactivate user.'),
        }),
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
    this.saving.set(true);
    this.api.updateUser(id, this.editForm.getRawValue()).subscribe({
      next: () => {
        this.saving.set(false);
        this.editingId.set(null);
        this.load();
        this.messages.add({
          severity: 'success',
          summary: 'Changes saved',
          detail: 'The user details were updated.',
        });
      },
      error: (e) => this.showError(e, 'Unable to update user.'),
    });
  }

  closeEdit(): void {
    this.editingId.set(null);
  }

  changePage(value: number): void {
    this.page = value;
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
