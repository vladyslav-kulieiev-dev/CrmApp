import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { TaskListItemDTO } from "src/models/DTO/tasks/TaskListItemDTO";
import { PagedResult } from "src/models/DTO/lists/PagedResult";
import { TaskPagedRequest } from "src/models/DTO/tasks/TaskPagedRequest";
import { Observable } from "rxjs";
import { TaskCommentDTO, TaskDTO } from "src/models/DTO/tasks/TaskDTO";
import { TaskUpsertDTO } from "src/models/DTO/tasks/TaskUpsertDTO";
import { ResultDTO } from "src/models/DTO/ResultDTO";
import { HttpParams } from "@angular/common/http";

@Injectable({
    providedIn: "root"
})
export class TasksService extends ApiService {

    constructor() {
        super();
        this.setControllerUrl('tasks');
    }

    fullUrl(url: string): string {
        return `${this.controllerUrl}/${url}`;
    }

    getMyTasks(req: TaskPagedRequest): Observable<PagedResult<TaskListItemDTO>> {
        return this.dbService.post<PagedResult<TaskListItemDTO>>(this.fullUrl("my"), req);
    }

    getAllTasks(req: TaskPagedRequest): Observable<PagedResult<TaskListItemDTO>> {
        return this.dbService.post<PagedResult<TaskListItemDTO>>(this.fullUrl("paged"), req);
    }

    getById(id: number): Observable<ResultDTO<TaskDTO>> {
        return this.dbService.getById<ResultDTO<TaskDTO>>(this.fullUrl("task"), id.toString());
    }

    create(dto: TaskDTO): Observable<ResultDTO<TaskDTO>> {
        return this.dbService.post<ResultDTO<TaskDTO>>(this.fullUrl("task"), dto);
    }

    update(id: number, dto: TaskDTO): Observable<ResultDTO<TaskDTO>> {
        dto.id = id;
        return this.dbService.put<ResultDTO<TaskDTO>>(this.fullUrl("task"), dto);
    }

    delete(id: number): Observable<ResultDTO<TaskDTO>> {
        return this.dbService.delete<ResultDTO<TaskDTO>>(this.fullUrl("task"), id.toString());
    }

    addComment(taskId: number, content: string): Observable<ResultDTO<TaskCommentDTO>> {
        var commentDTO = new TaskCommentDTO();
        commentDTO.taskId = taskId;
        commentDTO.content = content;
        return this.dbService.post<ResultDTO<TaskCommentDTO>>(this.fullUrl(`comment`), commentDTO);
    }

    updateComment(taskId: number, commentId: number, content: string): Observable<ResultDTO<TaskCommentDTO>> {
        var commentDTO = new TaskCommentDTO();
        commentDTO.id = commentId;
        commentDTO.taskId = taskId;
        commentDTO.content = content;
        return this.dbService.put<ResultDTO<TaskCommentDTO>>(this.fullUrl(`comment`), commentDTO);
    }

    deleteComment(commentId: number): Observable<void> {
        return this.dbService.delete<void>(this.fullUrl("comment"), commentId.toString());
    }
}