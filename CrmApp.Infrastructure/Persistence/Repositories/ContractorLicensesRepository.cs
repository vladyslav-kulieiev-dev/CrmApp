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
    public sealed class ContractorLicensesRepository(AppDbContext db) : IContractorLicensesRepository
    {
        private readonly AppDbContext _db = db;

        public Task<ContractorLicenses?> GetAsync(int id, CancellationToken ct) =>
            _db.ContractorLicenses.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<ContractorLicenses?> GetReadOnlyAsync(int id, CancellationToken ct) =>
            _db.ContractorLicenses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<IReadOnlyList<ContractorLicenses>> ListAllAsync(CancellationToken ct) =>
            await _db.ContractorLicenses.AsNoTracking()
                .Include(x => x.CatalogItem)
                .OrderByDescending(x => x.ValidFrom).ThenByDescending(x => x.ExpiresAt)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<ContractorLicenses>> ListByContractorId(int contractorId, CancellationToken ct) =>
            await _db.ContractorLicenses.AsNoTracking()
                .Where(x => x.ContractorId == contractorId)
                .Include(x => x.CatalogItem)
                .OrderByDescending(x => x.ValidFrom).ThenByDescending(x => x.ExpiresAt)
                .ToListAsync(ct);

        public Task AddAsync(ContractorLicenses item, CancellationToken ct) =>
            _db.ContractorLicenses.AddAsync(item, ct).AsTask();

        public Task UpdateAsync(ContractorLicenses item, CancellationToken ct)
        {
            _db.ContractorLicenses.Update(item);
            return Task.CompletedTask;
        }

        public Task Remove(ContractorLicenses item)
        {
            _db.ContractorLicenses.Remove(item);
            return Task.CompletedTask;
        }

        public async Task<IDbContextTransaction> BeginTransaction(CancellationToken ct)
        {
            try { return await _db.Database.BeginTransactionAsync(ct); }
            catch (SqlException ex) { throw DbExceptionTranslator.Translate(ex); }
            catch (DbException ex) { throw DbExceptionTranslator.Translate(ex); }
        }

        public async Task CommitTransaction(IDbContextTransaction transaction, CancellationToken ct)
        {
            try { await transaction.CommitAsync(ct); }
            catch (SqlException ex) { throw DbExceptionTranslator.Translate(ex); }
            catch (DbException ex) { throw DbExceptionTranslator.Translate(ex); }
        }


        public async Task RollbackTransaction(IDbContextTransaction transaction, CancellationToken ct)
        {
            try { await transaction.RollbackAsync(ct); }
            catch (SqlException ex) { throw DbExceptionTranslator.Translate(ex); }
            catch (DbException ex) { throw DbExceptionTranslator.Translate(ex); }
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            try { return _db.SaveChangesAsync(ct); }
            catch (DbUpdateException ex) { throw DbExceptionTranslator.Translate(ex); }
        }
    }
}
