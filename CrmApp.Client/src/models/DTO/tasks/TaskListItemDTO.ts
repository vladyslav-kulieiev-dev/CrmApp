import { AdditionalFieldValueDTO } from "../AdditionalFieldValueDTO";

export class TaskListItemDTO {
    id: number = 0;
    taskNumber: string = '';
    title: string = '';
    priority: string = '';
    state: string = '';
    description?: string = '';
    notes?: string = '';
    progress: number = 0
    createdAt: Date = new Date();
    dueDate?: Date;
    assignedTo?: number;
    assignedToFullName?: string;
    contractorId?: number;
    contractorName?: string;
    contractorContactId?: number;
    contractorContactName?: string;
    projectId?: number;
    projectName?: string;
    importedTaskNumber?: string;
    importedTaskUrl?: string;
    isDeleted: boolean = false;
    additionalFieldValues: AdditionalFieldValueDTO[] = [];
}