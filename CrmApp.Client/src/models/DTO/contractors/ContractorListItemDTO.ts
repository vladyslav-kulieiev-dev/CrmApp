import { AdditionalFieldValueDTO } from "../AdditionalFieldValueDTO";

export class ContractorListItemDTO {
    id: number = 0;
    name: string = '';
    code: string = '';
    displayName: string = '';
    nip?: string = '';   
    isXopero?: boolean = false;
    activeEngagementTypes: number[] = []
    hoursLimit?: number;
    hoursUsed?: number
    hoursRemaining?: number;
    additionalFieldValues: AdditionalFieldValueDTO[] = [];
}