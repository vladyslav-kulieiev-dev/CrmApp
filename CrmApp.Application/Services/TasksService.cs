using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.Configuration;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.AdditionalFields;
using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.DTO.TasksDTOs;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using CrmApp.Domain.Tools;

namespace CrmApp.Application.Services
{
    public class TasksService(
        ITasksRepository _tasksRepo,
        ITasksCommentsRepository _commentsRepo,
        IImportedTasksRepository _importedTasksRepo,
        IAdditionalFieldsService _additionalFieldsService,
        IUsersRepository _usersRepo,
        IContractorContactsRepository _contrContactRepo,
        IDictionariesService _dictionariesService) : ITasksService
    {
        public async Task<ResultDTO<TaskDTO>> GetByIdAsync(int id, int userId, List<string> rolesIds, CancellationToken ct = default)
        {
            var task = await _tasksRepo.GetReadOnlyAsync(id, ct);
            if (task == null) return new ErrorResultDTO<TaskDTO>(["Nie znaleziono zadania o podanym ID."]);

            var fields = await _additionalFieldsService.GetFieldsForTableAsync(TablesNames.Tasks, userId, rolesIds, ct);
            var additionalFieldIds = fields.Select(f => f.Id).ToList();
            var comments = await _commentsRepo.ListByTaskIdAsync(id, ct);
            var importedTask = task.ImportedTaskId.HasValue
                ? await _importedTasksRepo.GetReadOnlyAsync(task.ImportedTaskId.Value, ct)
                : null;
            var taskDto = new TaskDTO(task, comments.ToList(), importedTask);
            taskDto.AdditionalFieldValues = await _additionalFieldsService.GetValuesForRecordAsync(
                TablesNames.Tasks, id.ToString(), userId, rolesIds, ct);
            return new SuccessResultDTO<TaskDTO>(taskDto);
        }

        public async Task<PagedResult<TaskListItemDTO>> GetPagedAsync(TaskPagedRequest request, int userId, List<string> rolesIds, CancellationToken ct = default)
        {
            var fields = await _additionalFieldsService.GetFieldsForTableAsync(TablesNames.Tasks, userId, rolesIds, ct);
            var additionalFieldIds = fields.Select(f => f.Id).ToList();
            return await _tasksRepo.GetPagedAsync(request, additionalFieldIds, ct);
        }

        public async Task<List<Tasks>> ListAllWithImportedAsync()
        {
            List<Tasks> list = (await _tasksRepo.ListAllAsync()).ToList();
            var withImportedTasks = list.Where(t => t.ImportedTaskId != null).ToList();
            return withImportedTasks;
        }

        private string BuildNumber(int autonumeration) => $"ZAD/{DateTime.Now.Month}/{DateTime.Now.Year}/{autonumeration}";

        public async Task<ResultDTO<TaskDTO>> Add(TaskDTO taskDTO, int userId, List<string> rolesIds, CancellationToken ct = default)
        {
            int taskNumber = await _tasksRepo.GetNextNumberAsync(ct);
            Tasks newTask = new()
            {
                CreatedAt = DateTime.Now,
                CreatedBy = userId,
                ClosedAt = taskDTO.ClosedAt,
                AssignedTo = taskDTO.AssignedTo,
                AssignedAt = taskDTO.AssignedTo != null ? DateTime.Now : null,
                Title = taskDTO.Title,
                Description = taskDTO.Description,
                Notes = taskDTO.Notes,
                Priority = taskDTO.Priority,
                State = taskDTO.State,
                ProjectId = taskDTO.ProjectId,
                ContractorId = taskDTO.ContractorId,
                ContractorContactId = taskDTO.ContractorContactId,
                Progress = taskDTO.Progress,
                Autonumeration = taskNumber,
                TaskNumber = BuildNumber(taskNumber),
                ImportedTaskId = taskDTO.ImportedTaskId,
                Source = taskDTO.Source
            };
            if (!string.IsNullOrWhiteSpace(taskDTO.DueDateString))
                newTask.DueDate = DateTools.DateTimeFromString(taskDTO.DueDateString);

            await _tasksRepo.AddAsync(newTask, ct);
            await _tasksRepo.SaveChangesAsync(ct);
            var taskFromDb = await _tasksRepo.GetTaskByNumber(taskNumber, ct);

            if (taskFromDb == null) return new ErrorResultDTO<TaskDTO>(["Nie znaleziono zadania o podanym numerze."]); ;

            if (taskDTO.AdditionalFieldValues?.Count > 0)
                await _additionalFieldsService.SaveValuesForRecordAsync(
                    AdditionalFieldsHelper.GetAdditionalFieldValueSaveDTO(TablesNames.Tasks, taskFromDb.Id, taskDTO.AdditionalFieldValues),
                    userId, ct);

            return await GetByIdAsync(taskFromDb.Id, userId, rolesIds, ct);
        }

        public async Task<ResultDTO<TaskDTO>> AddTaskFromImported(ImportedTasks importedTask, int userId, List<string> rolesIds, CancellationToken ct = default)
        {
            UsersProfiles? assignedTo = null;
            if (!string.IsNullOrWhiteSpace(importedTask.AssigneeEmail))
            {
                assignedTo = await _usersRepo.GetByEmailAsync(importedTask.AssigneeEmail, ct);
            }

            ContractorContacts? assignedContacts = null;
            if (!string.IsNullOrWhiteSpace(importedTask.ContactEmail))
                assignedContacts = await _contrContactRepo.GetByEmailAsync(importedTask.ContactEmail, ct);

            ResultDTO<TaskDTO>? taskResult = null;
            var importedTaskFromDb = await _importedTasksRepo.GetImportedTaskByExternalId(importedTask.ExternalId, ct);
            if (importedTaskFromDb == null)
            {
                await _importedTasksRepo.AddAsync(importedTask, ct);
                await _importedTasksRepo.SaveChangesAsync(ct);
            }
            else
            {
                importedTaskFromDb.Number = importedTask.Number;
                importedTaskFromDb.State = importedTask.State;
                importedTaskFromDb.Subject = importedTask.Subject;
                importedTaskFromDb.Description = importedTask.Description;
                importedTaskFromDb.Category = importedTask.Category;
                importedTaskFromDb.Url = importedTask.Url;
                importedTaskFromDb.CreatedAt = importedTask.CreatedAt;
                importedTaskFromDb.DueDate = importedTask.DueDate;
                importedTaskFromDb.Channel = importedTask.Channel;
                importedTaskFromDb.Priority = importedTask.Priority;
                importedTaskFromDb.ResponseDueDate = importedTask.ResponseDueDate;
                importedTaskFromDb.ProductName = importedTask.ProductName;
                importedTaskFromDb.ClosedAt = importedTask.ClosedAt;
                importedTaskFromDb.ContactFirstName = importedTask.ContactFirstName;
                importedTaskFromDb.ContactLastName = importedTask.ContactLastName;
                importedTaskFromDb.ContactPhoneNumber = importedTask.ContactPhoneNumber;
                importedTaskFromDb.ContactEmail = importedTask.ContactEmail;
                importedTaskFromDb.AssigneeFirstName = importedTask.AssigneeFirstName;
                importedTaskFromDb.AssigneeLastName = importedTask.AssigneeLastName;
                importedTaskFromDb.AssigneeEmail = importedTask.AssigneeEmail;
                importedTaskFromDb.IsDeleted = importedTask.IsDeleted;
                importedTaskFromDb.ExternalId = importedTask.ExternalId;
                await _importedTasksRepo.UpdateAsync(importedTaskFromDb, ct);
                await _importedTasksRepo.SaveChangesAsync(ct);
            }

            importedTaskFromDb = await _importedTasksRepo.GetImportedTaskByExternalId(importedTask.ExternalId, ct);
            var taskFromDb = await _tasksRepo.GetTaskByImportedTaskId(importedTaskFromDb?.Id ?? 0, ct);
            var state = await MapStateToInternal(importedTask.State, ct) ?? "";
            var priority = importedTask.Priority != null
                ? (await MapPriorityToInternal(importedTask.Priority, ct) ?? "")
                : "";
            var source = await MapSourceToInternal(importedTask.Channel, ct);
            

            if (taskFromDb == null)
            {
                taskResult = await Add(new()
                {
                    State = state,
                    AssignedTo = assignedTo?.Id,
                    ClosedAt = importedTask.ClosedAt,
                    Title = importedTask.Subject,
                    Description = importedTask.Description,
                    ContractorId = assignedContacts?.ContractorId,
                    ContractorContactId = assignedContacts?.Id,
                    DueDate = importedTask.DueDate,
                    ImportedTaskId = importedTaskFromDb?.Id,
                    Priority = priority,
                    Source = source
                }, userId, rolesIds, ct);
            }
            else
            {
                taskResult = await Update(new()
                {
                    Id = taskFromDb.Id,
                    Priority = priority,
                    State = state,
                    AssignedTo = assignedTo?.Id,
                    Title = importedTask.Subject,
                    Description = importedTask.Description,
                    ContractorId = assignedContacts?.ContractorId,
                    ContractorContactId = assignedContacts?.Id,
                    DueDate = importedTask.DueDate,
                    ImportedTaskId = importedTaskFromDb?.Id,
                    Source = source
                }, userId, rolesIds, ct);
            }

            return taskResult;
        }

        private async Task<string?> MapStateToInternal(string? externalState, CancellationToken ct)
            => await MapToInternal(externalState, EDictionaryType.CrmTaskStates, ct);

        private async Task<string?> MapPriorityToInternal(string? externalPriority, CancellationToken ct)
            => await MapToInternal(externalPriority, EDictionaryType.CrmTasksPriorities, ct);

        private async Task<string?> MapSourceToInternal(string? externalSource, CancellationToken ct)
            => await MapToInternal(externalSource, EDictionaryType.TaskSource, ct);

        private async Task<string?> MapToInternal(string? externalValue, EDictionaryType type, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(externalValue)) return null;
            var dictionary = await _dictionariesService.GetByType(type, ct);
            if (dictionary != null)
            {
                var dictionaryElements = await _dictionariesService.GetDictionaryElements(dictionary.Id, ct);
                var elem = dictionaryElements
                    .Where(x => !string.IsNullOrWhiteSpace(x.AlternativeValuesForMapping) &&
                                (x.AlternativeValuesForMapping == externalValue || x.AlternativeValuesForMapping!.Split(",").Contains(externalValue)))
                    .FirstOrDefault();
                if (elem != null)
                    return elem.Key;
            }
            return externalValue;
        }

