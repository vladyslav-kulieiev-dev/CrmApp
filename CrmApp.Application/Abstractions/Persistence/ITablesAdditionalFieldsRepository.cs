using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface ITablesAdditionalFieldsRepository : IRepository<TablesAdditionalFields>
    {
        Task AddRangeAsync(IEnumerable<TablesAdditionalFields> items, CancellationToken ct = default);
        Task RemoveRangeAsync(IEnumerable<TablesAdditionalFields> items, CancellationToken ct = default);
        Task UpdateRangeAsync(IEnumerable<TablesAdditionalFields> items, CancellationToken ct = default);
        Task<IReadOnlyList<TablesAdditionalFields>> ListByTableNameAsync(
            string tableName, int? rowId, CancellationToken ct = default);

        Task<TablesAdditionalFields?> GetWithPermissionsAsync(
            int id, CancellationToken ct = default);

        Task<IReadOnlyList<TablesAdditionalFields>> ListByTableNameWithPermissionsAsync(
            string tableName, int? rowId, CancellationToken ct = default);

        Task ReorderAsync(
            string tableName, Dictionary<int, int> orders, CancellationToken ct = default);

        Task<int> GetMaxSortOrderAsync(
            string tableName, CancellationToken ct = default);
        Task<IReadOnlyList<TablesAdditionalFields>> GetMappedFieldsForDocumentDefinitionAsync(
            int definitionId, CancellationToken ct);
        Task<IReadOnlyList<TablesAdditionalFields>> GetManyByIdsAsync(
            List<int> ids, CancellationToken ct = default);
    }
}
