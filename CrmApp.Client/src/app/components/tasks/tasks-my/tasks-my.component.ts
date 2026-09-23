import { CdkDropList } from '@angular/cdk/drag-drop';
import {
  AfterViewInit, Component, EventEmitter, OnInit, Output,
  QueryList, TemplateRef, ViewChild, ViewChildren,
  signal, computed
} from '@angular/core';
import { DictionariesService } from 'src/app/core/services/dictionaries.service';
import { TasksService } from 'src/app/core/services/tasks.service';
import { DictionariesElements } from 'src/models/DictionariesElements';
import { TaskDTO } from 'src/models/DTO/tasks/TaskDTO';
import { TaskListItemDTO } from 'src/models/DTO/tasks/TaskListItemDTO';
import { TaskPagedRequest } from 'src/models/DTO/tasks/TaskPagedRequest';
import { TableConfig } from 'src/models/table/TableConfig';
import { EDictionaryType } from 'src/models/enums/EDictionaryType';

export type ViewMode = 'list' | 'kanban' | 'grid' | 'stats';

const BADGE_COLOR_CYCLE = [
  'badge-todo', 'badge-progress', 'badge-done', 'badge-urgent', 'badge-normal', 'badge-low'
];

@Component({
  selector: 'crm-tasks-my',
  standalone: false,
  templateUrl: './tasks-my.component.html',
  styleUrl: './tasks-my.component.css',
})
export class TasksMyComponent implements OnInit, AfterViewInit {
  @Output() taskSelected = new EventEmitter<TaskDTO>();
  @ViewChildren(CdkDropList) dropListRefs!: QueryList<CdkDropList>;

  // ── custom cell templates (static: true = dostępne od razu po ngOnInit) ──
  @ViewChild('titleCellTpl',    { static: true }) titleCellTpl!:    TemplateRef<any>;
  @ViewChild('assigneeCellTpl', { static: true }) assigneeCellTpl!: TemplateRef<any>;
  @ViewChild('stateCellTpl',    { static: true }) stateCellTpl!:    TemplateRef<any>;
  @ViewChild('priorityCellTpl', { static: true }) priorityCellTpl!: TemplateRef<any>;
  @ViewChild('progressCellTpl', { static: true }) progressCellTpl!: TemplateRef<any>;
  @ViewChild('dueDateCellTpl',  { static: true }) dueDateCellTpl!:  TemplateRef<any>;
  @ViewChild('actionsCellTpl',  { static: true }) actionsCellTpl!:  TemplateRef<any>;

  // ── dane ─────────────────────────────────────────────────────────────────
  tasks   = signal<TaskListItemDTO[]>([]);
  total   = signal(0);
  loading = signal(false);

  // ── crm-table config (grid view) ───────────────────────────────────────────
  tableConfig = signal<TableConfig<TaskListItemDTO> | null>(null);

  // ── widok ─────────────────────────────────────────────────────────────────
  viewMode = signal<ViewMode>('list');
  onlyMine = signal(true);
  sideNavOpen: Boolean = false;

  // ── słowniki ──────────────────────────────────────────────────────────────
  tasksPriorities = signal<DictionariesElements[]>([]);
  tasksStates     = signal<DictionariesElements[]>([]);

  // ── kanban kolumny ze słownika ────────────────────────────────────────────
  kanbanColumns = computed(() =>
    this.tasksStates().map((s, i) => ({
      key:        s.value,
      label:      s.key,
      badgeClass: BADGE_COLOR_CYCLE[i % BADGE_COLOR_CYCLE.length],
    }))
  );

  // ── podsumowanie (grid view) ──────────────────────────────────────────────
  gridSummary = computed(() => {
    const tasks     = this.tasks();
    const doneValue = this.tasksStates().at(-1)?.value ?? '';
    const now       = new Date();
    return {
      total:      tasks.length,
      done:       tasks.filter(t => t.state === doneValue).length,
      overdue:    tasks.filter(t =>
                    t.dueDate && new Date(t.dueDate as any) < now && t.state !== doneValue
                  ).length,
      inProgress: tasks.filter(t => {
                    const idx = this.tasksStates().findIndex(s => s.value === t.state);
                    return idx > 0 && idx < this.tasksStates().length - 1;
                  }).length,
      unassigned: tasks.filter(t => !t.assignedTo).length,
      avgProgress: tasks.length
        ? Math.round(tasks.reduce((s, t) => s + (t.progress ?? 0), 0) / tasks.length)
        : 0,
    };
  });

