import { EValueType } from "./enums/EValueType";
import { Settings } from "./Settings";

export class SettingsValuesDictionary {
    constructor(setting: Settings, value: string, name: string) {
        this.settingId = setting.id;
        this.settingKey = setting.key;
        this.valueType = setting.valueType;
        this.value = value;
        this.label = name;
    }

    id: number = 0;
    settingId: number;
    settingKey: string;
    valueType: EValueType;
    value: string;
    label: string;
}