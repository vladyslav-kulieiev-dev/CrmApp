using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions
{
    public interface ICurrentUser
    {
        string? UserId { get; }
        Task<int?> GetProfileIdAsync(CancellationToken ct = default);
    }
}
