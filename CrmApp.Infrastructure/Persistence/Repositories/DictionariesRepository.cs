using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using CrmApp.Infrastructure.Persistence.Errors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrmApp.Domain.DTO;

namespace CrmApp.Infrastructure.Persistence.Repositories
{
    public sealed class DictionariesRepository(AppDbContext db) : IDictionariesRepository
    {
        private readonly AppDbContext _db = db;

        public Task AddAsync(Dictionaries item, CancellationToken ct = default) =>
            _db.Dictionaries.AddAsync(item, ct).AsTask();

        public Task<Dictionaries?> GetAsync(int id, CancellationToken ct = default) =>
            _db.Dictionaries.FirstOrDefaultAsync(n => n.Id == id, ct);

        public Task<Dictionaries?> GetReadOnlyAsync(int id, CancellationToken ct = default) =>
            _db.Dictionaries.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id, ct);

        public async Task<Dictionaries?> GetByTypeAsync(EDictionaryType type, CancellationToken ct)
        {
            var dictionary = await _db.Dictionaries.AsNoTracking()
                .Where(x => x.DictionaryType == type && x.IsActive)
                .OrderByDescending(x => x.IsActive)
                .FirstOrDefaultAsync(ct);

            if (dictionary != null)
            {
                var elements = await GetDictionariesElements(dictionary.Id, ct);
                dictionary.DictionariesElements = elements.Select(elem => new DictionariesElementsDTO()
                {
                    Id = elem.Id,
                    DictionaryId = elem.DictionaryId,
                    Key = elem.Key,
                    Value = elem.Value,
                    CreatedAt = elem.CreatedAt,
                    CreatedBy = elem.CreatedBy,
                    IsCustom = elem.IsCustom,
                    IsActive = elem.IsActive,
                    IsDefault = elem.IsDefault,
                    ParentId = elem.ParentId,
                    AlternativeValuesForMapping = elem.AlternativeValuesForMapping,
                    Icon = elem.Icon,
                    IconColor = elem.IconColor,
                    OrdinalNumber = elem.OrdinalNumber
                }).OrderBy(x => x.OrdinalNumber).ToList();

            }
            return dictionary;
        }

        public async Task<List<DictionariesElements>> GetDictionariesElements(int dictionaryId, CancellationToken ct = default)
        {
            return await _db.DictionariesElements.AsNoTracking()
                    .Where(x => x.DictionaryId == dictionaryId)
                    .GroupBy(x => x.Key)
                    .Select(x => new DictionariesElements
                    {
                        Key = x.Key,
                        Value = x.OrderByDescending(x => x.IsActive).First().Value,
                        IsDefault = x.OrderByDescending(x => x.IsActive).First().IsDefault,
                        IsCustom = x.OrderByDescending(x => x.IsActive).First().IsCustom,
                        IsActive = x.OrderByDescending(x => x.IsActive).First().IsActive,
                        CreatedAt = x.OrderByDescending(x => x.IsActive).First().CreatedAt,
                        CreatedBy = x.OrderByDescending(x => x.IsActive).First().CreatedBy,
                        ParentId = x.OrderByDescending(x => x.IsActive).First().ParentId,
                        AlternativeValuesForMapping = x.OrderByDescending(x => x.IsActive).First().AlternativeValuesForMapping,
                        Icon = x.OrderByDescending(x => x.IsActive).First().Icon,
                        IconColor = x.OrderByDescending(x => x.IsActive).First().IconColor,
                        DictionaryId = dictionaryId,
                        OrdinalNumber = x.OrderByDescending(x => x.IsActive).First().OrdinalNumber
                    })
                    .OrderBy(x => x.OrdinalNumber)
                    .ToListAsync(ct);
        }

