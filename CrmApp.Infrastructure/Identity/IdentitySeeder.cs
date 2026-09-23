using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CrmApp.Application.Abstractions;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Identity;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Identity;

public sealed class IdentitySeeder(
    UsersManager usersManager,
    IdentityService identityService,
    RoleManager<IdentityRole> roles,
    IOptions<AdminUserOptions> adminOptions,
    ILogger<IdentitySeeder> logger,
    IConfiguration cfg
) : IIdentitySeeder
{
    private async Task SeedUserAsync(string? email, string? password, string? firstname, string? lastname, string? displayName, string[]? usrRoles, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogInformation($"Pominięto seeding użytkownika {firstname} {lastname}: brak e-maila w konfiguracji.");
            return;
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning($"Pominięto seeding użytkownika {firstname} {lastname}: brak hasła w konfiguracji.");
            return;
        }

        var user = await usersManager.GetUserByEmail(email);
        if (user == null)
        {
            var ur = await usersManager.AddAsync(new RegisterDTO
            {
                UserDTO = new UsersDTO
                {
                    Email = email,
                    FirstName = firstname ?? string.Empty,
                    LastName = lastname ?? string.Empty,
                    DisplayName = displayName ?? string.Empty,
                    PhoneNumber = ""
                },
                Password = password
            }, false, ct);
            if (!ur.Succeeded || string.IsNullOrEmpty(ur.Data?.UserId))
            {
                logger.LogError("Failed creating user {Email}: {Errors}",
                    email, ur.Errors != null ? string.Join(", ", ur.Errors) : "No errors");
                return;
            }
            user = new ApplicationUser
            {
                Id = ur.Data?.UserId ?? string.Empty,
                Email = ur.Data?.Email,
                UserName = ur.Data?.Email,
                PhoneNumber = ur.Data?.PhoneNumber
            }; 
        }

        // Zawsze dodaj admina do roli Administrator
        if (user != null && adminOptions.Value.Email == email && !await identityService.IsUserInRoleAsync(user.Id, AppRoles.AdminRole))
        {
            var ar = await identityService.AddUserToRoleAsync(user.Id, AppRoles.AdminRole);
            if (!ar.Succeeded)
                logger.LogError("Failed assigning role {Role} to user {Email}: {Errors}",
                    AppRoles.AdminRole, email, ar.Errors != null ? string.Join(", ", ar.Errors) : "No errors");
        }

        // Dodaj do pozostałych ról z konfiguracji (opcjonalnie)
        foreach (var roleName in usrRoles ?? Enumerable.Empty<string>())
        {
            if (!await identityService.IsUserInRoleAsync(user!.Id, roleName))
            {
                var ar = await identityService.AddUserToRoleAsync(user.Id, roleName);
                if (!ar.Succeeded)
                    logger.LogError("Failed assigning role {Role} to user {Email}: {Errors}",
                        roleName, email, ar.Errors != null ? string.Join(", ", ar.Errors) : "No errors");
            }

            var appRole = await roles.FindByNameAsync(roleName);
            if (appRole != null)
            {
                var roleClaims = await roles.GetClaimsAsync(appRole);
                foreach (var claim in roleClaims)
                {
                    if (!await identityService.IsUserInClaimAsync(user!.Id, claim.Value))
                    {
                        var ac = await identityService.AddUserClaimAsync(user.Id, AppClaimTypes.Permission, claim.Value, false);
                        if (!ac.Succeeded)
                            logger.LogError("Failed assigning claim {0} to user {1}: {2}",
                                claim.Value, email, ac.Errors != null ? string.Join(", ", ac.Errors) : "No errors");
                    }
                }
            }
        }
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        AdminUserOptions adminOpts = adminOptions.Value;
        adminOpts.Password = cfg["Identity:Admin:Password"];

        var requiredRoles = AppRoles.All
        .Union(adminOpts.Roles ?? Enumerable.Empty<string>())
        .Distinct()
        .ToArray();

        foreach (var roleName in requiredRoles)
        {
            if (!await roles.RoleExistsAsync(roleName))
            {
                var rr = await roles.CreateAsync(new IdentityRole(roleName));
                if (!rr.Succeeded)
                    logger.LogError("Nie udało się utworzyć roli {Role}: {Errors}",
                        roleName, string.Join(", ", rr.Errors.Select(e => e.Description)));
                else
                    logger.LogInformation("Utworzono rolę {Role}", roleName);
            }
        }

        foreach (var (roleName, claimValues) in AppClaims.RolesClaims)
        {
            var role = await roles.FindByNameAsync(roleName);
            if (role is null) continue;

            var existing = await roles.GetClaimsAsync(role);
            var existingPairs = existing
                .Where(c => c.Type == AppClaimTypes.Permission)
                .Select(c => c.Value)
                .ToHashSet();

            foreach (var value in claimValues.Distinct())
            {
                if (existingPairs.Contains(value)) continue;

                var addRes = await roles.AddClaimAsync(role, new Claim(AppClaimTypes.Permission, value));
                if (!addRes.Succeeded)
                    logger.LogError("Nie udało się dodać uprawnienia {Value} do roli {Role}: {Errors}",
                        value, roleName, string.Join(", ", addRes.Errors.Select(e => e.Description)));
                else
                    logger.LogInformation("Dodano uprawnienie {Value} do roli {Role}", value, roleName);
            }
        }

        if (adminOpts != null)
            await SeedUserAsync(adminOpts.Email, adminOpts.Password, adminOpts.FirstName, adminOpts.LastName, adminOpts.DisplayName, adminOpts.Roles, ct);
    }
}
