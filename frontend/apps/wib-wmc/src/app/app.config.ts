import { Routes } from '@angular/router';

export const appRoutes: Routes = [
  { path: 'labeling', loadComponent: () => import('./labeling-queue/labeling-queue.component').then(m => m.LabelingQueueComponent) },
  { path: 'price-trends', loadComponent: () => import('./price-trends/price-trends.component').then(m => m.PriceTrendsComponent) },
  { path: 'spending-dashboard', loadComponent: () => import('./spending-dashboard/spending-dashboard.component').then(m => m.SpendingDashboardComponent) },
  { path: '', redirectTo: 'labeling', pathMatch: 'full' }
];
