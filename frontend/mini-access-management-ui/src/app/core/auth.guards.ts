import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () =>
  inject(AuthService).user() ? true : inject(Router).parseUrl('/login');

export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.user() ? inject(Router).parseUrl(auth.homeUrl()) : true;
};

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.isAdmin() ? true : inject(Router).parseUrl(auth.homeUrl());
};

export const approverGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.canApprove() ? true : inject(Router).parseUrl(auth.homeUrl());
};

export const provisionerGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.canProvision() ? true : inject(Router).parseUrl(auth.homeUrl());
};
