export interface AdditionalFieldPermissionDTO {
    id: number;
    roleId?: string;
    userId?: number;
    roleName?: string;
    userName?: string;
    canView: boolean;
    canEdit: boolean;
}

export interface AdditionalFieldPermissionCreateDTO {
    roleId?: string;
    userId?: number;
    canView: boolean;
    canEdit: boolean;
}