import { ESystemType } from "../enums/ESystemType";
import { UserConfiguration } from "../identity/UserConfiguration";

export class UsersDTO {
    id: number = 0;
    userId: string = '';
    email: string = '';
    displayName: string = '';
    firstName: string = '';
    lastName: string = '';
    phoneNumber: string = '';
    foreignSystemObjectId?: string;
    foreignSystemType?: ESystemType;
    userConfiguration?: UserConfiguration = new UserConfiguration([], []);
    isDeleted: boolean = false;
    alternativeEmails?: string[] = [];
    calendarProvider?: string = '';
    calendarId?: string = '';
}