import { formatDate, Location } from '@angular/common';
import { Component, computed, OnDestroy, OnInit, signal } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { distinctUntilChanged, map, Observable, Subject, takeUntil, tap } from 'rxjs';
import { AuthService } from 'src/app/core/services/auth.service';
import { ContractorsService } from 'src/app/core/services/contractors.service';
import { ProjectsService } from 'src/app/core/services/projects.service';
import { DATE_TO_BACKEND_FORMAT, getJoinedMesseges } from 'src/app/core/services/extensions.service';
import { defaultSnackBarConfig, NotificationService } from 'src/app/core/services/notification.service';
import { DeviceService } from 'src/app/core/services/device.service';
import { ContractorContracts } from 'src/models/ContractorContracts';
import { ContractorLicenses } from 'src/models/ContractorLicenses';
import { ContractorContacts } from 'src/models/ContractorContacts';
import { Contractors } from 'src/models/Contractors';
import { Projects } from 'src/models/Projects';
import { ValueNameDTO } from 'src/models/DTO/ValueNameDTO';
import { EEngagementType } from 'src/models/enums/EEngagementType';
import { ContractorHoursSnapshots } from 'src/models/ContractorHoursSnapshots';
import { ToolsService } from 'src/app/core/services/tools.service';
import { CatalogItems } from 'src/models/CatalogItems';
import { CatalogItemsService } from 'src/app/core/services/catalog-items.service';
import { ELicenceType } from 'src/models/enums/ELicenceType';
import { AdditionalFieldDTO } from 'src/models/DTO/AdditionalFieldDTO';
import { AdditionalFieldsService, TableNames } from 'src/app/core/services/additional-fields.service';
import { AdditionalFieldValueDTO } from 'src/models/DTO/AdditionalFieldValueDTO';
import { EValueType } from 'src/models/enums/EValueType';
import { EBillingType } from 'src/models/enums/EBillingType';