        public async Task<List<DictionariesElements>> GetDictionariesElements(List<int> dictionariesIds, CancellationToken ct = default)
        {
            return await _db.DictionariesElements.AsNoTracking()
                    .Where(x => dictionariesIds.Contains(x.DictionaryId))
                    .GroupBy(x => new { x.DictionaryId, x.Key })
                    .Select(x => new DictionariesElements
                    {
                        Key = x.Key.Key,
                        Value = x.OrderByDescending(x => x.IsActive).First().Value,
                        IsDefault = x.OrderByDescending(x => x.IsActive).First().IsDefault,
                        IsCustom = x.OrderByDescending(x => x.IsActive).First().IsCustom,
                        IsActive = x.OrderByDescending(x => x.IsActive).First().IsActive,
                        CreatedAt = x.OrderByDescending(x => x.IsActive).First().CreatedAt,
                        CreatedBy = x.OrderByDescending(x => x.IsActive).First().CreatedBy,
                        ParentId = x.OrderByDescending(x => x.IsActive).First().ParentId,
                        AlternativeValuesForMapping = x.OrderByDescending(x => x.IsActive).First().AlternativeValuesForMapping,
                        Icon = x.OrderByDescending(x => x.IsActive).First().Icon,
                        IconColor = x.OrderByDescending(x => x.IsActive).First().IconColor,
                        OrdinalNumber = x.OrderByDescending(x => x.IsActive).First().OrdinalNumber,
                        DictionaryId = x.Key.DictionaryId
                    })
                    .OrderBy(x => x.OrdinalNumber)
                    .ToListAsync(ct);
        }

