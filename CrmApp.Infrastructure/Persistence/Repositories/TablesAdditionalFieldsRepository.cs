using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Domain.Configuration;
using CrmApp.Domain.Entities;
using CrmApp.Infrastructure.Identity;
using CrmApp.Infrastructure.Persistence.Errors;
using System.Data.Common;

namespace CrmApp.Infrastructure.Persistence.Repositories
{
    public sealed class TablesAdditionalFieldsRepository(AppDbContext db, ApplicationDbContext appDb)
        : ITablesAdditionalFieldsRepository
    {
        private readonly AppDbContext _db = db;
        private readonly ApplicationDbContext _appDb = appDb;

        public Task AddAsync(TablesAdditionalFields item, CancellationToken ct = default) =>
            _db.TablesAdditionalFields.AddAsync(item, ct).AsTask();
        public Task AddRangeAsync(
            IEnumerable<TablesAdditionalFields> items, CancellationToken ct = default) =>
            _db.TablesAdditionalFields.AddRangeAsync(items, ct);

        public Task RemoveRangeAsync(
            IEnumerable<TablesAdditionalFields> items,
            CancellationToken ct = default)
        {
            _db.TablesAdditionalFields.RemoveRange(items);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(
            IEnumerable<TablesAdditionalFields> items,
            CancellationToken ct = default)
        {
            _db.TablesAdditionalFields.UpdateRange(items);
            return Task.CompletedTask;
        }

        public Task<TablesAdditionalFields?> GetAsync(int id, CancellationToken ct = default) =>
            _db.TablesAdditionalFields.FirstOrDefaultAsync(n => n.Id == id, ct);

        public Task<TablesAdditionalFields?> GetReadOnlyAsync(int id, CancellationToken ct = default) =>
            _db.TablesAdditionalFields.AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id, ct);

        public async Task<TablesAdditionalFields?> GetWithPermissionsAsync(
            int id, CancellationToken ct = default)
        {
            var res = await _db.TablesAdditionalFields.AsNoTracking()
                .Include(f => f.Dictionary)
                .Include(f => f.Permissions!)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(f => f.Id == id, ct);

            var roleIds = res?.Permissions != null ? res.Permissions
                .Where(x => !string.IsNullOrWhiteSpace(x.RoleId))
                .Select(x => x.RoleId).Distinct().ToList() : [];

            if (roleIds.Count > 0 && res?.Permissions != null)
            {
                var roles = await _appDb.Roles
                    .AsNoTracking()
                    .Where(r => roleIds.Contains(r.Id))
                    .ToDictionaryAsync(r => r.Id, r => r.Name, ct);

                foreach (var perm in res.Permissions)
                {
                    if (perm.RoleId != null && roles.TryGetValue(perm.RoleId, out var roleName))
                        perm.RoleName = roleName;
                }
            }

            return res;
        }

        public async Task<IReadOnlyList<TablesAdditionalFields>> ListAllAsync(
            CancellationToken ct = default)
        {
            return await _db.TablesAdditionalFields
                .AsNoTracking()
                .OrderBy(f => f.TableName).ThenBy(f => f.SortOrder)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<TablesAdditionalFields>> ListByTableNameAsync(
            string tableName, int? rowId, CancellationToken ct = default)
        {
            return await _db.TablesAdditionalFields
                .AsNoTracking()
                .Where(f => f.TableName == tableName && f.RowId == rowId)
                .OrderBy(f => f.SortOrder)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<TablesAdditionalFields>> ListByTableNameWithPermissionsAsync(
            string tableName, int? rowId, CancellationToken ct = default)
        {
            var fields = await _db.TablesAdditionalFields
               .AsNoTracking()
               .Where(f => f.TableName == tableName && f.RowId == rowId)
               .Include(f => f.Dictionary)
               .Include(f => f.Permissions!).ThenInclude(p => p.User)
               .OrderBy(f => f.SortOrder)
               .ToListAsync(ct);

            var roleIds = fields
                .SelectMany(f => f.Permissions ?? [])
                .Where(p => p.RoleId != null)
                .Select(p => p.RoleId!)
                .Distinct()
                .ToList();

            if (roleIds.Count > 0)
            {
                var roles = await _appDb.Roles
                    .AsNoTracking()
                    .Where(r => roleIds.Contains(r.Id))
                    .ToDictionaryAsync(r => r.Id, r => r.Name, ct);

                foreach (var perm in fields.SelectMany(f => f.Permissions ?? []))
                {
                    if (perm.RoleId != null && roles.TryGetValue(perm.RoleId, out var roleName))
                        perm.RoleName = roleName;
                }
            }

            return fields;
        }

        public async Task ReorderAsync(
            string tableName, Dictionary<int, int> orders, CancellationToken ct = default)
        {
            var ids = orders.Keys.ToList();
            var fields = await _db.TablesAdditionalFields
                .Where(f => f.TableName == tableName && ids.Contains(f.Id))
                .ToListAsync(ct);

            foreach (var field in fields)
                field.SortOrder = orders[field.Id];

            _db.TablesAdditionalFields.UpdateRange(fields);
        }

        public async Task<int> GetMaxSortOrderAsync(
            string tableName, CancellationToken ct = default)
        {
            return await _db.TablesAdditionalFields
                .Where(f => f.TableName == tableName)
                .MaxAsync(f => (int?)f.SortOrder, ct) ?? 0;
        }

        public Task Remove(TablesAdditionalFields item)
        {
            _db.TablesAdditionalFields.Remove(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TablesAdditionalFields item, CancellationToken ct = default)
        {
            _db.TablesAdditionalFields.Update(item);
            return Task.CompletedTask;
        }
        public async Task<IReadOnlyList<TablesAdditionalFields>> GetMappedFieldsForDocumentDefinitionAsync(
            int definitionId, CancellationToken ct)
        {
            return await _db.TablesAdditionalFields
                .AsNoTracking()
                .Where(x => x.TableName == TablesNames.InternalDocumentsDefinitions
                         && x.RowId == definitionId
                         && x.IsMappedFromInitObject)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<TablesAdditionalFields>> GetManyByIdsAsync(
            List<int> ids, CancellationToken ct = default)
        {
            if (ids.Count == 0) return [];
            return await _db.TablesAdditionalFields
                .AsNoTracking()
                .Where(f => ids.Contains(f.Id))
                .ToListAsync(ct);
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