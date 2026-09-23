import {
    Component, OnInit, ViewChild, TemplateRef,
    signal, computed, AfterViewInit, OnDestroy
} from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { Router } from '@angular/router';
import { Observable, Subject, takeUntil } from 'rxjs';

import { AuthService } from 'src/app/core/services/auth.service';
import { CatalogItemsService } from 'src/app/core/services/catalog-items.service';
import { AdditionalFieldsService, TableNames } from 'src/app/core/services/additional-fields.service';
import { NotificationService, defaultSnackBarConfig } from 'src/app/core/services/notification.service';
import { DictionariesService } from 'src/app/core/services/dictionaries.service';
import { UsersService } from 'src/app/core/services/users.service';
import { DeviceService } from 'src/app/core/services/device.service';

import { AdditionalFieldDTO } from 'src/models/DTO/AdditionalFieldDTO';
import { CatalogItemsListItemDTO } from 'src/models/DTO/catalog-items/CatalogItemsListItemDTO';
import { CatalogItemPagedRequest } from 'src/models/DTO/catalog-items/CatalogItemPagedRequest';
import { PagedResult } from 'src/models/DTO/lists/PagedResult';
import { TableConfig } from 'src/models/table/TableConfig';
import { TableStateChange } from 'src/models/table/TableState';
import { DictionariesElements } from 'src/models/DictionariesElements';
import { UsersProfiles } from 'src/models/UsersProfiles';
import { EDictionaryType } from 'src/models/enums/EDictionaryType';
import { EValueType } from 'src/models/enums/EValueType';
import { getJoinedMesseges } from 'src/app/core/services/extensions.service';

@Component({
    selector: 'crm-catalog-items',
    standalone: false,
    templateUrl: './catalog-items.component.html',
    styleUrl: './catalog-items.component.css'
})
export class CatalogItemsComponent implements OnInit, AfterViewInit, OnDestroy {
    pagedData = signal<PagedResult<CatalogItemsListItemDTO> | null>(null);
    loading   = signal(true);
    additionalFields = signal<AdditionalFieldDTO[]>([]);
    tableConfig = signal<TableConfig<CatalogItemsListItemDTO> | null>(null);

    request: CatalogItemPagedRequest = {
        page: 1,
        pageSize: 25,
        sortBy: 'name',
        sortDescending: false,
        showNotActive: true
    } as CatalogItemPagedRequest;

    productTypes:      DictionariesElements[] = [];
    productCategories: DictionariesElements[] = [];
    units:             DictionariesElements[] = [];
    billingUnits:      DictionariesElements[] = [];
    allUsers:          UsersProfiles[]        = [];

    isMobile = false;
    readonly isMobile$: Observable<boolean>;
    private destroy$ = new Subject<void>();

    @ViewChild('nameCellTpl')    nameCellTpl!: TemplateRef<any>;
    @ViewChild('codeCellTpl')    codeCellTpl!: TemplateRef<any>;
    @ViewChild('activeCellTpl')  activeCellTpl!: TemplateRef<any>;
    @ViewChild('actionsCellTpl') actionsCellTpl!: TemplateRef<any>;

    constructor(
        private auth: AuthService,
        private catalogItemsService: CatalogItemsService,
        private additionalFieldsService: AdditionalFieldsService,
        private notificationService: NotificationService,
        private dictionariesService: DictionariesService,
        private usersService: UsersService,
        private device: DeviceService,
        private router: Router
    ) {
        this.isMobile$ = this.device.isMobile$;
        this.isMobile$.pipe(takeUntil(this.destroy$))
            .subscribe(m => this.isMobile = m);
    }

    async ngOnInit() {
        await Promise.all([
            this.loadDictionaries(),
            this.loadAdditionalFields()
        ]);
        this.buildTableConfig();
    }

