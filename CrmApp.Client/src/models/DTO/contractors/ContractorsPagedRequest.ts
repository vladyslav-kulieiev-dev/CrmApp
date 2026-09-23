import { PagedRequest } from "../lists/PagedRequest";

export class ContractorsPagedRequest extends PagedRequest {
    name?: string;
    code?: string;
    nip?: string;
    isXopero?: boolean;
    engagementTypes?: number[];
    hasHoursRemaining?: boolean;
}