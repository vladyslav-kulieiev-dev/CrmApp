using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Services
{
    public sealed class ContractorContactsService : IContractorContactsService
    {
        private readonly IContractorContactsRepository _repo;
        public ContractorContactsService(IContractorContactsRepository repo)
        {
            _repo = repo;
        }

        public Task<IReadOnlyList<ContractorContacts>> GetContractorContacts(int contractorId, CancellationToken ct = default) => _repo.ListByContractorId(contractorId, ct);

        public async Task<ResultDTO<IReadOnlyList<ContractorContacts>>> SaveContractorContacts(int contractorId, List<ContractorContacts> contacts, CancellationToken ct = default)
        {
            var current = await _repo.ListByContractorId(contractorId, ct);

            foreach (var incoming in contacts.Where(c => c.Id > 0))
            {
                var existing = current.FirstOrDefault(c => c.Id == incoming.Id);
                if (existing == null) continue;

                existing.Firstname = incoming.Firstname;
                existing.Lastname = incoming.Lastname;
                existing.DisplayName = incoming.DisplayName;
                existing.Position = incoming.Position;
                existing.Email = incoming.Email;
                existing.PhoneNumber = incoming.PhoneNumber;
                await _repo.UpdateAsync(existing, ct);
            }

            foreach (var incoming in contacts.Where(c => c.Id == 0))
            {
                incoming.ContractorId = contractorId;
                incoming.CreatedAt = DateTime.UtcNow;
                await _repo.AddAsync(incoming, ct);
            }

            var incomingIds = contacts.Where(c => c.Id > 0).Select(c => c.Id).ToHashSet();
            foreach (var toRemove in current.Where(c => !incomingIds.Contains(c.Id)))
                await _repo.Remove(toRemove);

            await _repo.SaveChangesAsync(ct);

            return new SuccessResultDTO<IReadOnlyList<ContractorContacts>>(
                contractorId.ToString(), [])
            { Data = await _repo.ListByContractorId(contractorId, ct) };
        }
    }
}
