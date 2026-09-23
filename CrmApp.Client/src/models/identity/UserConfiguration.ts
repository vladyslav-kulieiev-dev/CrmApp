import { KeyValue } from "@angular/common";

export class UserConfiguration {

    constructor(roles: string[], claims: string[]) {

    }

    roles: string[] = [];
    claims: string[] = [];

    readonly isAdmin: boolean = false;
    readonly isIdentityPolicyManager: boolean = false;
    readonly isContractorsPolicyManager: boolean = false;

    readonly canSystemConfig: boolean = false;

    readonly canManageIdentity: boolean = false;
    readonly canUsersView: boolean = false;
    readonly canUsersCreate: boolean = false;
    readonly canUsersUpdate: boolean = false;
    readonly canUsersDelete: boolean = false;

    readonly canRolesView: boolean = false;
    readonly canRolesCreate: boolean = false;
    readonly canRolesUpdate: boolean = false;
    readonly canRolesDelete: boolean = false;

    readonly canUserClaimsView: boolean = false;
    readonly canUserClaimsManage: boolean = false;
    readonly canUserPasswordUpdate: boolean = false;

    readonly canRoleClaimsView: boolean = false;
    readonly canRoleClaimsAdd: boolean = false;
    readonly canRoleClaimsRemove: boolean = false;

    readonly canContractorsView: boolean = false;
    readonly canContractorsCreate: boolean = false;
    readonly canContractorsUpdate: boolean = false;
    readonly canContractorsDelete: boolean = false;

    readonly canProjectsView: boolean = false;
    readonly canProjectsCreate: boolean = false;
    readonly canProjectsUpdate: boolean = false;
    readonly canProjectsDelete: boolean = false;

    readonly canContractorsContractsManage: boolean = false;
    readonly canCatalogItemsCreate: boolean = false;
    readonly canCatalogItemsUpdate: boolean = false;
    readonly canCatalogItemsDelete: boolean = false;
}
