using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.DTO.TasksDTOs;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Services
{
    public interface ITasksService
    {
        Task<ResultDTO<TaskDTO>> GetByIdAsync(int id, int userId, List<string> rolesIds, CancellationToken ct = default);
        Task<PagedResult<TaskListItemDTO>> GetPagedAsync(TaskPagedRequest request, int userId, List<string> rolesIds, CancellationToken ct = default);
        Task<List<Tasks>> ListAllWithImportedAsync();
        Task<ResultDTO<TaskDTO>> Add(TaskDTO taskDTO, int userId, List<string> rolesIds, CancellationToken ct = default);
        Task<ResultDTO<TaskDTO>> AddTaskFromImported(ImportedTasks importedTask, int userId, List<string> rolesIds, CancellationToken ct = default);
        Task<int> AddImportedTask(ImportedTasks imported, CancellationToken ct = default);
        Task AssignImportedTaskToTask(int taskId, int importedTaskId, CancellationToken ct = default);
        Task<ResultDTO<TaskDTO>> Update(TaskDTO task, int userId, List<string> rolesIds, CancellationToken ct = default);
        Task<ResultDTO<TaskDTO>> Delete(int id, int userId, List<string> rolesIds, CancellationToken ct = default);
        Task<ResultDTO<TaskCommentDTO>> AddComment(TaskCommentDTO dto, int userId, CancellationToken ct = default);
        Task<ResultDTO<TaskCommentDTO>> EditComment(TaskCommentDTO dto, int userId, CancellationToken ct = default);
        Task<ResultDTO<TaskCommentDTO>> DeleteComment(int commentId, int userId, CancellationToken ct = default);
    }
}
