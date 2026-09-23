using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class TasksComments
    {
        [Key]
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string Content { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual UsersProfiles? User { get; set; }
        [ForeignKey(nameof(TaskId))]
        public virtual Tasks? Task { get; set; }

        [NotMapped]
        public string? PostedAtString { get; set; }

        [NotMapped]
        public string? ModifiedAtString { get; set; }
    }
}
