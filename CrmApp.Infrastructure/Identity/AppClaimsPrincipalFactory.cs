using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using CrmApp.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Identity
{

    public sealed class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public AppClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        { }


        public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);

            foreach (var identity in principal.Identities)
            {
                var roleClaimType = identity.RoleClaimType;

                var toRemove = identity.Claims
                    .Where(c =>
                        c.Type == AppClaimTypes.Permission ||
                        c.Type == roleClaimType)
                    .ToList();

                foreach (var c in toRemove)
                    identity.RemoveClaim(c);
            }

            return principal;
        }
    }
}
