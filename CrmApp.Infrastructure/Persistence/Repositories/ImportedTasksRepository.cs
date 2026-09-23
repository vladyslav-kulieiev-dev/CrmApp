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
    public sealed class ImportedTasksRepository : IImportedTasksRepository
    {
        private readonly AppDbContext _db;
        public ImportedTasksRepository(AppDbContext db) => _db = db;

        public Task AddAsync(ImportedTasks item, CancellationToken ct = default) =>
            _db.ImportedTasks.AddAsync(item, ct).AsTask();

        public async Task<int> AddAsyncReturnId(ImportedTasks item, CancellationToken ct = default)
        {
            await _db.ImportedTasks.AddAsync(item, ct);
            await _db.SaveChangesAsync(ct);
            return item.Id;
        }

        public Task<ImportedTasks?> GetAsync(int id, CancellationToken ct = default) =>
            _db.ImportedTasks.FirstOrDefaultAsync(n => n.Id == id, ct);

        public Task<ImportedTasks?> GetReadOnlyAsync(int id, CancellationToken ct = default) =>
            _db.ImportedTasks.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id, ct);

        public Task<ImportedTasks?> GetImportedTaskByExternalId(string externalId, CancellationToken ct = default) =>
            _db.ImportedTasks.AsNoTracking().FirstOrDefaultAsync(t => t.ExternalId == externalId, ct);

        public async Task<IReadOnlyList<ImportedTasks>> ListAllAsync(CancellationToken ct = default)
        {
            return await _db.ImportedTasks
                .AsNoTracking()
                .OrderBy(n => n.CreatedAt)
                .ToListAsync(ct);
        }

        public Task Remove(ImportedTasks item)
        {
            _db.ImportedTasks.Remove(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ImportedTasks item, CancellationToken ct = default)
        {
            _db.ImportedTasks.Update(item);
            return Task.CompletedTask;
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
