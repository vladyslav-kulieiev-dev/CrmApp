using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Domain.DTO.AdditionalFields;
using CrmApp.Domain.DTO.CatalogItemsDTO;
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
    public sealed class CatalogItemsRepository(AppDbContext db) : ICatalogItemsRepository
    {
        private readonly AppDbContext _db = db;

        public Task AddAsync(CatalogItems item, CancellationToken ct = default) =>
            _db.CatalogItems.AddAsync(item, ct).AsTask();

        public Task<CatalogItems?> GetAsync(int id, CancellationToken ct = default) =>
            _db.CatalogItems.FirstOrDefaultAsync(n => n.Id == id, ct);

        public Task<CatalogItems?> GetReadOnlyAsync(int id, CancellationToken ct = default) =>
            _db.CatalogItems.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id, ct);

        public bool CheckIfExistsByCode(string code, int? id = null) =>
            _db.CatalogItems.Any(n => n.Code == code && (id == null || n.Id != id.Value));

        public bool CheckIfItemIsUsed(int id) => _db.ContractorLicenses.Any(cc => cc.CatalogItemId == id);

        public async Task<IReadOnlyList<CatalogItems>> ListAllAsync(CancellationToken ct = default)
        {
            return await _db.CatalogItems
                .AsNoTracking()
                .Include(x => x.TypeObj).Include(x => x.Category)
                .OrderBy(n => n.Name)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CatalogItems>> ListActiveAsync(CancellationToken ct = default)
        {
            return await _db.CatalogItems
                .Where(x => x.IsActive)
                .AsNoTracking()
                .OrderBy(n => n.Name)
                .ToListAsync(ct);
        }

        public async Task<List<int>> GetSupportedSystems(int catalogItemId, CancellationToken ct = default)
        {
            return await _db.CatalogItemSupportedSystems
               .Where(x => x.CatalogItemId == catalogItemId)
               .Select(x => x.SupportedSystemId)
               .ToListAsync(ct);
        }

        public Task Remove(CatalogItems item)
        {
            _db.CatalogItems.Remove(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(CatalogItems item, CancellationToken ct = default)
        {
            _db.CatalogItems.Update(item);
            return Task.CompletedTask;
        }


        public async Task UpdateSupportedSystems(CatalogItems catalogItem, CancellationToken ct)
        {
            var current = await _db.CatalogItemSupportedSystems
                .Where(x => x.CatalogItemId == catalogItem.Id)
                .Select(x => x.SupportedSystemId)
                .ToListAsync(ct);
            var toAdd = catalogItem.SupportedSystems?.Except(current).Select(systemId => new CatalogItemSupportedSystems
            {
                CatalogItem = catalogItem,
                SupportedSystemId = systemId
            }) ?? [];
            if (toAdd.Any()) _db.CatalogItemSupportedSystems.AddRange(toAdd);

            var toRemove = current.Except(catalogItem.SupportedSystems ?? []).ToList();
            if (toRemove.Any())
            {
                var rows = _db.CatalogItemSupportedSystems.Where(x => x.CatalogItemId == catalogItem.Id && toRemove.Contains(x.SupportedSystemId));
                _db.CatalogItemSupportedSystems.RemoveRange(rows);
            }
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

        public async Task<PagedResult<CatalogItemsListItemDTO>> GetPagedAsync(
            CatalogItemPagedRequest request, List<int> additionalFieldIds, CancellationToken ct = default)
        {
            var (dataSql, countSql, parameters) =
                new CatalogItemsQueryBuilder(request, additionalFieldIds).Build();

            await using var connection = new SqlConnection(
                _db.Database.GetConnectionString());

            await connection.OpenAsync(ct);
            var rawRows = (await connection.QueryAsync(dataSql, parameters)).ToList();
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            var items = rawRows.Select(row =>
            {
                var dict = (IDictionary<string, object?>)row;

                return new CatalogItemsListItemDTO
                {
                    Id = (int)(dict["Id"] ?? "0"),
                    Name = (string)(dict["Name"] ?? ""),
                    Code = (string)(dict["Code"] ?? ""),
                    Description = (string)(dict["Description"] ?? ""),
                    CategoryId = (int)(dict["CategoryId"] ?? "0"),
                    Type = (int)(dict["Type"] ?? "0"),
                    UnitName = (string)(dict["UnitName"] ?? ""),
                    BillingUnitName = (string)(dict["BillingUnitName"] ?? ""),
                    Price = (decimal)(dict["Price"] ?? "0"),
                    IsActive = dict["IsActive"] is bool b && b,
                    VatRate = (decimal)(dict["VatRate"] ?? "0"),
                    Currency = (string)(dict["Currency"] ?? ""),
                    ParentId = dict["ParentId"] as int?,
                    TechnicalSupervisorId = dict["TechnicalSupervisorId"] as int?,
                    ImplementationManagerId = dict["ImplementationManagerId"] as int?,

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

            return new PagedResult<CatalogItemsListItemDTO>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
