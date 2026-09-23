import { Contractors } from "./Contractors";
import { EProjectState } from "./enums/EProjectState";
import { UsersProfiles } from "./UsersProfiles";

export class Projects {
    id: number = 0;
    name: string = "";
    description: string = "";
    state: EProjectState = EProjectState.Created;
    startDate?: Date;
    endDate?: Date;
    createdAt: Date = new Date();
    createdBy: number = 0;
    modifiedBy?: number;
    projectManagerId!: number;
    contractorId?: number;

    membersIds: number[] = [];
    stateName: string = "";
    stateIcon?: string;
    stateIconClass?: string;
    createdUser?: UsersProfiles;
    modifiedUser?: UsersProfiles;
    projectManager?: UsersProfiles;
    contractor?: Contractors;
    startDateStr?: string;
    endDateStr?: string;
}