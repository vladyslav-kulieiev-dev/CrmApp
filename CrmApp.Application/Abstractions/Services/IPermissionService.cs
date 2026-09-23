using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Services
{
    public interface IPermissionService
    {
        Task<HashSet<string>> GetPermissionsAsync(string userId, CancellationToken ct = default);
    }
}
