import { formatDate } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { cloneDeep } from 'lodash';
import { Observable } from 'rxjs';
import { AuthService } from 'src/app/core/services/auth.service';
import { ContractorsService } from 'src/app/core/services/contractors.service';
import { DeviceService } from 'src/app/core/services/device.service';
import { DATE_TO_BACKEND_FORMAT } from 'src/app/core/services/extensions.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { ProjectsService } from 'src/app/core/services/projects.service';
import { ToolsService } from 'src/app/core/services/tools.service';
import { UsersService } from 'src/app/core/services/users.service';
import { Contractors } from 'src/models/Contractors';
import { UsersDTO } from 'src/models/DTO/UsersDTO';
import { ValueNameDTO } from 'src/models/DTO/ValueNameDTO';
import { EProjectState } from 'src/models/enums/EProjectState';
import { Projects } from 'src/models/Projects';
import { UsersProfiles } from 'src/models/UsersProfiles';

@Component({
  selector: 'crm-projects',
  standalone: false,
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.css']
})
export class ProjectsComponent implements OnInit {
  isEditPanelOpened: boolean = false;
  selectedProject?: Projects;
  projects: Projects[] = [];
  contractors: Contractors[] = [];
  dataSource: MatTableDataSource<Projects> = new MatTableDataSource<Projects>();
  displayedColumns: string[] = ['name', 'startDate', 'endDate', 'projectManager', 'contractor', 'actions'];
  filterValue?: string;
  users: UsersDTO[] = [];
  projectsStates: ValueNameDTO[] = [];
  EProjectState = EProjectState;
  private userId: number;
  readonly canProjectsCreate: boolean;
  readonly canProjectsUpdate: boolean;
  readonly canProjectsDelete: boolean;
  readonly isMobile$: Observable<boolean>;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private auth: AuthService,
    private projectsService: ProjectsService,
    private device: DeviceService,
    private notificationService: NotificationService,
    private usersService: UsersService,
    private toolsService: ToolsService,
    private contractorsService: ContractorsService
  ) {
    this.canProjectsCreate = this.auth.userConfiguration?.canProjectsCreate ?? false;
    this.canProjectsUpdate = this.auth.userConfiguration?.canProjectsUpdate ?? false;
    this.canProjectsDelete = this.auth.userConfiguration?.canProjectsDelete ?? false;
    this.userId = this.auth.user?.id ?? 0;
    this.isMobile$ = this.device.isMobile$;
    this.toolsService.getEnumValues("EProjectState").then((data) => {
      this.projectsStates = data ?? [];
    });
    this.isMobile$.subscribe(isMobile => {
      if (isMobile) {
        this.displayedColumns = ['name', 'contractor', 'actions'];
      } else {
        this.displayedColumns = ['name', 'startDate', 'endDate', 'projectManager', 'contractor', 'actions'];
      }
    });
  }

  ngOnInit(): void {
    this.getProjects();
    this.getUsers();
    this.getContractors();
  }

  closeEditPanel() {
    this.isEditPanelOpened = false;
    this.selectedProject = undefined;
  }

  getProjects() {
    this.projectsService.getAll().then((data) => {
      this.projects = data ?? [];
      this.setMatTableConfig();
    });
  }

  getUsers() {
    this.usersService.getAll().then((data) => {
      this.users = data ?? [];
    });
  }

  getContractors() {
    this.contractorsService.getAll().then((data) => {
      this.contractors = data ?? [];
    });
  }

  setMatTableConfig() {
    this.dataSource.data = this.projects;
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

  resetFilter() {
    this.filterValue = "";
    if (this.dataSource)
      this.dataSource!.filter = this.filterValue;

    if (this.dataSource && this.dataSource.paginator)
      this.dataSource.paginator.firstPage();
  }

  addProject() {
    this.selectedProject = new Projects();
    this.isEditPanelOpened = true;
  }

  editProject(item: Projects) {
    this.selectedProject = cloneDeep(item);
    this.selectedProject.startDateStr = this.selectedProject.startDate ? formatDate(this.selectedProject.startDate, DATE_TO_BACKEND_FORMAT, "en-US") : "";
    this.selectedProject.endDateStr = this.selectedProject.endDate ? formatDate(this.selectedProject.endDate, DATE_TO_BACKEND_FORMAT, "en-US") : "";
    this.isEditPanelOpened = true;
  }

  deleteProject(item: Projects) {
    if (item && item.id) {
      this.notificationService.confirm(`Czy na pewno chcesz usunąć projekt ${item.name}?`).subscribe((result) => {
          if (result) {
            this.projectsService.delete(item.id!).then((result) => {
              if (result?.succeeded) {
                this.notificationService.success("Usunięto projekt " + item.name);
                this.getProjects();
                if (this.selectedProject?.id == item.id) {
                  this.closeEditPanel();
                }
              } else {
                this.notificationService.error(result?.errors ? result.errors.join("\n") : "Nie usunięto projektu " + item.name);
              }
            }, (err) => {
              this.notificationService.error("Nie usunięto projektu " + item.name);
            });
          }
        });
    }
  }

  async saveChanges() {
    if (!this.selectedProject?.name) {
      this.notificationService.error("Zdefiniuj nazwę projektu");
      return;
    }
    if (!this.selectedProject?.projectManagerId) {
      this.notificationService.error("Wybierz managera projektu");
      return;
    }

    if (this.selectedProject && this.selectedProject.id) {
      this.projectsService.update(this.selectedProject, this.userId).then((result) => {
        if (result && result.succeeded) {
          this.notificationService.success("Zaktualizowano projekt " + this.selectedProject?.name);
          this.getProjects();
          this.closeEditPanel();
        } else {
          this.selectedProject = this.projects.find(x => x.id == this.selectedProject?.id);
          this.notificationService.error(result?.errors ? result.errors.join("\n") :
            "Nie zaktualizowano projektu " + this.selectedProject?.name);
        }
      }, (err) => {
        this.selectedProject = this.projects.find(x => x.id == this.selectedProject?.id);
        this.notificationService.error("Nie zaktualizowano projektu " + this.selectedProject?.name);
      });
    } else if (this.selectedProject) {
      this.projectsService.add(this.selectedProject, this.userId).then(async (result) => {
        if (result && result.succeeded) {
          this.notificationService.success("Dodano projekt " + this.selectedProject?.name);
          this.getProjects();
          this.addProject();
        } else {
          this.notificationService.error(result?.errors ? result.errors.join("\n") :
            "Nie dodano projektu " + this.selectedProject?.name);
        }
      }, (err) => {
        this.notificationService.error("Nie dodano projektu " + this.selectedProject?.name);
      });
    }
  }

  onDateChange(propertyName: 'startDate' | 'endDate') {
    if (propertyName == 'startDate') {
      if (this.selectedProject?.startDate)
        this.selectedProject!.startDateStr = formatDate(this.selectedProject.startDate, DATE_TO_BACKEND_FORMAT, "en-US");
      else 
        this.selectedProject!.startDateStr = "";
    } else {
      if (this.selectedProject?.endDate) {
        this.selectedProject?.endDate.setHours(23, 59, 59);
      }
      if (this.selectedProject?.endDate)
        this.selectedProject!.endDateStr = formatDate(this.selectedProject.endDate, DATE_TO_BACKEND_FORMAT, "en-US");
      else 
        this.selectedProject!.endDateStr = "";
    }
  }

  onProjectManagerChange() {
    if (!this.selectedProject) return;
    if (this.selectedProject!.projectManagerId && !this.selectedProject.membersIds.includes(this.selectedProject!.projectManagerId)) {
      this.selectedProject.membersIds.push(this.selectedProject!.projectManagerId);
      this.selectedProject.membersIds = [...this.selectedProject.membersIds];
    }
  }
}
