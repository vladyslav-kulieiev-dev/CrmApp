using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using CrmApp.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO
{
    public class UsersDTO
    {
        public UsersDTO() 
        {
            Id = 0;
            UserId = string.Empty;
            DisplayName = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            UserConfiguration = new();
        }
        public UsersDTO(UsersProfiles userProfile, string? email, string[] alternativeEmails, string? phoneNumber, UserConfiguration? userConfiguration = null)
        {
            Id = userProfile.Id;
            UserId = userProfile.UserId ?? "";
            Email = email;
            AlternativeEmails = alternativeEmails;
            DisplayName = userProfile.DisplayName;
            FirstName = userProfile.FirstName;
            LastName = userProfile.LastName;
            ForeignSystemOperatorId = userProfile.ForeignSystemOperatorId;
            ForeignSystemType = userProfile.ForeignSystemType;
            CalendarProvider = userProfile.CalendarProvider;
            CalendarId = userProfile.CalendarId;
            PhoneNumber = phoneNumber;
            UserConfiguration = userConfiguration ?? new();
            IsDeleted = userProfile.IsDeleted;
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public string? Email { get; set; }
        public string[] AlternativeEmails { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }    
        public string? ForeignSystemOperatorId { get; set; }
        public ESystemType? ForeignSystemType { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsDeleted { get; set; }
        public UserConfiguration UserConfiguration { get; set; }
        public string? CalendarProvider { get; set; } = default;
        public string? CalendarId { get; set; } = default;
    }
}
