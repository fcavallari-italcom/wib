import { Routes } from '@angular/router';

export const appRoutes: Routes = [
  { path: 'capture', loadComponent: () => import('./capture/capture.component').then(m => m.CaptureComponent) },
  { path: 'receipts', loadComponent: () => import('./receipts-list/receipts-list.component').then(m => m.ReceiptsListComponent) },
  { path: 'receipts/:id', loadComponent: () => import('./receipt-detail/receipt-detail.component').then(m => m.ReceiptDetailComponent) },
  { path: '', redirectTo: 'capture', pathMatch: 'full' }
];
