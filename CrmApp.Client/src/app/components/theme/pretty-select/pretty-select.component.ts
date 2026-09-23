import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, OnChanges, OnInit, Output, Renderer2, SimpleChanges, ViewChild } from '@angular/core';
import { MatMenu } from '@angular/material/menu';
import { ValueNameDTO } from 'src/models/DTO/ValueNameDTO';

@Component({
  selector: 'crm-pretty-select',
  standalone: false,
  templateUrl: './pretty-select.component.html',
  styleUrl: './pretty-select.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PrettySelectComponent implements OnInit, AfterViewInit, OnChanges {
  @Input() label: string = "Wybierz";
  @Input() placeholder: string = "Wybierz";
  @Input() valueExpr: string = "value";
  @Input() displayExpr: string = "name";
  @Input() dataSource: any[] = [];
  @Input() multiple: boolean = false;
  @Input() showSelectedCount: boolean = false;
  @Input() disabled: boolean = false;
  @Input() appendLabelToDisplayExpr: boolean = false;
  @Input() selectedValuesDisplayNames: number = 1;
  @Input() value: any | any[];
  @Input() enableSearchValue: boolean = false;
  @Input() enableCreateNew: boolean = false;
  @Input() showSelectAll: boolean = false;
  @Input() searchPlaceholder?: string;
  @Input() initialFilter = '';
  @Input() labelStyle: 'inside' | 'outside' = 'inside';

  @Output() valueChange: EventEmitter<any> = new EventEmitter<any>();
  @Output() afterValueChange: EventEmitter<any> = new EventEmitter<any>();
  @Output() created = new EventEmitter<any>(); // emits the created item

  @ViewChild('triggerBtn', { read: ElementRef }) triggerBtn!: ElementRef<HTMLElement>;
  @ViewChild('menu') menu!: MatMenu;

  filterText = '';

  constructor(private cdr: ChangeDetectorRef, private r: Renderer2) {

  }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    this.cdr.detectChanges();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialFilter'] && !changes['initialFilter'].firstChange) {
      this.filterText = this.initialFilter ?? '';
    }
  }

  onMenuOpened() {
    const w = this.triggerBtn?.nativeElement.getBoundingClientRect().width || 0;
    if (!w || !this.menu?.panelId) return;

    const panelEl = document.getElementById(this.menu.panelId);
    if (panelEl) {
      panelEl.classList.add('menu-match-trigger');
      this.r.setStyle(panelEl, 'width', `${w}px`);
      this.r.setStyle(panelEl, 'max-width', 'none');
      this.r.setStyle(panelEl, 'min-width', '0');
    }
  }

  select(val: any) {
    if (this.dataSource.some(x => x[this.valueExpr] == val)) {
      this.toggle(true, val);
      this.clearFilter();
    }
  }

  trackByVal = (_: number, item: any) => item?.[this.valueExpr];

  get filterTextTrimmed(): string {
    return (this.filterText || '').trim();
  }

  get filteredData(): any[] {
    if (!Array.isArray(this.dataSource) || !this.filterTextTrimmed) {
      return this.dataSource ?? [];
    }
    const needle = this.filterTextTrimmed.toLocaleLowerCase();
    return (this.dataSource ?? []).filter((item) => {
      const label = String(item?.[this.displayExpr] ?? '').toLocaleLowerCase();
      return label.includes(needle);
    });
  }

  clearFilter(ev?: MouseEvent): void {
    if (ev) ev?.stopPropagation();
    this.filterText = '';
  }

  isSelected(val: any): boolean {
    if (this.multiple) {
      const arr = Array.isArray(this.value) ? this.value : [];
      return arr.includes(val);
    }
    return this.value === val;
  }

  toggle(checked: boolean, val: any): void {
    if (this.multiple) {
      const arr = new Set<any>(Array.isArray(this.value) ? this.value : []);
      if (checked) {
        arr.add(val);
      } else {
        arr.delete(val);
      }
      const next = Array.from(arr);
      this.value = next;
      this.valueChange.emit(next);
    } else {
      this.value = checked ? val : null;
      this.valueChange.emit(this.value);
    }
    this.afterValueChange.emit();
  }

  get selectedValueName(): string {
    if (!this.dataSource || !this.dataSource.length) return '';
    if (this.multiple) {
      const ids: any[] = Array.isArray(this.value) ? this.value : [];
      const names = ids
        .map((id) => this.dataSource.find((x) => x?.[this.valueExpr] === id))
        .filter(Boolean)
        .map((x: any) => x[this.displayExpr]);
      return names.length > this.selectedValuesDisplayNames ?
        names.splice(0, this.selectedValuesDisplayNames).join(", ") + "..." : names.join(', ');
    } else {
      const found = this.dataSource.find((x) => x?.[this.valueExpr] === this.value);
      return found ? String(found[this.displayExpr]) : '';
    }
  }

  createNewFromFilter(ev?: MouseEvent): void {
    ev?.stopPropagation();
    const label = this.filterTextTrimmed;
    if (!this.enableCreateNew || !label) return;

    const newItem: any = {
      [this.displayExpr]: label,
      [this.valueExpr]: 0,
      __created__: true,
    };

    const exists = (this.dataSource ?? []).some(
      (x) => String(x?.[this.displayExpr]).toLocaleLowerCase() === label.toLocaleLowerCase()
    );
    if (!exists) {
      this.dataSource = [...(this.dataSource ?? []), newItem];
      this.created.emit(newItem);
    }
  }

  refresh() {
    this.cdr.detectChanges();
  }

  isAllSelected() {
    const arr = Array.isArray(this.value) ? this.value : [];
    return this.filteredData.every(x => arr.includes(x[this.valueExpr]));
  }

  toggleAll() {
    if (this.isAllSelected()) this.unselectAll();
    else this.selectAll();
  }

  unselectAll() {
    this.value = [];
    this.valueChange.emit(this.value);
    this.afterValueChange.emit();
  }
  selectAll() {
    this.value = this.filteredData.map(x => x[this.valueExpr]);
    this.valueChange.emit(this.value);
    this.afterValueChange.emit();
  }
}
