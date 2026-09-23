using System.Text.Json.Serialization;

namespace CrmApp.Domain.Identity
{
    public class UserConfiguration
    {
        public UserConfiguration() { }


        public List<string> Roles { get; set; } = new();
        public List<string> RolesIds { get; set; } = new();

        [JsonPropertyName("claims")]
        public List<string> ClaimKeys { get; set; } = new();

        [JsonIgnore]
        public List<KeyValuePair<string, string>> Claims =>
            AppClaimLabelsPL.Map.Where(x => ClaimKeys.Contains(x.Key)).ToList();

        public bool IsAdmin => Roles.Contains(AppRoles.AdminRole);
        public bool IsIdentityPolicyManager => Roles.Contains(AppRoles.ManageIdentity);
        public bool IsContractorsPolicyManager => Roles.Contains(AppRoles.ManageContractors);

        public bool CanSystemConfig => ClaimKeys.Contains(AppClaims.SystemSettings);

        public bool CanManageIdentity => ClaimKeys.Contains(AppClaims.ManageIdentity);
        public bool CanUsersView => ClaimKeys.Contains(AppClaims.UsersView);
        public bool CanUsersCreate => ClaimKeys.Contains(AppClaims.UsersCreate);
        public bool CanUsersUpdate => ClaimKeys.Contains(AppClaims.UsersUpdate);
        public bool CanUsersDelete => ClaimKeys.Contains(AppClaims.UsersDelete);

        public bool CanRolesView => ClaimKeys.Contains(AppClaims.RolesView);
        public bool CanRolesCreate => ClaimKeys.Contains(AppClaims.RolesCreate);
        public bool CanRolesUpdate => ClaimKeys.Contains(AppClaims.RolesUpdate);
        public bool CanRolesDelete => ClaimKeys.Contains(AppClaims.RolesDelete);

        public bool CanUserClaimsView => ClaimKeys.Contains(AppClaims.UserClaimsView);
        public bool CanUserClaimsManage => ClaimKeys.Contains(AppClaims.UserClaimsManage);
        public bool CanUserPasswordUpdate => ClaimKeys.Contains(AppClaims.UserPasswordUpdate);

        public bool CanRoleClaimsView => ClaimKeys.Contains(AppClaims.RoleClaimsView);
        public bool CanRoleClaimsAdd => ClaimKeys.Contains(AppClaims.RoleClaimsAdd);
        public bool CanRoleClaimsRemove => ClaimKeys.Contains(AppClaims.RoleClaimsRemove);

        public bool CanContractorsView => ClaimKeys.Contains(AppClaims.ContractorsView);
        public bool CanContractorsCreate => ClaimKeys.Contains(AppClaims.ContractorsCreate);
        public bool CanContractorsUpdate => ClaimKeys.Contains(AppClaims.ContractorsUpdate);
        public bool CanContractorsDelete => ClaimKeys.Contains(AppClaims.ContractorsDelete);

        public bool CanProjectsView => ClaimKeys.Contains(AppClaims.ProjectsView);
        public bool CanProjectsCreate => ClaimKeys.Contains(AppClaims.ProjectsCreate);
        public bool CanProjectsUpdate => ClaimKeys.Contains(AppClaims.ProjectsUpdate);
        public bool CanProjectsDelete => ClaimKeys.Contains(AppClaims.ProjectsDelete);

        public bool CanContractorsContractsManage => ClaimKeys.Contains(AppClaims.ContractorsContractsManage);
        public bool CanCatalogItemsCreate => ClaimKeys.Contains(AppClaims.CatalogItemsCreate);
        public bool CanCatalogItemsUpdate => ClaimKeys.Contains(AppClaims.CatalogItemsUpdate);
        public bool CanCatalogItemsDelete => ClaimKeys.Contains(AppClaims.CatalogItemsDelete);
    }
}
