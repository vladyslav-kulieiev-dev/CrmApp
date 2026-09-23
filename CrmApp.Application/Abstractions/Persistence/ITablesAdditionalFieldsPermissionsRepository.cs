using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface ITablesAdditionalFieldsPermissionsRepository
       : IRepository<TablesAdditionalFieldsPermissions>
    {
        Task<IReadOnlyList<TablesAdditionalFieldsPermissions>> ListByFieldIdAsync(
            int fieldId, CancellationToken ct = default);

        Task RemoveByFieldIdAsync(int fieldId, CancellationToken ct = default);

        Task AddRangeAsync(
            IEnumerable<TablesAdditionalFieldsPermissions> items,
            CancellationToken ct = default);
    }
}
