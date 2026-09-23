using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.DTO.TasksDTOs;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface ITasksRepository : IRepository<Tasks>
    {
        Task<int> GetNextNumberAsync(CancellationToken ct);
        Task<Tasks?> GetTaskByImportedTaskId(int importedTaskId, CancellationToken ct);
        Task<Tasks?> GetTaskByNumber(int number, CancellationToken ct);
        Task<PagedResult<TaskListItemDTO>> GetPagedAsync(TaskPagedRequest request, List<int> additionalFieldIds, CancellationToken ct = default);
    }
}
