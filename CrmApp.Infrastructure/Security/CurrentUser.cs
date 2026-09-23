using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CrmApp.Application.Abstractions;
using CrmApp.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Security
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _http;
        private readonly AppDbContext _db;

        public CurrentUser(IHttpContextAccessor http, AppDbContext db)
        { _http = http; _db = db; }

        public string? UserId => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public Task<int?> GetProfileIdAsync(CancellationToken ct = default) =>
            UserId is null
              ? Task.FromResult<int?>(null)
              : _db.UsersProfiles.Where(p => p.UserId == UserId)
                                 .Select(p => (int?)p.Id)
                                 .SingleOrDefaultAsync(ct);
    }
}
