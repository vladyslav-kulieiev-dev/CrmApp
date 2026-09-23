using CrmApp.Domain.DTO;
using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class UsersProfiles : ExtendableClass
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; } = default!; // FK do AspNetUsers.Id
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string? ForeignSystemOperatorId { get; set; }
        public ESystemType? ForeignSystemType { get; set; }
        public bool IsDeleted { get; set; }
        public string? AlternativeEmails { get; set; } = default;
        public string? CalendarProvider { get; set; } = default;
        public string? CalendarId { get; set; } = default;
        [NotMapped]
        public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper();
        [NotMapped]
        public string? Email { get; set; }
    }
}
