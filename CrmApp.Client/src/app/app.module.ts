import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { inject, NgModule, LOCALE_ID, provideAppInitializer } from '@angular/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { DateAdapter, MAT_DATE_FORMATS, MAT_NATIVE_DATE_FORMATS, NativeDateAdapter } from '@angular/material/core';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { LUCIDE_ICONS } from './core/icons/lucide-icons';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ReactiveFormsModule } from '@angular/forms';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormsModule } from '@angular/forms';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSelectModule } from '@angular/material/select';
import { MAT_DIALOG_DEFAULT_OPTIONS, MatDialogModule } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTreeModule } from '@angular/material/tree';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorIntl, MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatBadgeModule } from '@angular/material/badge';
import { MatSliderModule } from '@angular/material/slider';
import { PickerModule } from '@ctrl/ngx-emoji-mart';
import { DragDropModule } from '@angular/cdk/drag-drop';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { LoginViewComponent } from './auth/login-view/login-view.component';
import { HeaderComponent } from './components/theme/header/header.component';
import { FooterComponent } from './components/theme/footer/footer.component';
import { API_BASE_URL } from './api-tokens';
import { environment } from 'src/environments/environment';
import { ApiInterceptor } from './core/api.interceptor';
import { AuthService } from './core/services/auth.service';
import { GuestGuard } from './core/guards/guest.guard';
import { UsersComponent } from './components/users/users.component';
import { UserComponent } from './components/users/user/user.component';
import { DbService } from './core/services/db.service';
import { MessagePopupComponent } from './components/theme/notifications/message-popup/message-popup.component';
import { QuestionPopupComponent } from './components/theme/notifications/question-popup/question-popup.component';
import { NotificationService } from './core/services/notification.service';
import { UsersService } from './core/services/users.service';
import { ProjectsComponent } from './components/projects/projects.component';
import { ContractorsComponent } from './components/contractors/contractors.component';
import { ContractorsService } from './core/services/contractors.service';
import { PrettySelectComponent } from './components/theme/pretty-select/pretty-select.component';
import { SettingsComponent } from './components/settings/settings.component';
import { SettingsService } from './core/services/settings.service';
import { LoadingInterceptor } from './core/loading.interceptor';
import { LoadingOverlayComponent } from './components/theme/loading-overlay/loading-overlay.component';
import { LoadingService } from './core/services/loading.service';
import { ForgotPasswordComponent } from './auth/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './auth/reset-password/reset-password.component';
import { ResetPasswordEntryGuard } from './core/guards/reset-password.guard';
import { EmojiComponent } from '@ctrl/ngx-emoji-mart/ngx-emoji';
import { IconPickerComponent } from './components/theme/icon-picker/icon-picker.component';
import { DeviceService } from './core/services/device.service';
import { SafeUrlPipe } from './core/pipes/safe-url.pipe';
import { CrmTemplateDirective } from './core/crm-template.directive';
import { ContractorDialogComponent } from './components/contractors/contractor-dialog/contractor-dialog.component';
import { ProjectsService } from './core/services/projects.service';
import { CommonModule, registerLocaleData } from '@angular/common';
import localePl from '@angular/common/locales/pl';
import { PL_DOT_DATE_FORMATS } from './core/services/extensions.service';
import { DatesWithOffsetInterceptor } from './core/dates.interceptor';
import { QuillModule } from 'ngx-quill';
import { CatalogItemsComponent } from './components/catalog-items/catalog-items.component';
import { DictionariesComponent } from './components/dictionaries/dictionaries.component';
import { CatalogItemComponent } from './components/catalog-items/catalog-item/catalog-item.component';
import { CatalogItemsService } from './core/services/catalog-items.service';
import { DictionariesService } from './core/services/dictionaries.service';
import { ContractorPaneComponent } from './components/contractors/contractor-pane/contractor-pane.component';
import { AdditionalFieldsConfiguratorComponent } from './components/additional-fields-configurator/additional-fields-configurator.component';
import { FieldFormDialogComponent } from './components/additional-fields-configurator/field-form-dialog/field-form-dialog.component';
import { CdkTableModule } from "@angular/cdk/table";
import { PrettyRadioButtonComponent } from './components/theme/pretty-radio-button/pretty-radio-button.component';
import { CrmTableModule } from './components/theme/crm-table/crm-table.module';
import { CustomPaginatorIntl } from './core/intl/CustomPaginatorIntl';
import { ContractorSummaryComponent } from './components/contractors/contractor-summary/contractor-summary.component';
import { ZohoDeskService } from './core/services/zoho-desk.service';
import { TaskMailComposeComponent } from './components/tasks/task-mail-compose/task-mail-compose.component';
import { TasksMyComponent } from './components/tasks/tasks-my/tasks-my.component';
import { TasksAdminComponent } from './components/tasks/tasks-admin/tasks-admin.component';
import { TasksShellComponent } from './components/tasks/tasks-shell/tasks-shell.component';
import { TaskFormComponent } from './components/tasks/task-form/task-form.component';
import { MatIconPickerComponent } from './components/theme/mat-icon-picker/mat-icon-picker.component';
import { TasksStatsComponent } from './components/tasks/tasks-stats/tasks-stats.component';
import { ContractsAdminComponent } from './components/contractors/contracts-admin/contracts-admin.component';

