import { ContractorHoursSnapshots } from "./ContractorHoursSnapshots";
import { EBillingType } from "./enums/EBillingType";
import { EEngagementType } from "./enums/EEngagementType";
import { ERenewalType } from "./enums/ERenewalType";
import { UsersProfiles } from "./UsersProfiles";

export class ContractorContracts {
    id: number = 0;
    contractorId: number = 0;
    contractNumber: string = "";
    engagementType: EEngagementType = EEngagementType.None;
    createdAt: Date = new Date();
    createdBy: number = 0;
    validFrom?: Date;
    validTo?: Date;
    hoursLimit: number = 0;
    modifiedBy?: number;
    modifiedAt?: Date;
    billingType: EBillingType = EBillingType.None;
    billingAmount?: number;
    allowOverLimit: boolean = false;
    renewalType: ERenewalType = ERenewalType.None;
    renewalDate?: Date;

    engagementTypeName: string = "";
    hoursUsed?: number = 0;
    hoursRemaining?: number = 0;
    hoursUsedCurrentMonth?: number = 0;
    hoursRemainingCurrentMonth?: number = 0;
    hoursFromPrevMonth?: number = 0;
    snapshots: ContractorHoursSnapshots[] = [];
    validFromStr?: string;
    validToStr?: string;
    renewalDateStr?: string;

    setDefaultBillingTypeByEngagementType() {
        switch (this.engagementType) {
            case EEngagementType.None:
                this.billingType = EBillingType.None;
                break;
            case EEngagementType.Contract:
                this.billingType = EBillingType.OneTimePayment;
                break;
            case EEngagementType.HoursPackage:
                this.billingType = EBillingType.Monthly;
                break;
            case EEngagementType.MonthlyBilling:
                this.billingType = EBillingType.Monthly;
                break;
            default:
                this.billingType = EBillingType.None;
        }   
    }
}