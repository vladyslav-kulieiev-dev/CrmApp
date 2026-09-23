using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Application.Services;
using CrmApp.Domain.Configuration;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using CrmApp.Domain.Identity;
using CrmApp.Domain.Tools;
using CrmApp.Infrastructure.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CrmApp.Api.Controllers
{
    [ApiController]
    [Route("api/contractors")]
    public class ContractorsController(IContractorsService service, 
        IContractorsContractsService contractorsContractsService,
        IContractorContactsService contractorContactsService,
        IContractorLicensesService contractorLicensesService,
        IAdditionalFieldsService additionalFieldsService,
        UsersManager userManager) : ControllerBase
    {
        private readonly IContractorsService _service = service;
        private readonly IContractorsContractsService _contractorsContractsService = contractorsContractsService;
        private readonly IContractorContactsService _contractorContactsService = contractorContactsService;
        private readonly IContractorLicensesService _contractorLicensesService = contractorLicensesService;
        private readonly IAdditionalFieldsService _additionalFieldsService = additionalFieldsService;
        private readonly UsersManager _usersManager = userManager;

        [HttpGet("contractors")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsView)]
        public async Task<ActionResult<List<Contractors>>> GetContractors(CancellationToken ct = default)
        {
            var contractors = await _service.GetAll(ct);
            return Ok(contractors);
        }

        [HttpGet("contractor/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsView)]
        public async Task<ActionResult<ResultDTO<Contractors>>> GetContractor(int id, CancellationToken ct = default)
        {
            var contractor = await _service.GetById(id, ct);
            return Ok(contractor);
        }

        [HttpGet("contractor-light/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsView)]
        public async Task<ActionResult<ResultDTO<Contractors>>> GetContractorLight(int id, CancellationToken ct = default)
        {
            var contractor = await _service.GetByIdLightMode(id, ct);
            return Ok(contractor);
        }

        [HttpGet("contractor-contacts/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsView)]
        public async Task<ActionResult<IReadOnlyList<Contractors>>> GetContractorContacts(int id, CancellationToken ct = default)
        {
            var contractor = await _contractorContactsService.GetContractorContacts(id, ct);
            return Ok(contractor);
        }

        [HttpGet("contractor-licenses/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsView)]
        public async Task<ActionResult<IReadOnlyList<Contractors>>> GetContractorLicenses(int id, CancellationToken ct = default)
        {
            var contractor = await _contractorLicensesService.GetContractorLicenses(id, ct);
            return Ok(contractor);
        }

        [HttpGet("contractor-contracts/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsView)]
        public async Task<ActionResult<PagedResult<ContractorContracts>>> GetContractorContracts(int id, CancellationToken ct = default)
        {
            var contractor = await _contractorsContractsService.GetContractorContracts(id, ct);
            return Ok(contractor);
        }

        [HttpGet("contractor-contract/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsView)]
        public async Task<ActionResult<ContractorContracts>> GetContractorContract(int id, CancellationToken ct = default)
        {
            var contractor = await _contractorsContractsService.GetContractorContract(id, ct);
            return Ok(contractor);
        }

        [HttpPost("paged")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsView)]
        public async Task<ActionResult<PagedResult<ContractorListItemDTO>>> GetContractorsPaged(
            [FromBody] ContractorsPagedRequest request, CancellationToken ct = default)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            var pagedContractors = await _service.GetPagedAsync(request, userId, rolesIds, ct);
            return Ok(pagedContractors);
        }

        [HttpPost("contractor")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsCreate)]
        public async Task<ActionResult<ResultDTO<Contractors>>> AddContractor(Contractors contractor, CancellationToken ct = default)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            var result = await _service.Add(contractor, userId, ct);
            return Ok(result);
        }

        [HttpGet("contractor-nips/{contractorId}")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsView)]
        public async Task<ActionResult<ResultDTO<Contractors>>> GetContractorNips(int contractorId, CancellationToken ct = default)
        {
            var result = await _service.GetNipsById(contractorId, ct);
            return Ok(result);
        }

        [HttpPost("import")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsCreate)]
        public async Task<ActionResult<ResultDTO<object>>> ImportContractors(IFormFile file, CancellationToken ct = default)
        {
            if (file is null || file.Length == 0)
                return BadRequest("Prześlij plik .csv do importu");

            var result = await _service.Import(file, ct);
            return Ok(result);
        }

        [HttpPut("contractor")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsUpdate)]
        public async Task<ActionResult<ResultDTO<Contractors>>> UpdateContractor(Contractors contractor, CancellationToken ct = default)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            var result = await _service.Update(contractor, userId, ct);
            return Ok(result);
        }

        [HttpDelete("contractor/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsDelete)]
        public async Task<ActionResult<Contractors>> DeleteContractor(int id, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var result = await _service.Delete(id, ct);
            return Ok(result);
        }

        [HttpPut("contractor-contacts")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsUpdate)]
        public async Task<ActionResult<ResultDTO<IReadOnlyList<ContractorContacts>>>> SaveContractorContacts(
            [FromBody] List<ContractorContacts> contacts, [FromQuery] int contractorId, CancellationToken ct = default)
        {
            var result = await _contractorContactsService.SaveContractorContacts(contractorId, contacts, ct);
            return Ok(result);
        }


        [HttpPost("contractor-license")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsUpdate)]
        public async Task<ActionResult<ResultDTO<ContractorLicenses>>> AddLicense(
            [FromBody] ContractorLicenses license, CancellationToken ct = default)
        {
            var result = await _contractorLicensesService.Add(license, ct);
            return Ok(result);
        }


        [HttpPut("contractor-license")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsUpdate)]
        public async Task<ActionResult<ResultDTO<ContractorLicenses>>> UpdateLicense(
            [FromBody] ContractorLicenses license,
            CancellationToken ct = default)
        {
            var result = await _contractorLicensesService.Update(license, ct);
            return Ok(result);
        }


        [HttpDelete("contractor-license/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsUpdate)]
        public async Task<ActionResult<ResultDTO<object>>> DeleteLicense(int id, CancellationToken ct = default)
        {
            var result = await _contractorLicensesService.Delete(id, ct);
            return Ok(result);
        }


        [HttpPost("contractor-contract")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsUpdate)]
        public async Task<ActionResult<ResultDTO<ContractorContracts>>> AddContract(
            [FromBody] ContractorContracts contract, 
            CancellationToken ct = default)
        {
            var result = await _contractorsContractsService.Add(contract, ct);
            return Ok(result);
        }

        [HttpPut("contractor-contract")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsUpdate)]
        public async Task<ActionResult<ResultDTO<ContractorContracts>>> UpdateContract(
            [FromBody] ContractorContracts contract,
            CancellationToken ct = default)
        {
            var result = await _contractorsContractsService.Update(contract, ct);
            return Ok(result);
        }

        [HttpDelete("contractor-contract/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsUpdate)]
        public async Task<ActionResult<ResultDTO<object>>> DeleteContract(int id, CancellationToken ct = default)
        {
            var result = await _contractorsContractsService.Delete(id, ct);
            return Ok(result);
        }

        [HttpGet("contract-snapshots/{contractId}")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsView)]
        public async Task<ActionResult<IReadOnlyList<ContractorHoursSnapshots>>> GetContractSnapshots(
            int contractId, CancellationToken ct = default)
        {
            var snapshots = await _contractorsContractsService.GetContractSnapshots(contractId, ct);
            return Ok(snapshots);
        }

        [HttpPost("contract-snapshot")]
        [Authorize(Policy = "perm:" + AppClaims.ContractorsUpdate)]
        public async Task<ActionResult<ResultDTO<ContractorHoursSnapshots>>> AddContractSnapshot(
            [FromBody] ContractorHoursSnapshots contractorHours, CancellationToken ct = default)
        {
            var result = await _contractorsContractsService.AddSnapshot(contractorHours.ContractorContractId, contractorHours.HoursUsed, contractorHours.CreatedBy, ct);
            return Ok(result);
        }

    }
}
