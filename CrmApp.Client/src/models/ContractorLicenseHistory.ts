export class ContractorLicenseHistory {
    id: number = 0;
    contractorLicenseId: number = 0;
    changedAt: Date = new Date();
    changedBy: number = 0;
    fieldName: string = "";
    oldValue: string = "";
    newValue: string = "";
}