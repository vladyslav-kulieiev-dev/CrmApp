using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.CatalogItemsDTO;
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
    public interface ICatalogItemsService
    {
        Task<IReadOnlyList<CatalogItems>> ListAllAsync(bool onlyActive, CancellationToken ct = default);
        Task<CatalogItems?> GetById(int id, CancellationToken ct = default);
        Task<ResultDTO<CatalogItems>> Add(CatalogItems item, CancellationToken ct = default);
        Task<ResultDTO<CatalogItems>> Update(CatalogItems item, CancellationToken ct = default);
        Task<ResultDTO<CatalogItems>> ActivateDeactivateItem(int id, bool activate, CancellationToken ct = default);
        Task<ResultDTO<object>> Delete(int id, CancellationToken ct = default);
        Task<PagedResult<CatalogItemsListItemDTO>> GetPagedAsync(CatalogItemPagedRequest request, int userId, List<string> rolesIds, CancellationToken ct = default);
    }
}
