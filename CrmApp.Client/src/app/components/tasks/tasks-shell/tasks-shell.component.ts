import { Component, OnInit, signal, ViewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { TasksService } from 'src/app/core/services/tasks.service';
import { ZohoDeskService } from 'src/app/core/services/zoho-desk.service';
import { TaskDTO } from 'src/models/DTO/tasks/TaskDTO';

export type SidenavMode = 'detail' | 'create' | null;

@Component({
  selector: 'crm-tasks-shell',
  standalone: false,
  templateUrl: './tasks-shell.component.html',
  styleUrls: ['./tasks-shell.component.css'],
})
export class TasksShellComponent implements OnInit {
  @ViewChild('taskSidenav') taskSidenav!: MatSidenav;

  activeTab = signal<'my' | 'admin'>('my');
  sidenavMode = signal<SidenavMode>(null);
  selectedTask = signal<TaskDTO | null>(null);
  private activeComponent: any = null;

  constructor(private router: Router, private zohoDeskService: ZohoDeskService,
    private tasksService: TasksService
  ) {}

  ngOnInit(): void {
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe((e: any) => {
        this.activeTab.set(e.url.includes('admin') ? 'admin' : 'my');
      });

    if (this.router.url.includes('admin')) {
      this.activeTab.set('admin');
    }
  }

  navigateTo(tab: 'my' | 'admin'): void {
    this.activeTab.set(tab);
    this.router.navigate(['/tasks', tab]);
    this.closeSidenav();
  }

  syncWithZohoDesk() : void{
    this.zohoDeskService.getTickets().then(() => {
      this.callChildMethod();
      const current = this.selectedTask();          // ← wartość sygnału
      if (current && this.sidenavMode() === 'detail') {
        // pobierz świeżą wersję zadania z backendu
        this.tasksService.getById(current.id).subscribe(fresh => {
          if (fresh.data != undefined)
            this.onTaskUpdated(fresh.data);
        });
      }
    });
  }

  // ── otwieranie sidenavy ───────────────────────────────────────────────────

  openDetail(task: TaskDTO): void {
    this.selectedTask.set(task);
    this.sidenavMode.set('detail');
    this.taskSidenav?.open();
    this.activeComponent.onSideNavOpen();
  }

  openCreate(): void {
    this.selectedTask.set(null);
    this.sidenavMode.set('create');
    this.taskSidenav?.open();
    this.activeComponent.onSideNavOpen();
  }

  closeSidenav(): void {
    this.taskSidenav?.close();
    this.sidenavMode.set(null);
    this.selectedTask.set(null);
    this.activeComponent.onSideNavClose();
  }

  // ── eventy z child komponentów ────────────────────────────────────────────

  onTaskUpdated(task: TaskDTO): void {
    this.selectedTask.set(task);
    this.callChildMethod();
  }

  onTaskSaved(task: TaskDTO): void {
    // po zapisaniu przełącz na detail nowo utworzonego/zaktualizowanego zadania
    this.selectedTask.set(task);
    this.sidenavMode.set('detail');
    this.callChildMethod();
  }

  onActivate(component: any): void {
    if (component.taskSelected) {
      component.taskSelected.subscribe((task: TaskDTO) => this.openDetail(task));
    }
    if (component.taskUpdated){
      component.taskUpdated.subscribe((task: TaskDTO) => this.onTaskUpdated(task))
    }
    if (component.createRequested) {
      component.createRequested.subscribe(() => this.openCreate());
    }
    this.activeComponent = component;
  }

  callChildMethod(): void {
    // TO DO ODŚWIEŻENIE DANYCH PO SYNCHRONIZACJI Z ZOHO DESK
    this.activeComponent?.load();
  }
}