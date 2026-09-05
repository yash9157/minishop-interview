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
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    ButtonModule,
    InputTextModule,
    InputPasswordModule,
    SelectModule,
  ],
  templateUrl: './register.page.html',
})
export class RegisterPage implements OnInit {
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
    fullName: new FormControl('', { nonNullable: true, validators: Validators.required }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(8)],
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
    this.authService.register(this.form.getRawValue()).subscribe({
      next: () => void this.router.navigateByUrl('/catalog'),
      error: (error) => {
        this.error.set(error.error?.detail ?? 'Registration failed.');
        this.loading.set(false);
      },
    });
  }
}
