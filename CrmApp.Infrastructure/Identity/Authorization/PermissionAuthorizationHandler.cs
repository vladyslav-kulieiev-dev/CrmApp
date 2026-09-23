using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using CrmApp.Application.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Identity.Authorization
{
    public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;
        private readonly IMemoryCache _cache;

        public PermissionAuthorizationHandler(IPermissionService permissionService, IMemoryCache cache)
        {
            _permissionService = permissionService;
            _cache = cache;
        }

        protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                PermissionRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return;

            var cacheKey = $"perm:{userId}";

            var permissions = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _permissionService.GetPermissionsAsync(userId);
            });

            if (permissions is not null && permissions.Contains(requirement.Permission))
                context.Succeed(requirement);
        }
    }
}
