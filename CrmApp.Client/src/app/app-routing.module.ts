import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginViewComponent } from './auth/login-view/login-view.component';
import { authGuard } from './core/guards/auth.guard';
import { GuestGuard } from './core/guards/guest.guard';
import { UsersComponent } from './components/users/users.component';
import { UserComponent } from './components/users/user/user.component';
import { ContractorsComponent } from './components/contractors/contractors.component';
import { ProjectsComponent } from './components/projects/projects.component';
import { SettingsComponent } from './components/settings/settings.component';
import { ForgotPasswordComponent } from './auth/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './auth/reset-password/reset-password.component';
import { ResetPasswordEntryGuard } from './core/guards/reset-password.guard';
import { DictionariesComponent } from './components/dictionaries/dictionaries.component';
import { CatalogItemsComponent } from './components/catalog-items/catalog-items.component';
import { CatalogItemComponent } from './components/catalog-items/catalog-item/catalog-item.component';
import { ContractorPaneComponent } from './components/contractors/contractor-pane/contractor-pane.component';
import { AdditionalFieldsConfiguratorComponent } from './components/additional-fields-configurator/additional-fields-configurator.component';
import { TasksShellComponent } from './components/tasks/tasks-shell/tasks-shell.component';
import { TasksMyComponent } from './components/tasks/tasks-my/tasks-my.component';
import { TasksAdminComponent } from './components/tasks/tasks-admin/tasks-admin.component';

const routes: Routes = [
  { path: '', redirectTo: 'tasks/my', pathMatch: 'full' },
  { path: 'login', component: LoginViewComponent, canActivate: [GuestGuard] },
  { path: 'forgot-password', component: ForgotPasswordComponent, canActivate: [ResetPasswordEntryGuard] },
  { path: 'reset-password', component: ResetPasswordComponent, canActivate: [ResetPasswordEntryGuard] },
  { path: 'contractors', component: ContractorsComponent, data: { permission: ['canContractorsView'] }, canActivate: [authGuard] },
  { path: 'contractor-pane', component: ContractorPaneComponent, data: { permission: ['canContractorsView'] }, canActivate: [authGuard] },
  { path: 'contractor-pane/:id', component: ContractorPaneComponent, data: { permission: ['canContractorsView'] }, canActivate: [authGuard] },
  { path: 'projects', component: ProjectsComponent, data: { permission: ['canProjectsView'] }, canActivate: [authGuard] },
  { path: 'settings', component: SettingsComponent, data: { permission: ['canSystemConfig'] }, canActivate: [authGuard] },
  { path: 'dictionaries', component: DictionariesComponent, data: { permission: ['canSystemConfig'] }, canActivate: [authGuard] },
  { path: 'lists-config', component: AdditionalFieldsConfiguratorComponent, data: { permission: ['canSystemConfig'] }, canActivate: [authGuard] },
  { path: 'users', component: UsersComponent, data: { permission: ['canUsersView'] }, canActivate: [authGuard] },
  { path: 'user', component: UserComponent, data: { permission: ['canUsersView'] }, canActivate: [authGuard] },
  { path: 'user/:id', component: UserComponent, data: { permission: ['canUsersView'] }, canActivate: [authGuard] },
  { path: 'my-account/:id', component: UserComponent, canActivate: [authGuard] },
  { path: 'catalog-items', component: CatalogItemsComponent, data: { permission: ['canSystemConfig'] }, canActivate: [authGuard] },
  { path: 'catalog-item', component: CatalogItemComponent, data: { permission: ['canSystemConfig'] }, canActivate: [authGuard] },
  { path: 'catalog-item/:id', component: CatalogItemComponent, data: { permission: ['canSystemConfig'] }, canActivate: [authGuard] },
  {
    path: 'tasks',
    component: TasksShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'my', pathMatch: 'full' },
      { path: 'my', component: TasksMyComponent },
      { path: 'admin', component: TasksAdminComponent },
    ],
  },
  { path: '**', redirectTo: 'tasks/my' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
