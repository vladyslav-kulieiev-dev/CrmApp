using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface IContractorLicensesRepository : IRepository<ContractorLicenses>
    {
        Task<IReadOnlyList<ContractorLicenses>> ListByContractorId(int contractorId, CancellationToken ct);
    }
}
