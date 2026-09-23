using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Services
{
    public interface IContractorContactsService
    {
        Task<IReadOnlyList<ContractorContacts>> GetContractorContacts(int contractorId, CancellationToken ct = default);
        Task<ResultDTO<IReadOnlyList<ContractorContacts>>> SaveContractorContacts(int contractorId, List<ContractorContacts> contacts, CancellationToken ct = default);
    }
}
