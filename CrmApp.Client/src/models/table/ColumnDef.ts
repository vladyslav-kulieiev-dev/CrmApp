import { TemplateRef } from '@angular/core';

export type FilterType = 'text' | 'number' | 'select' | 'daterange' | 'none';
export type SortMode = 'backend' | 'frontend' | 'none';
export type FooterAggregation = 'sum' | 'avg' | 'min' | 'max' | 'count';

export interface ColumnFooterDef<T> {
  aggregation?: FooterAggregation;     // wbudowana agregacja po wierszach
  value?: (rows: T[]) => any;          // własne wyliczenie (priorytet nad aggregation)
  format?: (value: any) => string;     // formatowanie wyświetlania
  label?: string;                      // statyczny tekst (np. "Razem:") gdy brak agregacji
  template?: TemplateRef<any>;         // pełna własna komórka stopki
}

export interface SelectOption {
    value: any;
    label: string;
    icon?: string;
}

export interface ColumnFilter {
    type: FilterType;
    options?: SelectOption[];
    placeholder?: string;
}

export interface ColumnDef<T = any> {
    key: string;                        
    header: string;                     
    sortable?: boolean;                 
    filter?: ColumnFilter;              
    width?: string;                     
    sticky?: 'start' | 'end';          
    hidden?: boolean;                   
    footer?: ColumnFooterDef<T>;       
    exportable?: boolean;              
    exportHeader?: string;              
    cellTemplate?: TemplateRef<any>;    
    headerTemplate?: TemplateRef<any>;  
    getValue?: (row: T) => any;
    exportValue?: (row: T) => any;   
    cellClass?: string | ((row: T) => string);
}