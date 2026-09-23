import { PagedRequest } from "../lists/PagedRequest";

export class CatalogItemPagedRequest extends PagedRequest {
    code?: string;
    name?: string;
    categoryId?: number;
    type?: number
    showNotActive: boolean = false;
}