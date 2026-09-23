import {
    Component, OnInit, ViewChild, TemplateRef,
    signal, computed, AfterViewInit, OnDestroy
} from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subject, takeUntil } from 'rxjs';

import { AuthService } from 'src/app/core/services/auth.service';
import { ContractorsService } from 'src/app/core/services/contractors.service';
import { AdditionalFieldsService, TableNames } from 'src/app/core/services/additional-fields.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { DeviceService } from 'src/app/core/services/device.service';
import { ToolsService } from 'src/app/core/services/tools.service';

import { AdditionalFieldDTO } from 'src/models/DTO/AdditionalFieldDTO';
import { ContractorListItemDTO } from 'src/models/DTO/contractors/ContractorListItemDTO';
import { ContractorsPagedRequest } from 'src/models/DTO/contractors/ContractorsPagedRequest';
import { PagedResult } from 'src/models/DTO/lists/PagedResult';
import { TableConfig } from 'src/models/table/TableConfig';
import { TableStateChange } from 'src/models/table/TableState';
import { ValueNameDTO } from 'src/models/DTO/ValueNameDTO';
import { EValueType } from 'src/models/enums/EValueType';

@Component({
    selector: 'crm-contractors',
    standalone: false,
    templateUrl: './contractors.component.html',
    styleUrl: './contractors.component.css'
})
export class ContractorsComponent implements OnInit, AfterViewInit, OnDestroy {

    // ── Dane ──────────────────────────────────────────────────────────
    pagedData  = signal<PagedResult<ContractorListItemDTO> | null>(null);
    loading    = signal(true);

    // ── Additional fields ─────────────────────────────────────────────
    additionalFields = signal<AdditionalFieldDTO[]>([]);

    // ── Table config ──────────────────────────────────────────────────
    tableConfig = signal<TableConfig<ContractorListItemDTO> | null>(null);

    // ── Request ───────────────────────────────────────────────────────
    request: ContractorsPagedRequest = {
        page: 1,
        pageSize: 25,
        sortBy: 'displayName',
        sortDescending: false
    } as ContractorsPagedRequest;

    // ── UI ────────────────────────────────────────────────────────────
    selectedFile?: File;
    isMobile      = false;
    engagementTypes: ValueNameDTO[] = [];

    readonly canContractorsCreate: boolean;
    readonly canContractorsUpdate: boolean;
    readonly canContractorsDelete: boolean;
    readonly isMobile$: Observable<boolean>;

    private destroy$ = new Subject<void>();
    private userId: number;

    // ── Custom cell templates ─────────────────────────────────────────
    @ViewChild('nameCellTpl')        nameCellTpl!: TemplateRef<any>;
    @ViewChild('codeCellTpl')        codeCellTpl!: TemplateRef<any>;
    @ViewChild('engagementCellTpl')  engagementCellTpl!: TemplateRef<any>;
    @ViewChild('hoursCellTpl')       hoursCellTpl!: TemplateRef<any>;
    @ViewChild('actionsCellTpl')     actionsCellTpl!: TemplateRef<any>;

    constructor(
        private auth: AuthService,
        private contractorsService: ContractorsService,
        private additionalFieldsService: AdditionalFieldsService,
        private notificationService: NotificationService,
        private device: DeviceService,
        private toolsService: ToolsService,
        private router: Router
    ) {
        this.userId               = this.auth.user?.id ?? 0;
        this.canContractorsCreate = this.auth.userConfiguration?.canContractorsCreate ?? false;
        this.canContractorsUpdate = this.auth.userConfiguration?.canContractorsUpdate ?? false;
        this.canContractorsDelete = this.auth.userConfiguration?.canContractorsDelete ?? false;
        this.isMobile$            = this.device.isMobile$;

        this.isMobile$.pipe(takeUntil(this.destroy$))
            .subscribe(m => this.isMobile = m);
    }

    async ngOnInit() {
        const [fields, engTypes] = await Promise.all([
            this.additionalFieldsService.getFieldsForTable(TableNames.Contractors),
            this.toolsService.getEnumValues('EEngagementType')
        ]);

        this.additionalFields.set(fields ?? []);
        this.engagementTypes = engTypes ?? [];
        this.buildTableConfig();
    }

