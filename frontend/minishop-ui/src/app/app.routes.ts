import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './core/auth.guards';
export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.page').then((page) => page.LoginPage),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.page').then((page) => page.RegisterPage),
  },
  {
    path: 'catalog',
    loadComponent: () => import('./features/catalog/catalog.page').then((page) => page.CatalogPage),
    canActivate: [authGuard],
  },
  {
    path: 'admin',
    loadComponent: () => import('./features/dashboard/dashboard.page').then((page) => page.DashboardPage),
    canActivate: [authGuard, adminGuard],
  },
  {
    path: 'admin/categories',
    loadComponent: () => import('./features/categories/categories.page').then((page) => page.CategoriesPage),
    canActivate: [authGuard, adminGuard],
  },
  {
    path: 'admin/products',
    loadComponent: () => import('./features/products/products.page').then((page) => page.ProductsPage),
    canActivate: [authGuard, adminGuard],
  },
  {
    path: 'admin/customers',
    loadComponent: () => import('./features/customers/customers.page').then((page) => page.CustomersPage),
    canActivate: [authGuard, adminGuard],
  },
  {
    path: 'admin/orders',
    loadComponent: () => import('./features/orders/order-list/orders.page').then((page) => page.OrdersPage),
    canActivate: [authGuard, adminGuard],
  },
  {
    path: 'admin/orders/new',
    loadComponent: () => import('./features/orders/order-edit/order-edit.page').then((page) => page.OrderEditPage),
    canActivate: [authGuard, adminGuard],
  },
  {
    path: 'admin/orders/:id',
    loadComponent: () => import('./features/orders/order-edit/order-edit.page').then((page) => page.OrderEditPage),
    canActivate: [authGuard, adminGuard],
  },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' },
];
