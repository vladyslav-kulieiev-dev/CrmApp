using CrmApp.Domain.DTO.AdditionalFields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.TasksDTOs
{

    public class TaskListItemDTO
    {
        public int Id { get; set; }
        public string TaskNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? Notes { get; set; } = string.Empty;
        public int Progress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public int? AssignedTo { get; set; }
        public string? AssignedToFullName { get; set; }
        public int? ContractorId { get; set; }
        public string? ContractorName { get; set; }
        public int? ContractorContactId { get; set; }
        public string? ContractorContactName { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ImportedTaskNumber { get; set; }
        public string? ImportedTaskUrl { get; set; }
        public List<AdditionalFieldValueDTO> AdditionalFieldValues { get; set; } = [];
        public bool IsDeleted { get; set; }
    }
}
