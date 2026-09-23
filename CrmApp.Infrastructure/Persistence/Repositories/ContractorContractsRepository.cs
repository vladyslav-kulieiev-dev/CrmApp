using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.Entities;
using CrmApp.Infrastructure.Persistence.Errors;
using CrmApp.Infrastructure.Persistence.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Persistence.Repositories
{
    public sealed class ContractorContractsRepository(AppDbContext db) : IContractorContractsRepository
    {
        private readonly AppDbContext _db = db;

        public Task<ContractorContracts?> GetAsync(int id, CancellationToken ct) =>
            _db.ContractorContracts.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<ContractorContracts?> GetReadOnlyAsync(int id, CancellationToken ct) =>
            _db.ContractorContracts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<ContractorContracts?> GetReadOnlyAsyncWithHours(
            int id, CancellationToken ct)
        {
            var contract = await _db.ContractorContracts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (contract != null)
            { 
                var now = DateTime.UtcNow;
                var prevMonth = now.AddMonths(-1);

                var latestOverall = await LatestSnapshotPerContract([id], ct: ct);
                var latestCurrentMonth = await LatestSnapshotPerContract(
                    [id], now.Year, now.Month, ct);
                var latestPrevMonth = await LatestSnapshotPerContract(
                    [id], prevMonth.Year, prevMonth.Month, ct);

                contract.HoursRemaining = contract.HoursLimit;

                var overall = latestOverall.GetValueOrDefault(contract.Id);
                if (overall is not null)
                {
                    contract.HoursUsed = overall.HoursUsed;
                    contract.HoursRemaining = overall.HoursRemaining;
                }

                var current = latestCurrentMonth.GetValueOrDefault(contract.Id);
                contract.HoursUsedCurrentMonth = current?.HoursUsed ?? 0;
                contract.HoursRemainingCurrentMonth = contract.HoursLimit - contract.HoursUsedCurrentMonth;

                var prev = latestPrevMonth.GetValueOrDefault(contract.Id);
                if (prev is not null)
                {
                    var excess = prev.HoursUsed - contract.HoursLimit;
                    contract.HoursFromPrevMonth = excess > 0 ? -excess : 0;
                }
            }
            return contract;
        }
        public async Task<IReadOnlyList<ContractorContracts>> ListAllAsync(CancellationToken ct) =>
            await _db.ContractorContracts.AsNoTracking()
                .OrderBy(x => x.ValidFrom).ThenBy(x => x.ValidTo)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<ContractorContracts>> ListByContractorId(int contractorId, CancellationToken ct) =>
            await _db.ContractorContracts.AsNoTracking()
                .Where(x => x.ContractorId == contractorId)
                .OrderBy(x => x.ValidFrom).ThenBy(x => x.ValidTo)
                .ToListAsync(ct);

        public Task AddAsync(ContractorContracts item, CancellationToken ct) =>
            _db.ContractorContracts.AddAsync(item, ct).AsTask();

        public Task UpdateAsync(ContractorContracts item, CancellationToken ct)
        {
            _db.ContractorContracts.Update(item);
            return Task.CompletedTask;
        }

        public Task Remove(ContractorContracts item)
        {
            _db.ContractorContracts.Remove(item);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<ContractorContracts>> ListByContractorIdWithHours(
            int contractorId, CancellationToken ct)
        {
            var contracts = await _db.ContractorContracts.AsNoTracking()
                .Where(x => x.ContractorId == contractorId)
                .OrderBy(x => x.ValidFrom).ThenBy(x => x.ValidTo)
                .ToListAsync(ct);

            if (contracts.Count == 0) return contracts;

            var contractIds = contracts.Select(c => c.Id).ToList();

            var now = DateTime.UtcNow;
            var prevMonth = now.AddMonths(-1);

            var latestOverall = await LatestSnapshotPerContract(contractIds, ct: ct);
            var latestCurrentMonth = await LatestSnapshotPerContract(
                contractIds, now.Year, now.Month, ct);
            var latestPrevMonth = await LatestSnapshotPerContract(
                contractIds, prevMonth.Year, prevMonth.Month, ct);

            foreach (var c in contracts)
            {
                c.HoursRemaining = c.HoursLimit;

                var overall = latestOverall.GetValueOrDefault(c.Id);
                if (overall is not null)
                {
                    c.HoursUsed = overall.HoursUsed;
                    c.HoursRemaining = overall.HoursRemaining;
                }

                var current = latestCurrentMonth.GetValueOrDefault(c.Id);
                c.HoursUsedCurrentMonth = current?.HoursUsed ?? 0;
                c.HoursRemainingCurrentMonth = c.HoursLimit - c.HoursUsedCurrentMonth;

                var prev = latestPrevMonth.GetValueOrDefault(c.Id);
                if (prev is not null)
                {
                    var excess = prev.HoursUsed - c.HoursLimit;
                    c.HoursFromPrevMonth = excess > 0 ? -excess : 0;
                }
            }

            return contracts;
        }

        private async Task<Dictionary<int, ContractorHoursSnapshots>> LatestSnapshotPerContract(
            List<int> contractIds,
            int? year = null,
            int? month = null,
            CancellationToken ct = default)
        {
            var q = _db.ContractorHoursSnapshots.AsNoTracking()
                .Where(s => contractIds.Contains(s.ContractorContractId));

            if (year.HasValue && month.HasValue)
                q = q.Where(s => s.SnapshotDate.Year == year.Value
                               && s.SnapshotDate.Month == month.Value);

            var snapshots = await q
                .OrderByDescending(s => s.SnapshotDate)
                .ToListAsync(ct);

            return snapshots
                .GroupBy(s => s.ContractorContractId)
                .ToDictionary(g => g.Key, g => g.First());
        }

        public Task<int> CountByYearAndMonthAsync(int year, int month, CancellationToken ct) =>
            _db.ContractorContracts
                .CountAsync(c => c.CreatedAt.Year == year && c.CreatedAt.Month == month, ct);

        public async Task<IReadOnlyList<ContractorHoursSnapshots>> ListSnapshotsForContract(
            int contractId, CancellationToken ct)
        {
            return await _db.ContractorHoursSnapshots.AsNoTracking()
                .Include(x => x.CreatedByUser)
                .Where(x => x.ContractorContractId == contractId)
                .OrderByDescending(x => x.SnapshotDate)
                .ToListAsync(ct);
        }

        public async Task AddSnapshot(ContractorHoursSnapshots snapshot, CancellationToken ct)
        {
            await _db.ContractorHoursSnapshots.AddAsync(snapshot, ct);
        }

        public async Task<PagedResult<ContractorContracts>> ListPagedAsync(
             ContractorContractsRequest req, CancellationToken ct = default)
        {
            var (dataSql, countSql, parameters) = new ContractorContractsQueryBuilder(req).Build();

            var conn = _db.Database.GetDbConnection();
            var cmd = new CommandDefinition(dataSql, parameters, cancellationToken: ct);

            var contracts = (await conn.QueryAsync<ContractorContracts>(cmd)).AsList();
            var total = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(countSql, parameters, cancellationToken: ct));

            if (req.ContractorId.HasValue && contracts.Count > 0)
            {
                var ids = contracts.Select(c => c.Id).ToList();
                await AttachSnapshotsAsync(contracts, ids, ct);
            }

            return new PagedResult<ContractorContracts> { Items = contracts, TotalCount = total, Page = req.Page, PageSize = req.PageSize };
        }

        private async Task AttachSnapshotsAsync(
            List<ContractorContracts> contracts,
            List<int> contractIds,
            CancellationToken ct)
        {
            var allSnaps = await _db.ContractorHoursSnapshots.AsNoTracking()
                .Include(s => s.CreatedByUser)
                .Where(s => contractIds.Contains(s.ContractorContractId))
                .OrderByDescending(s => s.SnapshotDate)
                .ToListAsync(ct);

            var byContract = allSnaps
                .GroupBy(s => s.ContractorContractId)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<ContractorHoursSnapshots>)g.ToList());

            foreach (var c in contracts)
                c.Snapshots = byContract.GetValueOrDefault(c.Id, []);
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