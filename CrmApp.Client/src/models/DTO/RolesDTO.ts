export class RolesClaimsDTO {
    id!: string;
    name!: string;
    key!: string;
    isRole: boolean = false;
    isClaim: boolean = false;
    children: RolesClaimsDTO[] = [];
    parentId?: string;
}