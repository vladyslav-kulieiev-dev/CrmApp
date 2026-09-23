using Microsoft.AspNetCore.Identity;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.Identity;
using CrmApp.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Services
{
    public sealed class PermissionService : IPermissionService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<HashSet<string>> GetPermissionsAsync(string userId, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return new HashSet<string> (StringComparer.OrdinalIgnoreCase);

            var perms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // user claims (jeśli jeszcze gdzieś istnieją)
            var userClaims = await _userManager.GetClaimsAsync(user);
            foreach (var c in userClaims.Where(c => c.Type == AppClaimTypes.Permission))
                perms.Add(c.Value);

            // role claims (zalecane źródło)
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role is null) continue;

                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var c in roleClaims.Where(c => c.Type == AppClaimTypes.Permission))
                    perms.Add(c.Value);
            }

            return perms;
        }
    }
}
