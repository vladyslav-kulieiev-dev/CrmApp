using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.AdditionalFields;
using CrmApp.Infrastructure.Identity;
using System.Security.Claims;

namespace CrmApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/additional-fields")]
    public class AdditionalFieldsController : ControllerBase
    {
        private readonly IAdditionalFieldsService _service;
        private readonly UsersManager _usersManager;

        public AdditionalFieldsController(IAdditionalFieldsService service, UsersManager usersManager)
        {
            _service = service;
            _usersManager = usersManager;
        }


        [HttpGet("table/{tableName}")]
        public async Task<IActionResult> GetFieldsForTable(string tableName)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            var result = await _service.GetFieldsForTableAsync(tableName, userId, rolesIds);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetFieldByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdditionalFieldCreateDTO dto)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            var result = await _service.CreateFieldAsync(dto, userId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AdditionalFieldUpdateDTO dto)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            var result = await _service.UpdateFieldAsync(dto, userId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteFieldAsync(id);
            return NoContent();
        }

        [HttpPut("reorder")]
        public async Task<IActionResult> Reorder([FromBody] AdditionalFieldReorderDTO dto)
        {
            await _service.ReorderFieldsAsync(dto);
            return NoContent();
        }


        [HttpGet("values/{tableName}/{rowId}")]
        public async Task<IActionResult> GetValuesForRecord(string tableName, string rowId)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            var result = await _service.GetValuesForRecordAsync(tableName, rowId, userId, rolesIds);
            return Ok(result);
        }

        [HttpPost("values/batch-get")]
        public async Task<IActionResult> GetValuesForRecords(
            [FromBody] BatchGetValuesRequestDTO dto)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            var result = await _service.GetValuesForRecordsAsync(
                dto.TableName, dto.RowIds, userId, rolesIds);
            return Ok(result);
        }

        [HttpPost("values/save")]
        public async Task<IActionResult> SaveValues(
            [FromBody] AdditionalFieldValuesBatchSaveDTO dto)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            await _service.SaveValuesForRecordAsync(dto, userId);
            return NoContent();
        }
    }

    public record BatchGetValuesRequestDTO(string TableName, List<string> RowIds);
}