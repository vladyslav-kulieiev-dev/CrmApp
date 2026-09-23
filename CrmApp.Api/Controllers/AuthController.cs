using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Identity;
using CrmApp.Infrastructure.Identity;
using CrmApp.Infrastructure.Persistence;
using System;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using System.Transactions;

namespace CrmApp.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UsersManager _usersManager;
        public AuthController(UsersManager usersManager)
        {
            _usersManager = usersManager;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            // Check if the email already exists
            var result = await _usersManager.AddAsync(model, ct: HttpContext.RequestAborted);

            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        // POST /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var res = await _usersManager.Login(dto);
            return res.Succeeded ? Ok(res) : Unauthorized(res);
        }

        // POST /api/auth/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _usersManager.LogoutAsync();
            return NoContent();
        }

        // GET /api/auth/me
        [HttpGet("me")]
        [AllowAnonymous]
        public async Task<IActionResult> Me()
        {
            if (!(User?.Identity?.IsAuthenticated ?? false)) return Unauthorized();
            var res = await _usersManager.Me(User);
            return Ok(res);
        }

        [HttpGet("is-logged-in")]
        [AllowAnonymous]
        public IActionResult IsLoggedIn()
        {
            if (User?.Identity?.IsAuthenticated == true && _usersManager.IsLoggedIn()) 
                return Ok(new SuccessResultDTO<object>());

            return Ok(new ErrorResultDTO<object>([]));
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ResultDTO<string>>> ForgotPassword([FromBody] ForgotPasswordDTO req, CancellationToken ct)
        {
            var result = await _usersManager.ForgotPassword(req, ct);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ResultDTO<string>>> ResetPassword([FromBody] ResetPasswordWthTokenDTO req, CancellationToken ct)
        {
            var result = await _usersManager.ResetPasswordWithToken(req, ct);
            return Ok(result);
        }
    }
}
