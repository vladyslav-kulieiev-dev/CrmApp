using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface IContractorContactsRepository : IRepository<ContractorContacts>
    {
        Task<ContractorContacts?> GetByEmailAsync(string email, CancellationToken ct);
        Task<IReadOnlyList<ContractorContacts>> ListByContractorId(int contractorId, CancellationToken ct);
    }
}
