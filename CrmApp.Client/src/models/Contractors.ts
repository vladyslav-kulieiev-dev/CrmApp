import { ContractorContacts } from "./ContractorContacts";
import { ContractorContracts } from "./ContractorContracts";
import { ContractorLicenses } from "./ContractorLicenses";
import { EEngagementType } from "./enums/EEngagementType";
import { ExtendableClass } from "./ExtendableClass";

export class Contractors extends ExtendableClass {
    id: number = 0;
    foreignSystemObjectId?: number;
    name: string = "";
    code: string = "";
    displayName: string = "";
    nip?: string = "";
    euVAT?: string = "";
    alternativeNipNumbers?: string[] = [];

    contractorContacts: ContractorContacts[] = [];
    contractorLicences: ContractorLicenses[] = [];
    contractorContracts: ContractorContracts[] = [];
    currentEngagementTypes: number[] = [];
    
}