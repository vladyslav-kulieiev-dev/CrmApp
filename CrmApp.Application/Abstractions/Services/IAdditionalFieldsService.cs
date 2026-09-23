using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.AdditionalFields;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Services
{
    public interface IAdditionalFieldsService
    {
        Task<List<AdditionalFieldDTO>> GetFieldsForDocumentDefinitionAsync(
            int documentDefinitionId, int currentUserId, List<string> currentUserRoles,
            CancellationToken ct = default);
        Task<List<AdditionalFieldDTO>> GetFieldsForTableAsync(
            string tableName, int currentUserId, List<string> currentUserRoles,
            CancellationToken ct = default);
        Task<AdditionalFieldDTO> GetFieldByIdAsync(int id, CancellationToken ct = default);
        Task<ResultDTO<AdditionalFieldDTO>> CreateFieldAsync(AdditionalFieldCreateDTO dto, int currentUserId, CancellationToken ct = default);
        Task<ResultDTO<AdditionalFieldDTO>> UpdateFieldAsync(AdditionalFieldUpdateDTO dto, int currentUserId, CancellationToken ct = default);
        Task DeleteFieldAsync(int id, CancellationToken ct = default);
        Task ReorderFieldsAsync(AdditionalFieldReorderDTO dto, CancellationToken ct = default);
        Task<List<AdditionalFieldValueDTO>> GetValuesForRecordAsync(string tableName, string rowId, int currentUserId, List<string> currentUserRoles, CancellationToken ct = default);
        Task<Dictionary<string, List<AdditionalFieldValueDTO>>> GetValuesForRecordsAsync(string tableName, List<string> rowIds, int currentUserId, List<string> currentUserRoles, CancellationToken ct = default);
        Task SaveValuesForRecordAsync(AdditionalFieldValuesBatchSaveDTO dto, int currentUserId, CancellationToken ct = default);
        Task SaveFieldsForRecordAsync(AdditionalFieldsBatchSaveDTO dto, int currentUserId,
            CancellationToken ct = default);
        Task DeleteFieldsForRowAsync(string tableName, int? rowId, CancellationToken ct);
        Task<IReadOnlyList<TablesAdditionalFields>> GetMappedFieldsForDocumentDefinitionAsync(int definitionId, CancellationToken ct);
        Task DeleteValuesForRowAsync(string tableName, string rowId, CancellationToken ct);
        Task<IReadOnlyList<TablesAdditionalFieldsValues>> GetRawValuesForRecordAsync(
            string tableName, string rowId, List<int> fieldIds, CancellationToken ct);
        Task SaveDirectValuesForRowAsync(
            string tableName, string rowId,
            List<AdditionalFieldValueSaveDTO> values,
            int userId, CancellationToken ct);
    }
}
