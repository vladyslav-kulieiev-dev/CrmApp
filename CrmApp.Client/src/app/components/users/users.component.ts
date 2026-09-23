import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from 'src/app/core/services/auth.service';
import { DeviceService } from 'src/app/core/services/device.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { UsersService } from 'src/app/core/services/users.service';
import { UsersDTO } from 'src/models/DTO/UsersDTO';

@Component({
  selector: 'crm-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css'],
  standalone: false
})
export class UsersComponent implements OnInit {
  displayedColumns: string[] = ['firstName', 'lastName', 'displayName', 'email', 'isDeleted', 'actions'];
  dataSource: MatTableDataSource<UsersDTO> = new MatTableDataSource<UsersDTO>();
  filterValue?: string;
  users: UsersDTO[] = [];
  readonly canUsersUpdate: boolean;
  readonly canUsersCreate: boolean;
  readonly canUsersDelete: boolean;
  readonly canIdentityManage: boolean;
  readonly isMobile$: Observable<boolean>;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private usersService: UsersService,
    private notificationsService: NotificationService,
    private router: Router,
    private auth: AuthService,
    private device: DeviceService
  ) {
    this.canUsersCreate = this.auth.userConfiguration?.canUsersCreate ?? false;
    this.canUsersUpdate = this.auth.userConfiguration?.canUsersUpdate ?? false;
    this.canUsersDelete = this.auth.userConfiguration?.canUsersDelete ?? false;
    this.canIdentityManage = this.auth.userConfiguration?.canManageIdentity ?? false;
    this.isMobile$ = this.device.isMobile$;
    this.isMobile$.subscribe((isMobile) => {
      if (isMobile) {
        this.displayedColumns = ['displayName', 'isDeleted', 'actions'];
      } else {
        this.displayedColumns = ['firstName', 'lastName', 'displayName', 'email', 'isDeleted', 'actions'];
      }
    });
   }

  ngOnInit(): void {
    this.getUsers();
  }

  setMatTableConfig() {
    this.dataSource.data = this.users;
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  getUsers() {
    this.usersService.getAll().then((data) => {
      this.users = data ?? [];
      this.setMatTableConfig();
    });
  }

  applyFilter(e: Event) {
    const filterValue = (e.target as HTMLInputElement).value;
    if (this.dataSource)
      this.dataSource!.filter = filterValue.trim().toLocaleLowerCase();

    if (this.dataSource && this.dataSource.paginator)
      this.dataSource.paginator.firstPage();
  }

  deactivateUser(item: UsersDTO) {
    if (item && item.id) {
      this.notificationsService.confirm(`Czy na pewno chcesz dezaktywować konto użytkownika ${item.displayName}?`).subscribe((result) => {
          if (result) {
            this.usersService.deactivate(item.id).then((result) => {
              if (result?.succeeded) {
                this.notificationsService.success(result.messages ? result.messages.join("\n") : "Dezaktywowano konto użytkownika "
                  + item.displayName);
                this.getUsers();
              } else {
                this.notificationsService.error(result?.errors ? result.errors.join("\n") : "Nie dezaktywowano konta użytkownika" + item.displayName);
              }
            }, (err) => {
              this.notificationsService.error("Nie dezaktywowano konta użytkownika " + item.displayName);
            });
          }
        });
    }
  }

  activateUser(item: UsersDTO) {
    if (item && item.id) {
      this.notificationsService.confirm(`Czy na pewno chcesz aktywować konto użytkownika ${item.displayName}?`).subscribe((result) => {
          if (result) {
            this.usersService.activate(item.id).then((result) => {
              if (result?.succeeded) {
                this.notificationsService.success(result.messages ? result.messages.join("\n") : "Aktywowano konto użytkownika "
                  + item.displayName);
                this.getUsers();
              } else {
                this.notificationsService.error(result?.errors ? result.errors.join("\n") : "Nie aktywowano konta użytkownika" + item.displayName);
              }
            }, (err) => {
              this.notificationsService.error("Nie aktywowano konta użytkownika " + item.displayName);
            });
          }
        });
    }
  }

  editUser(item: UsersDTO) {
    this.router.navigate(['./user', item.id]);
  } 

  addUser() {
    this.router.navigate(['./user']);
  }

  resetFilter() {
    this.filterValue = "";
    if (this.dataSource)
      this.dataSource!.filter = this.filterValue;

    if (this.dataSource && this.dataSource.paginator)
      this.dataSource.paginator.firstPage();
  }
}
