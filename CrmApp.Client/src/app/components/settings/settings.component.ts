import { COMMA, ENTER, V } from '@angular/cdk/keycodes';
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Observable } from 'rxjs';
import { AuthService } from 'src/app/core/services/auth.service';
import { DeviceService } from 'src/app/core/services/device.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { SettingsService } from 'src/app/core/services/settings.service';
import { UsersService } from 'src/app/core/services/users.service';
import { UsersDTO } from 'src/models/DTO/UsersDTO';
import { EValueType } from 'src/models/enums/EValueType';
import { Settings, SettingsKeys } from 'src/models/Settings';

@Component({
  selector: 'crm-settings',
  standalone: false,
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent implements OnInit {
  settings: Settings[] = [];
  dataSource: MatTableDataSource<Settings> = new MatTableDataSource<Settings>();
  filterValue?: string;
  SettingsKeys = SettingsKeys;
  EValueType = EValueType;
  readonly separatorKeysCodes = [ENTER, COMMA] as const;
  displayedColumns: string[] = [
    'label', 'description', 'value', 'actions'
  ];
  prefixOpen: Record<string, boolean> = {};
  suffixOpen: Record<string, boolean> = {};
  valueOpen: Record<string, boolean> = {};
  users: UsersDTO[] = [];
  userId: number = 0;

  readonly canSystemConfig: boolean;
  readonly isMobile$: Observable<boolean>;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  
  constructor(
    private auth: AuthService,
    private settingsService: SettingsService,
    private notificationService: NotificationService,
    private device: DeviceService,
    private usersService: UsersService
  ) {
    this.userId = this.auth.user?.id ?? 0;
    this.canSystemConfig = this.auth.userConfiguration?.canSystemConfig ?? false;
    this.isMobile$ = this.device.isMobile$;
    this.isMobile$.subscribe((isMobile) => {
      if (isMobile) {
        this.displayedColumns = ['label', 'value', 'actions'];
      } else {
        this.displayedColumns = [
          'label', 'description', 'value', 'actions'
        ];
      }
    });
  }

  ngOnInit(): void {
    this.getSettings();
    this.getUsers();
  }


  getUsers() {
    this.usersService.getAll().then((data) => {
      this.users = data ?? [];
    });
  }
  
  copyToClipboard(text: string) {
    if (!text) return;
    navigator.clipboard?.writeText(text).catch(() => {
      const ta = document.createElement('textarea');
      ta.value = text;
      document.body.appendChild(ta);
      ta.select();
      document.execCommand('copy');
      document.body.removeChild(ta);
    });
  }

  setMatTableConfig() {
    this.dataSource.data = this.settings;
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  applyFilter(e: Event) {
    const filterValue = (e.target as HTMLInputElement).value;
    if (this.dataSource)
      this.dataSource!.filter = filterValue.trim().toLocaleLowerCase();

    if (this.dataSource && this.dataSource.paginator)
      this.dataSource.paginator.firstPage();
  }

  getSettings() {
    this.settingsService.getAll().then((data) => {
      this.settings = data ?? [];
      this.settings.forEach(x => {
        if (x.valueType == EValueType.List && x.tableName) x.valueObj = x.value ? Number(x.value) : null;
      });
      this.setMatTableConfig();
    });
  }

  updateValue(setting: Settings) {
    setting.value = setting.valueObj ? setting.valueObj.toString() : undefined;
    this.settingsService.updateValue(setting).then((result) => {
      if (result?.succeeded) {
        this.notificationService.success("Zaktualizowano ustawienie " + setting.label);
        this.getSettings();
      } else {
        this.notificationService.error(result?.errors ? result.errors.join("\n") : "Nie zaktualizowano ustawienia " + setting.label);
      }
    }, (err) => {
      this.notificationService.error("Nie zaktualizowano ustawienia " + setting.label);
    });
  }

  isExpanded(value: Record<string, boolean>, key: string, defaultValue: boolean = false) {
    if (value[key] != null && value[key] != undefined) return value[key];
    else return defaultValue; 
  }

  resetFilter() {
    this.filterValue = "";
    if (this.dataSource)
      this.dataSource!.filter = this.filterValue;

    if (this.dataSource && this.dataSource.paginator)
      this.dataSource.paginator.firstPage();
  }
}
