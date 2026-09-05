import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputPasswordModule } from 'primeng/inputpassword';
import { SelectModule } from 'primeng/select';
import { AuthService } from '../../../core/auth.service';
import { Tenant } from '../../../models';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    ButtonModule,
    InputTextModule,
    InputPasswordModule,
    SelectModule,
  ],
  templateUrl: './login.page.html',
})
export class LoginPage implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly tenants = signal<Tenant[]>([]);
  readonly form = new FormGroup({
    tenantCode: new FormControl('minishop', {
      nonNullable: true,
      validators: Validators.required,
    }),
    email: new FormControl('admin@minishop.local', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('Admin@12345', {
      nonNullable: true,
      validators: Validators.required,
    }),
  });

  ngOnInit(): void {
    this.authService.getTenants().subscribe({
      next: (tenants) => this.tenants.set(tenants),
      error: () => this.error.set('Could not load brands.'),
    });
  }

  submit(): void {
    if (this.form.invalid) return;

    this.loading.set(true);
    this.error.set('');
    this.authService.login(this.form.getRawValue()).subscribe({
      next: (response) =>
        void this.router.navigateByUrl(
          response.user.roles.includes('Admin') ? '/admin' : '/catalog',
        ),
      error: (error) => {
        this.error.set(error.error?.detail ?? 'Login failed.');
        this.loading.set(false);
      },
    });
  }
}
