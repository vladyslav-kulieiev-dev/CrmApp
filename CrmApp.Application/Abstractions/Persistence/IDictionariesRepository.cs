using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface IDictionariesRepository : IRepository<Dictionaries>
    {
        Task<IReadOnlyList<Dictionaries>> ListSystemDictionariesAsync(CancellationToken ct = default);
        Task<Dictionaries?> GetByTypeAsync(EDictionaryType type, CancellationToken ct);
        Task<List<DictionariesElements>> GetDictionariesElements(int dictionaryId, CancellationToken ct = default);
        Task<List<DictionariesElements>> GetDictionariesElements(List<int> dictionariesIds, CancellationToken ct = default);
        Task<string?> GetElementKeyById(int id, CancellationToken ct = default);
        Task UpdateDictionariesElements(Dictionaries dictionary, List<DictionariesElements> elements, CancellationToken ct = default);
        Task RemoveDictionaryElements(int dictionaryId);
        Task<bool> CheckDictionaryElementsToRemove(int dictionaryId, CancellationToken ct);
        Task<bool> CheckDictionaryElementToRemove(int dictionaryElementId, CancellationToken ct);
        Task<bool> CheckDictionaryToRemove(int dictionaryId, CancellationToken ct);
        Task<bool> CheckIfExistsByType(EDictionaryType type, int? id, CancellationToken ct);
        Task<bool> CheckIfExistsByNameAndType(string name, EDictionaryType type, int? id, CancellationToken ct);
    }
}
