using CrmApp.Domain.DTO.AdditionalFields;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.TasksDTOs
{
    public class TaskDTO
    {
        public TaskDTO() { }
        public TaskDTO(Tasks task, List<TasksComments> comments, ImportedTasks? importedTask)
        {
            Id = task.Id;
            TaskNumber = task.TaskNumber;
            Title = task.Title;
            Description = task.Description;
            Notes = task.Notes;
            Priority = task.Priority;
            State = task.State;
            Progress = task.Progress;
            CreatedAt = task.CreatedAt;
            AssignedAt = task.AssignedAt;
            ClosedAt = task.ClosedAt;
            DueDate = task.DueDate;
            AssignedTo = task.AssignedTo;
            AssignedToFullName = task.AssignedToUser != null ? task.AssignedToUser.DisplayName : string.Empty;
            CreatedBy = task.CreatedBy;
            CreatedByFullName = task.CreatedByUser != null ? task.CreatedByUser.DisplayName : string.Empty;
            ProjectId = task.ProjectId;
            ProjectName = task.Project != null ? task.Project.Name : string.Empty;
            ContractorId = task.ContractorId;
            ContractorName = task.Contractor != null ? task.Contractor.DisplayName : string.Empty;
            ContractorContactId = task.ContractorContactId;
            ContractorContactFullName = task.ContractorContact != null ? task.ContractorContact.DisplayName : string.Empty;
            ContractorContactEmail = task.ContractorContact != null ? task.ContractorContact.Email : string.Empty;
            ImportedTaskId = task.ImportedTaskId;
            ImportedTask = importedTask;
            IsDeleted = task.IsDeleted;
            Source = task.Source;

            Comments = comments.Select(c => new TaskCommentDTO
            {
                Id = c.Id,
                TaskId = c.TaskId,
                UserId = c.UserId,
                UserFullName = c.User != null ? c.User.DisplayName : string.Empty,
                Content = c.Content,
                PostedAt = c.PostedAt,
                ModifiedAt = c.ModifiedAt
            }).ToList();
            ImportedTask = importedTask;
        }

        public int Id { get; set; }
        public string TaskNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public int Progress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? DueDate { get; set; }  
        public string? DueDateString { get; set; }  

        public int? AssignedTo { get; set; }
        public string? AssignedToFullName { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByFullName { get; set; }

        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public int? ContractorId { get; set; }
        public string? ContractorName { get; set; }
        public int? ContractorContactId { get; set; }
        public string? ContractorContactFullName { get; set; }
        public string? ContractorContactEmail { get; set; }

        public int? ImportedTaskId { get; set; }
        public ImportedTasks? ImportedTask { get; set; }
        public bool IsDeleted { get; set; }
        public string? Source { get; set; }
        public List<TaskCommentDTO> Comments { get; set; } = [];
        public List<AdditionalFieldValueDTO> AdditionalFieldValues { get; set; } = [];
    }

    public class TaskCommentDTO
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public class TaskUpsertDTO
    {
        public int? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public int Progress { get; set; }
        public DateTime? DueDate { get; set; }
        public int? AssignedTo { get; set; }
        public int? ProjectId { get; set; }
        public int? ContractorId { get; set; }
        public int? ContractorContactId { get; set; }
    }
}
