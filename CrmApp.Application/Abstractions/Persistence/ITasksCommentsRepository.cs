using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface ITasksCommentsRepository : IRepository<TasksComments>
    {
        Task<IReadOnlyList<TasksComments>> ListByTaskIdReadOnlyAsync(int taskId, CancellationToken ct = default);
        Task<IReadOnlyList<TasksComments>> ListByTaskIdAsync(int taskId, CancellationToken ct = default);
        Task RemoveRange(List<TasksComments> items);
    }
}
