import { Dictionaries } from "./Dictionaries";
import { UsersProfiles } from "./UsersProfiles";

export class DictionariesElements {
    id: number = 0;
    dictionaryId: number = 0;
    key: string = "";
    value: string = "";
    createdAt: Date = new Date();
    createdBy?: number;
    isCustom: boolean = true;
    isActive: boolean = true;
    isDefault: boolean = false;
    parentId?: number;
    ordinalNumber: number = 0;
    alternativeValuesForMapping?: string;
    icon?: string;
    iconColor?: string;

    dictionary?: Dictionaries;
    createdByUser?: UsersProfiles;
}