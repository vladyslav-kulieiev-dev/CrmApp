using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.CatalogItemsDTO;
using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Identity;
using CrmApp.Infrastructure.Identity;

namespace CrmApp.Api.Controllers
{
    [ApiController]
    [Route("api/catalog-items")]
    public class CatalogItemsController(ICatalogItemsService _service, UsersManager userManager) : ControllerBase
    {
        private readonly ICatalogItemsService _service = _service;
        private readonly UsersManager _usersManager = userManager;

        [HttpGet("catalog-items")]
        [Authorize]
        public async Task<ActionResult<List<CatalogItems>>> GetCatalogItems(bool onlyActive, CancellationToken ct = default)
        {
            var items = await _service.ListAllAsync(onlyActive, ct);
            return Ok(items);
        }

        [HttpGet("catalog-item/{id}")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<CatalogItems>>> GetCatalogItemById(int id, CancellationToken ct = default)
        {
            var items = await _service.GetById(id, ct);
            return Ok(items);
        }

        [HttpPost("catalog-item")]
        [Authorize(Policy = "perm:" + AppClaims.CatalogItemsCreate)]
        public async Task<ActionResult<CatalogItems>> AddCatalogItem(CatalogItems item, CancellationToken ct = default)
        {
            var result = await _service.Add(item, ct);
            return Ok(result);
        }

        [HttpPut("catalog-item")]
        [Authorize(Policy = "perm:" + AppClaims.CatalogItemsUpdate)]
        public async Task<ActionResult<ResultDTO<CatalogItems>>> UpdateCatalogItem(CatalogItems item, CancellationToken ct = default)
        {
            var result = await _service.Update(item, ct);
            return Ok(result);
        }

        [HttpPut("activate-deactivate")]
        [Authorize(Policy = "perm:" + AppClaims.CatalogItemsUpdate)]
        public async Task<ActionResult<ResultDTO<CatalogItems>>> ActivateDeactivateItem([FromBody] int id, bool activate, CancellationToken ct = default)
        {
            var result = await _service.ActivateDeactivateItem(id, activate, ct);
            return Ok(result);
        }

        [HttpDelete("catalog-item/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.CatalogItemsDelete)]
        public async Task<ActionResult<ResultDTO<object>>> DeleteCatalogItem(int id, CancellationToken ct = default)
        {
            var result = await _service.Delete(id, ct);
            return Ok(result);
        }

        [HttpPost("paged")]
        [Authorize]
        public async Task<ActionResult<PagedResult<CatalogItemsListItemDTO>>> GetContractorsPaged(
            [FromBody] CatalogItemPagedRequest request, CancellationToken ct = default)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            var pagedList = await _service.GetPagedAsync(request, userId, rolesIds, ct);
            return Ok(pagedList);
        }
    }
}
