using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class ContractorLicenseHistory
    {
        [Key]
        public int Id { get; set; }
        public int ContractorLicenseId { get; set; }
        public DateTime ChangedAt { get; set; }
        public int ChangedBy { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;

        [ForeignKey(nameof(ContractorLicenseId))]
        public virtual ContractorLicenses? ContractorLicense { get; set; }
        [ForeignKey(nameof(ChangedBy))]
        public virtual UsersProfiles? ChangedByUser { get; set; }
    }
}
