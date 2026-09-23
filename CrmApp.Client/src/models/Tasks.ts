import { ContractorContacts } from "./ContractorContacts";
import { Contractors } from "./Contractors";
import { ImportedTasks } from "./ImportedTasks";
import { Projects } from "./Projects";
import { UsersProfiles } from "./UsersProfiles";

export interface Tasks {
    id: number;
    taskNumber: string;
    createdAt: Date;
    createdBy?: number;
    priority: string;
    state: string;
    assignedTo?: number;
    assignedAt?: Date;
    title: string;
    description?: string;
    notes?: string;
    projectId?: number;
    contractorId?: number;
    contractorContactId?: number;
    closedAt?: Date;
    isDeleted: boolean;
    importedTaskId?: number;

    createdByUser?: UsersProfiles;
    assignedToUser?: UsersProfiles;
    project?: Projects;
    contractor?: Contractors;
    contractorContact?: ContractorContacts;
    importedTask?: ImportedTasks;
}