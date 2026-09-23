using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Services
{
    public interface IDictionariesService
    {
        Task<Dictionaries?> GetByType(EDictionaryType type, CancellationToken ct);
        Task<ResultDTO<Dictionaries>> GetById(int id, CancellationToken ct);
        Task<IReadOnlyList<Dictionaries>> GetAll(CancellationToken ct);
        Task<IReadOnlyList<Dictionaries>> GetSystemDictionaries(CancellationToken ct);
        Task<List<DictionariesElementsDTO>> GetDictionaryElements(int dictionaryId, CancellationToken ct);
        Task<ResultDTO<Dictionaries>> Add(Dictionaries dictionary, CancellationToken ct);
        Task<ResultDTO<Dictionaries>> Update(Dictionaries dictionary, CancellationToken ct);
        Task<ResultDTO<Dictionaries>> ActivateDeactivate(Dictionaries dictionary, bool activate, CancellationToken ct);
        Task<ResultDTO<object>> Delete(int id, CancellationToken ct);
    }
}
