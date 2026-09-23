using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.Configuration;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.AdditionalFields;
using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Services
{
    public sealed class ContractorsService : IContractorsService
    {
        private readonly IContractorsRepository _repo;
        private readonly IAdditionalFieldsService _additionalFieldsService;
        public ContractorsService(IContractorsRepository repo, IAdditionalFieldsService additionalFieldsService)
        {
            _repo = repo;
            _additionalFieldsService = additionalFieldsService;
        }

        public async Task<IReadOnlyList<Contractors>> GetAll(CancellationToken ct)
        {
            var contractors = await _repo.ListAllAsync(ct);

            return contractors;
        }

        public async Task<PagedResult<ContractorListItemDTO>> GetPagedAsync(ContractorsPagedRequest request, int userId, List<string> rolesIds, CancellationToken ct = default)
        {
            var fields = await _additionalFieldsService.GetFieldsForTableAsync(TablesNames.Contractors, userId, rolesIds, ct);
            var additionalFieldIds = fields.Select(f => f.Id).ToList();
            return await _repo.GetPagedAsync(request, additionalFieldIds, ct);
        }

        public async Task<List<string>> GetNipsById(int id, CancellationToken ct) => await _repo.GetNipNumbersAsync(id, ct);
        public async Task<ResultDTO<Contractors>> GetById(int id, CancellationToken ct)
        {
            var contractor = await _repo.GetReadOnlyAsync(id, ct);
            if (contractor == null)
                return new ErrorResultDTO<Contractors>(["Nie znaleziono kontrahenta w bazie"]);

            contractor.AlternativeNipNumbers = await _repo.GetNipNumbersAsync(id, ct);

            contractor.CurrentEngagementTypes = await _repo.GetContractorCurrentEngagementTypes(id, ct);
            if (contractor.CurrentEngagementTypes.Count == 0) contractor.CurrentEngagementTypes.Add((int)EEngagementType.None);
            return new SuccessResultDTO<Contractors>(id.ToString(), []) { Data = contractor };
        }

        public async Task<ResultDTO<Contractors>> GetByIdLightMode(int id, CancellationToken ct)
        {
            var contractor = await _repo.GetReadOnlyAsync(id, ct);
            if (contractor == null)
                return new ErrorResultDTO<Contractors>(["Nie znaleziono kontrahenta w bazie"]);

            contractor.AlternativeNipNumbers = await _repo.GetNipNumbersAsync(id, ct);
            return new SuccessResultDTO<Contractors>(id.ToString(), []) { Data = contractor };
        }

        public async Task<string> GetNameById(int id, CancellationToken ct)
        {
            var contractor = await _repo.GetAsync(id, ct);
            if (contractor != null)
                return contractor.Name;
            return string.Empty;
        }
        
        public async Task<ResultDTO<Contractors>> Add(Contractors item, int userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(item.Name)) return new ErrorResultDTO<Contractors>(["Podaj nazwę kontrahenta"]);
            if (string.IsNullOrWhiteSpace(item.Code)) return new ErrorResultDTO<Contractors>(["Podaj kod kontrahenta"]);
            if (string.IsNullOrWhiteSpace(item.Nip)) return new ErrorResultDTO<Contractors>(["Podaj NIP kontrahenta"]);

            List<string> nipsToCheck = [item.Nip];
            if (item.AlternativeNipNumbers != null && item.AlternativeNipNumbers.Count > 0) nipsToCheck.AddRange(item.AlternativeNipNumbers);
            
            if (_repo.IsExistsByCode(item.Code))
                return new ErrorResultDTO<Contractors>(["Kontrahent o takim kodzie został już dodany"]);

            if (string.IsNullOrWhiteSpace(item.DisplayName))
                item.DisplayName = item.Code;

            await _repo.AddAsync(item, ct);
            await _repo.UpdateContractorsNipsAsync(item, item.Nip, item.AlternativeNipNumbers ?? [], ct);
            await _repo.SaveChangesAsync(ct);

            if (item.AdditionalFieldValues?.Count > 0)
            {
                await _additionalFieldsService.SaveValuesForRecordAsync(GetAdditionalFieldValueSaveDTO(item), userId, ct);
            }

            return new SuccessResultDTO<Contractors>(item.Id.ToString(), []) { Data = item };
        }

        private AdditionalFieldValuesBatchSaveDTO GetAdditionalFieldValueSaveDTO(Contractors item)
        {
            return new AdditionalFieldValuesBatchSaveDTO
            {
                TableName = TablesNames.Contractors,
                RowId = item.Id.ToString(),
                Values = item.AdditionalFieldValues.Select(v =>
                    new AdditionalFieldValueSaveDTO
                    {
                        TableAdditionalFieldId = v.TableAdditionalFieldId,
                        FieldValue = v.FieldType != EValueType.Boolean ? v.FieldValue : (v.FieldValueBool == true ? "true" : "false"),
                        DictionaryElementIds = !v.IsMultiple && v.DictionaryElementId != null ?[(int)v.DictionaryElementId] : v.DictionaryElementIds
                    }).ToList()
            };
        }

        public async Task<ResultDTO<Contractors>> Update(Contractors item, int userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(item.Name)) return new ErrorResultDTO<Contractors>(["Podaj nazwę kontrahenta"]);
            if (string.IsNullOrWhiteSpace(item.Code)) return new ErrorResultDTO<Contractors>(["Podaj kod kontrahenta"]);
            if (string.IsNullOrWhiteSpace(item.Nip)) return new ErrorResultDTO<Contractors>(["Podaj NIP kontrahenta"]);

            List<string> nipsToCheck = [item.Nip];
            if (item.AlternativeNipNumbers != null && item.AlternativeNipNumbers.Count > 0) nipsToCheck.AddRange(item.AlternativeNipNumbers);

            if (_repo.IsExistsByCode(item.Code, item.Id))
                return new ErrorResultDTO<Contractors>(["Kontrahent o takim kodzie został już dodany"]);

            if (string.IsNullOrWhiteSpace(item.DisplayName))
                item.DisplayName = item.Code;

            await _repo.UpdateAsync(item, ct);
            await _repo.UpdateContractorsNipsAsync(item, item.Nip, item.AlternativeNipNumbers ?? [], ct);
            await _repo.SaveChangesAsync(ct);

            if (item.AdditionalFieldValues?.Count > 0)
            {
                await _additionalFieldsService.SaveValuesForRecordAsync(GetAdditionalFieldValueSaveDTO(item), userId, ct);
            }

            return new SuccessResultDTO<Contractors>(item.Id.ToString(), []) { Data = item };
        }

        public async Task<ResultDTO<object>> Delete(int id, CancellationToken ct)
        {
            var item = await _repo.GetAsync(id, ct);
            if (item == null)
                return new ErrorResultDTO<object>(["Nie znaleziono kontrahenta w bazie"]);

            await _repo.Remove(item!);
            await _repo.SaveChangesAsync(ct);
            return new SuccessResultDTO<object>(id.ToString(), []);
        }

        private List<ContractorsImportDTO> GetContractorsRowsFromFile(IFormFile file)
        {
            try
            {
                if (Path.GetExtension(file.FileName).ToLowerInvariant() is ".csv")
                {
                    using var stream = file.OpenReadStream();
                    using var reader = new StreamReader(stream);
                    using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                        TrimOptions = TrimOptions.Trim,
                        MissingFieldFound = null,
                        HeaderValidated = null,
                        PrepareHeaderForMatch = args => args.Header?.Trim(),
                        Delimiter = ";"
                    });
                    csv.Context.RegisterClassMap<ContractorsImportMap>();
                    return csv.GetRecords<ContractorsImportDTO>().ToList();
                }
                else
                    return [];
            }
            catch (Exception ex)
            {
                return [];
            }
        }

        public async Task<ResultDTO<List<Contractors>>> Import(IFormFile file, CancellationToken ct)
        {
            if (Path.GetExtension(file.FileName).ToLowerInvariant() is not ".csv")
                return new ErrorResultDTO<List<Contractors>>(["Wgraj plik o rozszerzeniu .csv"]);

            var rows = GetContractorsRowsFromFile((IFormFile)file);

            var errors = new List<string>();
            var contractors = new List<Contractors>();

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                if (string.IsNullOrWhiteSpace(r.Code) || string.IsNullOrWhiteSpace(r.Name))
                {
                    errors.Add($"Brak wartości dla wymaganych kolumn Kod oraz Nazwa dla wiersza='{i + 1}'.");
                    continue;
                }
                if (!_repo.IsExistsByCode(r.Code))
                {
                    var entity = new Contractors
                    {
                        ForeignSystemObjectId = r.Id,
                        Code = r.Code.Trim(),
                        Name = r.Name.Trim(),
                        DisplayName = string.IsNullOrWhiteSpace(r.DisplayName) ? r.Code.Trim() : r.DisplayName.Trim(),
                        Nip = r.Nip?.Trim(),
                        EuVAT = r.EuVAT?.Trim()
                    };

                    await _repo.AddAsync(entity, ct);
                    contractors.Add(entity);
                }
            }
            if (contractors.Count > 0)
            {
                await _repo.SaveChangesAsync(ct);
                return new SuccessResultDTO<List<Contractors>>(contractors.Count.ToString(), [.. errors]) { Data = contractors };
            }
            else
                return new ErrorResultDTO<List<Contractors>>([.. errors]);
        }
    }
}
