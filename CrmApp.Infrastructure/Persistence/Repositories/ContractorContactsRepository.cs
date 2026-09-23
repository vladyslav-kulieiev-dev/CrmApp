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
    public sealed class ContractorContactsRepository(AppDbContext db) : IContractorContactsRepository
    {
        private readonly AppDbContext _db = db;

        public Task<ContractorContacts?> GetAsync(int id, CancellationToken ct) =>
            _db.ContractorContacts.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<ContractorContacts?> GetByEmailAsync(string email, CancellationToken ct) =>
            _db.ContractorContacts.FirstOrDefaultAsync(x => !string.IsNullOrWhiteSpace(x.Email) && x.Email == email, ct);

        public Task<ContractorContacts?> GetReadOnlyAsync(int id, CancellationToken ct) =>
            _db.ContractorContacts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<IReadOnlyList<ContractorContacts>> ListAllAsync(CancellationToken ct) =>
            await _db.ContractorContacts.AsNoTracking()
                .OrderBy(x => x.Lastname).ThenBy(x => x.Firstname)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<ContractorContacts>> ListByContractorId(int contractorId, CancellationToken ct) =>
            await _db.ContractorContacts.AsNoTracking()
                .Where(x => x.ContractorId == contractorId)
                .OrderBy(x => x.Lastname).ThenBy(x => x.Firstname)
                .ToListAsync(ct);

        public Task AddAsync(ContractorContacts item, CancellationToken ct) =>
            _db.ContractorContacts.AddAsync(item, ct).AsTask();

        public Task UpdateAsync(ContractorContacts item, CancellationToken ct)
        {
            _db.ContractorContacts.Update(item);
            return Task.CompletedTask;
        }

        public Task Remove(ContractorContacts item)
        {
            _db.ContractorContacts.Remove(item);
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
