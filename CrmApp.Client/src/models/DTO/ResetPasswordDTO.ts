export class ResetPasswordDTO {

    constructor(userId: number) {
        this.userId = userId;
    }

    userId: number;
    oldPassword?: string;
    newPassword: string = "";
}