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
    public interface IContractorContractsRepository : IRepository<ContractorContracts>
    {
        Task<ContractorContracts?> GetReadOnlyAsyncWithHours(int id, CancellationToken ct);
        Task<int> CountByYearAndMonthAsync(int year, int month, CancellationToken ct);
        Task<IReadOnlyList<ContractorContracts>> ListByContractorId(int contractorId, CancellationToken ct);
        Task<IReadOnlyList<ContractorContracts>> ListByContractorIdWithHours(int contractorId, CancellationToken ct);
        Task<IReadOnlyList<ContractorHoursSnapshots>> ListSnapshotsForContract(int contractId, CancellationToken ct);
        Task<PagedResult<ContractorContracts>> ListPagedAsync(ContractorContractsRequest req, CancellationToken ct);
        Task AddSnapshot(ContractorHoursSnapshots snapshot, CancellationToken ct);
    }
}
