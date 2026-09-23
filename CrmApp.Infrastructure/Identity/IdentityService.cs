using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CrmApp.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Identity
{
    public sealed class IdentityService
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly RoleManager<IdentityRole> _roles;
        private readonly SignInManager<ApplicationUser> _signIn; 

        private static readonly HashSet<string> AllowedSelfClaimTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "profile.timezone",
                "profile.language",
                "feature.beta"
                // add more safe types here; DO NOT include privilege claims
            };

        public IdentityService(
            UserManager<ApplicationUser> users,
            RoleManager<IdentityRole> roles,
            SignInManager<ApplicationUser> signIn)
        {
            _users = users;
            _roles = roles;
            _signIn = signIn;
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string roleName)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null) return false;
            return await _users.IsInRoleAsync(user, roleName);
        }

        public async Task<bool> IsUserInClaimAsync(string userId, string claim)
        {
            var claims = await GetUserClaimsAsync(userId);
            return claims.Any(x => x.Value == claim);
        }
        public async Task<bool> IsRoleExistsAsync(string roleName)
        {
            return await _roles.RoleExistsAsync(roleName);
        }

        public async Task<(bool Succeeded, string[] Errors)> CreateRoleAsync(string roleName)
        {
            if (await _roles.RoleExistsAsync(roleName)) return (true, Array.Empty<string>());
            var result = await _roles.CreateAsync(new IdentityRole(roleName));
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string[] Errors)> DeleteRoleAsync(string roleName)
        {
            var role = await _roles.FindByNameAsync(roleName);
            if (role is null) return (true, Array.Empty<string>());
            var result = await _roles.DeleteAsync(role);
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string[] Errors)> AddUserToRoleAsync(string userId, string roleName)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null) return (false, new[] { "User not found." });
            await CreateRoleAsync(roleName);
            var result = await _users.AddToRoleAsync(user, roleName);
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string[] Errors)> RemoveUserFromRoleAsync(string userId, string roleName)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null) return (false, new[] { "User not found." });
            var result = await _users.RemoveFromRoleAsync(user, roleName);
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<IReadOnlyList<string>> GetUserRolesAsync(string userId)
        {
            var u = await _users.FindByIdAsync(userId);
            if (u is null) return Array.Empty<string>();
            var roles = await _users.GetRolesAsync(u);
            return roles.ToList();
        }

        public async Task<(bool Succeeded, string[] Errors)> AddUserClaimAsync(string userId, string type, string value, bool refreshSignIn = false)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null) return (false, new[] { "User not found." });

            var result = await _users.AddClaimAsync(user, new Claim(type, value));
            if (!result.Succeeded) return (false, result.Errors.Select(e => e.Description).ToArray());

            if (refreshSignIn) await _signIn.RefreshSignInAsync(user); // so the new claim appears in current cookie
            return (true, Array.Empty<string>());
        }

        public async Task<(bool Succeeded, string[] Errors)> RemoveUserClaimAsync(string userId, string type, string value, bool refreshSignIn = false)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null) return (false, new[] { "User not found." });

            var result = await _users.RemoveClaimAsync(user, new Claim(type, value));
            if (!result.Succeeded) return (false, result.Errors.Select(e => e.Description).ToArray());

            if (refreshSignIn) await _signIn.RefreshSignInAsync(user);
            return (true, Array.Empty<string>());
        }

        public async Task<IReadOnlyList<Claim>> GetUserClaimsAsync(string userId)
        {
            var u = await _users.FindByIdAsync(userId);
            if (u is null) return Array.Empty<Claim>();
            var claims = await _users.GetClaimsAsync(u);
            return claims.ToList();
        }

        public async Task<(bool Succeeded, string[] Errors)> AddRoleClaimAsync(string roleName, string type, string value)
        {
            var role = await _roles.FindByNameAsync(roleName);
            if (role is null) return (false, new[] { "Role not found." });
            var result = await _roles.AddClaimAsync(role, new Claim(type, value));
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string[] Errors)> RemoveRoleClaimAsync(string roleName, string type, string value)
        {
            var role = await _roles.FindByNameAsync(roleName);
            if (role is null) return (false, new[] { "Role not found." });
            var result = await _roles.RemoveClaimAsync(role, new Claim(type, value));
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
        }

      
        public async Task<IReadOnlyList<Claim>> GetEffectiveClaimsAsync(string userId, CancellationToken ct = default)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null) return Array.Empty<Claim>();

            var userClaims = await _users.GetClaimsAsync(user);
            var roles = await _users.GetRolesAsync(user);

            var roleClaims = new List<Claim>();
            foreach (var roleName in roles)
            {
                var role = await _roles.FindByNameAsync(roleName);
                if (role is null) continue;
                var rc = await _roles.GetClaimsAsync(role);
                roleClaims.AddRange(rc);
            }

            // De-duplicate exact type+value pairs
            var all = userClaims.Concat(roleClaims)
                                .GroupBy(c => (c.Type, c.Value))
                                .Select(g => g.First())
                                .ToList();

            return all;
        }

        // ---------- Self-service: user creates a claim for themselves ----------
        // (Guarded by a whitelist of safe claim types)
        public async Task<(bool Succeeded, string[] Errors)> AddSelfClaimAsync(string userId, string type, string value, bool refreshSignIn = false)
        {
            if (!AllowedSelfClaimTypes.Contains(type))
                return (false, new[] { $"Claim type '{type}' is not allowed for self-service." });

            return await AddUserClaimAsync(userId, type, value, refreshSignIn);
        }

        public async Task<IReadOnlyList<RolesClaimsDTO>> GetRolesAndClaims()
        {
            var roles = await _roles.Roles.OrderBy(x => x.Name).ToListAsync();
            var roleDtos = new List<RolesClaimsDTO>();
            foreach (var role in roles)
            {
                var claims = await _roles.GetClaimsAsync(role);
                var claimsValues = claims.Select(x => x.Value);
                var claimsNames = AppClaimLabelsPL.Map.Where(x => claimsValues.Contains(x.Key));
                roleDtos.Add(new RolesClaimsDTO
                {
                    Id = role.Id,
                    IsRole = true,
                    Name = role.Name,
                    Key = role.Name,
                    Children = claims.Select(c => new RolesClaimsDTO
                    {
                        Id = $"{role.Id}:{c.Value}",
                        IsClaim = true,
                        Name = claimsNames.Any(x => x.Key == c.Value) ? claimsNames.First(x => x.Key == c.Value).Value : $"{c.Value}",
                        Key = $"{c.Value}",
                        ParentId = role.Id
                    }).OrderBy(x => x.Name).ToList()
                });
            }
            return roleDtos;
        }
    }
}
