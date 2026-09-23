using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using CrmApp.Domain.Identity;

namespace CrmApp.Api.Controllers
{
    [ApiController]
    [Route("api/dictionaries")]
    public class DictionariesController(IDictionariesService service) : ControllerBase
    {
        private readonly IDictionariesService _service = service;

        [HttpGet("dictionary-by-type/{type}")]
        [Authorize]
        public async Task<ActionResult<Dictionaries>> GetDictionaryByType(int type, CancellationToken ct = default)
        {
            var result = await _service.GetByType((EDictionaryType)type, ct);
            return Ok(result);
        }

        [HttpGet("dictionary/{id}")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<Dictionaries>>> GetDictionaryById(int id, CancellationToken ct = default)
        {
            var result = await _service.GetById(id, ct);
            return Ok(result);
        }

        [HttpGet("dictionary-elements/{id}")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<Dictionaries>>> GetDictionaryElements(int id, CancellationToken ct = default)
        {
            var result = await _service.GetDictionaryElements(id, ct);
            return Ok(result);
        }


        [HttpGet("dictionaries")]
        public async Task<ActionResult<List<Dictionaries>>> GetDictionaries(CancellationToken ct = default)
        {
            var result = await _service.GetAll(ct);
            return Ok(result);
        }

        [HttpGet("system-dictionaries")]
        public async Task<ActionResult<List<Dictionaries>>> GetSystemDictionaries(CancellationToken ct = default)
        {
            var result = await _service.GetSystemDictionaries(ct);
            return Ok(result);
        }

        [HttpPost("dictionary")]
        [Authorize(Policy = "perm:" + AppClaims.SystemSettings)]
        public async Task<ActionResult<ResultDTO<Dictionaries>>> AddDictionary(Dictionaries Dictionary, CancellationToken ct = default)
        {
            var result = await _service.Add(Dictionary, ct);
            return Ok(result);
        }

        [HttpPut("dictionary")]
        [Authorize(Policy = "perm:" + AppClaims.SystemSettings)]
        public async Task<ActionResult<ResultDTO<Dictionaries>>> UpdateDictionary(Dictionaries Dictionary, CancellationToken ct = default)
        {
            var result = await _service.Update(Dictionary, ct);
            return Ok(result);
        }

        [HttpPut("activate-deactivate")]
        [Authorize(Policy = "perm:" + AppClaims.SystemSettings)]
        public async Task<ActionResult<ResultDTO<Dictionaries>>> ActivateDeactivateDictionary(Dictionaries Dictionary, bool activate, CancellationToken ct = default)
        {
            var result = await _service.ActivateDeactivate(Dictionary, activate, ct);
            return Ok(result);
        }

        [HttpDelete("dictionary/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.SystemSettings)]
        public async Task<ActionResult<ResultDTO<Dictionaries>>> DeleteDictionary(int id, CancellationToken ct = default)
        {
            var result = await _service.Delete(id, ct);
            return Ok(result);
        }
    }
}
