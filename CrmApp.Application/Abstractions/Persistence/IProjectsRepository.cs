using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface IProjectsRepository : IRepository<Projects>
    {
        Task<IReadOnlyList<Projects>> ListByContractorIdAsync(int contractorId, CancellationToken ct = default);
        Task<bool> ExistsWithNameForContractorAsync(string name, int? contractorId, CancellationToken ct = default);
        Task<bool> ExistsWithNameForContractorAsync(int id, string name, int? contractorId, CancellationToken ct = default);
        Task UpdateProjectsMembers(Projects project, List<int> usersIds, int projectManagerId, CancellationToken ct = default);
        Task RemoveProjectMembers(int projectId);
    }
}
