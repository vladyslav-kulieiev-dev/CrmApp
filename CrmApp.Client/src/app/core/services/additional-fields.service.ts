import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { AdditionalFieldCreateDTO, AdditionalFieldDTO, AdditionalFieldReorderDTO, AdditionalFieldUpdateDTO } from 'src/models/DTO/AdditionalFieldDTO';
import { AdditionalFieldValueDTO, AdditionalFieldValuesBatchSaveDTO, AdditionalFieldValuesMap, BatchGetValuesRequestDTO } from 'src/models/DTO/AdditionalFieldValueDTO';
import { ResultDTO } from 'src/models/DTO/ResultDTO';

export const TableNames = {
    Contractors:  'Contractors',
    CatalogItems: 'CatalogItems',
    Users:        'UsersProfiles',
    Tasks: "Tasks"
} as const;

export type TableName = typeof TableNames[keyof typeof TableNames];

@Injectable({ providedIn: 'root' })
export class AdditionalFieldsService extends ApiService {

    constructor() {
        super();
        this.setControllerUrl('additional-fields');
    }

    private fullUrl(path: string): string {
        return `${this.controllerUrl}/${path}`;
    }

    getFieldsForTable(tableName: TableName) {
        return this.dbService
            .getById<AdditionalFieldDTO[]>(this.fullUrl('table'), tableName)
            .toPromise();
    }

    getFieldById(id: number) {
        return this.dbService
            .getById<AdditionalFieldDTO>(this.controllerUrl, id.toString())
            .toPromise();
    }

    createField(dto: AdditionalFieldCreateDTO) {
        return this.dbService
            .post<ResultDTO<AdditionalFieldDTO>>(this.controllerUrl, dto)
            .toPromise();
    }

    updateField(id: number, dto: AdditionalFieldUpdateDTO) {
        return this.dbService
            .put<ResultDTO<AdditionalFieldDTO>>(this.fullUrl(id.toString()), dto)
            .toPromise();
    }

    deleteField(id: number) {
        return this.dbService
            .delete<void>(this.controllerUrl, id.toString())
            .toPromise();
    }

    reorderFields(dto: AdditionalFieldReorderDTO) {
        return this.dbService
            .put<void>(this.fullUrl('reorder'), dto)
            .toPromise();
    }

    getValuesForRecord(tableName: TableName, rowId: string | number) {
        return this.dbService
            .getByParams<AdditionalFieldValueDTO[]>(
                this.fullUrl(`values/${tableName}/${rowId}`)
            )
            .toPromise();
    }

    getValuesForRecords(tableName: TableName, rowIds: (string | number)[]) {
        const dto: BatchGetValuesRequestDTO = {
            tableName,
            rowIds: rowIds.map(id => id.toString())
        };
        return this.dbService
            .post<AdditionalFieldValuesMap>(this.fullUrl('values/batch-get'), dto)
            .toPromise();
    }

    saveValues(dto: AdditionalFieldValuesBatchSaveDTO) {
        return this.dbService
            .post<void>(this.fullUrl('values/save'), dto)
            .toPromise();
    }

    buildSaveDto(
        tableName: TableName,
        rowId: string | number,
        fieldValues: Record<number, AdditionalFieldValueDTO>
    ): AdditionalFieldValuesBatchSaveDTO {
        return {
            tableName,
            rowId: rowId.toString(),
            values: Object.values(fieldValues).map(v => ({
                tableAdditionalFieldId: v.tableAdditionalFieldId,
                fieldValue: v.fieldValue,
                dictionaryElementIds: v.dictionaryElementIds ?? []
            }))
        };
    }

    indexValuesByFieldId(
        values: AdditionalFieldValueDTO[]
    ): Record<number, AdditionalFieldValueDTO> {
        return values.reduce((acc, v) => {
            acc[v.tableAdditionalFieldId] = v;
            return acc;
        }, {} as Record<number, AdditionalFieldValueDTO>);
    }
}
