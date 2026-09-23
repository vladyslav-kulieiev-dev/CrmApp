using Microsoft.AspNetCore.Http;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Services
{
    public interface IContractorsService
    {
        Task<ResultDTO<Contractors>> GetByIdLightMode(int id, CancellationToken ct);
        Task<ResultDTO<Contractors>> GetById(int id, CancellationToken ct);
        Task<IReadOnlyList<Contractors>> GetAll(CancellationToken ct);
        Task<string> GetNameById(int id, CancellationToken ct);
        Task<List<string>> GetNipsById(int id, CancellationToken ct);
        Task<ResultDTO<Contractors>> Add(Contractors item, int userId, CancellationToken ct);
        Task<ResultDTO<Contractors>> Update(Contractors item, int userId, CancellationToken ct);
        Task<ResultDTO<object>> Delete(int id, CancellationToken ct);
        Task<ResultDTO<List<Contractors>>> Import(IFormFile file, CancellationToken ct);
        Task<PagedResult<ContractorListItemDTO>> GetPagedAsync(ContractorsPagedRequest request, int userId, List<string> rolesIds, CancellationToken ct = default);
    }
}
