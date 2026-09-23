import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { EDictionaryType } from "src/models/enums/EDictionaryType";
import { ResultDTO } from "src/models/DTO/ResultDTO";
import { Dictionaries } from "src/models/Dictionaries";
import { DictionariesElements } from "src/models/DictionariesElements";

@Injectable({ providedIn: 'root' })
export class DictionariesService extends ApiService {

    constructor() {
        super();
        this.setControllerUrl('dictionaries');
    }

    fullUrl(url: string): string {
        return `${this.controllerUrl}/${url}`;
    }

    getByType(type: EDictionaryType) {
        return this.dbService.getById<Dictionaries>(this.fullUrl("dictionary-by-type"), type.toString()).toPromise();
    }
    
    getById(id: number) {
        return this.dbService.getById<ResultDTO<Dictionaries>>(this.fullUrl("dictionary"), id.toString()).toPromise();
    }
    
    getDictionaryElements(dictionaryId: number) {
        return this.dbService.getById<DictionariesElements[]>(this.fullUrl("dictionary-elements"), dictionaryId.toString()).toPromise();
    }

    getAll() {
        return this.dbService.getByParams<Dictionaries[]>(this.fullUrl("dictionaries")).toPromise();
    }    

    getSystemDictionaries() {
        return this.dbService.getByParams<Dictionaries[]>(this.fullUrl("system-dictionaries")).toPromise();
    }    

    add(item: Dictionaries, userId: number) {
        item.createdBy = userId;
        return this.dbService.post<ResultDTO<Dictionaries>>(this.fullUrl("dictionary"), item).toPromise();
    }

    update(item: Dictionaries, userId: number) {
        return this.dbService.put<ResultDTO<Dictionaries>>(this.fullUrl("dictionary"), item).toPromise();
    }

    activateDeactivate(item: Dictionaries, activate: boolean) {
        return this.dbService.put<ResultDTO<Dictionaries>>(this.fullUrl("activate-deactivate"), item, { activate: activate }).toPromise();
    }

    remove(dictionaryId: number) {
        return this.dbService.delete<ResultDTO<any>>(this.fullUrl("dictionary"), dictionaryId.toString()).toPromise();
    }
}