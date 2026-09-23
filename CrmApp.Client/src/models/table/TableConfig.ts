import { ColumnDef } from './ColumnDef';

export type PagingMode = 'backend' | 'frontend';

export interface TableConfig<T = any> {
    columns: ColumnDef<T>[];
    pagingMode?: PagingMode;           
    pageSizeOptions?: number[];        
    defaultPageSize?: number;          
    defaultSortBy?: string;
    defaultSortDesc?: boolean;
    stickyHeader?: boolean;            
    rowClickable?: boolean;            
    trackBy?: (row: T) => any;         
    emptyMessage?: string;
    emptyIcon?: string;
    showFooter?: boolean;                
    stickyFooter?: boolean;              
    exportable?: boolean;                
    exportFileName?: string;             
    fetchExportData?: () => Promise<T[]>;
}