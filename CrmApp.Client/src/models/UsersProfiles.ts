import { ESystemType } from "./enums/ESystemType";

export class UsersProfiles {
    id: number = 0;
    userId?: string;
    firstName?: string;
    lastName?: string;
    displayName?: string;
    initials?: string;
    foreignSystemOperatorId?: number;
    foreignSystemType?: ESystemType; 
    isDeleted: boolean = false;
    alternativeEmails?: string;
    calendarProvider?: string;
    calendarId?: string;
}