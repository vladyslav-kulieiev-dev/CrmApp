import { Injectable } from "@angular/core";
import { DbService } from "./db.service";
import { ApiService } from "./api.service";
import { UsersProfiles } from "src/models/UsersProfiles";
import { UsersDTO } from "src/models/DTO/UsersDTO";
import { ResultDTO } from "src/models/DTO/ResultDTO";
import { RegisterDTO } from "src/models/DTO/RegisterDTO";
import { ResetPasswordDTO } from "src/models/DTO/ResetPasswordDTO";

@Injectable({
  providedIn: 'root'
})
export class UsersService extends ApiService {

    constructor() {
        super();
        this.setControllerUrl('users');
    }

    fullUrl(url: string): string {
        return `${this.controllerUrl}/${url}`;
    }

    async getById(id: number) {
        return this.dbService.getById<ResultDTO<UsersDTO>>(this.fullUrl("user"), id.toString()).toPromise();
    }

    async getAll() {
        return this.dbService.getByParams<UsersDTO[]>(this.fullUrl("users")).toPromise();
    }

    async register(item: RegisterDTO) {
        return this.dbService.post<ResultDTO<UsersDTO>>(this.fullUrl("register"), item).toPromise();
    }

    async add(item: RegisterDTO) {
        return this.dbService.post<ResultDTO<UsersDTO>>(this.fullUrl("user"), item).toPromise();
    }

    async update(item: UsersDTO) {
        return this.dbService.put<ResultDTO<UsersDTO>>(this.fullUrl("user"), item).toPromise();
    }

    async changeOwnPassword(item: ResetPasswordDTO) {
        return this.dbService.put<ResultDTO<UsersDTO>>(this.fullUrl("own-password-change"), item).toPromise();
    }

    async changeOthersPassword(item: ResetPasswordDTO) {
        return this.dbService.put<ResultDTO<UsersDTO>>(this.fullUrl("password-change"), item).toPromise();
    }

    async deactivate(id: number) {
        return this.dbService.put<ResultDTO<any>>(this.fullUrl("deactivate"), id).toPromise();
    }

    async activate(id: number) {
        return this.dbService.put<ResultDTO<any>>(this.fullUrl("activate"), id).toPromise();
    }
}