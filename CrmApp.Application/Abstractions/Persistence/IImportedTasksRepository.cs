using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface IImportedTasksRepository : IRepository<ImportedTasks>
    {
        Task<ImportedTasks?> GetImportedTaskByExternalId(string externalId, CancellationToken ct);
        Task<int> AddAsyncReturnId(ImportedTasks item, CancellationToken ct = default);
    }
}
