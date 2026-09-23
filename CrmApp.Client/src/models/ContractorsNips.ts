import { Contractors } from "./Contractors";

export class ContractorsNips {
    id: number = 0;
    contractorId: number = 0;
    nip: string = "";
    isPrimary: boolean = false;
    contractor?: Contractors;
}