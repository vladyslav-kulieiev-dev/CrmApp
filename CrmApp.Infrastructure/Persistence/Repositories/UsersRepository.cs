using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Identity;
using CrmApp.Infrastructure.Identity;
using CrmApp.Infrastructure.Persistence.Errors;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Persistence.Repositories
{
    internal sealed class UsersRepository : IUsersRepository
    {
        private readonly AppDbContext _db;
        private readonly ApplicationDbContext _appDbContext;
        private readonly IOptions<AdminUserOptions> _adminOpt;

        public UsersRepository(AppDbContext dbContext, ApplicationDbContext appDbContext, IOptions<AdminUserOptions> adminOpt)
        {
            _db = dbContext;
            _appDbContext = appDbContext;
            _adminOpt = adminOpt;
        }

        public Task<UsersProfiles?> GetAsync(int id, CancellationToken ct = default) =>
            _db.UsersProfiles.FirstOrDefaultAsync(p => p.Id == id, ct);
        
        public Task<UsersProfiles?> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
            _db.UsersProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        
        public async Task<UsersProfiles?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            var userId = await _appDbContext.Users
                .AsNoTracking()
                .Where(u => u.Email == email)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(ct);
            if (userId == null) return null;

            return await _db.UsersProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId, ct);
        } 
        public async Task<string?> GetEmailAsync(int userId, CancellationToken ct = default)
        {
            var identityUser = await _db.UsersProfiles
                .AsNoTracking()
                .Where(p => p.Id == userId)
                .FirstOrDefaultAsync(ct);

            if (identityUser == null) return null;

            if (identityUser.Email == _adminOpt.Value.Email)
                return !string.IsNullOrWhiteSpace(identityUser.AlternativeEmails) ? identityUser.AlternativeEmails.Split(",").FirstOrDefault() : string.Empty;
            
            return await _appDbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == identityUser.UserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(ct);
        }

        public Task<bool> ProfileExistsAsync(string userId, CancellationToken ct = default) =>
            _db.UsersProfiles.AnyAsync(p => p.UserId == userId, ct);

        public Task AddAsync(UsersProfiles profile, CancellationToken ct = default) =>
            _db.UsersProfiles.AddAsync(profile, ct).AsTask();

        // ── Add these methods to UsersRepository ────────────────────────────────────
        // They use _appDbContext (ApplicationDbContext / ASP.NET Identity) for role
        // lookups, exactly like your existing GetUserConfiguration() method.

        public async Task<List<string>> GetEmailsByRoleAsync(string role, CancellationToken ct = default)
        {
            var roleId = await _appDbContext.Roles
                .AsNoTracking()
                .Where(r => r.Name == role)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(ct);
            if (roleId == null)
                return [];

            var identityUserIds = await _appDbContext.UserRoles
                .AsNoTracking()
                .Where(ur => ur.RoleId == roleId)
                .Select(x => x.UserId)
                .ToListAsync(ct);
            if (identityUserIds.Count == 0) return [];

            var emails = await _appDbContext.Users
                .AsNoTracking()
                .Where(u => identityUserIds.Contains(u.Id) && u.Email != null && u.Email != _adminOpt.Value.Email)
                .Select(u => u.Email!)
                .ToListAsync(ct);

            var adminAcc = await _appDbContext.Users
                .AsNoTracking()
                .Where(u => identityUserIds.Contains(u.Id) && u.Email == _adminOpt.Value.Email)
                .FirstOrDefaultAsync(ct);
            if (adminAcc != null)
            {
                var adminAltMails = await _db.UsersProfiles
                    .Where(x => x.UserId == adminAcc.Id)
                    .Select(x => x.AlternativeEmails)
                    .FirstOrDefaultAsync(ct);
                if (!string.IsNullOrWhiteSpace(adminAltMails)) emails.AddRange(adminAltMails.Split(","));
            }

            return emails;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            try { return _db.SaveChangesAsync(ct); }
            catch (DbUpdateException ex) { throw DbExceptionTranslator.Translate(ex); }
        }

        public Task<List<UsersProfiles>> GetUsersProfilesAsync(CancellationToken ct = default)
        {
            return _db.UsersProfiles.Where(x => !x.IsDeleted).ToListAsync(ct);
        }

        public UserConfiguration GetUserConfiguration(string userId)
        {
            var userRolesIds = _appDbContext.UserRoles.AsNoTracking()
                .Where(x => x.UserId.Equals(userId))
                .Select(x => x.RoleId);
            var userRoles = _appDbContext.Roles.AsNoTracking()
                .Where(x => userRolesIds.Contains(x.Id))
                .Select(x => x.Name ?? "").ToArray();
            var userClaims = _appDbContext.UserClaims.AsNoTracking()
                .Where(x => x.UserId.Equals(userId))
                .Select(x => x.ClaimValue ?? "").ToArray();
            return new UserConfiguration { Roles = userRoles.ToList(), ClaimKeys = userClaims.ToList(), RolesIds = userRolesIds.ToList() };
        }

        public async Task<(int userId, List<string> rolesIds)> GetLoggedUserRolesAsync(string userId, CancellationToken ct = default)
        {
            var userRolesIds = await _appDbContext.UserRoles.AsNoTracking()
                .Where(x => x.UserId.Equals(userId))
                .Select(x => x.RoleId).ToListAsync(ct);
            var userProfile = await _db.UsersProfiles.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId, ct);
            return (userProfile?.Id ?? 0, userRolesIds);
        }

        public Task<UsersProfiles?> GetReadOnlyAsync(int id, CancellationToken ct = default) =>
            _db.UsersProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<IReadOnlyList<UsersProfiles>> ListAllAsync(CancellationToken ct = default) 
        {
            return await _db.UsersProfiles.AsNoTracking().ToListAsync(ct);
        }

        public Task UpdateAsync(UsersProfiles item, CancellationToken ct = default)
        {
            _db.UsersProfiles.Update(item);
            return Task.CompletedTask;
        }

        public Task Remove(UsersProfiles item)
        {
            _db.UsersProfiles.Remove(item);
            return Task.CompletedTask;
        }

        public Task Deactivate(UsersProfiles item)
        {
            item.IsDeleted = true;
            _db.UsersProfiles.Update(item);
            return Task.CompletedTask;
        }

        public Task Activate(UsersProfiles item)
        {
            item.IsDeleted = false;
            _db.UsersProfiles.Update(item);
            return Task.CompletedTask;
        }

        public async Task<IDbContextTransaction> BeginTransaction(CancellationToken ct = default)
        {
            try { return await _db.Database.BeginTransactionAsync(ct); }
            catch (SqlException ex) { throw DbExceptionTranslator.Translate(ex); }
            catch (DbException ex) { throw DbExceptionTranslator.Translate(ex); }
        }

        public async Task CommitTransaction(IDbContextTransaction transaction, CancellationToken ct = default)
        {
            try { await transaction.CommitAsync(ct); }
            catch (SqlException ex) { throw DbExceptionTranslator.Translate(ex); }
            catch (DbException ex) { throw DbExceptionTranslator.Translate(ex); }
        }

        public async Task RollbackTransaction(IDbContextTransaction transaction, CancellationToken ct = default)
        {
            try { await transaction.RollbackAsync(ct); }
            catch (SqlException ex) { throw DbExceptionTranslator.Translate(ex); }
            catch (DbException ex) { throw DbExceptionTranslator.Translate(ex); }
        }
    }
}
