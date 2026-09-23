import { inject, Injectable } from "@angular/core";
import { DbService } from "./db.service";
import { HttpParams } from "@angular/common/http";

@Injectable({
    providedIn: 'root'
})
export class ApiService {
    protected dbService = inject(DbService);
    protected controllerUrl = 'api';

    setControllerUrl(url: string) {
        this.controllerUrl = url;
    }

    toParams(req: any): HttpParams {
        let p = new HttpParams();
        Object.entries(req).forEach(([k, v]) => {
        if (v !== null && v !== undefined && v !== '') {
            p = p.set(k, String(v));
        }
        });
        return p;
    }
}