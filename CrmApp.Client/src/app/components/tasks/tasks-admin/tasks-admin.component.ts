import {
  AfterViewInit, Component, computed, EventEmitter, OnInit,
  Output, signal, TemplateRef, ViewChild
} from '@angular/core';
import { DictionariesService } from 'src/app/core/services/dictionaries.service';
import { TasksService } from 'src/app/core/services/tasks.service';
import { DictionariesElements } from 'src/models/DictionariesElements';
import { TaskDTO } from 'src/models/DTO/tasks/TaskDTO';
import { TaskListItemDTO } from 'src/models/DTO/tasks/TaskListItemDTO';
import { TaskPagedRequest } from 'src/models/DTO/tasks/TaskPagedRequest';
import { UserWorkloadDTO } from 'src/models/DTO/tasks/UserWorkloadDTO';
import { PagedResult } from 'src/models/DTO/lists/PagedResult';
import { TableConfig } from 'src/models/table/TableConfig';
import { TableStateChange } from 'src/models/table/TableState';
import { EDictionaryType } from 'src/models/enums/EDictionaryType';

const BADGE_COLOR_CYCLE = ['badge-todo', 'badge-progress', 'badge-done', 'badge-urgent', 'badge-normal', 'badge-low'];
const PRIORITY_COLORS   = ['badge-urgent', 'badge-normal', 'badge-low'];

@Component({
  selector: 'crm-tasks-admin',
  standalone: false,
  templateUrl: './tasks-admin.component.html',
  styleUrl: './tasks-admin.component.css',
})
export class TasksAdminComponent implements OnInit, AfterViewInit {
  @Output() taskSelected = new EventEmitter<TaskDTO>();

  // ── custom cell templates ──────────────────────────────────────────────────
  @ViewChild('taskCellTpl',     { static: true }) taskCellTpl!:     TemplateRef<any>;
  @ViewChild('assigneeCellTpl', { static: true }) assigneeCellTpl!: TemplateRef<any>;
  @ViewChild('stateCellTpl',    { static: true }) stateCellTpl!:    TemplateRef<any>;
  @ViewChild('priorityCellTpl', { static: true }) priorityCellTpl!: TemplateRef<any>;
  @ViewChild('progressCellTpl', { static: true }) progressCellTpl!: TemplateRef<any>;
  @ViewChild('dueDateCellTpl',  { static: true }) dueDateCellTpl!:  TemplateRef<any>;
  @ViewChild('actionsCellTpl',  { static: true }) actionsCellTpl!:  TemplateRef<any>;

  // ── dane ─────────────────────────────────────────────────────────────────
  pagedData = signal<PagedResult<TaskListItemDTO> | null>(null);
  tasks     = signal<TaskListItemDTO[]>([]);   // bieżąca strona — do stats/workload
  total     = signal(0);
  loading   = signal(true);

  // ── crm-table ──────────────────────────────────────────────────────────────
  tableConfig = signal<TableConfig<TaskListItemDTO> | null>(null);

  // ── słowniki ──────────────────────────────────────────────────────────────
  tasksPriorities = signal<DictionariesElements[]>([]);
  tasksStates     = signal<DictionariesElements[]>([]);

  // ── shell compat ──────────────────────────────────────────────────────────
  sideNavOpen: Boolean = false;
  onSideNavOpen()  { this.sideNavOpen = true;  }
  onSideNavClose() { this.sideNavOpen = false; }

  // ── widok ─────────────────────────────────────────────────────────────────
  viewMode = signal<'table' | 'stats'>('table');

  // ── request ───────────────────────────────────────────────────────────────
  private request: TaskPagedRequest = {
    page: 1, pageSize: 25, sortBy: 'createdAt', sortDescending: true,
  } as TaskPagedRequest;

