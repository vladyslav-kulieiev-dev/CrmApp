import { EValueType } from "../enums/EValueType";
import { AdditionalFieldPermissionCreateDTO, AdditionalFieldPermissionDTO } from "./AdditionalFieldPermissionDTO";
import { DictionaryElementOption } from "./lists/DictionaryElementOption";

export interface AdditionalFieldDTO {
    id: number;
    tableName: string;
    fieldName: string;
    fieldType: EValueType;
    dictionaryId?: number;
    dictionaryName?: string;
    isMultiple: boolean;
    isShowOnLists: boolean;
    isRequired: boolean;
    sortOrder: number;
    defaultValue?: string;
    permissions: AdditionalFieldPermissionDTO[];
    presentInNewRow: boolean;
    rowSpan: number;
    colSpan: number;
    rowId?: number;
    fieldOptions: DictionaryElementOption[];
    canView: boolean;
    canEdit: boolean;
    isMappedFromInitObject: boolean;
    initObjectPropertyName?: string;
}

export interface AdditionalFieldCreateDTO {
    tableName: string;
    fieldName: string;
    fieldType: EValueType;
    dictionaryId?: number;
    isMultiple: boolean;
    isShowOnLists: boolean;
    isRequired: boolean;
    defaultValue?: string;
    isMappedFromInitObject: boolean;
    initObjectPropertyName?: string;
    permissions: AdditionalFieldPermissionCreateDTO[];
}

export interface AdditionalFieldUpdateDTO extends AdditionalFieldCreateDTO {
    id: number;
}

export interface AdditionalFieldReorderDTO {
    tableName: string;
    orders: Record<number, number>; // fieldId → sortOrder
}