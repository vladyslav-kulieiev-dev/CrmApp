using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface IUsersRepository : IRepository<UsersProfiles>
    {
        Task<string?> GetEmailAsync(int userId, CancellationToken ct = default);
        Task<List<string>> GetEmailsByRoleAsync(string role, CancellationToken ct = default);
        Task<UsersProfiles?> GetByUserIdAsync(string userId, CancellationToken ct = default);
        Task<UsersProfiles?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ProfileExistsAsync(string userId, CancellationToken ct = default);
        Task<List<UsersProfiles>> GetUsersProfilesAsync(CancellationToken ct = default);
        UserConfiguration GetUserConfiguration(string userId);
        Task<(int userId, List<string> rolesIds)> GetLoggedUserRolesAsync(string userId, CancellationToken ct = default);
        Task Deactivate(UsersProfiles item);
        Task Activate(UsersProfiles item);
    }
}
