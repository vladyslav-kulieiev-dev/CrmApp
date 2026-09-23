using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Domain.DTO.AdditionalFields;
using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using CrmApp.Infrastructure.Persistence.Errors;
using CrmApp.Infrastructure.Persistence.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Persistence.Repositories
{
    public sealed class ContractorsRepository(AppDbContext db) : IContractorsRepository
    {
        private readonly AppDbContext _db = db;

        private static string NormalizeNip(string s) => s?.Trim().Replace(" ", "").Replace("-", "") ?? ""; 
        public Task AddAsync(Contractors item, CancellationToken ct = default) => 
            _db.Contractors.AddAsync(item, ct).AsTask();

        public Task<Contractors?> GetAsync(int id, CancellationToken ct = default) => 
            _db.Contractors.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<Contractors?> GetReadOnlyAsync(int id, CancellationToken ct = default) => 
            _db.Contractors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<List<string>> GetNipNumbersAsync(int contractorId, CancellationToken ct = default) =>
            await _db.ContractorsNips.Where(x => x.ContractorId == contractorId).Select(x => x.Nip).ToListAsync(ct);

        public async Task UpdateContractorsNipsAsync(Contractors contractor, string defNip, List<string> altNips, CancellationToken ct = default)
        {
            if (!altNips.Contains(defNip)) altNips.Add(defNip);
            var current = await _db.ContractorsNips
                .Where(x => x.ContractorId == contractor.Id)
                .Select(x => x.Nip)
                .ToListAsync(ct);

            var toUpdate = _db.ContractorsNips.Where(x => x.ContractorId == contractor.Id && x.Nip != defNip && x.IsPrimary).ToList();
            if (toUpdate.Count > 0)
            {
                toUpdate.ForEach(x => x.IsPrimary = false);
                _db.ContractorsNips.UpdateRange(toUpdate);
            }

            var toAdd = altNips.Except(current).Select(nip => new ContractorsNips
            {
                Contractor = contractor,
                Nip = nip,
                IsPrimary = nip == defNip
            });
            if (toAdd.Any()) _db.ContractorsNips.AddRange(toAdd);

            var toRemove = current.Except(altNips).ToList();
            if (toRemove.Count > 0)
            {
                var rows = _db.ContractorsNips.Where(x => x.ContractorId == contractor.Id && toRemove.Contains(x.Nip));
                _db.ContractorsNips.RemoveRange(rows);
            }
        }

        public async Task<List<int>> GetContractorCurrentEngagementTypes(int contractorId, CancellationToken ct = default)
        {
            return await _db.ContractorContracts.AsNoTracking()
                .Where(x => x.ContractorId == contractorId && x.EngagementType != EEngagementType.None &&
                            x.ValidFrom <= DateTime.Now && (x.ValidTo == null || x.ValidTo > DateTime.Now))
                .Select(x => (int)x.EngagementType).Distinct().ToListAsync(ct);
        }

        public bool IsExistsByCode(string code) => _db.Contractors.Any(x => x.Code == code);
        public bool IsExistsByCode(string code, int id) => _db.Contractors.Any(x => x.Code == code && x.Id != id);

        public Task<List<string>> ContractorsCodesByNip(IEnumerable<string> nips, int? excludeId = null, CancellationToken ct = default)
        {
            var set = nips.Select(NormalizeNip).ToArray();
            return _db.ContractorsNips
                .Where(x => (excludeId == null || x.ContractorId != excludeId) && set.Contains(x.Nip))
                .Include(x => x.Contractor)
                .Select(x => x.Contractor!.Code)
                .Distinct().ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Contractors>> ListAllAsync(CancellationToken ct = default) => 
            await _db.Contractors .AsNoTracking()
                .OrderBy(x => x.DisplayName)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Contractors>> ListByIdsAsync(List<int> contractorsIds, CancellationToken ct = default) => 
            await _db.Contractors .AsNoTracking()
                .Where(x => contractorsIds.Contains(x.Id))
                .OrderBy(x => x.DisplayName)
                .ToListAsync(ct);

        public Task Remove(Contractors item)
        {
            _db.Contractors.Remove(item);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            try { return _db.SaveChangesAsync(ct); }
            catch (DbUpdateException ex) { throw DbExceptionTranslator.Translate(ex); }
        }

        public Task UpdateAsync(Contractors item, CancellationToken ct = default)
        {
            _db.Contractors.Update(item);
            return Task.CompletedTask;
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

        public async Task<PagedResult<ContractorListItemDTO>> GetPagedAsync(
            ContractorsPagedRequest request, List<int> additionalFieldIds, CancellationToken ct = default)
        {
            var (dataSql, countSql, parameters) =
                new ContractorsQueryBuilder(request, additionalFieldIds).Build();

            await using var connection = new SqlConnection(
                _db.Database.GetConnectionString());

            await connection.OpenAsync(ct);
            var rawRows = (await connection.QueryAsync(dataSql, parameters)).ToList();
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            var items = rawRows.Select(row =>
            {
                var dict = (IDictionary<string, object?>)row;

                return new ContractorListItemDTO
                {
                    Id = (int)(dict["Id"] ?? "0"),
                    Name = (string)(dict["Name"] ?? ""),
                    Code = (string)(dict["Code"] ?? ""),
                    DisplayName = (string)(dict["DisplayName"] ?? ""),
                    Nip = dict["Nip"]?.ToString(),
                    IsXopero = dict["IsXopero"] as bool?,

                    ActiveEngagementTypes = dict["ActiveEngagementTypes"]?.ToString() is { } s
                        ? s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(int.Parse).Distinct().ToList()
                        : [],

                    HoursLimit = dict["HoursLimit"] as decimal?,
                    HoursUsed = dict["HoursUsed"] as decimal?,
                    HoursRemaining = dict["HoursLimit"] is decimal lim && dict["HoursUsed"] is decimal used
                        ? lim - used : null,

                    AdditionalFieldValues = additionalFieldIds
                        .Select(fieldId => new AdditionalFieldValueDTO
                        {
                            TableAdditionalFieldId = fieldId,
                            DictionaryElementValue = dict.TryGetValue(
                                $"af_{fieldId}", out var v) ? v?.ToString() : null
                        })
                        .Where(v => v.DictionaryElementValue != null)
                        .ToList()
                };
            }).ToList();

            return new PagedResult<ContractorListItemDTO> 
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
