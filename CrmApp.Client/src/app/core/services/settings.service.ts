import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { HttpParams } from "@angular/common/http";
import { ResultDTO } from "src/models/DTO/ResultDTO";
import { Settings } from "src/models/Settings";

@Injectable({
  providedIn: 'root'
})
export class SettingsService extends ApiService {

    constructor() {
        super();
        this.setControllerUrl('settings');
    }

    fullUrl(url: string): string {
        return `${this.controllerUrl}/${url}`;
    }

    getAll() {
        return this.dbService.getByParams<Settings[]>(this.fullUrl("settings")).toPromise();
    }

    getValueByKey(key: string) {
        let params = new HttpParams();
        params = params.set("key", key);
        return this.dbService.getByParams<string>(this.fullUrl("setting-value"), params).toPromise();
    }

    updateValue(setting: Settings) {
        return this.dbService.put<ResultDTO<string>>(this.fullUrl("setting-value"), setting).toPromise();
    }
}