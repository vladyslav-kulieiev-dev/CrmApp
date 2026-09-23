using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrmApp.Domain.Identity
{
    public static class AppRoles
    {
        public const string AdminRole = "Administrator";
        public const string UserRole = "Użytkownik";
        public const string ManageIdentity = "Zarządzanie użytkownikami";
        public const string ManageContractors = "Zarządzanie kontrahentami";
        public const string ManageProjects = "Zarządzanie projektami";
        public const string ManageCatalogItems = "Zarządzanie produktami i usługami";

        public static readonly string[] All =
        [
            AdminRole, UserRole, ManageIdentity, ManageContractors, ManageProjects, ManageCatalogItems
        ];
    }

    public static class AppClaims
    {
        public const string SystemSettings = "system.settings";

        // --- Identity / Tożsamość ---
        public const string ManageIdentity = "identity.manage";
        public const string UsersView = "identity.users.view";
        public const string UsersCreate = "identity.users.create";
        public const string UsersUpdate = "identity.users.update";
        public const string UsersDelete = "identity.users.delete";

        public const string RolesView = "identity.roles.view";
        public const string RolesCreate = "identity.roles.create";
        public const string RolesUpdate = "identity.roles.update";
        public const string RolesDelete = "identity.roles.delete";

        public const string UserClaimsView = "identity.userclaims.view";
        public const string UserClaimsManage = "identity.userclaims.add";
        public const string UserPasswordUpdate = "identity.userspassword.update";

        public const string RoleClaimsView = "identity.roleclaims.view";
        public const string RoleClaimsAdd = "identity.roleclaims.add";
        public const string RoleClaimsRemove = "identity.roleclaims.remove";

        public const string ContractorsView = "contractors.view";
        public const string ContractorsCreate = "contractors.create";
        public const string ContractorsUpdate = "contractors.update";
        public const string ContractorsDelete = "contractors.delete";

        public const string ProjectsView = "projects.view";
        public const string ProjectsCreate = "projects.create";
        public const string ProjectsUpdate = "projects.update";
        public const string ProjectsDelete = "projects.delete";

        public const string ContractorsContractsManage = "contractors.contracts.manage";
        public const string CatalogItemsCreate = "catalogitems.create";
        public const string CatalogItemsUpdate = "catalogitems.update";
        public const string CatalogItemsDelete = "catalogitems.delete";

        public static string[] UserRoleClaims = [ContractorsView, ProjectsView];

        public static string[] ManageProjectsClaims =
        [
            ProjectsView, ProjectsCreate, ProjectsUpdate, ProjectsDelete, ContractorsView, UsersView
        ];

        public static string[] ManageIdentityClaims = [ManageIdentity, UsersView, UsersCreate, UsersUpdate, UsersDelete, UserClaimsManage, UserClaimsView];
        public static string[] ManageContractorsClaims = [ContractorsView, ContractorsCreate, ContractorsUpdate, ContractorsDelete, ProjectsView, ContractorsContractsManage];
        public static string[] ManageCatalogItemsClaims = [CatalogItemsCreate, CatalogItemsUpdate, CatalogItemsDelete];

        public static string[] AllClaims =
        [
            SystemSettings, ManageIdentity,
            UsersView, UsersCreate, UsersUpdate, UsersDelete,
            RolesView, RolesCreate, RolesUpdate, RolesDelete,
            RoleClaimsView, RoleClaimsAdd, RoleClaimsRemove,
            UserClaimsView, UserClaimsManage, UserPasswordUpdate,
            ContractorsView, ContractorsCreate, ContractorsUpdate, ContractorsDelete,
            ProjectsView, ProjectsCreate, ProjectsUpdate, ProjectsDelete,
            ContractorsContractsManage,
            CatalogItemsCreate, CatalogItemsUpdate, CatalogItemsDelete
        ];

        public static Dictionary<string, string[]> RolesClaims = new Dictionary<string, string[]>
        {
            [AppRoles.AdminRole] = AllClaims,
            [AppRoles.UserRole] = UserRoleClaims,
            [AppRoles.ManageIdentity] = ManageIdentityClaims,
            [AppRoles.ManageContractors] = ManageContractorsClaims,
            [AppRoles.ManageProjects] = ManageProjectsClaims,
            [AppRoles.ManageCatalogItems] = ManageCatalogItemsClaims
        };
    }

    public static class AppClaimLabelsPL
    {
        public static readonly IReadOnlyDictionary<string, string> Map =
            new Dictionary<string, string>
            {
                [AppClaims.SystemSettings] = "Konfiguracja systemu",

                // Tożsamość
                [AppClaims.ManageIdentity] = "Zarządzanie użytkownikami",
                [AppClaims.UsersView] = "Użytkownicy: podgląd",
                [AppClaims.UsersCreate] = "Użytkownicy: tworzenie",
                [AppClaims.UsersUpdate] = "Użytkownicy: edycja",
                [AppClaims.UsersDelete] = "Użytkownicy: usuwanie",

                [AppClaims.RolesView] = "Role: podgląd",
                [AppClaims.RolesCreate] = "Role: tworzenie",
                [AppClaims.RolesUpdate] = "Role: edycja",
                [AppClaims.RolesDelete] = "Role: usuwanie",

                [AppClaims.UserClaimsView] = "Uprawnienia użytkowników: podgląd",
                [AppClaims.UserClaimsManage] = "Uprawnienia użytkowników: zarządzanie",
                [AppClaims.UserPasswordUpdate] = "Hasła użytkowników: resetowanie",

                [AppClaims.RoleClaimsView] = "Uprawnienia ról: podgląd",
                [AppClaims.RoleClaimsAdd] = "Uprawnienia ról: dodawanie",
                [AppClaims.RoleClaimsRemove] = "Uprawnienia ról: usuwanie",

                [AppClaims.ContractorsView] = "Kontrahenci: podgląd",
                [AppClaims.ContractorsCreate] = "Kontrahenci: tworzenie",
                [AppClaims.ContractorsUpdate] = "Kontrahenci: edycja",
                [AppClaims.ContractorsDelete] = "Kontrahenci: usuwanie",

                [AppClaims.ProjectsView] = "Projekty: podgląd",
                [AppClaims.ProjectsCreate] = "Projekty: tworzenie",
                [AppClaims.ProjectsUpdate] = "Projekty: edycja",
                [AppClaims.ProjectsDelete] = "Projekty: usuwanie",

                [AppClaims.ContractorsContractsManage] = "Umowy i licecje kontrahentów: zarządzanie",
                [AppClaims.CatalogItemsCreate] = "Produkty i usługi: tworzenie",
                [AppClaims.CatalogItemsUpdate] = "Produkty i usługi: edycja",
                [AppClaims.CatalogItemsDelete] = "Produkty i usługi: usuwanie"
            };
    }

    public static class IdentityErrorPL
    {
        public static readonly IReadOnlyDictionary<string, string> Map =
            new Dictionary<string, string>
            {
                ["PasswordRequiresLower"] = "Hasło musi zawierać małą literę a-z",
                ["PasswordRequiresDigit"] = "Hasło musi zawierać cyfrę",
                ["PasswordMismatch"] = "Hasła są różne",
                ["PasswordRequiresUpper"] = "Hasło musi zawierać wielką literę A-Z",
                ["PasswordTooShort"] = "Hasło musi zawierać min. 6 znaków",
                ["PasswordRequiresNonAlphanumeric"] = "Hasło musi zawierać znak inny niż alfanumeryczny",
                ["PasswordRequiresUniqueChars"] = "Hasło musi zawierać określoną ilość unikalnych znaków",
            };
    }

    public static class AppClaimTypes
    {
        public const string Permission = "permission"; // single claim type for all permissions
    }
}
