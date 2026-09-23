import { AdditionalFieldValueDTO } from "../AdditionalFieldValueDTO";

export class CatalogItemsListItemDTO { 
    id: number = 0;
    name: string = "";
    code: string = "";
    description?: string;
    categoryId: number = 0;
    type: number = 0;
    unitName: string = "";
    billingUnitName: string = "";
    price: number = 0;
    isActive: boolean = true;
    vatRate: number = 0;
    currency: string = "";
    parentId?: number;
    technicalSupervisorId?: number;
    implementationManagerId?: number;
    additionalFieldValues: AdditionalFieldValueDTO[] = [];
}