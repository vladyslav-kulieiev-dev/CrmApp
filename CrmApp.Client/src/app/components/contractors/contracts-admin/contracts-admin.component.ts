import {
  Component, OnInit, OnDestroy, signal, computed, ViewChild
} from '@angular/core';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { FormControl } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { formatDate } from '@angular/common';
import { DATE_TO_BACKEND_FORMAT, getJoinedMesseges } from 'src/app/core/services/extensions.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { ContractorContracts } from 'src/models/ContractorContracts';
import { ContractorLicenses } from 'src/models/ContractorLicenses';
import { ContractorContractsRequest } from 'src/models/DTO/contractors/ContractorContractsRequest';
import { ValueNameDTO } from 'src/models/DTO/ValueNameDTO';
import { EEngagementType } from 'src/models/enums/EEngagementType';
import { UsersService } from 'src/app/core/services/users.service';
import { AuthService } from 'src/app/core/services/auth.service';

type AdminTab = 'contracts' | 'licenses';

@Component({
  selector: 'crm-contracts-admin',
  templateUrl: './contracts-admin.component.html',
  styleUrls: ['./contracts-admin.component.css'],
  standalone: false
})
export class ContractsAdminComponent implements OnInit, OnDestroy {

  private readonly destroy$ = new Subject<void>();

  // ── Auth ─────────────────────────────────────────────────────────────────
  userId: number;

  // ── Tabs ──────────────────────────────────────────────────────────────────
  activeTab: AdminTab = 'contracts';

  readonly tabs: ValueNameDTO[] = [
    { name: 'Umowy i pakiety', key: 'contracts',  value: 0 },
    { name: 'Licencje i dodatki', key: 'licenses', value: 1 },
  ];

  // ── Contracts table ───────────────────────────────────────────────────────
  contracts: ContractorContracts[] = [];
  totalContracts = 0;
  loadingContracts = false;

  readonly searchCtrl = new FormControl('');

  req: ContractorContractsRequest = {
    page: 1, pageSize: 25,
    sortBy: 'validFrom', sortDescending: false
  };

  readonly engagementTypeOptions: ValueNameDTO[] = [
    { name: 'Pakiet godzin',          value: EEngagementType.HoursPackage,    key: 'hours'   },
    { name: 'Rozliczenie miesięczne', value: EEngagementType.MonthlyBilling,  key: 'monthly' },
    { name: 'Umowa',                  value: EEngagementType.Contract,        key: 'contract'},
  ];

  selectedEngagementTypes = signal<number[]>([]);
  activeOnly              = signal(true);
  hasHoursDebt            = signal(false);
  isOverLimit             = signal(false);

  readonly activeFiltersCount = computed(() =>
    this.selectedEngagementTypes().length +
    (this.activeOnly()     ? 0 : 1) +
    (this.hasHoursDebt()   ? 1 : 0) +
    (this.isOverLimit()    ? 1 : 0)
  );

  // ── Edit / add panel ──────────────────────────────────────────────────────
  panelOpen = false;
  panelMode: 'edit' | 'add' = 'edit';
  editingContract: Partial<ContractorContracts> | null = null;
  savingContract = false;

  // ── Snapshots inline ──────────────────────────────────────────────────────
  expandedContractId: number | null = null;
  snapshotsCache = new Map<number, any[]>();
  loadingSnapshots: number | null = null;
  snapshotHoursInput: Record<number, number> = {};
  savingSnapshot: number | null = null;

  // ── Licenses table ────────────────────────────────────────────────────────
  licenses: ContractorLicenses[] = [];
  totalLicenses = 0;
  loadingLicenses = false;

  readonly renewalTypeOptions: ValueNameDTO[] = [
    { name: 'Jednorazowy', value: 0, key: '0' },
    { name: 'Miesięczne',  value: 1, key: '1' },
    { name: 'Roczne',      value: 2, key: '2' },
  ];

  EEngagementType = EEngagementType;

  constructor(
    //private contractsService: ContractorsContractsService,
    //private licensesService: ContractorsLicensesService,
    private notificationService: NotificationService,
    private auth: AuthService,
  ) {
    this.userId = this.auth.user?.id ?? 0;
  }

  ngOnInit(): void {
    // Debounce search
    this.searchCtrl.valueChanges.pipe(
      debounceTime(350),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.req.page = 1;
      this.load();
    });

    this.load();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Load ─────────────────────────────────────────────────────────────────

