import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';
import { MatMenuTrigger } from '@angular/material/menu';

export interface IconColor {
  label: string;
  value: string | null;   
  cssVar?: string;      
}

@Component({
  selector: 'crm-mat-icon-picker',
  standalone: false,
  templateUrl: './mat-icon-picker.component.html',
  styleUrl: './mat-icon-picker.component.css',
})
export class MatIconPickerComponent implements OnInit {
  @Input() value: string | null = null;
  @Input() color: string | null = null;      
  @Input() isReadonly: boolean = false;
  @Input() buttonSize: 'small' | 'big' = 'small';
  @Input() btnClass: string = '';
  @Input() size: number = 24;

  @Output() valueChange = new EventEmitter<string | null>();
  @Output() colorChange = new EventEmitter<string | null>();

  @ViewChild('iconMenuTrigger') menuTrigger!: MatMenuTrigger;

  filterValue = '';
  allIcons: string[] = [];

  readonly presetColors: IconColor[] = [
    { label: 'Domyślny',      value: null                                                     },
    { label: 'Główny',        value: 'var(--mat-sys-primary)',     cssVar: '--mat-sys-primary' },
    { label: 'Drugorzędny',   value: 'var(--mat-sys-secondary)',   cssVar: '--mat-sys-secondary' },
    { label: 'Trzeciorzędny', value: 'var(--mat-sys-tertiary)',    cssVar: '--mat-sys-tertiary' },
    { label: 'Błąd',          value: 'var(--mat-sys-error)',       cssVar: '--mat-sys-error' },
    { label: 'Tekst',         value: 'var(--crm-sys-text)',         cssVar: '--crm-sys-text' },
    { label: 'Wyciszony',     value: 'color-mix(in srgb, var(--crm-sys-text) 45%, transparent)' },
    { label: 'Zielony',       value: '#3B6D11' },
  ];

  constructor(private iconRegistry: MatIconRegistry) {}

  ngOnInit(): void {
    this.allIcons = this.loadRegisteredIcons();
  }

  private loadRegisteredIcons(): string[] {
    const map = (this.iconRegistry as any)._svgIconConfigs as Map<string, unknown>;
    return [...map.keys()]
      .map(key => (key.includes(':') ? key.split(':')[1] : key))
      .sort((a, b) => a.localeCompare(b, 'pl'));
  }

  get filteredIcons(): string[] {
    const q = this.filterValue.trim().toLowerCase();
    if (!q) return this.allIcons;
    return this.allIcons.filter(n => n.toLowerCase().includes(q));
  }

  selectIcon(icon: string): void {
    this.value = icon;
    this.valueChange.emit(icon);
    this.menuTrigger?.closeMenu();
  }

  selectPresetColor(c: IconColor): void {
    this.color = c.value;
    this.colorChange.emit(c.value);
  }

  selectCustomColor(hex: string): void {
    this.color = hex;
    this.colorChange.emit(hex);
  }

  isPresetActive(c: IconColor): boolean {
    return this.color === c.value;
  }

  get isCustomColor(): boolean {
    return !!this.color && !this.presetColors.some(p => p.value === this.color);
  }

  get customColorHex(): string {
    if (this.isCustomColor && this.color?.startsWith('#')) return this.color;
    return '#000000';
  }

  clearFilter(): void { this.filterValue = ''; }

  clearValue() {
    this.value = null;
    this.valueChange.emit(null);
    this.color = null;
    this.colorChange.emit(null);
  }
}