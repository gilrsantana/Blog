import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/blog-list/blog-list.component').then(m => m.BlogListComponent)
  },
  {
    path: 'article/:slug',
    loadComponent: () => import('./components/blog-detail/blog-detail.component').then(m => m.BlogDetailComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    loadComponent: () => import('./components/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
  },
  {
    path: 'admin/new',
    canActivate: [authGuard],
    loadComponent: () => import('./components/article-editor/article-editor.component').then(m => m.ArticleEditorComponent)
  },
  {
    path: 'admin/edit/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./components/article-editor/article-editor.component').then(m => m.ArticleEditorComponent)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
