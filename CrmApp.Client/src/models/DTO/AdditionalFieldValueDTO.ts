import { DictionariesElements } from "../DictionariesElements";
import { EValueType } from "../enums/EValueType";
import { DictionaryElementOption } from "./lists/DictionaryElementOption";

export interface AdditionalFieldValueDTO {
    id: number;
    tableAdditionalFieldId: number;
    fieldName: string;
    fieldType: EValueType;
    fieldValue?: string;
    defaultValue?: string;
    fieldValueBool?: boolean;
    isMultiple: boolean;
    isRequired: boolean;
    isMappedFromInitObject: boolean;
    presentInNewRow: boolean;
    canView: boolean;
    canEdit: boolean;
    colSpan: number;
    rowSpan: number;
    dictionaryElementId?: number;
    dictionaryElementValue?: string;
    dictionaryElementIds: number[];  
    fieldOptions: DictionaryElementOption[];
}

export interface AdditionalFieldValueSaveDTO {
    tableAdditionalFieldId: number;
    fieldValue?: string;
    dictionaryElementIds: number[];
}

export interface AdditionalFieldValuesBatchSaveDTO {
    tableName: string;
    rowId: string;
    values: AdditionalFieldValueSaveDTO[];
}

export interface BatchGetValuesRequestDTO {
    tableName: string;
    rowIds: string[];
}

export type AdditionalFieldValuesMap = Record<string, AdditionalFieldValueDTO[]>;