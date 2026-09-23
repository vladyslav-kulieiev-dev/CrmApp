using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Domain.Entities;
using CrmApp.Infrastructure.Persistence.Errors;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Persistence.Repositories
{
    public class ProjectsRepository(AppDbContext db) : IProjectsRepository
    {
        private readonly AppDbContext _db = db;

        public Task<bool> ExistsWithNameForContractorAsync(string name, int? contractorId, CancellationToken ct = default) =>
            _db.Projects.AnyAsync(x => x.Name == name && x.ContractorId == contractorId, ct);

        public Task<bool> ExistsWithNameForContractorAsync(int id, string name, int? contractorId, CancellationToken ct = default) =>
            _db.Projects.AnyAsync(x => x.Name == name && x.ContractorId == contractorId && x.Id != id, ct);
        public Task AddAsync(Projects item, CancellationToken ct = default) => _db.AddAsync(item, ct).AsTask();

        public Task<Projects?> GetAsync(int id, CancellationToken ct = default) => _db.Projects.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<Projects?> GetReadOnlyAsync(int id, CancellationToken ct = default) => _db.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        public async Task<IReadOnlyList<Projects>> ListAllAsync(CancellationToken ct = default) =>
            await _db.Projects.AsNoTracking()
            .Include(x => x.Contractor)
            .Include(x => x.ProjectManager)
            .ToListAsync(ct);
        public async Task<IReadOnlyList<Projects>> ListByContractorIdAsync(int contractorId, CancellationToken ct = default) =>
            await _db.Projects.AsNoTracking()
            .Where(x => x.ContractorId == contractorId)
            .Include(x => x.Contractor)
            .Include(x => x.ProjectManager)
            .ToListAsync(ct);

        public Task Remove(Projects item)
        {
            _db.Projects.Remove(item);
            return Task.CompletedTask;
        }

        public Task RemoveProjectMembers(int projectId)
        {
            var projectMembers = _db.ProjectsMembers.Where(x => x.ProjectId == projectId).ToList();
            _db.ProjectsMembers.RemoveRange(projectMembers);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Projects item, CancellationToken ct = default)
        {
            _db.Projects.Update(item);
            return Task.CompletedTask;
        }

        public async Task UpdateProjectsMembers(Projects project, List<int> usersIds, int projectManagerId, CancellationToken ct = default)
        {
            if (!usersIds.Contains(projectManagerId)) usersIds.Add(projectManagerId);

            var current = await _db.ProjectsMembers
                .Where(x => x.ProjectId == project.Id)
                .Select(x => x.UserProfileId)
                .ToListAsync(ct);

            var toAdd = usersIds.Except(current).Select(uid => new ProjectsMembers
            {
                Project = project,
                UserProfileId = uid,
                IsProjectManager = uid ==  projectManagerId
            });
            if (toAdd.Any()) _db.ProjectsMembers.AddRange(toAdd);

            var toRemove = current.Except(usersIds).ToList();
            if (toRemove.Count > 0)
            {
                var rows = _db.ProjectsMembers.Where(x => x.ProjectId == project.Id && toRemove.Contains(x.UserProfileId));
                _db.ProjectsMembers.RemoveRange(rows);
            }
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            try { return _db.SaveChangesAsync(ct); }
            catch (DbUpdateException ex) { throw DbExceptionTranslator.Translate(ex); }
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
