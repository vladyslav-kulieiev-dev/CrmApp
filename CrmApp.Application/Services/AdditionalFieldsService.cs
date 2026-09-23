using Microsoft.AspNetCore.Http.HttpResults;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.Configuration;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.AdditionalFields;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CrmApp.Application.Services
{
    public sealed class AdditionalFieldsService(
        ITablesAdditionalFieldsRepository fieldsRepo,
        ITablesAdditionalFieldsValuesRepository valuesRepo,
        ITablesAdditionalFieldsPermissionsRepository permissionsRepo,
        IDictionariesRepository dictionariesRepo)
        : IAdditionalFieldsService
    {

        public async Task<List<AdditionalFieldDTO>> GetFieldsForTableAsync(
            string tableName, int currentUserId, List<string> currentUserRoles,
            CancellationToken ct = default)
        {
            var fields = await fieldsRepo
                .ListByTableNameWithPermissionsAsync(tableName, null, ct);

            return fields
                .Where(f => CanView(f, currentUserId, currentUserRoles))
                .Select(MapToDTO)
                .ToList();
        }

        public async Task<List<AdditionalFieldDTO>> GetFieldsForDocumentDefinitionAsync(
            int documentDefinitionId, int currentUserId, List<string> currentUserRoles, 
            CancellationToken ct = default)
        {
            var fields = await fieldsRepo.ListByTableNameWithPermissionsAsync(TablesNames.InternalDocumentsDefinitions, documentDefinitionId, ct);

            return fields
                .Where(f => CanView(f, currentUserId, currentUserRoles))
                .Select(MapToDTO)
                .ToList();
        }

        public async Task<AdditionalFieldDTO> GetFieldByIdAsync(
            int id, CancellationToken ct = default)
        {
            var field = await fieldsRepo.GetWithPermissionsAsync(id, ct)
                ?? throw new KeyNotFoundException($"Field {id} not found.");
            return MapToDTO(field);
        }

        public async Task<ResultDTO<AdditionalFieldDTO>> CreateFieldAsync(
            AdditionalFieldCreateDTO dto, int currentUserId, CancellationToken ct = default)
        {
            var maxOrder = await fieldsRepo.GetMaxSortOrderAsync(dto.TableName, ct);

            var field = new TablesAdditionalFields
            {
                TableName = dto.TableName,
                FieldName = dto.FieldName,
                FieldType = dto.FieldType,
                DictionaryId = dto.FieldType == EValueType.List ? dto.DictionaryId : null,
                IsMultiple = dto.IsMultiple,
                IsShowOnLists = dto.IsShowOnLists,
                IsRequired = dto.IsRequired,
                DefaultValue = dto.DefaultValue,
                SortOrder = maxOrder + 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId,
                RowId = dto.TableName == TablesNames.InternalDocumentsDefinitions ? dto.RowId : null,
                IsMappedFromInitObject = dto.IsMappedFromInitObject,
                InitObjectPropertyName = dto.InitObjectPropertyName
            };

            await fieldsRepo.AddAsync(field, ct);
            await fieldsRepo.SaveChangesAsync(ct);

            await SyncPermissionsAsync(field.Id, dto.Permissions, currentUserId, ct);
            var added = await GetFieldByIdAsync(field.Id, ct);
            return new SuccessResultDTO<AdditionalFieldDTO>(added, added.Id.ToString());
        }

        public async Task<ResultDTO<AdditionalFieldDTO>> UpdateFieldAsync(
            AdditionalFieldUpdateDTO dto, int currentUserId, CancellationToken ct = default)
        {
            var field = await fieldsRepo.GetAsync(dto.Id, ct)
                ?? throw new KeyNotFoundException($"Field {dto.Id} not found.");

            field.FieldName = dto.FieldName;
            field.FieldType = dto.FieldType;
            field.DictionaryId = dto.FieldType == EValueType.List ? dto.DictionaryId : null;
            field.IsMultiple = dto.IsMultiple;
            field.IsShowOnLists = dto.IsShowOnLists;
            field.IsRequired = dto.IsRequired;
            field.DefaultValue = dto.DefaultValue;
            field.IsMappedFromInitObject = dto.IsMappedFromInitObject;
            field.InitObjectPropertyName = dto.InitObjectPropertyName;

            await fieldsRepo.UpdateAsync(field, ct);
            await fieldsRepo.SaveChangesAsync(ct);

            await SyncPermissionsAsync(field.Id, dto.Permissions, currentUserId, ct);

            var updated = await GetFieldByIdAsync(field.Id, ct);
            return new SuccessResultDTO<AdditionalFieldDTO>(updated, updated.Id.ToString());
        }

        public async Task DeleteFieldAsync(int id, CancellationToken ct = default)
        {
            var field = await fieldsRepo.GetAsync(id, ct)
                ?? throw new KeyNotFoundException($"Field {id} not found.");

            await valuesRepo.RemoveForFieldAsync(id, ct);
            await permissionsRepo.RemoveByFieldIdAsync(id, ct);
            await fieldsRepo.Remove(field);
            await fieldsRepo.SaveChangesAsync(ct);
        }

        public async Task ReorderFieldsAsync(
            AdditionalFieldReorderDTO dto, CancellationToken ct = default)
        {
            await fieldsRepo.ReorderAsync(dto.TableName, dto.Orders, ct);
            await fieldsRepo.SaveChangesAsync(ct);
        }


        public async Task<List<AdditionalFieldValueDTO>> GetValuesForRecordAsync(
            string tableName, string rowId,
            int currentUserId, List<string> currentUserRoles,
            CancellationToken ct = default)
        {
            var fields = await GetFieldsForTableAsync(
                tableName, currentUserId, currentUserRoles, ct);
            var fieldIds = fields.Select(f => f.Id).ToList();
            var dictionariesIds = fields.Where(x => x.DictionaryId != null)
                .Select(x => (int)x.DictionaryId).ToList();
            if (dictionariesIds != null && dictionariesIds.Count > 0)
            {
                var dictionaries = await dictionariesRepo.GetDictionariesElements(dictionariesIds, ct);
                foreach (var field in fields)
                {
                    if (field.DictionaryId != null)
                    {
                        field.FieldOptions = dictionaries.Where(d => d.DictionaryId == field.DictionaryId)
                            .Select(de => new DictionaryElementOption
                            {
                                Id = de.Id,
                                Value = de.Value
                            }).ToList();
                    }
                }
            }

            var values = await valuesRepo.ListForRecordAsync(tableName, rowId, fieldIds, ct);
            return BuildValueDTOs(fields, values.ToList(), currentUserId, currentUserRoles);
        }

        public async Task<Dictionary<string, List<AdditionalFieldValueDTO>>> GetValuesForRecordsAsync(
            string tableName, List<string> rowIds,
            int currentUserId, List<string> currentUserRoles,
            CancellationToken ct = default)
        {
            var fields = await GetFieldsForTableAsync(
                tableName, currentUserId, currentUserRoles, ct);
            var fieldIds = fields.Select(f => f.Id).ToList();

            var allValues = await valuesRepo.ListForRecordsAsync(tableName, rowIds, fieldIds, ct);

            return rowIds.ToDictionary(
                rowId => rowId,
                rowId => BuildValueDTOs(
                    fields,
                    allValues.Where(v => v.RowId == rowId).ToList(), 
                    currentUserId, currentUserRoles));
        }

        public async Task SaveValuesForRecordAsync(
            AdditionalFieldValuesBatchSaveDTO dto, int currentUserId,
            CancellationToken ct = default)
        {
            var fieldIds = dto.Values.Select(v => v.TableAdditionalFieldId).ToList();

            var fieldDefs = await fieldsRepo.ListByTableNameAsync(dto.TableName, null, ct);
            var relevantDefs = fieldDefs
                .Where(f => fieldIds.Contains(f.Id))
                .ToList();

            var existing = await valuesRepo.ListForRecordAsync(
                dto.TableName, dto.RowId, fieldIds, ct);

            var toAdd = new List<TablesAdditionalFieldsValues>();
            var toUpdate = new List<TablesAdditionalFieldsValues>();
            var toRemove = new List<TablesAdditionalFieldsValues>();

            foreach (var valueDto in dto.Values)
            {
                var fieldDef = relevantDefs
                    .FirstOrDefault(f => f.Id == valueDto.TableAdditionalFieldId);
                if (fieldDef is null) continue;

                var existingForField = existing
                    .Where(v => v.TableAdditionalFieldId == fieldDef.Id)
                    .ToList();

                if (fieldDef.FieldType == EValueType.List && fieldDef.IsMultiple)
                {
                    var incomingIds = valueDto.DictionaryElementIds.ToHashSet();
                    var existingIds = existingForField
                        .Where(v => v.DictionaryElementId.HasValue)
                        .Select(v => v.DictionaryElementId!.Value)
                        .ToHashSet();

                    foreach (var elemId in incomingIds.Except(existingIds))
                    {
                        toAdd.Add(new TablesAdditionalFieldsValues
                        {
                            TableAdditionalFieldId = fieldDef.Id,
                            TableName = dto.TableName,
                            RowId = dto.RowId,
                            FieldName = fieldDef.FieldName,
                            DictionaryElementId = elemId,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = currentUserId
                        });
                    }

                    toRemove.AddRange(existingForField
                        .Where(v => v.DictionaryElementId.HasValue
                                 && !incomingIds.Contains(v.DictionaryElementId!.Value)));
                }
                else if (fieldDef.FieldType == EValueType.List)
                {
                    var incomingElemId = valueDto.DictionaryElementIds.FirstOrDefault();
                    var existingRow = existingForField.FirstOrDefault();

                    if (existingRow is null && incomingElemId != 0)
                    {
                        toAdd.Add(new TablesAdditionalFieldsValues
                        {
                            TableAdditionalFieldId = fieldDef.Id,
                            TableName = dto.TableName,
                            RowId = dto.RowId,
                            FieldName = fieldDef.FieldName,
                            DictionaryElementId = incomingElemId,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = currentUserId
                        });
                    }
                    else if (existingRow is not null && incomingElemId == 0)
                    {
                        toRemove.Add(existingRow);
                    }
                    else if (existingRow is not null
                          && existingRow.DictionaryElementId != incomingElemId)
                    {
                        existingRow.DictionaryElementId = incomingElemId;
                        toUpdate.Add(existingRow);
                    }
                }
                else
                {
                    var existingRow = existingForField.FirstOrDefault();
                    var incoming = valueDto.FieldValue;

                    if (existingRow is null && !string.IsNullOrEmpty(incoming))
                    {
                        toAdd.Add(new TablesAdditionalFieldsValues
                        {
                            TableAdditionalFieldId = fieldDef.Id,
                            TableName = dto.TableName,
                            RowId = dto.RowId,
                            FieldName = fieldDef.FieldName,
                            FieldValue = incoming,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = currentUserId
                        });
                    }
                    else if (existingRow is not null && string.IsNullOrEmpty(incoming))
                    {
                        toRemove.Add(existingRow);
                    }
                    else if (existingRow is not null
                          && existingRow.FieldValue != incoming)
                    {
                        existingRow.FieldValue = incoming;
                        toUpdate.Add(existingRow);
                    }
                }
            }

            var incomingFieldIds = dto.Values
                .Select(v => v.TableAdditionalFieldId).ToHashSet();
            toRemove.AddRange(existing
                .Where(v => !incomingFieldIds.Contains(v.TableAdditionalFieldId)
                         && !toRemove.Contains(v)));

            if (toRemove.Count > 0)
                await valuesRepo.RemoveRangeAsync(toRemove, ct);

            if (toUpdate.Count > 0)
                await valuesRepo.UpdateRangeAsync(toUpdate, ct);

            if (toAdd.Count > 0)
                await valuesRepo.AddRangeAsync(toAdd, ct);

            await valuesRepo.SaveChangesAsync(ct);
        }

        public async Task SaveFieldsForRecordAsync(
            AdditionalFieldsBatchSaveDTO dto, int currentUserId,
            CancellationToken ct = default)
        {
            var existingInDb = await fieldsRepo.ListByTableNameAsync(dto.TableName, dto.RowId, ct);

            var toAdd = new List<TablesAdditionalFields>();
            var toUpdate = new List<TablesAdditionalFields>();

            foreach (var valueDto in dto.Fields)
            {
                var fieldDef = existingInDb.FirstOrDefault(f => f.Id == valueDto.Id);
                if (fieldDef is null)
                {
                    toAdd.Add(new TablesAdditionalFields
                    {
                        Id = 0,
                        TableName = dto.TableName,
                        FieldName = valueDto.FieldName,
                        FieldType = valueDto.FieldType,
                        DictionaryId = valueDto.DictionaryId,
                        IsMultiple = valueDto.IsMultiple,
                        IsShowOnLists = valueDto.IsShowOnLists,
                        CreatedAt = DateTime.Now,
                        CreatedBy = currentUserId,
                        SortOrder = valueDto.SortOrder,
                        PresentInNewRow = valueDto.PresentInNewRow,
                        RowSpan = valueDto.RowSpan,
                        ColSpan = valueDto.ColSpan,
                        IsRequired = valueDto.IsRequired,
                        DefaultValue = valueDto.DefaultValue,
                        RowId = dto.RowId,
                        IsMappedFromInitObject = valueDto.IsMappedFromInitObject,
                        InitObjectPropertyName = valueDto.InitObjectPropertyName
                    });
                }
                else 
                {
                    fieldDef.TableName = dto.TableName;
                    fieldDef.FieldName = valueDto.FieldName;
                    fieldDef.FieldType = valueDto.FieldType;
                    fieldDef.DictionaryId = valueDto.DictionaryId;
                    fieldDef.IsMultiple = valueDto.IsMultiple;
                    fieldDef.IsShowOnLists = valueDto.IsShowOnLists;
                    fieldDef.CreatedAt = DateTime.Now;
                    fieldDef.CreatedBy = currentUserId;
                    fieldDef.SortOrder = valueDto.SortOrder;
                    fieldDef.PresentInNewRow = valueDto.PresentInNewRow;
                    fieldDef.RowSpan = valueDto.RowSpan;
                    fieldDef.ColSpan = valueDto.ColSpan;
                    fieldDef.IsRequired = valueDto.IsRequired;
                    fieldDef.DefaultValue = valueDto.DefaultValue;
                    fieldDef.RowId = dto.RowId;
                    fieldDef.IsMappedFromInitObject = valueDto.IsMappedFromInitObject;
                    fieldDef.InitObjectPropertyName = valueDto.InitObjectPropertyName;
                    toUpdate.Add(fieldDef);
                }
            }

            var incomingFieldIds = dto.Fields.Select(v => v.Id).ToHashSet();
            var toRemove = existingInDb.Where(v => !incomingFieldIds.Contains(v.Id)).ToList();

            if (toRemove.Count > 0)
                await fieldsRepo.RemoveRangeAsync(toRemove, ct);

            if (toUpdate.Count > 0)
                await fieldsRepo.UpdateRangeAsync(toUpdate, ct);

            if (toAdd.Count > 0)
                await fieldsRepo.AddRangeAsync(toAdd, ct);

            await fieldsRepo.SaveChangesAsync(ct);
        }

        public async Task DeleteFieldsForRowAsync(string tableName, int? rowId, CancellationToken ct)
        {
            var existingInDb = await fieldsRepo.ListByTableNameAsync(tableName, rowId, ct);
            await fieldsRepo.RemoveRangeAsync(existingInDb, ct);
        }

        private static bool CanView(
            TablesAdditionalFields field, int userId, List<string> userRoles)
        {
            var perms = field.Permissions;
            if (perms is null || perms.Count == 0) return true;

            return perms.Any(p =>
                p.CanView && (
                    (p.RoleId != null && userRoles.Contains(p.RoleId)) ||
                    (p.UserId != null && p.UserId == userId)));
        }

        private static bool CanView(
            AdditionalFieldDTO field, int userId, List<string> userRoles)
        {
            var perms = field.Permissions;
            if (perms is null || perms.Count == 0) return true;

            return perms.Any(p =>
                p.CanView && (
                    (p.RoleId != null && userRoles.Contains(p.RoleId)) ||
                    (p.UserId != null && p.UserId == userId)));
        }
        private static bool CanEdit(
            AdditionalFieldDTO field, int userId, List<string> userRoles)
        {
            var perms = field.Permissions;
            if (perms is null || perms.Count == 0) return true;

            return perms.Any(p =>
                p.CanEdit && (
                    (p.RoleId != null && userRoles.Contains(p.RoleId)) ||
                    (p.UserId != null && p.UserId == userId)));
        }

        private static AdditionalFieldDTO MapToDTO(TablesAdditionalFields field) =>
            new()
            {
                Id = field.Id,
                TableName = field.TableName,
                FieldName = field.FieldName,
                FieldType = field.FieldType,
                DictionaryId = field.DictionaryId,
                DictionaryName = field.Dictionary?.Name,
                IsMultiple = field.IsMultiple,
                IsShowOnLists = field.IsShowOnLists,
                IsRequired = field.IsRequired,
                SortOrder = field.SortOrder,
                DefaultValue = field.DefaultValue,
                RowId = field.TableName == TablesNames.InternalDocumentsDefinitions ? field.RowId : null,
                PresentInNewRow = field.PresentInNewRow,
                ColSpan = field.ColSpan,
                RowSpan = field.RowSpan,
                IsMappedFromInitObject = field.IsMappedFromInitObject,
                InitObjectPropertyName = field.InitObjectPropertyName,
                Permissions = field.Permissions?.Select(p => new AdditionalFieldPermissionDTO
                {
                    Id = p.Id,
                    RoleId = p.RoleId,
                    UserId = p.UserId,
                    UserName = p.User is not null
                        ? $"{p.User.FirstName} {p.User.LastName}"
                        : null,
                    CanView = p.CanView,
                    CanEdit = p.CanEdit,
                    RoleName = p.RoleName
                }).ToList() ?? []
            };

        private static List<AdditionalFieldValueDTO> BuildValueDTOs(
            List<AdditionalFieldDTO> fields,
            List<TablesAdditionalFieldsValues> values,
            int currentUserId, List<string> currentUserRoles)
        {
            var result = new List<AdditionalFieldValueDTO>();

            foreach (var field in fields)
            {
                var fieldValues = values
                    .Where(v => v.TableAdditionalFieldId == field.Id)
                    .ToList();

                var dto = new AdditionalFieldValueDTO
                {
                    TableAdditionalFieldId = field.Id,
                    FieldName = field.FieldName,
                    FieldType = field.FieldType,
                    IsMultiple = field.IsMultiple,
                    IsRequired = field.IsRequired,
                    FieldOptions = field.FieldOptions,
                    CanView = CanView(field, currentUserId, currentUserRoles),
                    CanEdit = CanEdit(field, currentUserId, currentUserRoles),
                    FieldValue = field.DefaultValue,
                    ColSpan = field.ColSpan,
                    RowSpan = field.RowSpan,
                    IsMappedFromInitObject = field.IsMappedFromInitObject,
                    PresentInNewRow = field.PresentInNewRow,
                    DefaultValue = field.DefaultValue
                };

                if (field.FieldType == EValueType.List)
                {
                    dto.DictionaryElementIds = fieldValues
                        .Where(v => v.DictionaryElementId.HasValue)
                        .Select(v => v.DictionaryElementId!.Value)
                        .ToList();

                    dto.DictionaryElementId = dto.DictionaryElementIds.FirstOrDefault();

                    dto.DictionaryElementValue = string.Join(", ", fieldValues
                        .Where(v => v.DictionaryElement?.Value is not null)
                        .Select(v => v.DictionaryElement!.Value));
                }
                else
                {
                    var single = fieldValues.FirstOrDefault();
                    dto.Id = single?.Id ?? 0;
                    dto.FieldValue = single?.FieldValue ?? field.DefaultValue;
                    dto.FieldValueBool = field.FieldType == EValueType.Boolean && dto.FieldValue == "true";
                }

                result.Add(dto);
            }

            return result;
        }

        private async Task SyncPermissionsAsync(
            int fieldId,
            List<AdditionalFieldPermissionCreateDTO> permissions,
            int currentUserId,
            CancellationToken ct)
        {
            await permissionsRepo.RemoveByFieldIdAsync(fieldId, ct);

            if (permissions.Count > 0)
            {
                var toInsert = permissions.Select(p =>
                    new TablesAdditionalFieldsPermissions
                    {
                        TableAdditionalFieldId = fieldId,
                        RoleId = p.RoleId,
                        UserId = p.UserId,
                        CanView = p.CanView,
                        CanEdit = p.CanEdit,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = currentUserId
                    }).ToList();

                await permissionsRepo.AddRangeAsync(toInsert, ct);
            }

            await permissionsRepo.SaveChangesAsync(ct);
        }

        public Task<IReadOnlyList<TablesAdditionalFields>> GetMappedFieldsForDocumentDefinitionAsync(
            int definitionId, CancellationToken ct) => fieldsRepo.GetMappedFieldsForDocumentDefinitionAsync(definitionId, ct);

        public Task DeleteValuesForRowAsync(string tableName, string rowId, CancellationToken ct) => valuesRepo.RemoveAllForRecordAsync(tableName, rowId, ct);

        public async Task<IReadOnlyList<TablesAdditionalFieldsValues>> GetRawValuesForRecordAsync(
            string tableName, string rowId, List<int> fieldIds, CancellationToken ct)
        {
            if (fieldIds.Count == 0) return [];
            return await valuesRepo.ListForRecordAsync(tableName, rowId, fieldIds, ct);
        }

        public async Task SaveDirectValuesForRowAsync(
            string tableName, string rowId,
            List<AdditionalFieldValueSaveDTO> values,
            int userId, CancellationToken ct)
        {
            if (values.Count == 0) return;

            var fieldIds = values.Select(v => v.TableAdditionalFieldId).ToList();

            // Pobierz istniejące wartości dla tych pól
            var existing = await valuesRepo.ListForRecordAsync(tableName, rowId, fieldIds, ct);
            var existingMap = existing.ToDictionary(e => e.TableAdditionalFieldId);

            // Pobierz definicje pól żeby znać FieldName
            var fieldDefs = await fieldsRepo.GetManyByIdsAsync(fieldIds, ct);
            var fieldDefMap = fieldDefs.ToDictionary(f => f.Id);

            var toAdd = new List<TablesAdditionalFieldsValues>();
            var toUpdate = new List<TablesAdditionalFieldsValues>();

            foreach (var dto in values)
            {
                if (!fieldDefMap.TryGetValue(dto.TableAdditionalFieldId, out var fieldDef))
                    continue;

                if (existingMap.TryGetValue(dto.TableAdditionalFieldId, out var existingVal))
                {
                    // Update
                    existingVal.FieldValue = dto.FieldValue;
                    existingVal.DictionaryElementId = dto.DictionaryElementIds?.FirstOrDefault();
                    toUpdate.Add(existingVal);
                }
                else
                {
                    // Insert
                    toAdd.Add(new TablesAdditionalFieldsValues
                    {
                        TableAdditionalFieldId = dto.TableAdditionalFieldId,
                        TableName = tableName,
                        RowId = rowId,
                        FieldName = fieldDef.FieldName,
                        FieldValue = dto.FieldValue,
                        DictionaryElementId = dto.DictionaryElementIds?.FirstOrDefault(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId,
                    });
                }
            }

            if (toAdd.Count > 0) await valuesRepo.AddRangeAsync(toAdd, ct);
            if (toUpdate.Count > 0) await valuesRepo.UpdateRangeAsync(toUpdate, ct);
            await valuesRepo.SaveChangesAsync(ct);
        }
    }
}