    ngAfterViewInit() {
        this.loadPage();
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    async loadDictionaries() {
        const [dicts, users] = await Promise.all([
            this.dictionariesService.getSystemDictionaries(),
            this.usersService.getAll()
        ]);

        if (dicts?.length) {
            this.productTypes      = dicts.find(d =>
                d.dictionaryType === EDictionaryType.ProductTypes)
                ?.dictionariesElements ?? [];
            this.productCategories = dicts.find(d =>
                d.dictionaryType === EDictionaryType.ProductCategories)
                ?.dictionariesElements ?? [];
            this.units             = dicts.find(d =>
                d.dictionaryType === EDictionaryType.UnitsOfMeasure)
                ?.dictionariesElements ?? [];
            this.billingUnits      = dicts.find(d =>
                d.dictionaryType === EDictionaryType.BillingUnits)
                ?.dictionariesElements ?? [];
        }

        this.allUsers  = (users ?? []).map(dto => {
            const p = new UsersProfiles();
            p.id = dto.id; p.displayName = dto.displayName;
            p.firstName = dto.firstName; p.lastName = dto.lastName;
            return p;
        });
    }

    async loadAdditionalFields() {
        const fields = await this.additionalFieldsService
            .getFieldsForTable(TableNames.CatalogItems);
        this.additionalFields.set(fields ?? []);
    }

    async loadPage() {
        this.loading.set(true);
        try {
            const result = await this.catalogItemsService.getCatalogItemsPaged(this.request);
            this.pagedData.set(result ?? null);
        } finally {
            this.loading.set(false);
        }
    }

    // ── Table config ──────────────────────────────────────────────────

    buildTableConfig() {
        const config: TableConfig<CatalogItemsListItemDTO> = {
            pagingMode:      'backend',
            defaultSortBy:   'name',
            defaultPageSize: 25,
            pageSizeOptions: [10, 25, 50, 100],
            rowClickable:    true,
            emptyMessage:    'Brak produktów spełniających kryteria',
            emptyIcon:       'inventory_2',
            columns: [
                {
                    key:      'name',
                    header:   'Nazwa',
                    sortable: true,
                    filter:   { type: 'text', placeholder: 'Szukaj nazwy...' },
                    cellTemplate: this.nameCellTpl
                },
                {
                    key:      'code',
                    header:   'Kod',
                    sortable: true,
                    filter:   { type: 'text', placeholder: 'Szukaj kodu...' },
                    cellTemplate: this.codeCellTpl
                },
                {
                    key:    'typeName',
                    header: 'Typ',
                    filter: {
                        type: 'select',
                        options: this.productTypes.map(t => ({
                            value: t.id, label: t.value
                        }))
                    },
                    getValue: row => this.getTypeName(row.type)
                },
                {
                    key:    'categoryName',
                    header: 'Kategoria',
                    filter: {
                        type: 'select',
                        options: this.productCategories.map(c => ({
                            value: c.id, label: c.value
                        }))
                    },
                    getValue: row => this.getCategoryName(row.categoryId)
                },
                {
                    key:    'unitName',
                    header: 'Jm',
                    getValue: row => row.unitName || '—'
                },
                {
                    key:    'billingUnitName',
                    header: 'J. rozlicz.',
                    hidden: true,
                    getValue: row => row.billingUnitName || '—'
                },
                {
                    key:      'price',
                    header:   'Cena',
                    sortable: true,
                    filter:   { type: 'number', placeholder: 'Min. cena...' },
                    getValue: row =>
                        `${row.price?.toFixed(2)} ${row.currency || 'PLN'}`
                },
                {
                    key:    'technicalSupervisorId',
                    header: 'Opiekun techn.',
                    hidden: true,
                    getValue: row => this.getSupervisorName(row.technicalSupervisorId)
                },
                {
                    key:    'implementationManagerId',
                    header: 'Opiekun wdroż.',
                    hidden: true,
                    getValue: row => this.getSupervisorName(row.implementationManagerId)
                },
                {
                    key:    'isActive',
                    header: 'Aktywny',
                    filter: {
                        type: 'select',
                        options: [
                            { value: true,  label: 'Aktywne'    },
                            { value: false, label: 'Nieaktywne' }
                        ]
                    },
                    cellTemplate: this.activeCellTpl,
                    width: '90px'
                },
                // Custom fields dynamicznie
                ...this.additionalFields()
                    .filter(f => f.isShowOnLists)
                    .map(f => ({
                        key:      `af_${f.id}`,
                        header:   f.fieldName,
                        sortable: true,
                        filter:   { type: 'text' as const },
                        getValue: (row: CatalogItemsListItemDTO) =>
                            this.getFieldValue(row, f.id)
                    })),
                {
                    key:          'actions',
                    header:       '',
                    sortable:     false,
                    sticky:       'end' as const,
                    width:        '110px',
                    cellTemplate: this.actionsCellTpl
                }
            ]
        };
        this.tableConfig.set(config);
    }

    // ── State change z crm-table ───────────────────────────────────────

    onStateChange(change: TableStateChange) {
        const f = change.state.filters;

        this.request = {
            ...this.request,
            page:          change.state.page,
            pageSize:      change.state.pageSize,
            sortBy:        change.state.sortBy,
            sortDescending: change.state.sortDescending,
            search:         change.state.search,
            name: f['name'] as string | undefined,
            code: f['code'] as string | undefined,
            categoryId: f['categoryName'] as number | undefined,
            type:       f['typeName']     as number | undefined,
            showNotActive: f['isActive'] == null
                ? true
                : !(f['isActive'] as boolean),
            customFieldFilters: Object.entries(f)
                .filter(([k]) => k.startsWith('af_'))
                .reduce((acc, [k, v]) => ({
                    ...acc,
                    [parseInt(k.replace('af_', ''))]: String(v)
                }), {})
        } as CatalogItemPagedRequest;

        this.loadPage();
    }

    // ── Helpers ───────────────────────────────────────────────────────

    getTypeName(type: number): string {
        return this.productTypes.find(t => t.id === type)?.value ?? '—';
    }

    getCategoryName(categoryId: number): string {
        return this.productCategories.find(c => c.id === categoryId)?.value ?? '—';
    }

    getSupervisorName(id?: number): string {
        if (!id) return '—';
        return this.allUsers.find(u => u.id === id)?.displayName ?? '—';
    }

    getFieldValue(item: CatalogItemsListItemDTO, fieldId: number): string {
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

    addItem() { this.router.navigate(['./catalog-item']); }

    editItem(item: CatalogItemsListItemDTO) {
        this.router.navigate(['./catalog-item', item.id]);
    }

    activateItem(item: CatalogItemsListItemDTO, event: Event) {
        event.stopPropagation();
        this.catalogItemsService.activate(item.id).then(res => {
            if (res?.succeeded) {
                this.notificationService.success('Aktywowano ' + item.name);
                this.loadPage();
            } else {
                this.notificationService.error(
                    getJoinedMesseges('Nie aktywowano ' + item.name, res?.errors),
                    defaultSnackBarConfig);
            }
        });
    }

    deactivateItem(item: CatalogItemsListItemDTO, event: Event) {
        event.stopPropagation();
        this.catalogItemsService.deactivate(item.id).then(res => {
            if (res?.succeeded) {
                this.notificationService.success('Dezaktywowano ' + item.name);
                this.loadPage();
            } else {
                this.notificationService.error(
                    getJoinedMesseges('Nie dezaktywowano ' + item.name, res?.errors),
                    defaultSnackBarConfig);
            }
        });
    }

    deleteItem(item: CatalogItemsListItemDTO, event: Event) {
        event.stopPropagation();
        this.notificationService.confirm(
            `Czy na pewno chcesz usunąć ${item.name}?`
        ).subscribe(confirmed => {
            if (!confirmed) return;
            this.catalogItemsService.delete(item.id).then(res => {
                if (res?.succeeded) {
                    this.notificationService.success(`Usunięto ${item.name}`);
                    this.loadPage();
                } else {
                    this.notificationService.error(
                        getJoinedMesseges('Nie usunięto ' + item.name, res?.errors),
                        defaultSnackBarConfig);
                }
            });
        });
    }
}