        public Task<string?> GetElementKeyById(int id, CancellationToken ct = default) =>
            _db.DictionariesElements.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.Key).FirstOrDefaultAsync(ct);

        public async Task<IReadOnlyList<Dictionaries>> ListAllAsync(CancellationToken ct = default)
        {
            return await _db.Dictionaries
                .AsNoTracking()
                .Include(x => x.CreatedByUser)
                .OrderBy(n => n.Name)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Dictionaries>> ListSystemDictionariesAsync(CancellationToken ct = default)
        {
            return await _db.DictionariesElements
                .AsNoTracking().Include(x => x.Dictionary)
                .Where(x => x.Dictionary != null && x.Dictionary.DictionaryType != EDictionaryType.Custom)
                .GroupBy(x => x.Dictionary!)
                .Select(x => new Dictionaries
                {
                    Id = x.Key.Id,
                    Name = x.Key.Name,
                    DictionaryType = x.Key.DictionaryType,
                    Description = x.Key.Description,
                    CreatedAt = x.Key.CreatedAt,
                    IsActive = x.Key.IsActive,
                    IsCustom = x.Key.IsCustom,
                    DictionariesElements = x.Select(elem => new DictionariesElementsDTO()
                    {
                        Id = elem.Id,
                        DictionaryId = elem.DictionaryId,
                        Key = elem.Key,
                        Value = elem.Value,
                        CreatedAt = elem.CreatedAt,
                        CreatedBy = elem.CreatedBy,
                        IsCustom = elem.IsCustom,
                        IsActive = elem.IsActive,
                        IsDefault = elem.IsDefault,
                        ParentId = elem.ParentId,
                        AlternativeValuesForMapping = elem.AlternativeValuesForMapping,
                        Icon = elem.Icon,
                        IconColor = elem.IconColor,
                        OrdinalNumber = elem.OrdinalNumber
                    }).OrderBy(e => e.OrdinalNumber).ToList(),
                })
                .ToListAsync(ct);
        }

        public Task Remove(Dictionaries item)
        {
            _db.Dictionaries.Remove(item);
            return Task.CompletedTask;
        }

        public Task RemoveDictionaryElements(int dictionaryId)
        {
            var dictElems = _db.DictionariesElements
                .Where(x => x.DictionaryId == dictionaryId);
            if (dictElems.Any()) _db.DictionariesElements.RemoveRange(dictElems);
            return Task.CompletedTask;
        }

        public async Task<bool> CheckDictionaryElementsToRemove(int dictionaryId, CancellationToken ct)
        {
            var elementsIds = await _db.DictionariesElements
                .Where(x => x.DictionaryId == dictionaryId)
                .Select(x => x.Id).ToListAsync(ct);

            var isExistsInTablesAdditionalFieldsValues = await _db.TablesAdditionalFieldsValues
                .Where(x => x.DictionaryElementId != null && elementsIds.Contains((int)x.DictionaryElementId))
                .AnyAsync(ct);
            var isExistsAsUnit = await _db.CatalogItems
                .Where(x => elementsIds.Contains(x.UnitId))
                .AnyAsync(ct);
            var isExistsAsBillingUnit = await _db.CatalogItems
                .Where(x => elementsIds.Contains(x.BillingUnitId))
                .AnyAsync(ct);
            var isExistsAsCategory = await _db.CatalogItems
                .Where(x => x.CategoryId != null && elementsIds.Contains((int)x.CategoryId))
                .AnyAsync(ct);
            return !isExistsInTablesAdditionalFieldsValues && !isExistsAsCategory && !isExistsAsUnit && !isExistsAsBillingUnit;
        }

        public async Task<bool> CheckDictionaryToRemove(int dictionaryId, CancellationToken ct)
        {
            var elementsIds = await _db.DictionariesElements
                .Where(x => x.DictionaryId == dictionaryId)
                .Select(x => x.Id).ToListAsync(ct);

            var isExistsInTablesAdditionalFields = await _db.TablesAdditionalFields.AsNoTracking()
                .Where(x => x.DictionaryId == dictionaryId)
                .AnyAsync(ct);
            var canRemoveElements = await CheckDictionaryElementsToRemove(dictionaryId, ct);
            return !isExistsInTablesAdditionalFields && canRemoveElements;
        }

        public async Task<bool> CheckDictionaryElementToRemove(int dictionaryElementId, CancellationToken ct)
        {
            var isExistsInTablesAdditionalFieldsValues = await _db.TablesAdditionalFieldsValues
                .Where(x => x.DictionaryElementId == dictionaryElementId)
                .AnyAsync(ct);
            var isExistsAsUnit = await _db.CatalogItems
                .Where(x => x.UnitId == dictionaryElementId)
                .AnyAsync(ct);
            var isExistsAsBillingUnit = await _db.CatalogItems
                .Where(x => x.BillingUnitId == dictionaryElementId)
                .AnyAsync(ct);
            var isExistsAsCategory = await _db.CatalogItems
                .Where(x => x.CategoryId == dictionaryElementId)
                .AnyAsync(ct);
            var isExistsAsParent = await _db.DictionariesElements
                .Where(x => x.ParentId == dictionaryElementId)
                .AnyAsync(ct);
            return !isExistsInTablesAdditionalFieldsValues && !isExistsAsCategory && !isExistsAsUnit && !isExistsAsBillingUnit && !isExistsAsParent;
        }

        public Task<bool> CheckIfExistsByType(EDictionaryType type, int? id, CancellationToken ct) => _db.Dictionaries
                .Where(x => x.DictionaryType == type && x.IsActive && (id == null || x.Id != id))
                .AnyAsync(ct);

        public Task<bool> CheckIfExistsByNameAndType(string name, EDictionaryType type, int? id, CancellationToken ct) => _db.Dictionaries
                .Where(x => x.Name == name && x.DictionaryType == type &&
                            x.IsActive && (id == null || x.Id != id))
                .AnyAsync(ct);

        public Task UpdateAsync(Dictionaries item, CancellationToken ct = default)
        {
            _db.Dictionaries.Update(item);
            return Task.CompletedTask;
        }

        public async Task UpdateDictionariesElements(Dictionaries dictionary, List<DictionariesElements> elements, CancellationToken ct = default)
        {
            if (elements == null) elements = [];
            var current = await _db.DictionariesElements
                .Where(x => x.DictionaryId == dictionary.Id)
                .ToListAsync(ct);
            var currentKeys = current.Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var allKeys = elements.Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var newKeys = allKeys.Except(currentKeys);

            var toAdd = elements.Where(x => newKeys.Contains(x.Key));
            if (toAdd.Any())
            {
                foreach (var item in toAdd)
                {
                    item.Id = 0;
                    item.Dictionary = dictionary;
                    item.DictionaryId = dictionary.Id;
                    item.CreatedAt = DateTime.Now;
                }
                _db.DictionariesElements.AddRange(toAdd);
            }

            var toUpdate = current.Where(x => allKeys.Contains(x.Key));
            if (toUpdate.Any())
            {
                foreach (var item in toUpdate)
                {
                    var elemDTO = elements.First(x => x.Key.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                    item.Value = elemDTO.Value;
                    item.IsDefault = elemDTO.IsDefault;
                    item.IsActive = elemDTO.IsActive;
                    item.ParentId = elemDTO.ParentId;
                    item.OrdinalNumber = elemDTO.OrdinalNumber;
                    item.AlternativeValuesForMapping = elemDTO.AlternativeValuesForMapping;
                    item.Icon = elemDTO.Icon;
                    item.IconColor = elemDTO.IconColor;
                }
                _db.DictionariesElements.UpdateRange(toUpdate);
            }

            var toRemove = current.Where(x => !allKeys.Contains(x.Key));
            if (toRemove.Any()) _db.DictionariesElements.RemoveRange(toRemove);
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
