import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () =>
  inject(AuthService).user() ? true : inject(Router).parseUrl('/login');

export const adminGuard: CanActivateFn = () =>
  inject(AuthService).isAdmin() ? true : inject(Router).parseUrl('/dashboard');

export const approverGuard: CanActivateFn = () =>
  inject(AuthService).canApprove() ? true : inject(Router).parseUrl('/dashboard');

export const provisionerGuard: CanActivateFn = () =>
  inject(AuthService).canProvision() ? true : inject(Router).parseUrl('/dashboard');
