using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Application.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using CrmApp.Domain.Identity;
using CrmApp.Domain.Tools;
using CrmApp.Infrastructure.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CrmApp.Api.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _service;
        public SettingsController(ISettingsService service)
        {
            _service = service;
        }

        [HttpGet("settings")]
        [Authorize]
        public async Task<ActionResult<List<Settings>>> GetAll(CancellationToken ct = default)
        {
            var settings = await _service.GetAll(ct);
            return Ok(settings);
        }

        [HttpGet("setting-value")]
        [Authorize]
        public async Task<ActionResult<List<Settings>>> GetValueByKey(string key, CancellationToken ct = default)
        {
            var value = await _service.GetSettingValueByKey(key, ct);
            return Ok(value);
        }

        [HttpPut("setting-value")]
        [Authorize(Policy = "perm:" + AppClaims.SystemSettings)]
        public async Task<ActionResult<ResultDTO<string>>> UpdateValueByKey([FromBody] Settings setting, CancellationToken ct = default)
        {
            var result = await _service.SetSettingValueByKey(setting.Key, setting.Value, ct);
            return Ok(result);
        }
    }
}
