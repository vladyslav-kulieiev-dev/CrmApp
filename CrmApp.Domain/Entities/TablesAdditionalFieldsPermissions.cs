using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class TablesAdditionalFieldsPermissions
    {
        [Key]
        public int Id { get; set; }
        public int TableAdditionalFieldId { get; set; }
        public string? RoleId { get; set; }
        public int? UserId { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }

        [ForeignKey(nameof(TableAdditionalFieldId))]
        public virtual TablesAdditionalFields? TableAdditionalField { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UsersProfiles? User { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual UsersProfiles? CreatedByUser { get; set; }
        [NotMapped]
        public string? RoleName { get; set; }
    }
}
