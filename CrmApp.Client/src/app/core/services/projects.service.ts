import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { ResultDTO } from "src/models/DTO/ResultDTO";
import { HttpParams } from "@angular/common/http";
import { Projects } from "src/models/Projects";
import { formatDate } from "@angular/common";
import { DATE_TO_BACKEND_FORMAT } from "./extensions.service";


@Injectable({
    providedIn: 'root'
})
export class ProjectsService extends ApiService {

    constructor() {
        super();
        this.setControllerUrl('projects');
    }

    fullUrl(url: string): string {
        return `${this.controllerUrl}/${url}`;
    }

    async getById(id: number) {
        return this.dbService.getById<ResultDTO<Projects>>(this.fullUrl("project"), id.toString()).toPromise();
    }

    async getByContractorId(contractorId: number) {
        return this.dbService.getById<Projects[]>(this.fullUrl("contractor-projects"), contractorId.toString()).toPromise();
    }

    async getAll() {
        return this.dbService.getByParams<Projects[]>(this.fullUrl("projects")).toPromise();
    }

    async add(project: Projects, userId: number) {
        project.createdBy = userId;
        project.startDateStr = project.startDate ? formatDate(project.startDate, DATE_TO_BACKEND_FORMAT, "en-US") : "";
        project.endDateStr = project.endDate ? formatDate(project.endDate, DATE_TO_BACKEND_FORMAT, "en-US") : "";

        return this.dbService.post<ResultDTO<Projects>>(this.fullUrl("project"), project).toPromise();
    }

    async update(project: Projects, userId: number) {
        project.modifiedBy = userId;
        project.startDateStr = project.startDate ? formatDate(project.startDate, DATE_TO_BACKEND_FORMAT, "en-US") : "";
        project.endDateStr = project.endDate ? formatDate(project.endDate, DATE_TO_BACKEND_FORMAT, "en-US") : "";
        return this.dbService.put<ResultDTO<Projects>>(this.fullUrl("project"), project).toPromise();
    }

    async startProject(projectId: number) {
        return this.dbService.put<ResultDTO<Projects>>(this.fullUrl("project-start"), projectId).toPromise();
    }

    async closeProject(projectId: number) {
        return this.dbService.put<ResultDTO<Projects>>(this.fullUrl("project-close"), projectId).toPromise();
    }

    async cancelProject(projectId: number) {
        return this.dbService.put<ResultDTO<Projects>>(this.fullUrl("project-cancel"), projectId).toPromise();
    }

    async delete(projectId: number) {
        return this.dbService.delete<ResultDTO<Projects>>(this.fullUrl("project"), projectId.toString()).toPromise();
    }
}