  // ── request ───────────────────────────────────────────────────────────────
  private request: TaskPagedRequest = {
    page: 1, pageSize: 50, sortBy: 'dueDate', sortDescending: false,
  } as TaskPagedRequest;

  constructor(
    private tasksService: TasksService,
    private dictionariesService: DictionariesService,
  ) {}

  // ── lifecycle ─────────────────────────────────────────────────────────────

  async ngOnInit(): Promise<void> {
    await this.getDictionaries();
    // Po await sieć zdążyła zwrócić dane — ngAfterViewInit już się wykonał,
    // więc @ViewChild(static:true) templates są dostępne
    this.buildTableConfig();
    this.load();
  }

  ngAfterViewInit(): void {
    // zabezpieczenie gdy sieć jest bardzo szybka
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
      pagingMode:      'frontend',
      defaultSortBy:   'dueDate',
      defaultPageSize: 25,
      pageSizeOptions: [15, 25, 50, 100],
      rowClickable:    true,
      emptyMessage:    'Brak zadań spełniających kryteria',
      emptyIcon:       'task',
      columns: [
        {
          key:      'taskNumber',
          header:   'Nr',
          sortable: true,
          filter:   { type: 'text', placeholder: 'Szukaj...' },
          getValue: row => row.taskNumber,
          width:    '110px',
        },
        {
          key:          'title',
          header:       'Tytuł',
          sortable:     true,
          filter:       { type: 'text', placeholder: 'Szukaj tytułu...' },
          getValue:     row => row.title,
          cellTemplate: this.titleCellTpl,
        },
        {
          key:      'contractorName',
          header:   'Kontrahent',
          sortable: true,
          filter:   { type: 'text', placeholder: 'Szukaj...' },
          getValue: row => row.contractorName ?? '—',
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
          key:          'assignedToFullName',
          header:       'Przypisany',
          sortable:     true,
          filter:       { type: 'text', placeholder: 'Szukaj...' },
          getValue:     row => row.assignedToFullName ?? '',
          cellTemplate: this.assigneeCellTpl,
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
          filter:       { type: 'number', placeholder: 'min %' },
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

  load(): void {
    this.loading.set(true);
    const call = this.onlyMine()
      ? this.tasksService.getMyTasks(this.request)
      : this.tasksService.getAllTasks(this.request);

    call.subscribe({
      next: res => { this.tasks.set(res.items); this.total.set(res.totalCount); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  setFilter(patch: Partial<TaskPagedRequest>): void {
    this.request = { ...this.request, ...patch, page: 1 };
    this.load();
  }

  // ── widok ─────────────────────────────────────────────────────────────────

  setView(mode: ViewMode): void { this.viewMode.set(mode); }
  toggleMine(): void            { this.onlyMine.update(v => !v); this.load(); }
  onSideNavOpen()               { this.sideNavOpen = true;  }
  onSideNavClose()              { this.sideNavOpen = false; }

  onTaskClick(item: TaskListItemDTO): void {
    this.tasksService.getById(item.id).subscribe(task => this.taskSelected.emit(task.data));
  }

  // ── kanban ────────────────────────────────────────────────────────────────

  tasksByState(stateValue: string): TaskListItemDTO[] {
    return this.tasks().filter(t => t.state === stateValue);
  }

  get dropLists(): CdkDropList[] {
    return this.dropListRefs ? this.dropListRefs.toArray() : [];
  }

  onKanbanDrop(event: any, newStateValue: string): void {
    const task = event.item.data as TaskListItemDTO;
    if (task.state === newStateValue) return;
    this.tasks.update(list =>
      list.map(t => t.id === task.id ? { ...t, state: newStateValue } : t)
    );
    this.tasksService.getById(task.id).subscribe(full => {
      this.tasksService.update(task.id, { ...full.data, state: newStateValue } as TaskDTO).subscribe();
    });
  }

  // ── helpers ───────────────────────────────────────────────────────────────

  isDueDateOverdue(d?: Date | string): boolean {
    return !!d && new Date(d as any) < new Date();
  }

  stateLabel(value: string): string {
    return this.tasksStates().find(s => s.value === value)?.key ?? value;
  }

  stateClass(value: string): string {
    const idx = this.tasksStates().findIndex(s => s.value === value);
    return idx >= 0 ? BADGE_COLOR_CYCLE[idx % BADGE_COLOR_CYCLE.length] : 'badge-todo';
  }

  priorityLabel(value: string): string {
    return this.tasksPriorities().find(p => p.value === value)?.key ?? value;
  }

  priorityClass(value: string): string {
    const PRIORITY_COLORS = ['badge-urgent', 'badge-normal', 'badge-low'];
    const idx = this.tasksPriorities().findIndex(p => p.value === value);
    return idx >= 0 ? PRIORITY_COLORS[idx % PRIORITY_COLORS.length] : 'badge-normal';
  }

  stateDotClass(value: string): string {
    const idx = this.tasksStates().findIndex(s => s.value === value);
    if (idx === 0) return 'dot-ToDo';
    if (idx === 1) return 'dot-InProgress';
    return 'dot-Done';
  }

  // ── eksport ───────────────────────────────────────────────────────────────

  private buildExportRows(): (string | number)[][] {
    const s    = this.gridSummary();
    const rows = this.tasks();
    const headers = ['Nr', 'Tytuł', 'Kontrahent', 'Projekt', 'Przypisany',
                     'Status', 'Priorytet', 'Postęp (%)', 'Termin', 'Import nr'];
    const data = rows.map(t => [
      t.taskNumber ?? '',  t.title ?? '',
      t.contractorName ?? '',  t.projectName ?? '',  t.assignedToFullName ?? '',
      this.stateLabel(t.state),  this.priorityLabel(t.priority),
      t.progress ?? 0,
      t.dueDate ? new Date(t.dueDate as any).toLocaleDateString('pl-PL') : '',
      t.importedTaskNumber ?? '',
    ] as (string | number)[]);

    return [
      headers, ...data, [],
      ['', '', '', '', '', '', 'Łącznie:',        s.total,       '', ''],
      ['', '', '', '', '', '', 'W toku:',          s.inProgress,  '', ''],
      ['', '', '', '', '', '', 'Ukończone:',       s.done,        '', ''],
      ['', '', '', '', '', '', 'Przeterminowane:', s.overdue,     '', ''],
      ['', '', '', '', '', '', 'Śr. postęp (%):', s.avgProgress, '', ''],
      ['', '', '', '', '', '', 'Nieprzypisane:',   s.unassigned,  '', ''],
    ];
  }

  exportCsv(): void {
    const csv = this.buildExportRows()
      .map(r => r.map(c => `"${String(c).replace(/"/g, '""')}"`).join(';'))
      .join('\r\n');
    this.triggerDownload(
      new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8;' }),
      `zadania_${this.todayStr()}.csv`
    );
  }

  exportExcel(): void {
    import('xlsx').then(XLSX => {
      const ws = XLSX.utils.aoa_to_sheet(this.buildExportRows());
      ws['!cols'] = [
        { wch: 13 }, { wch: 36 }, { wch: 24 }, { wch: 22 }, { wch: 24 },
        { wch: 15 }, { wch: 13 }, { wch: 12 }, { wch: 16 }, { wch: 16 },
      ];
      const wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, 'Zadania');
      XLSX.writeFile(wb, `zadania_${this.todayStr()}.xlsx`);
    }).catch(() => this.exportCsv());
  }

  private todayStr = () => new Date().toISOString().slice(0, 10);

  private triggerDownload(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const a   = Object.assign(document.createElement('a'), { href: url, download: filename });
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }
}