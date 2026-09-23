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
    public sealed class SettingsRepository : ISettingsRepository
    {
        private readonly AppDbContext _db;
        public SettingsRepository(AppDbContext db) => _db = db;

        public Task<Settings?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _db.Settings.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<Settings?> GetByIdReadOnlyAsync(int id, CancellationToken ct = default) =>
            _db.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<Settings?> GetByKeyAsync(string key, CancellationToken ct = default) =>
            _db.Settings.FirstOrDefaultAsync(x => x.Key == key, ct);

        public Task<Settings?> GetByKeyReadOnlyAsync(string key, CancellationToken ct = default) =>
            _db.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, ct);

        public async Task<IReadOnlyList<Settings>> ListAllAsync(CancellationToken ct = default)
        {
            return await _db.Settings.AsNoTracking().OrderBy(x => x.Label).ToListAsync(ct);
        }

        public async Task<IReadOnlyList<SettingsValuesDictionary>> ListValuesDictionaryAsync(CancellationToken ct = default)
        {
            return await _db.SettingsValuesDictionary.AsNoTracking().ToListAsync(ct);
        }

        public async Task<IReadOnlyList<SettingsValuesDictionary>> ListSettingValuesAsync(int settingId, CancellationToken ct = default)
        {
            return await _db.SettingsValuesDictionary.AsNoTracking().Where(x => x.SettingId == settingId).OrderBy(x => x.Label).ToListAsync(ct);
        }

        public Task<string?> GetValueByIdAsync(int settingId, CancellationToken ct = default) =>
            _db.Settings.Where(x => x.Id == settingId).Select(x => x.Value).FirstOrDefaultAsync(ct);

        public Task<string?> GetValueByKeyAsync(string key, CancellationToken ct = default) =>
            _db.Settings.Where(x => x.Key == key).Select(x => x.Value).FirstOrDefaultAsync(ct);

        public async Task SetValueByIdAsync(int settingId, string? value, CancellationToken ct = default)
        {
            var setting = await GetByIdAsync(settingId, ct);
            setting.Value = value;
            _db.Settings.Update(setting);
        }

        public async Task SetValueByKeyAsync(string key, string? value, CancellationToken ct = default)
        {
            var setting = await GetByKeyAsync(key, ct);
            setting.Value = value;
            _db.Settings.Update(setting);
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
