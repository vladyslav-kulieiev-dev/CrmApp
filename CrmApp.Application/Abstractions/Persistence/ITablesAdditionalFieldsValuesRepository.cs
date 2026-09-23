using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface ITablesAdditionalFieldsValuesRepository : IRepository<TablesAdditionalFieldsValues>
    {
        Task<IReadOnlyList<TablesAdditionalFieldsValues>> ListForRecordAsync(
            string tableName, string rowId,
            IEnumerable<int> fieldIds,
            CancellationToken ct = default);

        Task<IReadOnlyList<TablesAdditionalFieldsValues>> ListForRecordsAsync(
            string tableName, IEnumerable<string> rowIds,
            IEnumerable<int> fieldIds,
            CancellationToken ct = default);

        Task RemoveForFieldAsync(int fieldId, CancellationToken ct = default);
        Task RemoveAllForRecordAsync(
            string tableName, string rowId,
            CancellationToken ct = default);
        Task RemoveForRecordAsync(
            string tableName, string rowId,
            IEnumerable<int> fieldIds,
            CancellationToken ct = default);

        Task AddRangeAsync(
            IEnumerable<TablesAdditionalFieldsValues> items,
            CancellationToken ct = default); 
        
        Task RemoveRangeAsync(
            IEnumerable<TablesAdditionalFieldsValues> items,
            CancellationToken ct = default);

        Task UpdateRangeAsync(
            IEnumerable<TablesAdditionalFieldsValues> items,
            CancellationToken ct = default);
    }
}
