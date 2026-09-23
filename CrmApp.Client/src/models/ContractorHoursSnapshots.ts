import { UsersProfiles } from "./UsersProfiles";

export class ContractorHoursSnapshots {
    id: number = 0;
    contractorContractId: number = 0;
    snapshotDate: Date = new Date();
    hoursUsed: number = 0;
    hoursRemaining: number = 0;
    createdAt: Date = new Date();
    createdBy: number = 0;
    createdByUser?: UsersProfiles;
}