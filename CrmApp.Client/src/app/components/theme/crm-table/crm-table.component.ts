// src/app/shared/crm-table/crm-table.component.ts
import {
  Component, Input, Output, EventEmitter,
  OnInit, OnChanges, OnDestroy, AfterViewInit,
  ViewChild, ContentChildren, QueryList,
  TemplateRef, SimpleChanges, signal, computed,
  ChangeDetectionStrategy, ChangeDetectorRef,
  Directive,
  ContentChild,
  ElementRef
} from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { FormControl, FormGroup } from '@angular/forms';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import * as XLSX from 'xlsx';
import { ColumnDef, FilterType, SelectOption } from 'src/models/table/ColumnDef';
import { TableConfig, PagingMode } from 'src/models/table/TableConfig';
import { TableState, TableStateChange, FilterValue, DateRangeFilter } from 'src/models/table/TableState';
import { PagedResult } from 'src/models/DTO/lists/PagedResult';

@Directive({ selector: '[crmTableToolbar]', standalone: false })
export class CrmTableToolbarDirective { }
@Component({
  selector: 'crm-table',
  templateUrl: './crm-table.component.html',
  styleUrls: ['./crm-table.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CrmTableComponent<T> implements OnInit, OnChanges, AfterViewInit, OnDestroy {
  @Input() config!: TableConfig<T>;
  @Input() pagedData?: PagedResult<T> | null;
  @Input() data?: T[] | null;
  @Input() loading = false;
  @Input() storageKey?: string;
  @Input() showSearch = true;
  @Input() searchPlaceholder = 'Szukaj...';

  @Output() stateChange = new EventEmitter<TableStateChange>();
  @Output() rowClick = new EventEmitter<T>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) matSort!: MatSort;
  @ContentChild(CrmTableToolbarDirective, { read: ElementRef }) toolbarContent?: ElementRef;

  frontendDataSource = new MatTableDataSource<T>([]);
  filterValues = signal<Record<string, FilterValue>>({});
  searchControl = new FormControl('');
  filterForms: Record<string, FormControl | FormGroup> = {};
  displayedColumnKeys = computed(() => {
    const vis = this.columnVisibility();
    return this.config.columns
      .filter(c => vis[c.key] !== false)
      .map(c => c.key);
  });
  totalCount = signal(0);
  columnVisibility = signal<Record<string, boolean>>({});
  columnChooserOpen = signal(false);
  configurableColumns = computed(() =>
    this.config.columns.filter(c => c.sticky !== 'end')
  );
  showFooter = computed(() =>
    !!this.config.showFooter || this.config.columns.some(c => !!c.footer));

  private state: TableState = {
    page: 1,
    pageSize: 25,
    sortDescending: false,
    filters: {}
  };

  private destroy$ = new Subject<void>();

  constructor(private cdr: ChangeDetectorRef) { }

  // ── Lifecycle ─────────────────────────────────────────────────────

  ngOnInit() {
    this.initState();
    this.initColumns();
    this.initFilterForms();

    this.searchControl.valueChanges
      .pipe(debounceTime(350), takeUntil(this.destroy$))
      .subscribe(val => {
        this.state.search = val?.trim() || undefined;
        this.state.page = 1;
        if (this.isFrontend) {
          // dla frontend — dodaj do filtrów jako specjalny klucz
          this.applyFilter('__search', val?.trim() ?? null);
        } else {
          if (this.paginator) this.paginator.firstPage();
          this.emit('filter');
        }
      });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['pagedData'] && this.pagedData) {
      this.totalCount.set(this.pagedData.totalCount);
      this.cdr.markForCheck();
    }
    if (changes['data'] && this.data && this.isFrontend) {
      this.frontendDataSource.data = this.data;
      this.cdr.markForCheck();
    }
    if (changes['config'] && !changes['config'].firstChange) {
      this.initColumns();
    }
  }

  ngAfterViewInit() {
    if (this.isFrontend) {
      this.frontendDataSource.paginator = this.paginator;
      this.frontendDataSource.sort = this.matSort;
      // Custom sort accessor
      this.frontendDataSource.sortingDataAccessor = (row, key) => {
        const col = this.config.columns.find(c => c.key === key);
        return col?.getValue ? col.getValue(row) : (row as any)[key];
      };
    } else {
      // Backend — słuchaj eventów
      this.matSort.sortChange
        .pipe(takeUntil(this.destroy$))
        .subscribe((sort: Sort) => {
          this.state.sortBy = sort.active;
          this.state.sortDescending = sort.direction === 'desc';
          this.state.page = 1;
          if (this.paginator) this.paginator.firstPage();
          this.emit('sort');
        });

      this.paginator.page
        .pipe(takeUntil(this.destroy$))
        .subscribe((e: PageEvent) => {
          this.state.page = e.pageIndex + 1;
          this.state.pageSize = e.pageSize;
          this.emit('page');
        });
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Init helpers ──────────────────────────────────────────────────

  private initState() {
    this.state = {
      page: 1,
      pageSize: this.config.defaultPageSize ?? 25,
      sortBy: this.config.defaultSortBy,
      sortDescending: this.config.defaultSortDesc ?? false,
      filters: {}
    };
  }

  private initColumns() {
    const saved = this.storageKey
      ? this.loadColumnVisibility()
      : null;

    const visibility: Record<string, boolean> = {};
    for (const col of this.config.columns) {
      visibility[col.key] = saved?.[col.key] ?? !col.hidden;
    }
    this.columnVisibility.set(visibility);
  }

  toggleColumn(key: string) {
    this.columnVisibility.update(vis => ({
      ...vis,
      [key]: !vis[key]
    }));
    if (this.storageKey) this.saveColumnVisibility();
  }

  isColumnVisible(key: string): boolean {
    return this.columnVisibility()[key] !== false;
  }

  private saveColumnVisibility() {
    try {
      localStorage.setItem(
        `crm-table-cols-${this.storageKey}`,
        JSON.stringify(this.columnVisibility())
      );
    } catch { }
  }

  private loadColumnVisibility(): Record<string, boolean> | null {
    try {
      const raw = localStorage.getItem(
        `crm-table-cols-${this.storageKey}`);
      return raw ? JSON.parse(raw) : null;
    } catch { return null; }
  }

  resetColumnVisibility() {
    const visibility: Record<string, boolean> = {};
    for (const col of this.config.columns) {
      visibility[col.key] = !col.hidden;
    }
    this.columnVisibility.set(visibility);
    if (this.storageKey) {
      localStorage.removeItem(`crm-table-cols-${this.storageKey}`);
    }
  }

  private initFilterForms() {
    for (const col of this.config.columns) {
      if (!col.filter || col.filter.type === 'none') continue;

      if (col.filter.type === 'daterange') {
        const fg = new FormGroup({
          from: new FormControl<string | null>(null),
          to: new FormControl<string | null>(null)
        });
        fg.valueChanges
          .pipe(debounceTime(300), takeUntil(this.destroy$))
          .subscribe(val => {
            if (val.from || val.to) {
              this.applyFilter(col.key, {
                from: val.from ?? undefined,
                to: val.to ?? undefined
              });
            } else {
              this.applyFilter(col.key, null);
            }
          });
        this.filterForms[col.key] = fg;
      } else {
        const fc = new FormControl<string | number | null>(null);
        fc.valueChanges
          .pipe(
            debounceTime(col.filter.type === 'select' ? 0 : 350),
            takeUntil(this.destroy$)
          )
          .subscribe(val => this.applyFilter(col.key, val));
        this.filterForms[col.key] = fc;
      }
    }
  }

  // ── Filtry ────────────────────────────────────────────────────────

  applyFilter(key: string, value: FilterValue) {
    const current = { ...this.filterValues() };

    if (value === null || value === '' || value === undefined) {
      delete current[key];
    } else {
      current[key] = value;
    }

    this.filterValues.set(current);
    this.state.filters = current;
    this.state.page = 1;

    if (this.isFrontend) {
      this.applyFrontendFilters();
    } else {
      if (this.paginator) this.paginator.firstPage();
      this.emit('filter');
    }
  }

  clearFilter(key: string) {
    const fc = this.filterForms[key];
    if (fc instanceof FormGroup) {
      fc.reset(undefined, { emitEvent: false });
    } else if (fc instanceof FormControl) {
      fc.setValue(null, { emitEvent: false });
    }
    this.applyFilter(key, null);
  }

  clearAllFilters() {
    for (const key of Object.keys(this.filterForms)) {
      const fc = this.filterForms[key];
      if (fc instanceof FormGroup) {
        fc.reset(undefined, { emitEvent: false });
      } else {
        (fc as FormControl).setValue(null, { emitEvent: false });
      }
    }
    this.filterValues.set({});
    this.state.filters = {};
    this.state.page = 1;

    if (this.isFrontend) {
      this.frontendDataSource.filter = '';
      this.applyFrontendFilters();
    } else {
      if (this.paginator) this.paginator.firstPage();
      this.emit('filter');
    }
  }

  isFiltered(key: string): boolean {
    const val = this.filterValues()[key];
    if (val == null) return false;
    if (typeof val === 'object') {
      return !!(val as DateRangeFilter).from || !!(val as DateRangeFilter).to;
    }
    return val !== '';
  }

  hasAnyFilter = computed(() =>
    Object.keys(this.filterValues()).length > 0);

  activeFilterChips = computed(() => {
    const chips: { key: string; label: string; colHeader: string }[] = [];
    for (const [key, val] of Object.entries(this.filterValues())) {
      const col = this.config.columns.find(c => c.key === key);
      if (!col) continue;
      let label = '';
      if (typeof val === 'object' && val !== null) {
        const dr = val as DateRangeFilter;
        label = [dr.from, dr.to].filter(Boolean).join(' — ');
      } else if (col.filter?.type === 'select') {
        label = col.filter.options?.find(o => o.value === val)?.label ?? String(val);
      } else {
        label = String(val);
      }
      chips.push({ key, label, colHeader: col.header });
    }
    return chips;
  });

  // Frontend filtering
  getDateRangeControl(key: string, field: 'from' | 'to'): FormControl {
    return (this.filterForms[key] as FormGroup).get(field) as FormControl;
  }

  private applyFrontendFilters() {
    const filters = this.filterValues();
    const search = this.state.search?.toLowerCase();

    this.frontendDataSource.filterPredicate = (row: T) => {
      // Globalny search — po wszystkich kolumnach
      if (search) {
        const allValues = this.config.columns
          .map(col => String(this.getCellValue(row, col) ?? '').toLowerCase())
          .join(' ');
        if (!allValues.includes(search)) return false;
      }

      // Kolumnowe filtry
      for (const [key, val] of Object.entries(filters)) {
        if (key === '__search' || val === null || val === '') continue;
        const col = this.config.columns.find(c => c.key === key);
        const cell = col?.getValue
          ? col.getValue(row)
          : (row as any)[key];

        if (typeof val === 'object') {
          const dr = val as DateRangeFilter;
          const date = new Date(cell);
          if (dr.from && date < new Date(dr.from)) return false;
          if (dr.to && date > new Date(dr.to)) return false;
        } else if (col?.filter?.type === 'select') {
          if (cell !== val) return false;
        } else {
          if (!String(cell ?? '').toLowerCase()
            .includes(String(val).toLowerCase()))
            return false;
        }
      }
      return true;
    };

    this.frontendDataSource.filter =
      JSON.stringify({ ...filters, __search: this.state.search });
  }

  get isFrontend(): boolean {
    return this.config.pagingMode === 'frontend';
  }

  get rows(): T[] {
    return this.isFrontend
      ? this.frontendDataSource.filteredData
      : (this.pagedData?.items ?? []);
  }

  get dataSource(): T[] | MatTableDataSource<T> {
    return this.isFrontend
      ? this.frontendDataSource
      : (this.pagedData?.items ?? []);
  }

  getCellValue(row: T, col: ColumnDef<T>): any {
    return col.getValue ? col.getValue(row) : (row as any)[col.key];
  }

  getCellClass(row: T, col: ColumnDef<T>): string {
    if (!col.cellClass) return '';
    return typeof col.cellClass === 'function'
      ? col.cellClass(row)
      : col.cellClass;
  }

  getFormControl(key: string): FormControl {
    const fc = this.filterForms[key];
    if (fc instanceof FormGroup) {
      throw new Error(`Column '${key}' uses FormGroup, not FormControl`);
    }
    return (fc ?? new FormControl()) as FormControl;
  }
  getFormGroup(key: string): FormGroup {
    return this.filterForms[key] as FormGroup;
  }

  getSelectOptions(key: string): SelectOption[] {
    return this.config.columns
      .find(c => c.key === key)?.filter?.options ?? [];
  }

  getSelectedLabel(key: string): string {
    const val = this.filterValues()[key];
    const options = this.getSelectOptions(key);
    return options.find(o => o.value === val)?.label ?? '';
  }

  onRowClick(row: T) {
    if (this.config.rowClickable) this.rowClick.emit(row);
  }

  trackByFn = (index: number, row: T): any => {
    return this.config.trackBy ? this.config.trackBy(row) : row;
  };

  private emit(trigger: TableStateChange['trigger']) {
    this.stateChange.emit({ state: { ...this.state }, trigger });
  }

  getFooterValue(col: ColumnDef<T>): any {
    const f = col.footer;
    if (!f) return null;
    if (f.value) return f.value(this.rows);
    if (!f.aggregation) return f.label ?? null;

    const nums = this.rows
      .map(r => Number(this.getCellValue(r, col)))
      .filter(n => !isNaN(n));
    switch (f.aggregation) {
      case 'sum': return nums.reduce((a, b) => a + b, 0);
      case 'avg': return nums.length ? nums.reduce((a, b) => a + b, 0) / nums.length : 0;
      case 'min': return nums.length ? Math.min(...nums) : null;
      case 'max': return nums.length ? Math.max(...nums) : null;
      case 'count': return this.rows.length;
      default: return null;
    }
  }

  getFooterDisplay(col: ColumnDef<T>): string {
    const f = col.footer;
    if (!f) return '';
    const v = this.getFooterValue(col);
    return f.format ? f.format(v) : (v ?? '');
  }

  async exportCsv() { await this.exportData('csv'); }
  async exportExcel() { await this.exportData('xlsx'); }

  private async getExportRows(): Promise<T[]> {
    if (this.config.fetchExportData) return await this.config.fetchExportData();
    return this.rows;   // frontend: wszystkie przefiltrowane; backend: bieżąca strona
  }

  private async exportData(format: 'csv' | 'xlsx') {
    const rows = await this.getExportRows();
    const cols = this.config.columns.filter(c =>
      c.exportable !== false && this.isColumnVisible(c.key));

    const aoa: any[][] = [cols.map(c => c.exportHeader ?? c.header ?? c.key)];
    for (const row of rows) {
      aoa.push(cols.map(c => {
        const v = c.exportValue ? c.exportValue(row) : this.getCellValue(row, c);
        return v ?? '';
      }));
    }
    if (this.showFooter()) {
      aoa.push(cols.map(c => (c.footer ? this.getFooterValue(c) ?? '' : '')));
    }

    const ws = XLSX.utils.aoa_to_sheet(aoa);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Dane');
    const name = this.config.exportFileName ?? 'tabela';

    if (format === 'csv') {
      const csv = XLSX.utils.sheet_to_csv(ws, { FS: ';' });   // ; pod polski Excel
      this.downloadBlob(new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8;' }), `${name}.csv`);
    } else {
      XLSX.writeFile(wb, `${name}.xlsx`);
    }
  }

  private downloadBlob(blob: Blob, filename: string) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }
}