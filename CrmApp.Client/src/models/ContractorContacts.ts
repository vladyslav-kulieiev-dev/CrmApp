export class ContractorContacts {
    
    constructor() {
        this.isEditMode = true;
    }

    id: number = 0;
    contractorId: number = 0;
    firstname: string = "";
    lastname: string = "";
    displayName: string = "";
    position?: string = "";
    email?: string = "";
    phoneNumber?: string = "";
    createdAt: Date = new Date();
    createdBy: number = 0;

    isEditMode: boolean = false;
}