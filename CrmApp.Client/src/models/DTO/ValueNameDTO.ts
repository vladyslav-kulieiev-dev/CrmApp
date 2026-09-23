export class ValueNameDTO<T=number> {

    constructor(_val: T, _name: string, _icon?: string, _iconClass?: string, _key?: string, _isDefault?: boolean) {
        this.value = _val;
        this.name = _name;
        this.icon = _icon;
        this.iconClass = _iconClass;
        this.key = _key;
        this.isDefault = _isDefault;
    }

    value: T;
    name: string;
    icon?: string;
    iconClass?: string;
    key?: string;
    isDefault?: boolean;
}