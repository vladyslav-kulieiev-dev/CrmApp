using CrmApp.Domain.DTO.CatalogItemsDTO;
using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface ICatalogItemsRepository : IRepository<CatalogItems>
    {
        Task<IReadOnlyList<CatalogItems>> ListActiveAsync(CancellationToken ct = default);
        Task<List<int>> GetSupportedSystems(int catalogItemId, CancellationToken ct = default);
        bool CheckIfExistsByCode(string code, int? id = null);
        bool CheckIfItemIsUsed(int id);
        Task UpdateSupportedSystems(CatalogItems catalogItem, CancellationToken ct);
        Task<PagedResult<CatalogItemsListItemDTO>> GetPagedAsync(CatalogItemPagedRequest request, List<int> additionalFieldIds, CancellationToken ct = default);
    }
}
