import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { HttpParams } from "@angular/common/http";
import { CatalogItems } from "src/models/CatalogItems";
import { ResultDTO } from "src/models/DTO/ResultDTO";
import { CatalogItemsListItemDTO } from "src/models/DTO/catalog-items/CatalogItemsListItemDTO";
import { CatalogItemPagedRequest } from "src/models/DTO/catalog-items/CatalogItemPagedRequest";
import { PagedResult } from "src/models/DTO/lists/PagedResult";

@Injectable({
    providedIn: 'root'
})
export class CatalogItemsService  extends ApiService {

    constructor() {
        super();
        this.setControllerUrl('catalog-items');
    }

    fullUrl(url: string): string {
        return `${this.controllerUrl}/${url}`;
    }

    getList(onlyActive: boolean = false) {
        let params = new HttpParams();
        if (onlyActive) {
            params = params.append('onlyActive', 'true');
        }
        return this.dbService.getByParams<CatalogItems[]>(this.fullUrl("catalog-items"), params).toPromise();
    }

    getById(id: number) {
        return this.dbService.getById<CatalogItems>(this.fullUrl("catalog-item"), id.toString()).toPromise();
    }

    add(item: CatalogItems) {
        return this.dbService.post<ResultDTO<CatalogItems>>(this.fullUrl("catalog-item"), item).toPromise();
    }

    update(item: CatalogItems) {
        return this.dbService.put<ResultDTO<CatalogItems>>(this.fullUrl("catalog-item"), item, item.id).toPromise();
    }

    delete(id: number) {
        return this.dbService.delete<ResultDTO<any>>(this.fullUrl("catalog-item"), id.toString()).toPromise();
    }

    activate(id: number) {
        return this.dbService.put<ResultDTO<CatalogItems>>(this.fullUrl('activate-deactivate'), id, { 'activate': 'true' }).toPromise();
    }

    deactivate(id: number) {
        return this.dbService.put<ResultDTO<CatalogItems>>(this.fullUrl('activate-deactivate'), id, { 'activate': 'false' }).toPromise();
    }

    async getCatalogItemsPaged(request: CatalogItemPagedRequest) {
        return this.dbService.post<PagedResult<CatalogItemsListItemDTO>>(
            this.fullUrl('paged'), request
        ).toPromise();
    }
}
