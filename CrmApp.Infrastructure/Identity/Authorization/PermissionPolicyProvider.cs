using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Identity.Authorization
{
    public sealed class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public const string Prefix = "perm:";

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

        public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                var permission = policyName.Substring(Prefix.Length);

                var policy = new AuthorizationPolicyBuilder()
                                .AddRequirements(new PermissionRequirement(permission))
                                .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return base.GetPolicyAsync(policyName);
        }
    }
}
