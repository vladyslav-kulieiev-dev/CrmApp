using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.Configuration;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.CatalogItemsDTO;
using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Services
{
    public sealed class CatalogItemsService : ICatalogItemsService
    {
        private readonly ICatalogItemsRepository _repository;
        private readonly IDictionariesRepository _dictRepository;
        private readonly IAdditionalFieldsService _additionalFieldsService;
        public CatalogItemsService(ICatalogItemsRepository repository, IDictionariesRepository dictionariesRepository,
        IAdditionalFieldsService additionalFieldsService)
        {
            _repository = repository;
            _dictRepository = dictionariesRepository;
            _additionalFieldsService = additionalFieldsService;
        }

        public async Task<IReadOnlyList<CatalogItems>> ListAllAsync(bool onlyActive, CancellationToken ct = default)
        {
            var catalogItems = onlyActive ? await _repository.ListActiveAsync(ct) : await _repository.ListAllAsync(ct);
            foreach (var item in catalogItems)
            {
                item.SupportedSystems = await _repository.GetSupportedSystems(item.Id, ct);
                item.CanDelete = !_repository.CheckIfItemIsUsed(item.Id);
            }
            return catalogItems;
        }

        public async Task<CatalogItems?> GetById(int id, CancellationToken ct = default)
        {
            var item = await _repository.GetReadOnlyAsync(id, ct);
            if (item != null)
            {
                item.SupportedSystems = await _repository.GetSupportedSystems(item.Id, ct);
            }
            return item;
        }

        public async Task<ResultDTO<CatalogItems>> Add(CatalogItems item, CancellationToken ct = default)
        {
            if (_repository.CheckIfExistsByCode(item.Code))
                return new ErrorResultDTO<CatalogItems>(["W bazie istnieje już produkt o kodzie " + item.Code]);

            item.CreatedAt = DateTime.Now;
            await _repository.AddAsync(item, ct);
            await _repository.SaveChangesAsync(ct);
            await _repository.UpdateSupportedSystems(item, ct);
            await _repository.SaveChangesAsync(ct);
            return new SuccessResultDTO<CatalogItems>(item, item.Id.ToString());
        }

        public async Task<ResultDTO<CatalogItems>> ActivateDeactivateItem(int id, bool activate, CancellationToken ct = default)
        {
            var catalogItem = await _repository.GetAsync(id, ct);
            if (catalogItem == null)
                return new ErrorResultDTO<CatalogItems>(["Nie znaleziono produktu o podanym Id"]);

            catalogItem.IsActive = activate;
            await _repository.UpdateAsync(catalogItem, ct);
            await _repository.SaveChangesAsync(ct);
            return new SuccessResultDTO<CatalogItems>(catalogItem, id.ToString());
        }

        public async Task<ResultDTO<CatalogItems>> Update(CatalogItems item, CancellationToken ct = default)
        {
            var catalogItem = await _repository.GetAsync(item.Id, ct);
            if (catalogItem == null)
                return new ErrorResultDTO<CatalogItems>(["Nie znaleziono produktu o podanym Id"]);

            if (_repository.CheckIfExistsByCode(item.Code, item.Id))
                return new ErrorResultDTO<CatalogItems>(["W bazie istnieje już produkt o kodzie " + item.Code]);

            catalogItem.Name = item.Name;
            catalogItem.Code = item.Code;
            catalogItem.Description = item.Description;
            catalogItem.CategoryId = item.CategoryId;
            catalogItem.Type = item.Type;
            catalogItem.UnitId = item.UnitId;
            catalogItem.UnitName = await _dictRepository.GetElementKeyById(item.UnitId, ct) ?? "";
            catalogItem.BillingUnitId = item.BillingUnitId;
            catalogItem.BillingUnitName = await _dictRepository.GetElementKeyById(item.BillingUnitId, ct) ?? "";
            catalogItem.Price = item.Price;
            catalogItem.VatRate = item.VatRate;
            catalogItem.Currency = item.Currency;
            catalogItem.IsActive = item.IsActive;
            catalogItem.ParentItemId = item.ParentItemId;
            catalogItem.TechnicalSupervisorId = item.TechnicalSupervisorId;
            catalogItem.ImplementationManagerId = item.ImplementationManagerId;
            catalogItem.SupportedSystems = item.SupportedSystems;

            await _repository.UpdateAsync(catalogItem, ct);
            await _repository.UpdateSupportedSystems(catalogItem, ct);
            await _repository.SaveChangesAsync(ct);
            return new SuccessResultDTO<CatalogItems>(catalogItem, catalogItem.Id.ToString());
        }

        public async Task<ResultDTO<object>> Delete(int id, CancellationToken ct = default)
        {
            var catalogItem = await _repository.GetAsync(id, ct);
            if (catalogItem == null)
                return new ErrorResultDTO<object>(["Nie znaleziono produktu o podanym Id"]);

            if (_repository.CheckIfItemIsUsed(id))
                return new ErrorResultDTO<object>(["Nie można usunąć produktu, który jest aktywny w licencjach kontrahenta"]);


            await _repository.Remove(catalogItem);
            await _repository.SaveChangesAsync(ct);
            return new SuccessResultDTO<object>(id.ToString());
        }
        public async Task<PagedResult<CatalogItemsListItemDTO>> GetPagedAsync(CatalogItemPagedRequest request, int userId, List<string> rolesIds, CancellationToken ct = default)
        {
            var fields = await _additionalFieldsService.GetFieldsForTableAsync(TablesNames.CatalogItems, userId, rolesIds, ct);
            var additionalFieldIds = fields.Select(f => f.Id).ToList();
            return await _repository.GetPagedAsync(request, additionalFieldIds, ct);
        }
    }
}
