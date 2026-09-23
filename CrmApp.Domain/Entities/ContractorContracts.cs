using CrmApp.Domain.DTO;
using CrmApp.Domain.Enums;
using CrmApp.Domain.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class ContractorContracts : ExtendableClass
    {
        [Key]
        public int Id { get; set; }
        public int ContractorId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public EEngagementType EngagementType { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public decimal HoursLimit { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public EBillingType BillingType { get; set; }
        public decimal? BillingAmount { get; set; }
        public bool AllowOverLimit { get; set; }
        public ERenewalType RenewalType { get; set; }
        public DateTime? RenewalDate { get; set; }

        [ForeignKey(nameof(ContractorId))]
        public virtual Contractors? Contractor { get; set; }
        [ForeignKey(nameof(CreatedBy))]
        public virtual UsersProfiles? CreatedByUser { get; set; }

        [NotMapped] public string EngagementTypeName => EngagementType.GetDescription();
        [NotMapped] public decimal? HoursUsed { get; set; }
        [NotMapped] public decimal? HoursRemaining { get; set; }
        [NotMapped] public string? ValidFromStr { get; set; }
        [NotMapped] public string? ValidToStr { get; set; }
        [NotMapped] public string? RenewalDateStr { get; set; }
        [NotMapped] public decimal? HoursUsedCurrentMonth { get; set; }
        [NotMapped] public decimal? HoursRemainingCurrentMonth { get; set; }
        [NotMapped] public decimal? HoursFromPrevMonth { get; set; }
        [NotMapped] public IReadOnlyList<ContractorHoursSnapshots> Snapshots { get; set; } = [];
    }
}
