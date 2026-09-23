using System.ComponentModel.DataAnnotations;

namespace CrmApp.Domain.Entities
{
    public class ImportedTasks
    {
        [Key]
        public int Id { get; set; }
        public string ExternalId { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? Category { get; set; } = string.Empty;
        public string? Url { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Channel { get; set; } = string.Empty;
        public string? Priority { get; set; } = string.Empty;
        public DateTime? ResponseDueDate { get; set; }
        public string? ProductName { get; set; } = string.Empty;
        public DateTime? ClosedAt { get; set; }
        public string? ContactFirstName { get; set; } = string.Empty;
        public string? ContactLastName { get; set; } = string.Empty;
        public string? ContactPhoneNumber { get; set; } = string.Empty;
        public string? ContactEmail { get; set; } = string.Empty;
        public string? AssigneeFirstName { get; set; } = string.Empty;
        public string? AssigneeLastName { get; set; } = string.Empty;
        public string? AssigneeEmail { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }
}
