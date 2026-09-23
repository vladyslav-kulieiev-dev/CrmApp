using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Identity
{
    public class UserOptions
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? DisplayName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
    public class AdminUserOptions : UserOptions
    {
        public string[] Roles { get; set; } = ["Administrator"];
    }
}
