import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccessApiService } from '../../core/access-api.service';
import { Role, User } from '../../models';

@Component({
  selector: 'app-users',
  imports: [ReactiveFormsModule],
  templateUrl: './users.page.html',
})
export class UsersPage implements OnInit {
  private readonly api = inject(AccessApiService);
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
  showPermissions(user: User): void {
    this.api.effectivePermissions(user.id).subscribe((x) => this.permissions.set(x));
  }
  remove(user: User): void {
    if (confirm(`Deactivate ${user.fullName}?`))
      this.api.deleteUser(user.id).subscribe(() => this.load());
  }
  edit(user: User): void {
    const fullName = prompt('Full name', user.fullName)?.trim();
    if (!fullName) return;
    this.api.updateUser(user.id, {
      fullName, managerId: user.managerId, isActive: user.isActive,
    }).subscribe(() => this.load());
  }
}
