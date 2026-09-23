import { formatDate } from '@angular/common';
import {
  Component, computed, EventEmitter, Input,
  OnChanges, OnInit, Output, signal
} from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { DATE_TO_BACKEND_FORMAT, getJoinedMesseges } from 'src/app/core/services/extensions.service';
import { DictionariesService } from 'src/app/core/services/dictionaries.service';
import { TasksService } from 'src/app/core/services/tasks.service';
import { UsersService } from 'src/app/core/services/users.service';
import { ContractorsService } from 'src/app/core/services/contractors.service';
import { ProjectsService } from 'src/app/core/services/projects.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { DictionariesElements } from 'src/models/DictionariesElements';
import { TaskDTO, TaskCommentDTO } from 'src/models/DTO/tasks/TaskDTO';
import { UsersDTO } from 'src/models/DTO/UsersDTO';
import { Contractors } from 'src/models/Contractors';
import { ContractorContacts } from 'src/models/ContractorContacts';
import { Projects } from 'src/models/Projects';
import { EDictionaryType } from 'src/models/enums/EDictionaryType';
import { ZohoDeskService } from 'src/app/core/services/zoho-desk.service';
import { lastValueFrom } from 'rxjs';
import { ContractorListItemDTO } from 'src/models/DTO/contractors/ContractorListItemDTO';
import { ToolsService } from 'src/app/core/services/tools.service';
import { ValueNameDTO } from 'src/models/DTO/ValueNameDTO';

const BADGE_COLOR_CYCLE = ['badge-todo', 'badge-progress', 'badge-done', 'badge-urgent', 'badge-normal', 'badge-low'];
const PRIORITY_COLORS   = ['badge-urgent', 'badge-normal', 'badge-low'];

@Component({
  selector: 'crm-task-form',
  standalone: false,
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.css',
})
export class TaskFormComponent implements OnChanges, OnInit {
  @Input() task: TaskDTO | null = null;

  @Output() saved        = new EventEmitter<TaskDTO>();
  @Output() taskUpdated  = new EventEmitter<TaskDTO>();
  @Output() cancelled    = new EventEmitter<void>();
  @Output() close        = new EventEmitter<void>();

  get isEditMode(): boolean { return !!this.task; }

  form = new FormGroup({
    title:               new FormControl('',  Validators.required),
    state:               new FormControl('',  Validators.required),
    priority:            new FormControl('',  Validators.required),
    source:              new FormControl<string | null>(null),
    progress:            new FormControl<number>(0, [Validators.min(0), Validators.max(100)]),
    dueDate:             new FormControl<Date | null>(null),
    assignedTo:          new FormControl<number | null>(null),
    projectId:           new FormControl<number | null>(null),
    contractorId:        new FormControl<number | null>(null),
    contractorContactId: new FormControl<number | null>(null),
    description:         new FormControl(''),
    notes:               new FormControl(''),
  });

  saving = signal(false);

  showMailCompose    = signal(false);
  commentControl     = new FormControl('', Validators.required);
  editingCommentId   = signal<number | null>(null);
  editCommentControl = new FormControl('');

  contractorSummary        = signal<ContractorListItemDTO | null>(null);
  contractorSummaryLoading = signal(false);

  private selectedProjectId = signal<number | null>(null);

  selectedProject = computed(() =>
    this.projects.find(p => p.id === this.selectedProjectId()) ?? null
  );

  tasksStates     = signal<DictionariesElements[]>([]);
  tasksPriorities = signal<DictionariesElements[]>([]);
  taskSources     = signal<DictionariesElements[]>([]);

  users              : UsersDTO[]           = [];
  contractors        : Contractors[]        = [];
  contractorContacts : ContractorContacts[] = [];
  projects           : Projects[]           = [];
  engagementTypes    : ValueNameDTO[]       = [];

  hasImported = () => !!this.task?.importedTask;

  contactEmail(): string | null {
    const t = this.task;
    if (!t) return null;
    if (!t.importedTask) return `${t.contractorContactEmail ?? ''}`.trim() || null;
    return `${t.importedTask.contactEmail ?? ''}`.trim() || null;
  }

  contactFullName(): string | null {
    const t = this.task;
    if (!t) return null;
    if (!t.importedTask) return `${t.contractorContactFullName ?? ''}`.trim() || null;
    return `${t.importedTask.contactFirstName ?? ''} ${t.importedTask.contactLastName ?? ''}`.trim() || null;
  }

  constructor(
    private svc:            TasksService,
    private dictSvc:        DictionariesService,
    private usersSvc:       UsersService,
    private contractorsSvc: ContractorsService,
    private projectsSvc:    ProjectsService,
    private notif:          NotificationService,
    private zohoService:    ZohoDeskService,
    private toolsService:   ToolsService
  ) {}

  // ── lifecycle ─────────────────────────────────────────────────────────────

