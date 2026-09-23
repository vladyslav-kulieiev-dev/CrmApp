using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Identity;
using CrmApp.Infrastructure.Identity;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CrmApp.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UsersManager _usersManager;
        private readonly IAuthorizationService _authorization;
        public UsersController(UsersManager usersManager, IAuthorizationService authorizationService)
        {
            _usersManager = usersManager;
            _authorization = authorizationService;
        }

        [HttpGet("users")]
        [Authorize]
        public async Task<ActionResult<List<UsersDTO>>> GetUsersProfilesAsync(CancellationToken ct = default)
        {
            var usersProfiles = await _usersManager.GetUsers(ct);
            return Ok(usersProfiles);
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<UsersProfiles>>> GetUserProfileAsync(int id, CancellationToken ct = default)
        {
            var userProfile = await _usersManager.GetUserByIdAsync(id, ct);
            if (userProfile is null)
            {
                return NotFound(new ErrorResultDTO<UsersDTO>(["Nie znaleziono profilu użytkownika"]));
            }
            return Ok(new SuccessResultDTO<UsersDTO>(id.ToString(), []) { Data = userProfile });
        }

        [HttpPost("user")]
        [Authorize(Policy = "perm:" + AppClaims.UsersCreate)]
        public async Task<ActionResult<ResultDTO<UsersDTO>>> CreateUser([FromBody] RegisterDTO register, CancellationToken ct = default)
        {
            var canClaimsManage = (await _authorization.AuthorizeAsync(User, $"perm:{AppClaims.UserClaimsManage}")).Succeeded;
            var result = await _usersManager.AddAsync(register, canClaimsManage, ct);
            return Ok(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ResultDTO<UsersDTO>>> Register([FromBody] RegisterDTO register, CancellationToken ct = default)
        {
            var result = await _usersManager.AddAsync(register, false, ct);
            return Ok(result);
        }

        [HttpPut("user")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<UsersDTO>>> Update([FromBody] UsersDTO user, CancellationToken ct = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var canUserUpdate = (await _authorization.AuthorizeAsync(User, $"perm:{AppClaims.UserClaimsManage}")).Succeeded;
            if (!canUserUpdate && userId != user.UserId)
                return BadRequest();

            var canClaimsManage = (await _authorization.AuthorizeAsync(User, $"perm:{AppClaims.UserClaimsManage}")).Succeeded;
            var result = await _usersManager.UpdateAsync(user, canClaimsManage, ct);
            return Ok(result);
        }

        [HttpPut("password-change")]
        [Authorize(Policy = "perm:" + AppClaims.UserPasswordUpdate)]
        public async Task<ActionResult<ResultDTO<object>>> ChangeOthersPassword([FromBody] ResetPasswordDTO passwordReset, CancellationToken ct = default)
        {
            var result = await _usersManager.UpdateOthersPassword(passwordReset.UserId, passwordReset.NewPassword);
            return Ok(result);
        }

        [HttpPut("own-password-change")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<object>>> ChangeOwnPassword([FromBody] ResetPasswordDTO passwordReset, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(passwordReset.OldPassword))
                return Ok(new ErrorResultDTO<object>(["Podaj stare hasło. Jeśli nie pamiętasz hasła, zresetuj je."]));

            var result = await _usersManager.UpdateOwnPassword(passwordReset.UserId, passwordReset.OldPassword, passwordReset.NewPassword);
            return Ok(result);
        }

        [HttpPut("deactivate")]
        [Authorize(Policy = "perm:" + AppClaims.ManageIdentity)]
        public async Task<ActionResult<ResultDTO<object>>> DeactivateUser([FromBody] int id, CancellationToken ct = default)
        {
            var result = await _usersManager.Deactivate(id, ct);
            return Ok(result);
        }

        [HttpPut("activate")]
        [Authorize(Policy = "perm:" + AppClaims.ManageIdentity)]
        public async Task<ActionResult<ResultDTO<object>>> ActivateUser([FromBody] int id, CancellationToken ct = default)
        {
            var result = await _usersManager.ActivateAccount(id, ct);
            return Ok(result);
        }
    }
}