  // ── statystyki (z bieżącej strony) ───────────────────────────────────────
  stats = computed(() => {
    const t         = this.tasks();
    const doneValue = this.tasksStates().at(-1)?.value ?? 'Done';
    return {
      total:      this.total(),
      inProgress: t.filter(x => {
                    const idx = this.tasksStates().findIndex(s => s.value === x.state);
                    return idx > 0 && idx < this.tasksStates().length - 1;
                  }).length,
      overdue: t.filter(x => x.dueDate && new Date(x.dueDate) < new Date() && x.state !== doneValue).length,
      done:    t.filter(x => x.state === doneValue).length,
    };
  });

  // ── obciążenie zespołu (z bieżącej strony) ────────────────────────────────
  workload = computed<UserWorkloadDTO[]>(() => {
    const doneValue = this.tasksStates().at(-1)?.value ?? 'Done';
    const map = new Map<number, { name: string; total: number; overdue: number }>();
    this.tasks().forEach(t => {
      if (!t.assignedTo) return;
      const e = map.get(t.assignedTo) ?? { name: t.assignedToFullName ?? '', total: 0, overdue: 0 };
      e.total++;
      if (t.dueDate && new Date(t.dueDate) < new Date() && t.state !== doneValue) e.overdue++;
      map.set(t.assignedTo, e);
    });
    const max = Math.max(...[...map.values()].map(v => v.total), 1);
    return [...map.entries()].map(([id, v]) => ({
      userId:          id,
      fullName:        v.name,
      initials:        v.name.split(' ').map((n: string) => n[0]).join('').slice(0, 2).toUpperCase(),
      totalTasks:      v.total,
      overdueTasks:    v.overdue,
      workloadPercent: Math.round((v.total / max) * 100),
    }));
  });

  constructor(
    private tasksService: TasksService,
    private dictionariesService: DictionariesService,
  ) {}

  // ── lifecycle ─────────────────────────────────────────────────────────────

  async ngOnInit(): Promise<void> {
    await this.getDictionaries();
    this.buildTableConfig();
    this.loadPage();
  }

  ngAfterViewInit(): void {
    if (!this.tableConfig()) this.buildTableConfig();
  }

  // ── słowniki ──────────────────────────────────────────────────────────────

  async getDictionaries(): Promise<void> {
    const [priorities, states] = await Promise.all([
      this.dictionariesService.getByType(EDictionaryType.CrmTasksPriorities),
      this.dictionariesService.getByType(EDictionaryType.CrmTaskStates),
    ]);
    if (priorities?.dictionariesElements) this.tasksPriorities.set(priorities.dictionariesElements);
    if (states?.dictionariesElements)     this.tasksStates.set(states.dictionariesElements);
  }

  // ── konfiguracja tabeli ───────────────────────────────────────────────────