  async ngOnInit(): Promise<void> {
    const [states, prios, sources, users, contractors, projects] = await Promise.all([
      this.dictSvc.getByType(EDictionaryType.CrmTaskStates),
      this.dictSvc.getByType(EDictionaryType.CrmTasksPriorities),
      this.dictSvc.getByType(EDictionaryType.TaskSource),
      this.usersSvc.getAll(),
      this.contractorsSvc.getAll(),
      this.projectsSvc.getAll(),
    ]);

    if (states?.dictionariesElements)  this.tasksStates.set(states.dictionariesElements);
    if (prios?.dictionariesElements)   this.tasksPriorities.set(prios.dictionariesElements);
    if (sources?.dictionariesElements) this.taskSources.set(sources.dictionariesElements);
    this.users       = users       ?? [];
    this.contractors = contractors ?? [];
    this.projects    = projects    ?? [];

    // domyślny status przy tworzeniu
    if (!this.isEditMode && this.tasksStates().length) {
      this.form.patchValue({ state: this.tasksStates()[0].value });
    }

    if (this.task?.contractorId) {
      await Promise.all([
        this.loadContractorContacts(this.task.contractorId),
        this.loadContractorSummary(this.task.contractorId),
      ]);
    }
    this.engagementTypes = await this.toolsService.getEnumValues("EEngagementType") ?? [];
    this.selectedProjectId.set(this.task?.projectId ?? null);

    this.form.get('projectId')!.valueChanges.subscribe(v =>
      this.selectedProjectId.set(v ?? null)
    );
  }

  ngOnChanges(): void {
    if (this.task) {
      this.resetForm();
      this.showMailCompose.set(false);
      this.editingCommentId.set(null);
      this.commentControl.reset();
      this.selectedProjectId.set(this.task.projectId ?? null);
      if (this.task.contractorId) {
        this.loadContractorContacts(this.task.contractorId);
        this.loadContractorSummary(this.task.contractorId);
      } else {
        this.contractorSummary.set(null);
        this.contractorContacts = [];
      }
    } else {
      this.form.reset({ progress: 0 });
      this.contractorSummary.set(null);
      this.contractorContacts = [];
    }
  }

  private resetForm(): void {
    if (!this.task) return;
    this.form.reset({
      title:               this.task.title               ?? '',
      state:               this.task.state               ?? '',
      priority:            this.task.priority            ?? '',
      source:              this.task.source              ?? null,
      progress:            this.task.progress            ?? 0,
      dueDate:             this.task.dueDate ? new Date(this.task.dueDate) : null,
      assignedTo:          this.task.assignedTo          ?? null,
      projectId:           this.task.projectId           ?? null,
      contractorId:        this.task.contractorId        ?? null,
      contractorContactId: this.task.contractorContactId ?? null,
      description:         new DOMParser().parseFromString(this.task.description ?? '', 'text/html').body.textContent,
      notes:               this.task.notes               ?? '',
    });
  }

  // ── zapis ─────────────────────────────────────────────────────────────────

  async saveTask(notification?: string): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.value;
    this.saving.set(true);

    const dto: TaskDTO = {
      ...(this.task ?? {}),
      id:                  this.task?.id ?? 0,
      taskNumber:          this.task?.taskNumber ?? '',
      title:               v.title!,
      state:               v.state!,
      priority:            v.priority!,
      source:              v.source ?? undefined,
      progress:            v.progress ?? 0,
      description:         v.description ?? '',
      notes:               v.notes ?? '',
      dueDate:             v.dueDate,
      dueDateString:       v.dueDate
                             ? formatDate(v.dueDate as Date, DATE_TO_BACKEND_FORMAT, 'en-US')
                             : undefined,
      assignedTo:          v.assignedTo          ?? undefined,
      projectId:           v.projectId           ?? undefined,
      contractorId:        v.contractorId        ?? undefined,
      contractorContactId: v.contractorContactId ?? undefined,
      comments:            this.task?.comments ?? [],
      importedTask:        this.task?.importedTask,
      importedTaskId:      this.task?.importedTaskId,
    } as TaskDTO;

