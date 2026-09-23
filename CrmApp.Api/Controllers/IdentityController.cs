using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CrmApp.Domain.Identity;
using CrmApp.Infrastructure.Identity;
using System.Security.Claims;

namespace CrmApp.Api.Controllers
{
    [ApiController]
    [Route("api/identity")]
    public class IdentityController : ControllerBase
    {
        private readonly IdentityService _id;

        public IdentityController(IdentityService id) => _id = id;

        [HttpPost("roles")]
        [Authorize(Policy = "perm:" + AppClaims.RolesCreate)]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            var (ok, errors) = await _id.CreateRoleAsync(roleName);
            return ok ? Ok() : BadRequest(new { errors });
        }

        [HttpPost("users/{userId}/roles")]
        [Authorize(Policy = "perm:" + AppClaims.UserClaimsManage)]
        public async Task<IActionResult> AddUserToRole(string userId, [FromBody] string roleName)
        {
            var (ok, errors) = await _id.AddUserToRoleAsync(userId, roleName);
            return ok ? Ok() : BadRequest(new { errors });
        }

        [HttpPost("me/claims")]
        [Authorize(Policy = "perm:" + AppClaims.UserClaimsManage)]
        public async Task<IActionResult> AddSelfClaim([FromBody] ClaimRequest req)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (ok, errors) = await _id.AddSelfClaimAsync(userId, req.Type, req.Value, refreshSignIn: false);
            return ok ? Ok() : BadRequest(new { errors });
        }

        [HttpGet("users/{userId}/claims")]
        [Authorize(Policy = "perm:" + AppClaims.UserClaimsView)]
        public async Task<IActionResult> GetEffectiveClaims(string userId)
        {
            var claims = await _id.GetEffectiveClaimsAsync(userId);
            var dto = claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(dto);
        }

        [HttpGet("roles")]
        [Authorize(Policy = "perm:" + AppClaims.RolesView)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _id.GetRolesAndClaims();
            return Ok(roles);
        }
    }

    public sealed record ClaimRequest(string Type, string Value);
}
