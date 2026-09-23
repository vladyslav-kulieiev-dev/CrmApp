import { PagedRequest } from "../lists/PagedRequest";

export class TaskPagedRequest extends PagedRequest {
    taskNumber?: string;
    importedTaskNumber?: string;
    title?: string;
    description?: string;
    notes?: string;
    priority?: string;
    state?: string;
    assignedTo?: number;
    projectId?: number;
    contractorId?: number;
    contractorContactId?: number;
    isOverdue?: boolean;
}