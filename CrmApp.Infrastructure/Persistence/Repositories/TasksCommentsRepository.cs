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
    public sealed class TasksCommentsRepository : ITasksCommentsRepository
    {
        private readonly AppDbContext _db;
        public TasksCommentsRepository(AppDbContext db) => _db = db;

        public Task AddAsync(TasksComments item, CancellationToken ct = default) =>
            _db.TasksComments.AddAsync(item, ct).AsTask();

        public Task<TasksComments?> GetAsync(int id, CancellationToken ct = default) =>
            _db.TasksComments.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<TasksComments?> GetReadOnlyAsync(int id, CancellationToken ct = default) =>
            _db.TasksComments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<IReadOnlyList<TasksComments>> ListAllAsync(CancellationToken ct = default)
        {
            return await _db.TasksComments
                .AsNoTracking()
                .OrderBy(x => x.PostedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<TasksComments>> ListByTaskIdReadOnlyAsync(int taskId, CancellationToken ct = default)
        {
            return await _db.TasksComments
                .AsNoTracking()
                .Where(x => x.TaskId == taskId)
                .OrderBy(x => x.PostedAt)
                .ToListAsync(ct);
        }
        public async Task<IReadOnlyList<TasksComments>> ListByTaskIdAsync(int taskId, CancellationToken ct = default)
        {
            return await _db.TasksComments
                .Where(x => x.TaskId == taskId)
                .OrderBy(x => x.PostedAt)
                .ToListAsync(ct);
        }

        public Task RemoveRange(List<TasksComments> items)
        {
            _db.TasksComments.RemoveRange(items);
            return Task.CompletedTask;
        }

        public Task Remove(TasksComments item)
        {
            _db.TasksComments.Remove(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TasksComments item, CancellationToken ct = default)
        {
            _db.TasksComments.Update(item);
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