registerLocaleData(localePl);

@NgModule({
  declarations: [
    AppComponent,
    LoginViewComponent,
    HeaderComponent,
    FooterComponent,
    UsersComponent,
    UserComponent,
    MessagePopupComponent,
    QuestionPopupComponent,
    ProjectsComponent,
    ContractorsComponent,
    PrettySelectComponent,
    SettingsComponent,
    LoadingOverlayComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
    IconPickerComponent,
    ContractorDialogComponent,
    CatalogItemsComponent,
    DictionariesComponent,
    CatalogItemComponent,
    ContractorPaneComponent,
    AdditionalFieldsConfiguratorComponent,
    FieldFormDialogComponent,
    PrettyRadioButtonComponent,
    ContractorSummaryComponent,
    TasksShellComponent,
    TasksAdminComponent,
    TasksMyComponent,
    TaskMailComposeComponent,
    TaskFormComponent,
    MatIconPickerComponent,
    TasksStatsComponent,
    ContractsAdminComponent,
  ],
  bootstrap: [AppComponent, LoadingOverlayComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    CommonModule,
    AppRoutingModule,
    MatButtonModule,
    MatIconModule,
    ReactiveFormsModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatMenuModule,
    MatSnackBarModule,
    MatCheckboxModule,
    MatSidenavModule,
    MatSlideToggleModule,
    MatButtonToggleModule,
    MatSelectModule,
    MatDialogModule,
    MatTabsModule,
    MatTreeModule,
    MatExpansionModule,
    MatDividerModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatProgressBarModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    PickerModule,
    EmojiComponent,
    SafeUrlPipe,
    CrmTemplateDirective,
    MatDatepickerModule,
    MatToolbarModule,
    MatBadgeModule,
    DragDropModule,
    QuillModule.forRoot(),
    CdkTableModule,
    CrmTableModule,
    MatSliderModule
  ],
  providers: [
    GuestGuard,
    ResetPasswordEntryGuard,
    AuthService,
    DbService,
    MatIconRegistry,
    NotificationService,
    UsersService,
    ContractorsService,
    SettingsService,
    LoadingService,
    DeviceService,
    ProjectsService,
    ZohoDeskService,
    CatalogItemsService,
    DictionariesService,
    provideHttpClient(withInterceptorsFromDi()),
    provideAppInitializer(() => inject(AuthService).bootstrap()),
    { provide: API_BASE_URL, useValue: environment.apiBaseUrl },
    { provide: HTTP_INTERCEPTORS, useClass: ApiInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: LoadingInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: DatesWithOffsetInterceptor, multi: true },
    { provide: MAT_DIALOG_DEFAULT_OPTIONS, useValue: { hasBackdrop: true, outerWidth: '100vw', outerHeight: '100vh' } },
    { provide: MAT_DATE_LOCALE, useValue: 'pl-PL' },
    { provide: LOCALE_ID, useValue: 'pl-PL' },
    { provide: DateAdapter, useClass: NativeDateAdapter, deps: [MAT_DATE_LOCALE] },
    { provide: MAT_DATE_FORMATS, useValue: PL_DOT_DATE_FORMATS },
    { provide: MatPaginatorIntl, useClass: CustomPaginatorIntl }
  ]
})
export class AppModule {
  constructor(iconRegistry: MatIconRegistry, sanitizer: DomSanitizer) {
    for (const [name, file] of Object.entries(LUCIDE_ICONS)) {
      iconRegistry.addSvgIcon(name, sanitizer.bypassSecurityTrustResourceUrl(`assets/lucide/${file}.svg`));
    }
  }
}
