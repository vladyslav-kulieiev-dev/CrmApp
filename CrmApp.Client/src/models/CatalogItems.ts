import { DictionariesElements } from "./DictionariesElements";
import { UsersProfiles } from "./UsersProfiles";

export class CatalogItems {
    id: number = 0;
    name: string = "";
    code: string = "";
    description?: string;
    categoryId?: number;
    type: number = 0;
    unitId: number = 0;
    unitName: string = "";
    billingUnitId: number = 0;
    billingUnitName: string = "";
    price: number = 0;
    isActive: boolean = true;
    vatRate: number = 0;
    currency: string = "";
    parentItemId?: number;
    createdAt: Date = new Date();
    createdBy: number = 0;
    technicalSupervisorId?: number;
    implementationManagerId?: number;
    supportedSystems?: number[];

    typeObj?: DictionariesElements;
    category?: DictionariesElements;
    unit?: DictionariesElements;
    billingUnit?: DictionariesElements;
    createdByUser?: UsersProfiles;
    technicalSupervisor?: UsersProfiles;
    implementationManager?: UsersProfiles;
    canDelete: boolean = false;
}