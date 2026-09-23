export class PagedResult<T> {
    items: T[] = [];
    totalCount: number = 0;
    page: number = 1;
    pageSize: number = 25;
    totalPages: number = 0;
}