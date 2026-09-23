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
import { ThemeToggleComponent } from './components/theme/theme-toggle/theme-toggle.component';
import { ThemeService } from './core/services/theme.service';
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
    ThemeToggleComponent,
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
    ThemeService,
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
    iconRegistry.addSvgIcon(
      'network_intel_node',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/network_intel_node_24dp_FFFFFF_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'account_circle',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/account_circle_24dp_FFFFFF_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'logout',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/logout_24dp_FFFFFF_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'delete',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/delete_24dp_FFFFFF_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'folder',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/folder_24dp_FFFFFF_FILL0_wght0_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'folder_check',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/folder_check_24dp_FFFFFF_FILL0_wght0_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'warning',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/error_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'error',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/cancel_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'success',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/check_circle_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'info',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/info_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'close',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/close_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'close_bold',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/close_24dp_000000_FILL0_wght700_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'person_shield',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/person_shield_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'admin_panel',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/admin_panel_settings_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'folder_data',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/folder_data_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'note_stack',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/note_stack_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'note_stack_add',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/note_stack_add_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'dark_mode',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/dark_mode_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'light_mode',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/light_mode_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'system_mode',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/settings_night_sight_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'add',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/add_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'add_circle',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/add_circle_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'send',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/send_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'edit',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/edit_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'save',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/save_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'sort',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/sort_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'question',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/help_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'back',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/arrow_back_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'website',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/captive_portal_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'content_copy',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/content_copy_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'upload',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/upload_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'chevron_right',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/chevron_right_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'keyboard_arrow_up',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/keyboard_arrow_up_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'arrow_right',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/arrow_right_alt_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'undo',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/undo_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'category',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/category_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'person_card',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/co_present_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'tag',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/tag_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'strategy',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/strategy_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'keyboard_arrow_down',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/keyboard_arrow_down_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'lock_reset',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/lock_reset_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'key_vertical',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/key_vertical_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'book',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/book_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'insert_text',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/insert_text_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'settings',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/settings_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'folder_shared',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/folder_shared_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'arrow_menu_close',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/arrow_menu_close_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'arrow_menu_open',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/arrow_menu_open_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'open_in_full',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/open_in_full_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'hide',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/hide_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'account_active',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/how_to_reg_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'account_off',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/person_off_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'pending',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/pending_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'retry',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/autorenew_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'adaptive_audio_mic',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/adaptive_audio_mic_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'interpreter_mode',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/interpreter_mode_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'download',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/download_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'mic',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/mic_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'cloud_sync',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/cloud_sync_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'chat_paste_go',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/chat_paste_go_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'check_box',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/check_box_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'check_box_blank',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/check_box_outline_blank_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'open_in_new',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/open_in_new_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'pause_circle',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/pause_circle_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'frame_inspect',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/frame_inspect_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'pin',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/keep_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'pin_off',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/keep_off_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'automation',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/automation_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'table_edit',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/table_edit_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'shopping_cart',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/shopping_cart_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'contact_mail',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/contact_mail_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'contract',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/contract_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'list',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/list_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'toggle_on',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/toggle_on_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'text_fields',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/text_fields_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'calculate',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/calculate_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'pin',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/pin_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'person',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/person_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'shield',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/shield_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'shield_lock',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/shield_lock_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'view_column',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/view_column_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'search',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/search_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'encrypted_add',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/encrypted_add_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'shield_question',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/shield_question_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'filter_alt',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/filter_alt_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'filter_alt_off',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/filter_alt_off_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'radio_button_checked',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/radio_button_checked_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'radio_button_unchecked',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/radio_button_unchecked_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'search_off',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/search_off_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'timer',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/timer_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'history',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/history_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'all_inclusive',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/all_inclusive_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'hourglass_top',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/hourglass_top_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'schedule',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/schedule_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'drag_indicator',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/drag_indicator_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'productivity',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/productivity_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'sync_alt',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/sync_alt_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'task',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/task_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'business_center',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/business_center_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'view_kanban',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/view_kanban_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'schedule',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/schedule_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'today',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/today_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'add_task',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/add_task_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'hourglass_empty',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/hourglass_empty_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'finance_mode',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/finance_mode_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'timer',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/timer_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
    iconRegistry.addSvgIcon(
      'history',
      sanitizer.bypassSecurityTrustResourceUrl('assets/icons/history_24dp_000000_FILL0_wght400_GRAD0_opsz24.svg')
    );
  }
}
