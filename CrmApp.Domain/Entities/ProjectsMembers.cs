using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class ProjectsMembers
    {
        [Key]
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int UserProfileId { get; set; }
        public bool IsProjectManager { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public virtual Projects? Project { get; set; }
        [ForeignKey(nameof(UserProfileId))]
        public virtual UsersProfiles? UserProfile { get; set; }
    }
}