  buildTableConfig(): void {
    const config: TableConfig<TaskListItemDTO> = {
      pagingMode:      'backend',
      defaultSortBy:   'createdAt',
      defaultSortDesc: true,
      defaultPageSize: 25,
      pageSizeOptions: [15, 25, 50, 100, 250],
      rowClickable:    true,
      emptyMessage:    'Brak zadań spełniających kryteria',
      emptyIcon:       'task',
      columns: [
        {
          key:          'taskNumber',
          header:       'Nr',
          sortable:     true,
          filter:       { type: 'text', placeholder: 'Szukaj nr...' },
          getValue:     row => row.taskNumber,
          width:        '110px',
        },
        {
          key:          'title',
          header:       'Tytuł',
          sortable:     true,
          filter:       { type: 'text', placeholder: 'Szukaj tytułu...' },
          getValue:     row => row.title,
          cellTemplate: this.taskCellTpl,
        },
        {
          key:          'assignedToFullName',
          header:       'Przypisany',
          sortable:     true,
          filter:       { type: 'text', placeholder: 'Szukaj...' },
          getValue:     row => row.assignedToFullName ?? '',
          cellTemplate: this.assigneeCellTpl,
        },
        {
          key:      'contractorName',
          header:   'Kontrahent',
          sortable: true,
          filter:   { type: 'text', placeholder: 'Szukaj...' },
          getValue: row => row.contractorName ?? '—',
          hidden:   true,
        },
        {
          key:      'projectName',
          header:   'Projekt',
          sortable: true,
          filter:   { type: 'text', placeholder: 'Szukaj...' },
          getValue: row => row.projectName ?? '—',
          hidden:   true,
        },
        {
          key:          'state',
          header:       'Status',
          sortable:     true,
          filter: {
            type:    'select',
            options: this.tasksStates().map(s => ({ value: s.value, label: s.key })),
          },
          getValue:     row => row.state,
          cellTemplate: this.stateCellTpl,
          width:        '130px',
        },
        {
          key:          'priority',
          header:       'Priorytet',
          sortable:     true,
          filter: {
            type:    'select',
            options: this.tasksPriorities().map(p => ({ value: p.value, label: p.key })),
          },
          getValue:     row => row.priority,
          cellTemplate: this.priorityCellTpl,
          width:        '120px',
        },
        {
          key:          'progress',
          header:       'Postęp',
          sortable:     true,
          getValue:     row => row.progress,
          cellTemplate: this.progressCellTpl,
          width:        '150px',
        },
        {
          key:          'dueDate',
          header:       'Termin',
          sortable:     true,
          filter:       { type: 'daterange' },
          getValue:     row => row.dueDate,
          cellTemplate: this.dueDateCellTpl,
          width:        '140px',
        },
        {
          key:          'actions',
          header:       '',
          sortable:     false,
          sticky:       'end',
          width:        '56px',
          cellTemplate: this.actionsCellTpl,
        },
      ],
    };
    this.tableConfig.set(config);
  }

  // ── ładowanie ─────────────────────────────────────────────────────────────

  loadPage(): void {
    this.loading.set(true);
    this.tasksService.getAllTasks(this.request).subscribe({
      next: res => {
        this.pagedData.set(res);
        this.tasks.set(res.items);
        this.total.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  // alias wywoływany przez tasks-shell po synchronizacji
  load(): void { this.loadPage(); }

  // ── crm-table state change (backend mode) ─────────────────────────────────

  onStateChange(change: TableStateChange): void {
    const f = change.state.filters;
    this.request = {
      ...this.request,
      page:           change.state.page,
      pageSize:       change.state.pageSize,
      sortBy:         change.state.sortBy ?? 'createdAt',
      sortDescending: change.state.sortDescending,
      search:         change.state.search,
      // kolumnowe filtry
      title:          f['title']              as string | undefined,
      state:          f['state']              as string | undefined,
      priority:       f['priority']           as string | undefined,
      taskNumber:     f['taskNumber']         as string | undefined,
    } as TaskPagedRequest;
    this.loadPage();
  }

  // ── klik wiersza ─────────────────────────────────────────────────────────

  onRowClick(item: TaskListItemDTO): void {
    this.tasksService.getById(item.id).subscribe(task => this.taskSelected.emit(task.data));
  }

  // ── helpers ───────────────────────────────────────────────────────────────

  isDueDateOverdue(d?: string | Date): boolean {
    return !!d && new Date(d as any) < new Date();
  }

  stateLabel(value: string): string {
    return this.tasksStates().find(s => s.value === value)?.key ?? value;
  }

  stateClass(value: string): string {
    const idx = this.tasksStates().findIndex(s => s.value === value);
    return idx >= 0 ? BADGE_COLOR_CYCLE[idx % BADGE_COLOR_CYCLE.length] : '';
  }

  priorityLabel(value: string): string {
    return this.tasksPriorities().find(p => p.value === value)?.key ?? value;
  }

  priorityClass(value: string): string {
    const idx = this.tasksPriorities().findIndex(p => p.value === value);
    return idx >= 0 ? PRIORITY_COLORS[idx % PRIORITY_COLORS.length] : '';
  }
}