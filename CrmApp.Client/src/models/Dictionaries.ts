import { DictionariesElements } from "./DictionariesElements";
import { EDictionaryType } from "./enums/EDictionaryType";
import { UsersProfiles } from "./UsersProfiles";

export class Dictionaries {
    id: number = 0;
    name: string = "";
    description?: string = "";
    dictionaryType: EDictionaryType = EDictionaryType.Custom;
    createdAt: Date = new Date();
    createdBy?: number;
    isCustom: boolean = true;
    isActive: boolean = true;
    createdByUser?: UsersProfiles;
    dictionariesElements: DictionariesElements[] = [];
}