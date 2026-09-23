export interface DateRangeFilter {
    from?: string;  // ISO date
    to?: string;
}

export type FilterValue = string | number | null | DateRangeFilter | boolean;

export interface TableState {
    page: number;
    pageSize: number;
    sortBy?: string;
    sortDescending: boolean;
    filters: Record<string, FilterValue>;  
    search?: string;
}

export interface TableStateChange {
    state: TableState;
    trigger: 'page' | 'sort' | 'filter';
}