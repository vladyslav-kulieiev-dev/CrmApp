using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.AdditionalFields
{
    public class AdditionalFieldPermissionDTO
    {
        public int Id { get; set; }
        public string? RoleId { get; set; }
        public int? UserId { get; set; }
        public string? RoleName { get; set; }
        public string? UserName { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
    }

    public class AdditionalFieldPermissionCreateDTO
    {
        public string? RoleId { get; set; }
        public int? UserId { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
    }
}
