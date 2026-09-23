using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class ContractorHoursSnapshots
    {
        [Key]
        public int Id { get; set; }
        public int ContractorContractId { get; set; }
        public DateTime SnapshotDate { get; set; }
        public decimal HoursUsed { get; set; }
        public decimal HoursRemaining { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        [ForeignKey(nameof(ContractorContractId))]
        public virtual ContractorContracts? ContractorContract { get; set; }
        [ForeignKey(nameof(CreatedBy))]
        public virtual UsersProfiles? CreatedByUser { get; set; }
    }
}
