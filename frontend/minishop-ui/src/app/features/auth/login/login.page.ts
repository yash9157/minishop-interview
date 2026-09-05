import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputPasswordModule } from 'primeng/inputpassword';
import { AuthService } from '../../../core/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, ButtonModule, InputTextModule, InputPasswordModule],
  templateUrl: './login.page.html',
})
export class LoginPage {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly form = new FormGroup({
    email: new FormControl('admin@minishop.local', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('Admin@12345', {
      nonNullable: true,
      validators: Validators.required,
    }),
  });

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
