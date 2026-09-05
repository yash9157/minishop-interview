import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/auth.guards';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.page').then((x) => x.LoginPage),
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.page').then((x) => x.DashboardPage),
  },
  {
    path: 'users',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/users/users.page').then((x) => x.UsersPage),
  },
  {
    path: 'roles',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/roles/roles.page').then((x) => x.RolesPage),
  },
  {
    path: 'requests',
    canActivate: [authGuard],
    loadComponent: () => import('./features/requests/requests.page').then((x) => x.RequestsPage),
  },
  {
    path: 'approvals',
    canActivate: [authGuard],
    loadComponent: () => import('./features/approvals/approvals.page').then((x) => x.ApprovalsPage),
  },
  {
    path: 'provisioning',
    canActivate: [authGuard],
    loadComponent: () => import('./features/provisioning/provisioning.page').then((x) => x.ProvisioningPage),
  },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' },
];
