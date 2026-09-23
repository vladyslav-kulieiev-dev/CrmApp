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
    public class Projects : ExtendableClass
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public EProjectState State { get; set; } = EProjectState.Created;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int ProjectManagerId { get; set; }
        public int? ContractorId { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual UsersProfiles? CreatedUser { get; set; }

        [ForeignKey(nameof(ModifiedBy))]
        public virtual UsersProfiles? ModifiedUser { get; set; }

        [ForeignKey(nameof(ProjectManagerId))]
        public virtual UsersProfiles? ProjectManager { get; set; }
        [ForeignKey(nameof(ContractorId))]
        public virtual Contractors? Contractor { get; set; }

        [NotMapped]
        public List<int> MembersIds { get; set; } = [];
        [NotMapped]
        public string StateName => State.GetDescription();
        [NotMapped]
        public string? StateIcon => State.GetIcon()?.Icon;
        [NotMapped]
        public string? StateIconClass => State.GetIcon()?.IconClass;
        [NotMapped]
        public string? StartDateStr { get; set; }
        [NotMapped]
        public string? EndDateStr { get; set; }
    }
}
