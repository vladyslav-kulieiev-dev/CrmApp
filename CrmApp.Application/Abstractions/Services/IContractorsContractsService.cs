using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Services
{
    public interface IContractorsContractsService
    {
        Task<ContractorContracts?> GetContractorContract(int contractId, CancellationToken ct = default);
        Task<PagedResult<ContractorContracts>> GetContractorContracts(int contractorId, CancellationToken ct = default);
        Task<IReadOnlyList<ContractorHoursSnapshots>> GetContractSnapshots(int contractId, CancellationToken ct = default);
        Task<ResultDTO<ContractorHoursSnapshots>> AddSnapshot(int contractId, decimal hoursUsed, int userId, CancellationToken ct = default);
        Task<ResultDTO<ContractorContracts>> Add(ContractorContracts contract, CancellationToken ct = default);
        Task<ResultDTO<ContractorContracts>> Update(ContractorContracts contract, CancellationToken ct = default);
        Task<ResultDTO<object>> Delete(int id, CancellationToken ct = default);
    }
}
