export interface ImportedTasks {
    id: number;
    externalId: string;
    number: string;
    state: string;
    subject: string;
    description?: string;
    category?: string;
    url?: string;
    createdAt: Date;
    dueDate?: Date;
    channel?: string;
    priority?: string;
    responseDueDate?: Date;
    productName?: string;
    closedAt?: Date;
    contactFirstName?: string;
    contactLastName?: string;
    contactPhoneNumber?: string;
    contactEmail?: string;
    assigneeFirstName?: string;
    assigneeLastName?: string;   
    assigneeEmail?: string;   
    isDeleted: boolean;
}