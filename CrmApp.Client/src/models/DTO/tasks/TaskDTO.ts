import { ImportedTasks } from "src/models/ImportedTasks";

export class TaskDTO {
    id: number = 0;
    taskNumber: string = '';
    title: string = '';
    description?: string = '';
    notes?: string = '';
    priority: string = '';
    state: string = '';
    progress: number = 0;
    createdAt: Date = new Date();
    assignedAt?: Date;
    closedAt?: Date;
    dueDate?: Date;
    dueDateString?: string;
    assignedTo?: number;
    assignedToFullName?: string;
    createdBy?: number;
    createdByFullName?: string;
    projectId?: number;
    projectName?: string;
    contractorId?: number;
    contractorName?: string;
    contractorContactId?: number;
    contractorContactFullName?: string;
    contractorContactEmail?: string;
    importedTaskId?: number;
    importedTask?: ImportedTasks;
    isDeleted?: boolean;
    source?: string;
    comments: TaskCommentDTO[] = [];
}

export class TaskCommentDTO {
    id: number = 0;
    taskId: number = 0
    userId: number = 0;
    userFullName: string = '';
    content: string = '';
    postedAt: Date = new Date();
    modifiedAt?: Date;
}