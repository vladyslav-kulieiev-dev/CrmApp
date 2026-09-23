using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using CrmApp.Domain.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Services
{
    public sealed class DictionariesService(IDictionariesRepository repository) : IDictionariesService
    {
        private readonly IDictionariesRepository _repository = repository;

        public Task<IReadOnlyList<Dictionaries>> GetAll(CancellationToken ct) => _repository.ListAllAsync(ct);
        public Task<IReadOnlyList<Dictionaries>> GetSystemDictionaries(CancellationToken ct) => _repository.ListSystemDictionariesAsync(ct);

        public async Task<ResultDTO<Dictionaries>> GetById(int id, CancellationToken ct)
        {
            var dictionary = await _repository.GetReadOnlyAsync(id, ct);
            if (dictionary == null) return new ErrorResultDTO<Dictionaries>(["Nie znaleziono słownika o podanym identyfikatorze w bazie"]);

            var dictionariesElements = await _repository.GetDictionariesElements(id, ct);
            var dictionariesElementsDTO = dictionariesElements.Select(elem => new DictionariesElementsDTO()
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
                OrdinalNumber = elem.OrdinalNumber,
                AlternativeValuesForMapping = elem.AlternativeValuesForMapping,
                Icon = elem.Icon,
                IconColor = elem.IconColor
            }).ToList();
            dictionary.DictionariesElements = dictionariesElementsDTO.OrderBy(x => x.OrdinalNumber).ToList();
            return new SuccessResultDTO<Dictionaries>(dictionary, dictionary.Id.ToString());
        }

        public Task<Dictionaries?> GetByType(EDictionaryType type, CancellationToken ct) => _repository.GetByTypeAsync(type, ct);

        public async Task<List<DictionariesElementsDTO>> GetDictionaryElements(int dictionaryId, CancellationToken ct)
        {
            var dictionariesElements = await _repository.GetDictionariesElements(dictionaryId, ct);
            var dictionariesElementsDTO = dictionariesElements.Select(elem => new DictionariesElementsDTO()
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
                OrdinalNumber = elem.OrdinalNumber,
                AlternativeValuesForMapping = elem.AlternativeValuesForMapping,
                Icon = elem.Icon,
                IconColor = elem.IconColor
            }).OrderBy(x => x.OrdinalNumber).ToList();
            return dictionariesElementsDTO;
        }

        public async Task<ResultDTO<Dictionaries>> Add(Dictionaries dictionary, CancellationToken ct)
        {
            dictionary.IsCustom = dictionary.DictionaryType == EDictionaryType.Custom;
            if (dictionary.IsCustom && (await _repository.CheckIfExistsByNameAndType(dictionary.Name, dictionary.DictionaryType, null, ct)))
                return new ErrorResultDTO<Dictionaries>([$"Słownik o nazwie {dictionary.Name} już istnieje"]);
            else if (!dictionary.IsCustom && (await _repository.CheckIfExistsByType(dictionary.DictionaryType, null, ct)))
                return new ErrorResultDTO<Dictionaries>([$"Słownik o typie {dictionary.DictionaryType.GetDescription()} już istnieje"]);

            dictionary.CreatedAt = DateTime.Now;

            await _repository.AddAsync(dictionary, ct);
            var dictionariesElements = dictionary.DictionariesElements.Select(elem => new DictionariesElements()
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
                OrdinalNumber = elem.OrdinalNumber,
                AlternativeValuesForMapping = elem.AlternativeValuesForMapping,
                Icon = elem.Icon,
                IconColor = elem.IconColor
            }).ToList();
            await _repository.UpdateDictionariesElements(dictionary, dictionariesElements, ct);
            await _repository.SaveChangesAsync(ct);
            return new SuccessResultDTO<Dictionaries>(dictionary, dictionary.Id.ToString());
        }

        public async Task<ResultDTO<Dictionaries>> Update(Dictionaries dictionary, CancellationToken ct)
        {
            var dictionaryToUpdate = await _repository.GetReadOnlyAsync(dictionary.Id, ct);
            if (dictionaryToUpdate == null) return new ErrorResultDTO<Dictionaries>(["Nie znaleziono słownika o podanym identyfikatorze w bazie"]);

            if (dictionaryToUpdate.IsCustom && (await _repository.CheckIfExistsByNameAndType(dictionary.Name, dictionaryToUpdate.DictionaryType, dictionary.Id, ct)))
                return new ErrorResultDTO<Dictionaries>([$"Słownik o nazwie {dictionary.Name} już istnieje"]);
            else if (!dictionaryToUpdate.IsCustom && (await _repository.CheckIfExistsByType(dictionaryToUpdate.DictionaryType, dictionary.Id, ct)))
                return new ErrorResultDTO<Dictionaries>([$"Słownik o typie {dictionary.DictionaryType.GetDescription()} już istnieje"]);

            dictionaryToUpdate.Name = dictionary.Name;
            dictionaryToUpdate.Description = dictionary.Description;
            dictionaryToUpdate.IsActive = dictionary.IsActive;
            dictionaryToUpdate.IsCustom = dictionary.IsCustom;

            await _repository.UpdateAsync(dictionaryToUpdate, ct);
            var dictionariesElements = dictionary.DictionariesElements.Select(elem => new DictionariesElements()
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
                OrdinalNumber = elem.OrdinalNumber,
                AlternativeValuesForMapping = elem.AlternativeValuesForMapping,
                Icon = elem.Icon,
                IconColor = elem.IconColor
            }).ToList();
            await _repository.UpdateDictionariesElements(dictionaryToUpdate, dictionariesElements, ct);
            await _repository.SaveChangesAsync(ct);
            return new SuccessResultDTO<Dictionaries>(dictionaryToUpdate, dictionaryToUpdate.Id.ToString());
        }

        public async Task<ResultDTO<Dictionaries>> ActivateDeactivate(Dictionaries dictionary, bool activate, CancellationToken ct)
        {
            var dictionaryToUpdate = await _repository.GetReadOnlyAsync(dictionary.Id, ct);
            if (dictionaryToUpdate == null) return new ErrorResultDTO<Dictionaries>(["Nie znaleziono słownika o podanym identyfikatorze w bazie"]);

            dictionaryToUpdate.IsActive = activate;

            await _repository.UpdateAsync(dictionaryToUpdate, ct);
            await _repository.SaveChangesAsync(ct);
            return new SuccessResultDTO<Dictionaries>(dictionaryToUpdate, dictionaryToUpdate.Id.ToString());
        }

        public async Task<ResultDTO<object>> Delete(int id, CancellationToken ct)
        {
            var dictionary = await _repository.GetReadOnlyAsync(id, ct);
            if (dictionary == null) return new ErrorResultDTO<object>(["Nie znaleziono słownika o podanym identyfikatorze w bazie"]);

            var canRemoveDictionary = await _repository.CheckDictionaryToRemove(id, ct);
            if (!canRemoveDictionary) return new ErrorResultDTO<object>(["Słownik lub jego elementy są używane w aplikacji i nie mogą zostać usunięte"]);

            await _repository.RemoveDictionaryElements(id);
            await _repository.Remove(dictionary);
            return new SuccessResultDTO<object>(id.ToString(), ["Usunięto słownik " + dictionary.Name]);
        }
    }
}