@Component({
  selector: 'crm-contractor-pane',
  standalone: false,
  templateUrl: './contractor-pane.component.html',
  styleUrl: './contractor-pane.component.css'
})
export class ContractorPaneComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  contractor: Contractors = new Contractors();
  id: number = 0;
  userId: number = 0;
  altNip?: string;
  headerLabel: string = '';
  savingContractor = false;
  savingContacts = false;

  selectedView?: (ValueNameDTO & { kicker?: string });

  contractorContactsDataSource: ContractorContacts[] = [];
  contractorLicensesDataSource: ContractorLicenses[] = [];
  contractorContractsDataSource: ContractorContracts[] = [];
  contractorProjectsDataSource: Projects[] = [];
  EEngagementType = EEngagementType;
  EValueType = EValueType;
  private openSnapshotPanels = new Set<number>();
  private snapshotsCache = new Map<number, ContractorHoursSnapshots[]>();
  snapshotHoursInput: Record<number, number> = {};
  savingSnapshot: number | null = null;
  loadingSnapshots: number | null = null;

  newLicense: ContractorLicenses | null = null;
  newContract: ContractorContracts | null = null;
  savingLicense = false;
  savingContract = false;
  catalogItems: CatalogItems[] = [];
  licenseTypeOptions: ValueNameDTO[] = [];
  engagementTypeOptions: ValueNameDTO[] = [];
  filteredEngagementTypes: ValueNameDTO[] = [];
  renewalTypeOptions: ValueNameDTO[] = [];
  billingTypes: ValueNameDTO[] = [];
  additionalFields: AdditionalFieldValueDTO[] = [];

  expandedContractId: number | null = null;
  editingContractId: number | null = null;
  editingContractCopy: Partial<ContractorContracts> | null = null;
  editingLicenseId: number | null = null;
  editingLicenseCopy: ContractorLicenses | null = null;
  savingEditContract = false;
  savingEditLicense = false;
  loadingContracts = false;

  readonly menuItems: (ValueNameDTO & { kicker?: string })[] = [
    { name: 'Dane kontrahenta', kicker: 'Identyfikacja', icon: 'person_card', key: 'contractor-data', value: 1 },
    { name: 'Osoby kontaktowe', kicker: 'Relacje', icon: 'contact_mail', key: 'contractor-contacts', value: 2 },
    { name: 'Licencje i dodatki', kicker: 'Produkty', icon: 'shopping_cart', key: 'catalog-items', value: 3 },
    { name: 'Umowy i pakiety', kicker: 'Współpraca', icon: 'contract', key: 'contracts', value: 4 },
    { name: 'Projekty', kicker: 'Wdrożenia', icon: 'strategy', key: 'projects', value: 6 },
  ];

  readonly isMobile$: Observable<boolean>;
  readonly canContractorsUpdate: boolean;
  readonly canContractorsCreate: boolean;

  constructor(
    private activatedRoute: ActivatedRoute,
    private auth: AuthService,
    private contractorsService: ContractorsService,
    private projectsService: ProjectsService,
    private location: Location,
    private router: Router,
    private notificationService: NotificationService,
    private device: DeviceService,
    private toolsService: ToolsService,
    private catalogItemsService: CatalogItemsService,
    private additionalFieldsService: AdditionalFieldsService
  ) {
    this.canContractorsUpdate = this.auth.userConfiguration?.canContractorsUpdate ?? false;
    this.canContractorsCreate = this.auth.userConfiguration?.canContractorsCreate ?? false;
    this.isMobile$ = this.device.isMobile$;
    this.selectedView = this.menuItems[0];
    this.toolsService.getEnumValues('ELicenceType').then(res => this.licenseTypeOptions = res ?? []);
    this.toolsService.getEnumValues('EEngagementType').then(res => {
      this.engagementTypeOptions = res ?? []
      this.filteredEngagementTypes = this.engagementTypeOptions.filter(e => e.value !== EEngagementType.None);
    });
    this.toolsService.getEnumValues('EBillingType').then(res => {
      this.billingTypes = res ?? [];
    });
    this.toolsService.getEnumValues('ERenewalType').then(res => {
      this.renewalTypeOptions = res ?? [];
    });
  }

  ngOnInit() {
    this.userId = this.auth.user?.id ?? 0;
    this.detectIdFromRoute();
    this.getCatalogItems();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Routing ───────────────────────────────────────────────────────────────

  detectIdFromRoute() {
    this.activatedRoute.paramMap.pipe(
      map(params => {
        const v = params.get('id');
        return v ? Number(v) : 0;
      }),
      distinctUntilChanged(),
      tap(id => {
        this.id = id;
        if (id) this.loadContractor();
        else {
          this.contractor = new Contractors();
          this.headerLabel = 'Nowy kontrahent';
        }
        this.getAdditionalFields();
      }),
      takeUntil(this.destroy$)
    ).subscribe();
  }

  // ── Data loading ──────────────────────────────────────────────────────────

  loadContractor() {
    this.contractorsService.getById(this.id).then(res => {
      if (res?.succeeded && res.data) {
        this.contractor = res.data;
        this.headerLabel = this.contractor.displayName;
        this.loadAllSubData();
      } else {
        this.notificationService.error(getJoinedMesseges('Nie znaleziono kontrahenta', res?.errors));
      }
    });
  }

  loadAllSubData() {
    this.getContractorContacts();
    this.getContractorLicenses();
    this.getContractorContracts();
    this.getContractorProjects();
  }

  getAdditionalFields() {
    this.additionalFieldsService.getValuesForRecord(TableNames.Contractors, this.id).then(res => {
      this.additionalFields = res ?? [];
    });
  }

  getCatalogItems() {
    this.catalogItemsService.getList().then((res) => {
      this.catalogItems = res ?? [];
    });
  }

  getContractorContacts() {
    this.contractorsService.getContractorContacts(this.id).then(res => {
      this.contractorContactsDataSource = res ?? [];
    });
  }

  getContractorLicenses() {
    this.contractorsService.getContractorLicenses(this.id).then(res => {
      this.contractorLicensesDataSource = res ?? [];
    });
  }

  async getContractorContracts(): Promise<void> {
    this.loadingContracts = true;
    try {
      const result = await this.contractorsService.getContractorContracts(this.id);
      if (result && result.items) {
        this.contractorContractsDataSource = result.items;

        for (const c of result.items) {
          this.snapshotsCache.set(c.id, c.snapshots ?? []);
        }
      }
    } finally {
      this.loadingContracts = false;
    }
  }

  getContractorProjects() {
    this.projectsService.getByContractorId(this.id).then(res => {
      this.contractorProjectsDataSource = res ?? [];
    });
  }

  async saveContractor() {
    if (!this.contractor.code || !this.contractor.name || !this.contractor.nip) {
      this.notificationService.error('Zdefiniuj kod, nazwę i NIP kontrahenta');
      return;
    }
    if (this.altNip?.length && !this.contractor.alternativeNipNumbers?.includes(this.altNip)) {
      this.addAlternativeNip();
    }

    this.savingContractor = true;
    try {
      this.contractor.additionalFieldValues = this.additionalFields;

      if (this.id) {
        const result = await this.contractorsService.update(this.contractor);
        if (result?.succeeded) {
          this.notificationService.success('Zaktualizowano kontrahenta ' + this.contractor.displayName);
        } else {
          this.notificationService.error(getJoinedMesseges('Nie zaktualizowano kontrahenta ' + this.contractor.displayName, result?.errors));
          this.loadContractor();
        }
      } else {
        const result = await this.contractorsService.add(this.contractor);
        if (result?.succeeded && result.data) {
          this.notificationService.success('Dodano kontrahenta ' + this.contractor.displayName);
          this.router.navigate(['./contractor-pane', result.data.id], { replaceUrl: true });
        } else {
          this.notificationService.error(getJoinedMesseges('Nie dodano kontrahenta ' + this.contractor.displayName, result?.errors));
        }
      }
    } finally {
      this.savingContractor = false;
    }
  }

  async saveContractorContacts() {
    this.savingContacts = true;
    try {
      const data = await this.contractorsService.saveContractorContacts(
        this.contractor.id, this.contractorContactsDataSource
      );
      if (data?.succeeded) {
        this.notificationService.success('Zaktualizowano osoby kontaktowe');
        this.getContractorContacts();
      } else {
        this.notificationService.error(
          getJoinedMesseges('Nie zaktualizowano osób kontaktowych', data?.errors),
          defaultSnackBarConfig
        );
      }
    } finally {
      this.savingContacts = false;
    }
  }

  addNewContractorContact() {
    this.contractorContactsDataSource = [
      ...this.contractorContactsDataSource,
      { ...new ContractorContacts(), isEditMode: true } as ContractorContacts & { isEditMode: boolean }
    ];
  }

  removeContractorContact(element: ContractorContacts) {
    this.contractorContactsDataSource = this.contractorContactsDataSource.filter(x => x !== element);
  }

  onContactNameChange(element: ContractorContacts, field: 'firstname' | 'lastname') {
    element.displayName = `${element.lastname ?? ''} ${element.firstname ?? ''}`.trim();
  }

  addAlternativeNip() {
    if (!this.altNip) return;
    this.contractor.alternativeNipNumbers ??= [];
    if (this.contractor.nip === this.altNip) {
      this.notificationService.warning('Ten NIP jest już ustawiony jako główny');
      return;
    }
    if (this.contractor.alternativeNipNumbers.includes(this.altNip)) {
      this.notificationService.warning('Ten NIP już istnieje na liście');
      return;
    }
    this.contractor.alternativeNipNumbers = [...this.contractor.alternativeNipNumbers, this.altNip];
    this.altNip = '';
  }

  removeNip(idx: number) {
    this.contractor.alternativeNipNumbers?.splice(idx, 1);
    this.contractor.alternativeNipNumbers = [...(this.contractor.alternativeNipNumbers ?? [])];
  }

  onContractorNameChange() {
    if (!this.contractor.displayName && this.contractor.code) {
      this.contractor.displayName = this.contractor.code;
    }
  }


  selectView(item: ValueNameDTO) {
    this.selectedView = item;
  }

  get contractorInitials(): string {
    const name = this.contractor?.displayName || this.contractor?.name || '';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return name.slice(0, 2).toUpperCase();
  }

  contactInitials(c: ContractorContacts): string {
    const f = c.firstname?.[0] ?? '';
    const l = c.lastname?.[0] ?? '';
    return (l + f).toUpperCase() || '?';
  }

  getBadge(key: string): number {
    switch (key) {
      case 'contractor-contacts': return this.contractorContactsDataSource.length;
      case 'catalog-items': return this.contractorLicensesDataSource.length;
      case 'contracts': return this.contractorContractsDataSource.length;
      case 'projects': return this.contractorProjectsDataSource.length;
      default: return 0;
    }
  }

  contractIcon(type: EEngagementType): string {
    switch (type) {
      case EEngagementType.HoursPackage: return 'timer';
      case EEngagementType.MonthlyBilling: return 'calendar_month';
      case EEngagementType.Contract: return 'description';
      default: return 'handshake';
    }
  }

  hoursPercent(c: ContractorContracts): number {
    if (!c.hoursLimit) return 0;
    return Math.min(100, Math.round(((c.hoursUsed ?? 0) / c.hoursLimit) * 100));
  }


  isContractExpired(c: ContractorContracts): boolean {
    if (!c.validTo) return false;
    return new Date(c.validTo) < new Date();
  }

  isContractActive(c: ContractorContracts): boolean {
    const now = new Date();
    const from = c.validFrom ? new Date(c.validFrom) : null;
    const to = c.validTo ? new Date(c.validTo) : null;
    if (from && from > now) return false;
    if (to && to < now) return false;
    return true;
  }

  isSnapshotPanelOpen(c: ContractorContracts): boolean {
    return this.openSnapshotPanels.has(c.id);
  }

  getSnapshots(c: ContractorContracts): ContractorHoursSnapshots[] {
    return this.snapshotsCache.get(c.id) ?? [];
  }

  snapshotPercent(s: ContractorHoursSnapshots, c: ContractorContracts): number {
    if (!c.hoursLimit) return 0;
    return Math.min(100, Math.round((s.hoursUsed / c.hoursLimit) * 100));
  }

  async toggleSnapshots(c: ContractorContracts): Promise<void> {
    if (this.openSnapshotPanels.has(c.id)) {
      this.openSnapshotPanels.delete(c.id);
      return;
    }

    this.openSnapshotPanels.add(c.id);
    this.snapshotHoursInput[c.id] ??= c.hoursUsed ?? 0;

    if (!this.snapshotsCache.has(c.id)) {
      this.loadingSnapshots = c.id;
      try {
        const data = await this.contractorsService.getContractSnapshots(c.id);
        this.snapshotsCache.set(c.id, data ?? []);
      } finally {
        this.loadingSnapshots = null;
      }
    }
  }

  async saveSnapshot(c: ContractorContracts): Promise<void> {
    const hoursUsed = this.snapshotHoursInput[c.id];
    if (hoursUsed == null || hoursUsed < 0) {
      this.notificationService.error('Podaj liczbę godzin');
      return;
    }
    if (!c.allowOverLimit && c.hoursLimit > 0 && hoursUsed > c.hoursLimit) {
      this.notificationService.error(
        `Godziny (${hoursUsed}h) przekraczają limit (${c.hoursLimit}h). ` +
        `Włącz opcję "Bez twardego limitu" w ustawieniach umowy.`
      );
      return;
    }

    this.savingSnapshot = c.id;
    try {
      const result = await this.contractorsService.addContractSnapshot(c.id, hoursUsed, this.userId);
      if (result?.succeeded && result.data) {
        this.notificationService.success('Zaktualizowano stan godzin');

        // Aktualizuj pola na obiekcie (bez reloadu całej listy)
        c.hoursUsed = result.data.hoursUsed;
        c.hoursRemaining = result.data.hoursRemaining;
        c.hoursUsedCurrentMonth = hoursUsed;
        c.hoursRemainingCurrentMonth = c.hoursLimit - hoursUsed;

        // Prepend do cache
        const existing = this.snapshotsCache.get(c.id) ?? [];
        this.snapshotsCache.set(c.id, [result.data, ...existing]);
      } else {
        this.notificationService.error(result?.errors?.join('\n') ?? 'Nie zapisano');
      }
    } finally {
      this.savingSnapshot = null;
    }
  }


  openAddLicenseDialog() {
    this.newLicense = {
      id: 0,
      contractorId: this.id,
      catalogItemId: 0,
      licenseType: ELicenceType.PerSeat,
      quantity: 1,
      serialNumber: '',
      issuedAt: new Date(),
      validFrom: new Date(),
      expiresAt: null,
      warrantyEndDate: null,
      warrantyStatus: null,
      upgradeDate: null,
    } as unknown as ContractorLicenses;
  }

  cancelNewLicense() {
    this.newLicense = null;
  }

  async saveLicense() {
    if (!this.newLicense) return;
    if (!this.newLicense.catalogItemId) {
      this.notificationService.error('Wybierz produkt z katalogu');
      return;
    }
    if (!this.newLicense.licenseType) {
      this.notificationService.error('Wybierz typ licencji');
      return;
    }

    this.newLicense.contractorId = this.id;
    this.newLicense.serialNumber = "";
    if (this.newLicense.validFrom)
      this.newLicense.validFromStr = formatDate(this.newLicense.validFrom, DATE_TO_BACKEND_FORMAT, 'en-US');
    if (this.newLicense.expiresAt)
      this.newLicense.expiresAtStr = formatDate(this.newLicense.expiresAt, DATE_TO_BACKEND_FORMAT, 'en-US');
    if (this.newLicense.warrantyEndDate)
      this.newLicense.warrantyEndDateStr = formatDate(this.newLicense.warrantyEndDate, DATE_TO_BACKEND_FORMAT, 'en-US');
    if (this.newLicense.upgradeDate)
      this.newLicense.upgradeDateStr = formatDate(this.newLicense.upgradeDate, DATE_TO_BACKEND_FORMAT, 'en-US');

    this.savingLicense = true;
    try {
      const result = await this.contractorsService.addLicense(this.newLicense, this.userId);
      if (result?.succeeded && result.data) {
        this.notificationService.success('Dodano licencję');
        this.contractorLicensesDataSource = [result.data, ...this.contractorLicensesDataSource];
        this.newLicense = null;
      } else {
        this.notificationService.error(getJoinedMesseges('Nie dodano licencji', result?.errors));
      }
    } finally {
      this.savingLicense = false;
    }
  }

  openAddContractDialog() {
    this.newContract = {
      ...new ContractorContracts(),
      contractorId: this.id,
      engagementType: EEngagementType.Contract,
      hoursLimit: 0,
    } as ContractorContracts;
  }

  cancelNewContract() {
    this.newContract = null;
  }

  async saveContract() {
    if (!this.newContract) return;

    this.savingContract = true;
    this.newContract.contractNumber = "";
    if (this.newContract.validFrom)
      (this.newContract as any).validFromStr = formatDate(this.newContract.validFrom, DATE_TO_BACKEND_FORMAT, 'en-US');
    if (this.newContract.validTo)
      (this.newContract as any).validToStr = formatDate(this.newContract.validTo, DATE_TO_BACKEND_FORMAT, 'en-US');

    try {
      const result = await this.contractorsService.addContract(this.newContract, this.userId);
      if (result?.succeeded && result.data) {
        this.savingContract = false;
        this.notificationService.success('Dodano umowę');
        result.data.hoursUsed = 0;
        result.data.hoursUsedCurrentMonth = 0;
        result.data.hoursRemaining = result.data.hoursLimit;
        result.data.hoursRemainingCurrentMonth = result.data.hoursLimit;
        this.contractorContractsDataSource = [result.data, ...this.contractorContractsDataSource];
        this.newContract = null;
      } else {
        this.savingContract = false;
        this.notificationService.error(getJoinedMesseges('Nie dodano umowy', result?.errors));
      }
    } finally {
      this.savingContract = false;
    }
  }

  hoursMonthPercent(c: ContractorContracts): number {
    const limit = (c as any).hoursStarting ?? c.hoursLimit ?? 0;
    const used = (c as any).hoursUsedCurrentMonth ?? c.hoursUsed ?? 0;
    if (!limit) return 0;
    return Math.round((used / limit) * 100);
  }

  min100(v: number): number {
    return Math.min(100, v);
  }

  getRenewalLabel(renewalType: number): string {
    return this.renewalTypeOptions.find(r => r.value === renewalType)?.name ?? '—';
  }

  toggleExpand(c: ContractorContracts): void {
    if (this.expandedContractId === c.id) {
      this.expandedContractId = null;
      this.editingContractId = null;
      this.editingContractCopy = null;
      return;
    }
    this.expandedContractId = c.id;
    this.snapshotHoursInput[c.id] ??= c.hoursUsedCurrentMonth ?? c.hoursUsed ?? 0;

    if (!this.snapshotsCache.has(c.id) || this.snapshotsCache.get(c.id)!.length === 0 && c.snapshots.length === 0) {
      this.loadingSnapshots = c.id;
      this.contractorsService.getContractSnapshots(c.id).then(data => {
        this.snapshotsCache.set(c.id, data ?? []);
        this.loadingSnapshots = null;
      });
    }
  }

  openEditContract(c: ContractorContracts, event: MouseEvent): void {
    event.stopPropagation();
    if (this.editingContractId === c.id) {
      this.cancelEditContract();
      return;
    }
    this.expandedContractId = c.id;
    this.editingContractId = c.id;
    this.editingContractCopy = { ...c };
    if (!this.snapshotsCache.has(c.id)) {
      this.loadingSnapshots = c.id;
      this.contractorsService.getContractSnapshots(c.id).then(data => {
        this.snapshotsCache.set(c.id, data ?? []);
        this.loadingSnapshots = null;
      });
    }
  }

  cancelEditContract(): void {
    this.editingContractId = null;
    this.editingContractCopy = null;
  }

  async saveEditContract(): Promise<void> {
    if (!this.editingContractCopy) return;
    this.savingEditContract = true;
    try {
      if (this.editingContractCopy.validFrom)
        (this.editingContractCopy as any).validFromStr = formatDate(
          this.editingContractCopy.validFrom, DATE_TO_BACKEND_FORMAT, 'en-US');
      if (this.editingContractCopy.validTo)
        (this.editingContractCopy as any).validToStr = formatDate(
          this.editingContractCopy.validTo, DATE_TO_BACKEND_FORMAT, 'en-US');
      if ((this.editingContractCopy as any).renewalDate)
        (this.editingContractCopy as any).renewalDateStr = formatDate(
          (this.editingContractCopy as any).renewalDate, DATE_TO_BACKEND_FORMAT, 'en-US');

      const result = await this.contractorsService.updateContract(
        this.editingContractCopy as ContractorContracts, this.userId);
      if (result?.succeeded && result.data) {
        const idx = this.contractorContractsDataSource.findIndex(
          x => x.id === this.editingContractCopy!.id);
        if (idx > -1) this.contractorContractsDataSource[idx] = result.data;
        this.contractorContractsDataSource = [...this.contractorContractsDataSource];
        this.notificationService.success('Zaktualizowano umowę');
        this.cancelEditContract();
      } else {
        this.notificationService.error(getJoinedMesseges('Nie zapisano umowy', result?.errors));
      }
    } finally {
      this.savingEditContract = false;
    }
  }

  openEditLicense(l: ContractorLicenses, event: MouseEvent): void {
    event.stopPropagation();
    if (this.editingLicenseId === l.id) {
      this.cancelEditLicense();
      return;
    }
    this.editingLicenseId = l.id;
    this.editingLicenseCopy = { ...l };
  }

  cancelEditLicense(): void {
    this.editingLicenseId = null;
    this.editingLicenseCopy = null;
  }

  async saveEditLicense(): Promise<void> {
    if (!this.editingLicenseCopy) return;
    this.savingEditLicense = true;
    try {
      if (this.editingLicenseCopy.validFrom)
        this.editingLicenseCopy.validFromStr = formatDate(
          this.editingLicenseCopy.validFrom, DATE_TO_BACKEND_FORMAT, 'en-US');
      if (this.editingLicenseCopy.expiresAt)
        this.editingLicenseCopy.expiresAtStr = formatDate(
          this.editingLicenseCopy.expiresAt, DATE_TO_BACKEND_FORMAT, 'en-US');
      if (this.editingLicenseCopy.warrantyEndDate)
        this.editingLicenseCopy.warrantyEndDateStr = formatDate(
          this.editingLicenseCopy.warrantyEndDate, DATE_TO_BACKEND_FORMAT, 'en-US');
      if (this.editingLicenseCopy.upgradeDate)
        this.editingLicenseCopy.upgradeDateStr = formatDate(
          this.editingLicenseCopy.upgradeDate, DATE_TO_BACKEND_FORMAT, 'en-US');

      const result = await this.contractorsService.updateLicense(
        this.editingLicenseCopy, this.userId);
      if (result?.succeeded && result.data) {
        const idx = this.contractorLicensesDataSource.findIndex(
          x => x.id === this.editingLicenseCopy!.id);
        if (idx > -1) this.contractorLicensesDataSource[idx] = result.data;
        this.contractorLicensesDataSource = [...this.contractorLicensesDataSource];
        this.notificationService.success('Zaktualizowano licencję');
        this.cancelEditLicense();
      } else {
        this.notificationService.error(getJoinedMesseges('Nie zapisano licencji', result?.errors));
      }
    } finally {
      this.savingEditLicense = false;
    }
  }

  onEngagementTypeChange(c: ContractorContracts) {
    if (c.engagementType == EEngagementType.MonthlyBilling) {
      c.billingType = EBillingType.Monthly;
    }
  }
}