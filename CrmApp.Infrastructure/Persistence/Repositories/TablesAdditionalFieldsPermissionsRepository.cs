using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Domain.Entities;
using CrmApp.Infrastructure.Persistence.Errors;
using System.Data.Common;

namespace CrmApp.Infrastructure.Persistence.Repositories
{
    public sealed class TablesAdditionalFieldsPermissionsRepository(AppDbContext db)
        : ITablesAdditionalFieldsPermissionsRepository
    {
        private readonly AppDbContext _db = db;

        public Task AddAsync(
            TablesAdditionalFieldsPermissions item, CancellationToken ct = default) =>
            _db.TablesAdditionalFieldsPermissions.AddAsync(item, ct).AsTask();

        public Task AddRangeAsync(
            IEnumerable<TablesAdditionalFieldsPermissions> items, CancellationToken ct = default) =>
            _db.TablesAdditionalFieldsPermissions.AddRangeAsync(items, ct);

        public Task<TablesAdditionalFieldsPermissions?> GetAsync(
            int id, CancellationToken ct = default) =>
            _db.TablesAdditionalFieldsPermissions.FirstOrDefaultAsync(p => p.Id == id, ct);

        public Task<TablesAdditionalFieldsPermissions?> GetReadOnlyAsync(
            int id, CancellationToken ct = default) =>
            _db.TablesAdditionalFieldsPermissions.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<IReadOnlyList<TablesAdditionalFieldsPermissions>> ListAllAsync(
            CancellationToken ct = default)
        {
            return await _db.TablesAdditionalFieldsPermissions.AsNoTracking().ToListAsync(ct);
        }

        public async Task<IReadOnlyList<TablesAdditionalFieldsPermissions>> ListByFieldIdAsync(
            int fieldId, CancellationToken ct = default)
        {
            return await _db.TablesAdditionalFieldsPermissions
                .AsNoTracking()
                .Where(p => p.TableAdditionalFieldId == fieldId)
                .Include(p => p.User)
                .ToListAsync(ct);
        }

        public async Task RemoveByFieldIdAsync(int fieldId, CancellationToken ct = default)
        {
            var perms = await _db.TablesAdditionalFieldsPermissions
                .Where(p => p.TableAdditionalFieldId == fieldId)
                .ToListAsync(ct);

            if (perms.Count > 0)
                _db.TablesAdditionalFieldsPermissions.RemoveRange(perms);
        }

        public Task Remove(TablesAdditionalFieldsPermissions item)
        {
            _db.TablesAdditionalFieldsPermissions.Remove(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            TablesAdditionalFieldsPermissions item, CancellationToken ct = default)
        {
            _db.TablesAdditionalFieldsPermissions.Update(item);
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

        public async Task CommitTransaction(
            IDbContextTransaction transaction, CancellationToken ct = default)
        {
            try { await transaction.CommitAsync(ct); }
            catch (SqlException ex) { throw DbExceptionTranslator.Translate(ex); }
            catch (DbException ex) { throw DbExceptionTranslator.Translate(ex); }
        }

        public async Task RollbackTransaction(
            IDbContextTransaction transaction, CancellationToken ct = default)
        {
            try { await transaction.RollbackAsync(ct); }
            catch (SqlException ex) { throw DbExceptionTranslator.Translate(ex); }
            catch (DbException ex) { throw DbExceptionTranslator.Translate(ex); }
        }
    }
}