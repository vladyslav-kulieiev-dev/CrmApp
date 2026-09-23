import { EValueType } from "./enums/EValueType";
import { SettingsValuesDictionary } from "./SettingsValuesDictionary";

export class Settings {
    constructor(key: string, label: string, valueType: EValueType) {
        this.key = key;
        this.label = label;
        this.valueType = valueType;
    }

    id: number = 0;
    key: string;
    label: string;
    description?: string;
    valueType: EValueType;
    value?: string;
    isMultiple: boolean = false;
    tableName?: string;
    valueFixedPrefix?: string;
    valueFixedSuffix?: string;

    acceptedValues?: SettingsValuesDictionary[];
    valueObj?: any;
}

export enum SettingsKeys {
    TranscriptionsSummarySystemPrompt = "TranscriptionsSummarySystemPrompt",
    ZohoDeskOrganizationId = "ZohoDeskOrganizationId",
    ZohoDeskDepartmentId = "ZohoDeskDepartmentId",
    ZohoDeskMcpUri = "ZohoDeskMcpUri",
    ZohoDeskMcpName = "ZohoDeskMcpName",
}