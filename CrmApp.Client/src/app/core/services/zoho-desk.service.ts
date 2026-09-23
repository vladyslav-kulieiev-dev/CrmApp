import { ApiService } from "./api.service";
import { ResultDTO } from "src/models/DTO/ResultDTO";
import { TaskDTO } from "src/models/DTO/tasks/TaskDTO";

export class ZohoDeskService extends ApiService {
    async getTickets() {
        return this.dbService.getByParams<ResultDTO<TaskDTO[]>>("zoho-desk/tickets").toPromise();
    }
    async getTicket(){
        return this.dbService.getByParams<ResultDTO<any>>("zoho-desk/ticket").toPromise();
    }
    updateTicket(task: TaskDTO){
        return this.dbService.putSkipLoader<ResultDTO<TaskDTO>>("zoho-desk/ticket", task);
    }
    async addCommentToTicket(){
        this.dbService.putSkipLoader<ResultDTO<any>>("zoho-desk/comment-on-ticket", "").toPromise();
    }
    async sendReply(){
        this.dbService.putSkipLoader<ResultDTO<any>>("zoho-desk/send-reply", "").toPromise();
    }
}