        public async Task<int> AddImportedTask(ImportedTasks imported, CancellationToken ct = default)
        {
            var it = await _importedTasksRepo.GetImportedTaskByExternalId(imported.ExternalId, ct);
            if (it == null)
            {
                return await _importedTasksRepo.AddAsyncReturnId(imported);
            }
            else
            {
                return it.Id;
            }
        }
        public async Task AssignImportedTaskToTask(int taskId, int importedTaskId, CancellationToken ct = default)
        {
            var task = await _tasksRepo.GetAsync(taskId, ct) ?? throw new Exception($"Task {taskId} not found.");
            var importedTask = await _importedTasksRepo.GetAsync(importedTaskId, ct)
                ?? throw new Exception($"ImportedTask {importedTaskId} not found.");

            task.ImportedTaskId = importedTaskId;
            task.ImportedTask = importedTask;

            await _tasksRepo.UpdateAsync(task, ct);
            await _tasksRepo.SaveChangesAsync(ct);
        }
        public async Task<ResultDTO<TaskDTO>> Update(TaskDTO task, int userId, List<string> rolesIds, CancellationToken ct = default)
        {
            var taskFromDb = await _tasksRepo.GetAsync(task.Id, ct);
            if (taskFromDb == null) return new ErrorResultDTO<TaskDTO>(["Nie znaleziono zadania " + task.TaskNumber + " w bazie"]);

            taskFromDb.Title = task.Title;
            taskFromDb.Description = task.Description;
            taskFromDb.Notes = task.Notes;
            taskFromDb.State = task.State;
            taskFromDb.Priority = task.Priority;
            taskFromDb.ProjectId = task.ProjectId;
            taskFromDb.ContractorId = task.ContractorId;
            taskFromDb.ContractorContactId = task.ContractorContactId;
            taskFromDb.Progress = task.Progress;
            taskFromDb.Source = task.Source;

            if (!string.IsNullOrWhiteSpace(task.DueDateString))
                taskFromDb.DueDate = DateTools.DateTimeFromString(task.DueDateString);


            if (task.AssignedTo != taskFromDb.AssignedTo)
            {
                taskFromDb.AssignedTo = task.AssignedTo;
                taskFromDb.AssignedAt = task.AssignedTo != null ? DateTime.Now : null;
            }

            await _tasksRepo.UpdateAsync(taskFromDb, ct);
            await _tasksRepo.SaveChangesAsync(ct);

            if (task.AdditionalFieldValues?.Count > 0)
                await _additionalFieldsService.SaveValuesForRecordAsync(
                    AdditionalFieldsHelper.GetAdditionalFieldValueSaveDTO(TablesNames.Tasks, task.Id, task.AdditionalFieldValues),
                    userId, ct);

            return await GetByIdAsync(taskFromDb.Id, userId, rolesIds, ct);
        }