    ngAfterViewInit() {
        this.loadPage();
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    async loadPage() {
        this.loading.set(true);
        try {
            const result = await this.contractorsService
                .getContractorsPaged(this.request);
            this.pagedData.set(result ?? null);
        } finally {
            this.loading.set(false);
        }
    }
    buildTableConfig() {
        const config: TableConfig<ContractorListItemDTO> = {
            pagingMode:      'backend',
            defaultSortBy:   'displayName',
            defaultPageSize: 25,
            pageSizeOptions: [15, 25, 50, 100, 250],
            rowClickable:    false,
            emptyMessage:    'Brak kontrahentów spełniających kryteria',
            emptyIcon:       'person_search',
            columns: [
                {
                    key:          'displayName',
                    header:       'Nazwa',
                    sortable:     true,
                    filter:       { type: 'text', placeholder: 'Szukaj nazwy...' },
                    cellTemplate: this.nameCellTpl
                },
                {
                    key:          'code',
                    header:       'Kod',
                    sortable:     true,
                    filter:       { type: 'text', placeholder: 'Szukaj kodu...' },
                    cellTemplate: this.codeCellTpl,
                    width:        '130px'
                },
                {
                    key:      'nip',
                    header:   'NIP',
                    sortable: true,
                    filter:   { type: 'text', placeholder: 'Szukaj NIP...' },
                    getValue: row => row.nip ?? '—',
                    width:    '140px'
                },
                {
                    key:          'engagementTypes',
                    header:       'Typ umowy',
                    filter: {
                        type: 'select',
                        options: this.engagementTypes.map(et => ({
                            value: et.value, label: et.name
                        }))
                    },
                    cellTemplate: this.engagementCellTpl
                },
                {
                    key:          'hours',
                    header:       'Godziny',
                    cellTemplate: this.hoursCellTpl,
                    width:        '160px'
                },
                ...this.additionalFields()
                    .filter(f => f.isShowOnLists)
                    .map(f => ({
                        key:      `af_${f.id}`,
                        header:   f.fieldName,
                        sortable: true,
                        filter:   { type: 'text' as const },
                        getValue: (row: ContractorListItemDTO) =>
                            this.getFieldValue(row, f.id)
                    })),
                {
                    key:          'actions',
                    header:       '',
                    sortable:     false,
                    sticky:       'end' as const,
                    width:        '90px',
                    cellTemplate: this.actionsCellTpl
                }
            ]
        };
        this.tableConfig.set(config);
    }

    onStateChange(change: TableStateChange) {
        const f = change.state.filters;

        this.request = {
            ...this.request,
            page:           change.state.page,
            pageSize:       change.state.pageSize,
            sortBy:         change.state.sortBy,
            sortDescending: change.state.sortDescending,
            search:         change.state.search,
            name:         f['name'] as string | undefined,
            code:         f['code'] as string | undefined,
            nip:            f['nip']         as string | undefined,
            engagementTypes: f['engagementTypes'] != null
                ? [f['engagementTypes'] as number]
                : undefined,
            customFieldFilters: Object.entries(f)
                .filter(([k]) => k.startsWith('af_'))
                .reduce((acc, [k, v]) => ({
                    ...acc,
                    [parseInt(k.replace('af_', ''))]: String(v)
                }), {})
        } as ContractorsPagedRequest;

        this.loadPage();
    }

    engagementTypeLabel(type: number): string {
        return this.engagementTypes.find(et => et.value === type)?.name
            ?? type.toString();
    }

    hasHoursWarning(item: ContractorListItemDTO): boolean {
        if (item.hoursRemaining == null || item.hoursLimit == null) return false;
        return item.hoursRemaining <= item.hoursLimit * 0.1;
    }

    getFieldValue(item: ContractorListItemDTO, fieldId: number): string {
        const val = item.additionalFieldValues
            ?.find(v => v.tableAdditionalFieldId === fieldId);
        if (!val) return '—';
        switch (val.fieldType) {
            case EValueType.Boolean:
                return val.fieldValue === 'true' ? 'Tak'
                     : val.fieldValue === 'false' ? 'Nie' : '—';
            case EValueType.List:
                return val.dictionaryElementValue || '—';
            default:
                return val.fieldValue || '—';
        }
    }

    // ── Akcje ─────────────────────────────────────────────────────────

    addContractor() {
        this.router.navigate(['./contractor-pane', 0]);
    }

    editContractor(item: ContractorListItemDTO) {
        this.router.navigate(['./contractor-pane', item.id]);
    }

    deleteContractor(item: ContractorListItemDTO, event: Event) {
        event.stopPropagation();
        this.notificationService.confirm(
            `Czy na pewno chcesz usunąć kontrahenta ${item.displayName}?`
            + ` Notatniki i dokumenty przypisane do kontrahenta zostaną od niego odpięte.`
        ).subscribe(confirmed => {
            if (!confirmed) return;
            this.contractorsService.delete(item.id).then(result => {
                if (result?.succeeded) {
                    this.notificationService.success(
                        `Usunięto kontrahenta ${item.displayName}`);
                    this.loadPage();
                } else {
                    this.notificationService.error(
                        result?.errors?.join('\n') ??
                        `Nie usunięto kontrahenta ${item.displayName}`);
                }
            });
        });
    }

    onFileSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        if (!input.files?.length) return;
        this.selectedFile = input.files[0];
    }

    cancelUpload(fileInput: HTMLInputElement) {
        this.selectedFile = undefined;
        fileInput.value   = '';
    }

    importContractors(fileInput: HTMLInputElement) {
        if (!this.selectedFile) return;
        this.contractorsService.import(this.selectedFile).then(result => {
            if (result?.succeeded) {
                this.notificationService.success('Zaimportowano kontrahentów z pliku');
                this.cancelUpload(fileInput);
                this.loadPage();
            } else {
                this.notificationService.error(
                    result?.errors?.join('\n') ??
                    'Nie zaimportowano kontrahentów z pliku');
            }
        });
    }

    copy(text: string, event: Event) {
        event.stopPropagation();
        navigator.clipboard.writeText(text).then(() => {
            this.notificationService.success('Skopiowano do schowka');
        });
    }
}