  load(): void {
    if (this.activeTab === 'contracts') this.loadContracts();
    else this.loadLicenses();
  }

  async loadContracts(): Promise<void> {
    // this.loadingContracts = true;
    // try {
    //   const result = await this.contractsService.getContractsPaged({
    //     ...this.req,
    //     search: this.searchCtrl.value?.trim() || undefined,
    //     engagementTypes: this.selectedEngagementTypes().length
    //       ? this.selectedEngagementTypes() : undefined,
    //     activeOnly:             this.activeOnly()   || undefined,
    //     hasHoursDebt:           this.hasHoursDebt() || undefined,
    //     isOverLimitCurrentMonth: this.isOverLimit() || undefined,
    //   });
    //   if (result?.succeeded && result.data) {
    //     this.contracts      = result.data.items;
    //     this.totalContracts = result.data.totalCount;
    //   }
    // } finally {
    //   this.loadingContracts = false;
    // }
  }

  async loadLicenses(): Promise<void> {
    this.loadingLicenses = true;
    try {
      // TODO: implement paged licenses endpoint
    } finally {
      this.loadingLicenses = false;
    }
  }

  // ── Tabs ──────────────────────────────────────────────────────────────────

  switchTab(tab: AdminTab): void {
    this.activeTab = tab;
    this.closePanel();
    this.load();
  }

  // ── Filter toggles ────────────────────────────────────────────────────────

  toggleEngagementType(value: number): void {
    const cur = this.selectedEngagementTypes();
    this.selectedEngagementTypes.set(
      cur.includes(value) ? cur.filter(v => v !== value) : [...cur, value]
    );
    this.req.page = 1;
    this.loadContracts();
  }

  toggleActiveOnly(): void {
    this.activeOnly.set(!this.activeOnly());
    this.req.page = 1;
    this.loadContracts();
  }

  toggleHoursDebt(): void {
    this.hasHoursDebt.set(!this.hasHoursDebt());
    this.req.page = 1;
    this.loadContracts();
  }

  toggleOverLimit(): void {
    this.isOverLimit.set(!this.isOverLimit());
    this.req.page = 1;
    this.loadContracts();
  }

  clearFilters(): void {
    this.searchCtrl.setValue('');
    this.selectedEngagementTypes.set([]);
    this.activeOnly.set(true);
    this.hasHoursDebt.set(false);
    this.isOverLimit.set(false);
    this.req.page = 1;
    this.loadContracts();
  }

  // ── Pagination ────────────────────────────────────────────────────────────

  onPage(e: PageEvent): void {
    this.req.page     = e.pageIndex + 1;
    this.req.pageSize = e.pageSize;
    this.load();
  }

  // ── Sort ─────────────────────────────────────────────────────────────────

  sort(col: string): void {
    if (this.req.sortBy === col) this.req.sortDescending = !this.req.sortDescending;
    else { this.req.sortBy = col; this.req.sortDescending = false; }
    this.req.page = 1;
    this.loadContracts();
  }

  sortIcon(col: string): string {
    if (this.req.sortBy !== col) return 'unfold_more';
    return this.req.sortDescending ? 'keyboard_arrow_down' : 'keyboard_arrow_up';
  }

  // ── Edit panel ────────────────────────────────────────────────────────────

  openAdd(): void {
    this.editingContract = {
      engagementType: EEngagementType.HoursPackage,
      hoursLimit: 0, allowOverLimit: false, renewalType: 1
    };
    this.panelMode = 'add';
    this.panelOpen = true;
  }

  openEdit(c: ContractorContracts, event: MouseEvent): void {
    event.stopPropagation();
    this.editingContract = { ...c };
    this.panelMode = 'edit';
    this.panelOpen = true;
  }

  closePanel(): void {
    this.panelOpen = false;
    this.editingContract = null;
  }

