using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Domain.Entities;
using CrmApp.Infrastructure.Persistence.Errors;
using System.Data.Common;

namespace CrmApp.Infrastructure.Persistence.Repositories
{
    public sealed class TablesAdditionalFieldsValuesRepository(AppDbContext db)
        : ITablesAdditionalFieldsValuesRepository
    {
        private readonly AppDbContext _db = db;

        public Task AddAsync(TablesAdditionalFieldsValues item, CancellationToken ct = default) =>
            _db.TablesAdditionalFieldsValues.AddAsync(item, ct).AsTask();

        public Task AddRangeAsync(
            IEnumerable<TablesAdditionalFieldsValues> items, CancellationToken ct = default) =>
            _db.TablesAdditionalFieldsValues.AddRangeAsync(items, ct);

        public Task<TablesAdditionalFieldsValues?> GetAsync(
            int id, CancellationToken ct = default) =>
            _db.TablesAdditionalFieldsValues.FirstOrDefaultAsync(v => v.Id == id, ct);

        public Task<TablesAdditionalFieldsValues?> GetReadOnlyAsync(
            int id, CancellationToken ct = default) =>
            _db.TablesAdditionalFieldsValues.AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id, ct);

        public async Task<IReadOnlyList<TablesAdditionalFieldsValues>> ListAllAsync(
            CancellationToken ct = default)
        {
            return await _db.TablesAdditionalFieldsValues.AsNoTracking().ToListAsync(ct);
        }

        public async Task<IReadOnlyList<TablesAdditionalFieldsValues>> ListForRecordAsync(
            string tableName, string rowId,
            IEnumerable<int> fieldIds,
            CancellationToken ct = default)
        {
            var fieldIdList = fieldIds.ToList();
            return await _db.TablesAdditionalFieldsValues
                .AsNoTracking()
                .Where(v => v.TableName == tableName
                         && v.RowId == rowId
                         && fieldIdList.Contains(v.TableAdditionalFieldId))
                .Include(v => v.DictionaryElement)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<TablesAdditionalFieldsValues>> ListForRecordsAsync(
            string tableName, IEnumerable<string> rowIds,
            IEnumerable<int> fieldIds,
            CancellationToken ct = default)
        {
            var rowIdList = rowIds.ToList();
            var fieldIdList = fieldIds.ToList();

            return await _db.TablesAdditionalFieldsValues
                .AsNoTracking()
                .Where(v => v.TableName == tableName
                         && rowIdList.Contains(v.RowId)
                         && fieldIdList.Contains(v.TableAdditionalFieldId))
                .Include(v => v.DictionaryElement)
                .ToListAsync(ct);
        }

        public async Task RemoveForFieldAsync(int fieldId, CancellationToken ct = default)
        {
            var values = await _db.TablesAdditionalFieldsValues
                .Where(v => v.TableAdditionalFieldId == fieldId)
                .ToListAsync(ct);

            if (values.Count > 0)
                _db.TablesAdditionalFieldsValues.RemoveRange(values);
        }

        public async Task RemoveAllForRecordAsync(
            string tableName, string rowId,
            CancellationToken ct = default)
        {
            var values = await _db.TablesAdditionalFieldsValues
                .Where(v => v.TableName == tableName && v.RowId == rowId)
                .ToListAsync(ct);

            if (values.Count > 0)
                _db.TablesAdditionalFieldsValues.RemoveRange(values);
        }

        public async Task RemoveForRecordAsync(
            string tableName, string rowId,
            IEnumerable<int> fieldIds,
            CancellationToken ct = default)
        {
            var fieldIdList = fieldIds.ToList();
            var values = await _db.TablesAdditionalFieldsValues
                .Where(v => v.TableName == tableName
                         && v.RowId == rowId
                         && fieldIdList.Contains(v.TableAdditionalFieldId))
                .ToListAsync(ct);

            if (values.Count > 0)
                _db.TablesAdditionalFieldsValues.RemoveRange(values);
        }

        public Task RemoveRangeAsync(
            IEnumerable<TablesAdditionalFieldsValues> items,
            CancellationToken ct = default)
        {
            _db.TablesAdditionalFieldsValues.RemoveRange(items);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(
            IEnumerable<TablesAdditionalFieldsValues> items,
            CancellationToken ct = default)
        {
            _db.TablesAdditionalFieldsValues.UpdateRange(items);
            return Task.CompletedTask;
        }

        public Task Remove(TablesAdditionalFieldsValues item)
        {
            _db.TablesAdditionalFieldsValues.Remove(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TablesAdditionalFieldsValues item, CancellationToken ct = default)
        {
            _db.TablesAdditionalFieldsValues.Update(item);
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