import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { HttpParams } from "@angular/common/http";
import { ValueNameDTO } from "src/models/DTO/ValueNameDTO";

@Injectable({
  providedIn: 'root'
})
export class ToolsService extends ApiService {
    constructor() {
        super();
        this.setControllerUrl('tools');
    }

    fullUrl(url: string): string {
        return `${this.controllerUrl}/${url}`;
    }

    async getEnumValues(enumName: string) {
        var paramsMap = new HttpParams();
        paramsMap = paramsMap.set('enumName', enumName);
        return this.dbService.getByParamsSkipLoader<ValueNameDTO<number>[]>(this.fullUrl("enum-values"), paramsMap).toPromise();
    }

    async getTableColumns(tableName: string) {
        return this.dbService.getByIdSkipLoader<string[]>(this.fullUrl("columns"), tableName).toPromise();
    }
}
