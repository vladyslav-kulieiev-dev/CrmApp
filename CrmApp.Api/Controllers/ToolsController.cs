using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using CrmApp.Domain.Tools;

namespace CrmApp.Api.Controllers
{
    [ApiController]
    [Route("api/tools")]
    public class ToolsController : ControllerBase
    {
        [HttpGet("enum-values")]
        [Authorize]
        public ActionResult<List<ValueNameDTO<int>>> GetEnumValues(string enumName)
        {
            var type = typeof(ESystemType).Assembly.GetTypes()
                .Where(x => x.Name == enumName || x.FullName == enumName).FirstOrDefault();
            var enumValues = type != null ? EnumTools.GetEnumValues(type) : new List<ValueNameDTO<int>>();
            return Ok(enumValues);
        }

        [HttpGet("columns/{tableName}")]
        [Authorize]
        public IActionResult GetColumns(string tableName)
        {
            var typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "CatalogItems", typeof(CatalogItems) },
                { "Contractors", typeof(Contractors) },
                { "Tasks",       typeof(Tasks) },
                { "UsersProfiles", typeof(UsersProfiles) },
            };

            if (!typeMap.TryGetValue(tableName, out var type))
                return NotFound($"Nieznana tabela: {tableName}");

            var columns = type.GetProperties()
                .Where(p => !p.GetGetMethod()?.IsVirtual ?? true) 
                .Select(p => p.Name)
                .ToList();

            return Ok(columns);
        }
    }
}
