import { CatalogItems } from "./CatalogItems";
import { Contractors } from "./Contractors";
import { ELicenceType } from "./enums/ELicenceType";
import { EWarrantyState } from "./enums/EWarrantyState";
import { UsersProfiles } from "./UsersProfiles";

export class ContractorLicenses {
        id: number = 0;
        contractorId: number = 0;
        catalogItemId: number = 0;
        licenseType: ELicenceType = ELicenceType.Server;
        quantity: number = 1;
        implementationOwnerId?: number;
        technicalOwnerId?: number;
        serialNumber: string = "";
        issuedAt: Date = new Date();
        validFrom: Date = new Date();
        expiresAt?: Date;
        upgradeDate?: Date;
        warrantyStatus?: EWarrantyState;
        warrantyEndDate?: Date;
        createdAt: Date = new Date();
        createdBy: number = 0;
        modifiedBy?: number;
        modifiedAt?: Date;

        warrantyStatusName?: string;
        licenseTypeName?: string;
        contractor?: Contractors;
        catalogItem?: CatalogItems;    
        implementationOwner?: UsersProfiles;    
        technicalOwner?: UsersProfiles;    
        createdByUser?: UsersProfiles;    
        validFromStr?: string;
        expiresAtStr?: string;
        warrantyEndDateStr?: string;
        upgradeDateStr?: string;
}