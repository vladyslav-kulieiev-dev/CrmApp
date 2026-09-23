export class PagedRequest {
    search?: string;
    page: number = 1;
    pageSize: number = 25;
    customFieldFilters: { [fieldId: number]: string } = {};
    sortBy?: string = 'displayName';
    sortDescending: boolean = false;
}