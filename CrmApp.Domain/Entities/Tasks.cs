using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmApp.Domain.Entities
{
    public class Tasks
    {
        [Key]
        public int Id { get; set; }
        public int Autonumeration { get; set; }
        public string TaskNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public int? AssignedTo { get; set; }
        public DateTime? AssignedAt {  get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description {  get; set; } = string.Empty;
        public string? Notes { get; set; } = string.Empty;
        public int? ProjectId { get; set; }
        public int? ContractorId { get; set; }
        public int? ContractorContactId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool IsDeleted { get; set; }
        public int? ImportedTaskId { get; set; }
        public int Progress { get; set; }
        public string? Source { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public UsersProfiles? CreatedByUser { get; set; }

        [ForeignKey(nameof(AssignedTo))]
        public UsersProfiles? AssignedToUser { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Projects? Project { get; set; }

        [ForeignKey(nameof(ContractorId))]
        public Contractors? Contractor { get; set; }

        [ForeignKey(nameof(ContractorContactId))]
        public ContractorContacts? ContractorContact { get; set; }

        [ForeignKey(nameof(ImportedTaskId))]
        public ImportedTasks? ImportedTask { get; set; }
    }
}
