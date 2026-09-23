using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Services
{
    public interface IContractorLicensesService
    {
        Task<IReadOnlyList<ContractorLicenses>> GetContractorLicenses(int contractorId, CancellationToken ct = default);
        Task<ResultDTO<ContractorLicenses>> Add(ContractorLicenses license, CancellationToken ct = default);
        Task<ResultDTO<ContractorLicenses>> Update(ContractorLicenses license, CancellationToken ct = default);
        Task<ResultDTO<object>> Delete(int id, CancellationToken ct = default);
    }
}
