using Microsoft.EntityFrameworkCore.Storage;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface IRepository <T> where T : class
    {
        Task<T?> GetAsync(int id, CancellationToken ct = default);
        Task<T?> GetReadOnlyAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<T>> ListAllAsync(CancellationToken ct = default);
        Task AddAsync(T item, CancellationToken ct = default);
        Task UpdateAsync(T item, CancellationToken ct = default);
        Task Remove(T item);
        Task SaveChangesAsync(CancellationToken ct = default);
        Task<IDbContextTransaction> BeginTransaction(CancellationToken ct = default);
        Task CommitTransaction(IDbContextTransaction transaction, CancellationToken ct = default);
        Task RollbackTransaction(IDbContextTransaction transaction, CancellationToken ct = default);
    }
}
