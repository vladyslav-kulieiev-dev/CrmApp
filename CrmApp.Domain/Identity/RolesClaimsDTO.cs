using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Identity
{
    public class RolesClaimsDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public bool IsRole { get; set; } = false;
        public bool IsClaim { get; set; } = false;
        public string? ParentId { get; set; } = null;
        public List<RolesClaimsDTO> Children { get; set; } = new List<RolesClaimsDTO>();
    }
}
