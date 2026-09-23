using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface IContractorsRepository : IRepository<Contractors>
    {
        Task<IReadOnlyList<Contractors>> ListByIdsAsync(List<int> contractorsIds, CancellationToken ct = default);
        bool IsExistsByCode(string code);
        bool IsExistsByCode(string code, int id);
        Task<List<string>> ContractorsCodesByNip(IEnumerable<string> nips, int? excludeId = null, CancellationToken ct = default);
        Task<List<string>> GetNipNumbersAsync(int contractorId, CancellationToken ct = default);
        Task UpdateContractorsNipsAsync(Contractors contractor, string defNip, List<string> altNips, CancellationToken ct = default);
        Task<List<int>> GetContractorCurrentEngagementTypes(int contractorId, CancellationToken ct = default);
        Task<PagedResult<ContractorListItemDTO>> GetPagedAsync(ContractorsPagedRequest request, List<int> additionalFieldIds, CancellationToken ct = default);
    }
}
