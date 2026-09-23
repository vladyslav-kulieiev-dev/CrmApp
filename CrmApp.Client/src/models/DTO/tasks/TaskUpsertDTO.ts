export interface TaskUpsertDTO {
    id?: number;
    title: string;
    description?: string;
    notes?: string;
    priority: string;
    state: string;
    progress: number;
    dueDate?: string;
    assignedTo?: number;
    projectId?: number;
    contractorId?: number;
    contractorContactId?: number;
}