        public async Task<ResultDTO<TaskDTO>> Delete(int id, int userId, List<string> rolesIds, CancellationToken ct = default)
        {
            var task = await _tasksRepo.GetAsync(id, ct);
            if (task == null) return new ErrorResultDTO<TaskDTO>(["Nie znaleziono zadania o podanym ID."]);

            task.IsDeleted = true;
            //ustawić imported task na deleted
            if (task.ImportedTaskId != null)
            {
                var importedTask = await _importedTasksRepo.GetAsync(task.ImportedTaskId.Value, ct);
                if (importedTask != null) importedTask.IsDeleted = true;
            }

            await _tasksRepo.UpdateAsync(task, ct);
            await _tasksRepo.SaveChangesAsync(ct);

            return new SuccessResultDTO<TaskDTO>(new TaskDTO());
        }

        public async Task<ResultDTO<TaskCommentDTO>> AddComment(TaskCommentDTO dto, int userId, CancellationToken ct = default)
        {
            var task = await _tasksRepo.GetReadOnlyAsync(dto.TaskId, ct);
            if (task == null) return new ErrorResultDTO<TaskCommentDTO>(["Nie znaleziono zadania o podanym ID."]);

            var comment = new TasksComments
            {
                TaskId = dto.TaskId,
                UserId = userId,
                Content = dto.Content,
                PostedAt = DateTime.Now,
            };

            await _commentsRepo.AddAsync(comment, ct);
            await _commentsRepo.SaveChangesAsync(ct);

            var user = await _usersRepo.GetReadOnlyAsync(userId, ct);

            return new SuccessResultDTO<TaskCommentDTO>(new TaskCommentDTO
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserFullName = user?.DisplayName ?? string.Empty,
                Content = comment.Content,
                PostedAt = comment.PostedAt,
            });
        }

        public async Task<ResultDTO<TaskCommentDTO>> EditComment(TaskCommentDTO dto, int userId, CancellationToken ct = default)
        {
            var comment = await _commentsRepo.GetAsync(dto.Id, ct);
            if (comment == null) return new ErrorResultDTO<TaskCommentDTO>(["Nie znaleziono komentarza o podanym ID."]);
            if (comment.UserId != userId) return new ErrorResultDTO<TaskCommentDTO>(["Brak uprawnień do edycji tego komentarza."]);

            comment.Content = dto.Content;
            comment.ModifiedAt = DateTime.Now;

            await _commentsRepo.UpdateAsync(comment, ct);
            await _commentsRepo.SaveChangesAsync(ct);

            var user = await _usersRepo.GetReadOnlyAsync(userId, ct);

            return new SuccessResultDTO<TaskCommentDTO>(new TaskCommentDTO
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserFullName = user?.DisplayName ?? string.Empty,
                Content = comment.Content,
                PostedAt = comment.PostedAt,
                ModifiedAt = comment.ModifiedAt,
            });
        }

        public async Task<ResultDTO<TaskCommentDTO>> DeleteComment(int commentId, int userId, CancellationToken ct = default)
        {
            var comment = await _commentsRepo.GetAsync(commentId, ct);
            if (comment == null) return new ErrorResultDTO<TaskCommentDTO>(["Nie znaleziono komentarza o podanym ID."]);
            if (comment.UserId != userId) return new ErrorResultDTO<TaskCommentDTO>(["Brak uprawnień do usunięcia tego komentarza."]);

            await _commentsRepo.Remove(comment);
            await _commentsRepo.SaveChangesAsync(ct);

            return new SuccessResultDTO<TaskCommentDTO>(new TaskCommentDTO { Id = commentId });
        }
    }
}