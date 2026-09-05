import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputPasswordModule } from 'primeng/inputpassword';
import { AuthService } from '../../../core/auth.service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, ButtonModule, InputTextModule, InputPasswordModule],
  templateUrl: './register.page.html',
})
export class RegisterPage {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal('');
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
  });

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