  async savePanel(): Promise<void> {
    if (!this.editingContract) return;
    this.savingContract = true;
    // try {
    //   if (this.editingContract.validFrom)
    //     (this.editingContract as any).validFromStr =
    //       formatDate(this.editingContract.validFrom, DATE_TO_BACKEND_FORMAT, 'en-US');
    //   if (this.editingContract.validTo)
    //     (this.editingContract as any).validToStr =
    //       formatDate(this.editingContract.validTo, DATE_TO_BACKEND_FORMAT, 'en-US');
    //   if ((this.editingContract as any).renewalDate)
    //     (this.editingContract as any).renewalDateStr =
    //       formatDate((this.editingContract as any).renewalDate, DATE_TO_BACKEND_FORMAT, 'en-US');

    //   const result = this.panelMode === 'add'
    //     ? await this.contractsService.addContract(this.editingContract as ContractorContracts)
    //     : await this.contractsService.updateContract(this.editingContract as ContractorContracts);

    //   if (result?.succeeded) {
    //     this.notificationService.success(
    //       this.panelMode === 'add' ? 'Dodano umowę' : 'Zaktualizowano umowę');
    //     this.closePanel();
    //     this.loadContracts();
    //   } else {
    //     this.notificationService.error(getJoinedMesseges('Nie zapisano', result?.errors));
    //   }
    // } finally {
    //   this.savingContract = false;
    // }
  }

  async deleteContract(c: ContractorContracts, event: MouseEvent): Promise<void> {
    // event.stopPropagation();
    // if (!confirm(`Usunąć umowę ${c.contractNumber}?`)) return;
    // const result = await this.contractsService.deleteContract(c.id);
    // if (result?.succeeded) {
    //   this.notificationService.success('Usunięto umowę');
    //   this.loadContracts();
    // } else {
    //   this.notificationService.error('Nie usunięto umowy');
    // }
  }

  // ── Row expand (snapshots) ────────────────────────────────────────────────

  toggleExpand(c: ContractorContracts): void {
    // if (this.expandedContractId === c.id) { this.expandedContractId = null; return; }
    // this.expandedContractId = c.id;
    // this.snapshotHoursInput[c.id] ??= c.hoursUsedCurrentMonth ?? 0;
    // if (!this.snapshotsCache.has(c.id)) {
    //   this.loadingSnapshots = c.id;
    //   this.contractsService.getContractSnapshots(c.id).then(data => {
    //     this.snapshotsCache.set(c.id, data ?? []);
    //     this.loadingSnapshots = null;
    //   });
    // }
  }

  getSnapshots(c: ContractorContracts): any[] {
    return this.snapshotsCache.get(c.id) ?? [];
  }

  async saveSnapshot(c: ContractorContracts): Promise<void> {
    // const h = this.snapshotHoursInput[c.id];
    // if (h == null || h < 0) { this.notificationService.error('Podaj godziny'); return; }
    // if (!c.allowOverLimit && c.hoursLimit > 0 && h > c.hoursLimit) {
    //   this.notificationService.error(`Przekroczono limit (${c.hoursLimit}h)`); return;
    // }
    // this.savingSnapshot = c.id;
    // try {
    //   const result = await this.contractsService.addContractSnapshot(c.id, h, this.userId);
    //   if (result?.succeeded && result.data) {
    //     c.hoursUsed = result.data.hoursUsed;
    //     c.hoursRemaining = result.data.hoursRemaining;
    //     c.hoursUsedCurrentMonth = h;
    //     c.hoursRemainingCurrentMonth = c.hoursLimit - h;
    //     const existing = this.snapshotsCache.get(c.id) ?? [];
    //     this.snapshotsCache.set(c.id, [result.data, ...existing]);
    //     this.notificationService.success('Zapisano');
    //   }
    // } finally { this.savingSnapshot = null; }
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  isHoursBased(c: ContractorContracts): boolean {
    return c.engagementType === EEngagementType.HoursPackage
        || c.engagementType === EEngagementType.MonthlyBilling;
  }

  isActive(c: ContractorContracts): boolean {
    const now = new Date();
    const from = c.validFrom ? new Date(c.validFrom) : null;
    const to   = c.validTo   ? new Date(c.validTo)   : null;
    return (!from || from <= now) && (!to || to > now);
  }

  hoursPercent(c: ContractorContracts): number {
    // if (!c.hoursLimit) return 0;
    // return Math.min(200, Math.round((c.hoursUsedCurrentMonth / c.hoursLimit) * 100));
    return 0;
  }

  min100(v: number): number { return Math.min(100, v); }

  contractIcon(t: EEngagementType): string {
    return t === EEngagementType.HoursPackage   ? 'timer'
         : t === EEngagementType.MonthlyBilling ? 'autorenew'
         : 'description';
  }

  renewalLabel(v: number): string {
    return this.renewalTypeOptions.find(r => r.value === v)?.name ?? '—';
  }

  trackById(_: number, c: ContractorContracts): number { return c.id; }
}