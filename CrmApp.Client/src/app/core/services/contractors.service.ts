import { Contractors } from "src/models/Contractors";
import { ApiService } from "./api.service";
import { ResultDTO } from "src/models/DTO/ResultDTO";
import { ContractorContacts } from "src/models/ContractorContacts";
import { ContractorLicenses } from "src/models/ContractorLicenses";
import { ContractorContracts } from "src/models/ContractorContracts";
import { ContractorHoursSnapshots } from "src/models/ContractorHoursSnapshots";
import { ContractorsPagedRequest } from "src/models/DTO/contractors/ContractorsPagedRequest";
import { PagedResult } from "src/models/DTO/lists/PagedResult";
import { ContractorListItemDTO } from "src/models/DTO/contractors/ContractorListItemDTO";

export class ContractorsService extends ApiService {

    constructor() {
        super();
        this.setControllerUrl('contractors');
    }

    fullUrl(url: string): string {
        return `${this.controllerUrl}/${url}`;
    }

    getAll() {
        return this.dbService.getByParams<Contractors[]>(this.fullUrl("contractors")).toPromise();
    }

    getById(id: number) {
        return this.dbService.getById<ResultDTO<Contractors>>(this.fullUrl("contractor"), id.toString()).toPromise();
    }

    getByIdLight(id: number) {
        return this.dbService.getById<ResultDTO<Contractors>>(this.fullUrl("contractor-light"), id.toString()).toPromise();
    }

    getContractorContacts(id: number) {
        return this.dbService.getById<ContractorContacts[]>(this.fullUrl("contractor-contacts"), id.toString()).toPromise();
    }

    getContractorLicenses(id: number) {
        return this.dbService.getById<ContractorLicenses[]>(this.fullUrl("contractor-licenses"), id.toString()).toPromise();
    }

    getContractorContracts(id: number) {
        return this.dbService.getById<PagedResult<ContractorContracts>>(this.fullUrl("contractor-contracts"), id.toString()).toPromise();
    }

    getContractorContract(id: number) {
        return this.dbService.getById<ContractorContracts>(this.fullUrl("contractor-contract"), id.toString()).toPromise();
    }

    getContractorDetails(id: number): Promise<ResultDTO<ContractorListItemDTO> | undefined> {
        return this.dbService
            .getById<ResultDTO<ContractorListItemDTO>>(
                this.fullUrl('contractor-details'), id.toString()
            )
            .toPromise();
    }

    getNipsById(contractorId: number) {
        return this.dbService.getById<string[]>(this.fullUrl("contractor-nips"), contractorId.toString()).toPromise();
    }

    saveContractorContacts(contractorId: number, contractorContacts: ContractorContacts[]) {
        return this.dbService.put<ResultDTO<ContractorContacts[]>>(this.fullUrl("contractor-contacts"), contractorContacts, { contractorId }).toPromise();
    }

    getContractSnapshots(contractId: number) {
        return this.dbService.getById<ContractorHoursSnapshots[]>(
            this.fullUrl('contract-snapshots'), contractId.toString()
        ).toPromise();
    }

    async getContractorsPaged(request: ContractorsPagedRequest) {
        return this.dbService.post<PagedResult<ContractorListItemDTO>>(
            this.fullUrl('paged'), request
        ).toPromise();
    }

    addContractSnapshot(contractId: number, hoursUsed: number, userId: number) {
        var contractHoursSnapshot = new ContractorHoursSnapshots();
        contractHoursSnapshot.contractorContractId = contractId;
        contractHoursSnapshot.hoursUsed = hoursUsed;
        contractHoursSnapshot.createdBy = userId;
        return this.dbService.post<ResultDTO<ContractorHoursSnapshots>>(
            this.fullUrl('contract-snapshot'), contractHoursSnapshot).toPromise();
    } 
    async add(item: Contractors) {
        return this.dbService.post<ResultDTO<Contractors>>(this.fullUrl("contractor"), item).toPromise();
    }

    async update(item: Contractors) {
        return this.dbService.put<ResultDTO<Contractors>>(this.fullUrl("contractor"), item).toPromise();
    }

    async delete(id: number) {
        return this.dbService.delete<ResultDTO<any>>(this.fullUrl("contractor"), id.toString()).toPromise();
    }

    async import(file: File) {
        return this.dbService.upload<ResultDTO<Contractors[]>>(this.fullUrl("import"), file).toPromise();
    }


    addLicense(license: ContractorLicenses, userId: number) {
        license.createdBy = userId;
        return this.dbService.post<ResultDTO<ContractorLicenses>>(
            this.fullUrl('contractor-license'), license
        ).toPromise();
    }

    updateLicense(license: ContractorLicenses, userId: number) {
        license.modifiedBy = userId;
        return this.dbService.put<ResultDTO<ContractorLicenses>>(
            this.fullUrl('contractor-license'), license
        ).toPromise();
    }

    deleteLicense(id: number) {
        return this.dbService.delete<ResultDTO<object>>(
            this.fullUrl('contractor-license'), id.toString()
        ).toPromise();
    }

    addContract(contract: ContractorContracts, userId: number) {
        contract.createdBy = userId;
        return this.dbService.post<ResultDTO<ContractorContracts>>(
            this.fullUrl('contractor-contract'), contract
        ).toPromise();
    }

    updateContract(contract: ContractorContracts, userId: number) {
        contract.modifiedBy = userId;
        return this.dbService.put<ResultDTO<ContractorContracts>>(
            this.fullUrl('contractor-contract'), contract
        ).toPromise();
    }

    deleteContract(id: number) {
        return this.dbService.delete<ResultDTO<object>>(
            this.fullUrl('contractor-contract'), id.toString()
        ).toPromise();
    }
}