    try {
      const full = this.isEditMode
        ? await lastValueFrom(this.svc.update(this.task!.id, dto))
        : await lastValueFrom(this.svc.create(dto));

      if (!full.data?.id) {
        this.notif.error('Nie udało się odczytać ID zapisanego zadania.');
        return;
      }

      if (this.isEditMode) {
        this.task = full.data;
        this.resetForm();
        this.taskUpdated.emit(full.data);
      } else {
        this.closePanel();
      }
      this.saved.emit(full.data);
      this.notif.success(notification ?? (this.isEditMode ? 'Zadanie zaktualizowane.' : 'Zadanie zostało utworzone.'));
    } catch {
      this.notif.error('Nie udało się zapisać zadania.');
    } finally {
      this.saving.set(false);
    }
  }

  async updateTaskInZohoDesk(): Promise<void> {
    await this.saveTask('Zadanie zostało wysłane do Zoho Desk');
    const full = await lastValueFrom(this.zohoService.updateTicket(this.task ?? new TaskDTO));
    this.task = full.data ?? this.task;
  }

  undoChanges(): void {
    this.resetForm();
    this.selectedProjectId.set(this.task?.projectId ?? null);
  }

  // ── zmiana statusu (edit — szybka akcja) ─────────────────────────────────

  changeState(stateValue: string): void {
    if (!this.task) return;
    const dto: TaskDTO = {
      ...this.task,
      state:         stateValue,
      progress:      stateValue === 'Zakonczone' ? 100 : this.task.progress,
      dueDateString: this.task.dueDate
        ? formatDate(this.task.dueDate, DATE_TO_BACKEND_FORMAT, 'en-US') : undefined,
    } as TaskDTO;
    this.svc.update(this.task.id, dto).subscribe((res) => {
      if (res.succeeded && res.data) {
        this.task = res.data;
        this.resetForm();
        this.taskUpdated.emit(res.data);
        this.saved.emit(res.data);
      } else {
        this.notif.error(getJoinedMesseges("Nie zaktualizowano statusu", res.errors));
      }
    }, (err) => {
      this.notif.error("Nie zaktualizowano statusu");
    });
  }

  async onContractorChange(): Promise<void> {
    this.form.patchValue({ contractorContactId: null });
    const id = this.form.value.contractorId;
    if (id) {
      await Promise.all([
        this.loadContractorContacts(id),
        this.loadContractorSummary(id),
      ]);
    } else {
      this.contractorContacts = [];
      this.contractorSummary.set(null);
    }
  }

  private async loadContractorContacts(contractorId: number): Promise<void> {
    this.contractorContacts = await this.contractorsSvc.getContractorContacts(contractorId) ?? [];
  }

  private async loadContractorSummary(contractorId: number): Promise<void> {
    this.contractorSummaryLoading.set(true);
    try {
      const result = await this.contractorsSvc.getContractorDetails(contractorId);
      this.contractorSummary.set(result?.data ?? null);
    } catch {
      this.contractorSummary.set(null);
    } finally {
      this.contractorSummaryLoading.set(false);
    }
  }

  // ── komentarze ────────────────────────────────────────────────────────────

  addComment(): void {
    if (!this.task) return;
    const content = this.commentControl.value?.trim();
    if (!content) return;
    this.svc.addComment(this.task.id, content).subscribe(res => {
      if (res.succeeded && res.data)
        this.task = { ...this.task!, comments: [...(this.task!.comments ?? []), res.data] };
      this.commentControl.reset();
    });
  }

  startEditComment(c: TaskCommentDTO): void {
    this.editingCommentId.set(c.id);
    this.editCommentControl.setValue(c.content);
  }

  saveEditComment(c: TaskCommentDTO): void {
    if (!this.task) return;
    const content = this.editCommentControl.value?.trim();
    if (!content) return;
    this.svc.updateComment(this.task.id, c.id, content).subscribe(() => {
      this.task = { ...this.task!, comments: this.task!.comments.map(x => x.id === c.id ? { ...x, content } : x) };
      this.editingCommentId.set(null);
    });
  }

  deleteComment(c: TaskCommentDTO): void {
    if (!this.task) return;
    this.svc.deleteComment(c.id).subscribe(() => {
      this.task = { ...this.task!, comments: this.task!.comments.filter(x => x.id !== c.id) };
    });
  }

  // ── helpers ───────────────────────────────────────────────────────────────

  isDueDateOverdue(): boolean {
    const d = this.form.value.dueDate;
    if (d) return (d as Date) < new Date();
    return !!this.task?.dueDate && new Date(this.task.dueDate) < new Date();
  }

  isImportedDueDateOverdue(): boolean {
    return !!this.task?.importedTask?.responseDueDate
      && new Date(this.task.importedTask.responseDueDate) < new Date();
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

  hoursBarClass(pct: number | null): string {
    if (pct === null) return '';
    if (pct >= 90) return 'hours-bar-critical';
    if (pct >= 70) return 'hours-bar-warning';
    return 'hours-bar-ok';
  }

  initials(name?: string | null): string {
    if (!name) return '?';
    return name.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase();
  }

  dateFilter = (d: Date | null): boolean => {
    const today = new Date(); today.setHours(0, 0, 0, 0);
    return (d ?? new Date()) >= today;
  };

  closePanel(): void {
    this.close.emit();
    this.cancelled.emit();
  }

  engagementTypeLabel(type: number): string {
      return this.engagementTypes.find(et => et.value === type)?.name
          ?? type.toString();
  }
}