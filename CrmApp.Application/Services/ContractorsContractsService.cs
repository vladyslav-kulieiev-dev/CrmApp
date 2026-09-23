using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
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
    public sealed class ContractorsContractsService : IContractorsContractsService
    {
        private readonly IContractorContractsRepository _repo;
        public ContractorsContractsService(IContractorContractsRepository repo)
        {
            _repo = repo;
        }

        public async Task<ContractorContracts?> GetContractorContract(
            int contractId, CancellationToken ct = default)
        {
            return await _repo.GetReadOnlyAsync(contractId, ct);
        }

        public Task<PagedResult<ContractorContracts>> GetContractorContracts(
            int contractorId, CancellationToken ct = default)
            => _repo.ListPagedAsync(new ContractorContractsRequest
            {
                ContractorId = contractorId,
                PageSize = 200   
            }, ct);

        public Task<PagedResult<ContractorContracts>> GetContractsPaged(
            ContractorContractsRequest req, CancellationToken ct = default)
            => _repo.ListPagedAsync(req, ct);

        public Task<IReadOnlyList<ContractorHoursSnapshots>> GetContractSnapshots(
            int contractId, CancellationToken ct = default)
            => _repo.ListSnapshotsForContract(contractId, ct);

        public async Task<ResultDTO<ContractorContracts>> Add(ContractorContracts contract, CancellationToken ct = default)
        {
            contract.Id = 0;
            contract.CreatedAt = DateTime.UtcNow;
            contract.HoursRemaining = contract.HoursLimit;

            if (string.IsNullOrWhiteSpace(contract.ContractNumber))
                contract.ContractNumber = await GenerateContractNumber(ct);

            if (!string.IsNullOrWhiteSpace(contract.ValidFromStr))
                contract.ValidFrom = DateTools.DateTimeFromString(contract.ValidFromStr);
            else
                contract.ValidFrom = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(contract.ValidToStr))
                contract.ValidTo = DateTools.DateTimeFromString(contract.ValidToStr);

            if (!string.IsNullOrWhiteSpace(contract.RenewalDateStr))
                contract.RenewalDate = DateTools.DateTimeFromString(contract.RenewalDateStr);

            // AllowOverLimit i RenewalType mapowane bezpośrednio z frontendu

            await _repo.AddAsync(contract, ct);
            await _repo.SaveChangesAsync(ct);

            return new SuccessResultDTO<ContractorContracts>(contract.Id.ToString()) { Data = contract };
        }

        public async Task<ResultDTO<ContractorHoursSnapshots>> AddSnapshot(
            int contractId, decimal hoursUsed, int userId, CancellationToken ct = default)
        {
            var contract = await _repo.GetAsync(contractId, ct);
            if (contract == null)
                return new ErrorResultDTO<ContractorHoursSnapshots>(["Nie znaleziono umowy"]);

            // Snapshoty obsługują kontrakty godzinowe (HoursPackage i MonthlyBilling)
            var isHoursBased = contract.EngagementType is EEngagementType.HoursPackage
                                                       or EEngagementType.MonthlyBilling;
            if (!isHoursBased)
                return new ErrorResultDTO<ContractorHoursSnapshots>(
                    ["Snapshoty godzin dotyczą tylko umów z limitem godzin (Pakiet godzin, Rozliczenie miesięczne)"]);

            // Walidacja przekroczenia limitu — tylko gdy brak flagi AllowOverLimit
            if (!contract.AllowOverLimit && contract.HoursLimit > 0 && hoursUsed > contract.HoursLimit)
                return new ErrorResultDTO<ContractorHoursSnapshots>(
                    [$"Podane godziny ({hoursUsed}h) przekraczają limit umowy ({contract.HoursLimit}h). " +
                     $"Włącz opcję 'Bez twardego limitu' w ustawieniach umowy, aby zezwolić na przekroczenie."]);

            var snapshot = new ContractorHoursSnapshots
            {
                ContractorContractId = contractId,
                SnapshotDate = DateTime.UtcNow,
                HoursUsed = hoursUsed,
                HoursRemaining = contract.HoursLimit - hoursUsed,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _repo.AddSnapshot(snapshot, ct);
            await _repo.SaveChangesAsync(ct);

            return new SuccessResultDTO<ContractorHoursSnapshots>(snapshot.Id.ToString()) { Data = snapshot };
        }

        public async Task<ResultDTO<ContractorContracts>> Update(
            ContractorContracts contract, CancellationToken ct = default)
        {
            var existing = await _repo.GetAsync(contract.Id, ct);
            if (existing == null)
                return new ErrorResultDTO<ContractorContracts>(["Nie znaleziono umowy"]);

            existing.EngagementType = contract.EngagementType;
            existing.BillingType = contract.BillingType;
            existing.BillingAmount = contract.BillingAmount;
            existing.HoursLimit = contract.HoursLimit;
            existing.AllowOverLimit = contract.AllowOverLimit;
            existing.RenewalType = contract.RenewalType;
            existing.ModifiedBy = contract.ModifiedBy;
            existing.ModifiedAt = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(contract.ValidFromStr))
                existing.ValidFrom = DateTools.DateTimeFromString(contract.ValidFromStr);
            else
                existing.ValidFrom = contract.ValidFrom ?? DateTime.Now;

            if (!string.IsNullOrWhiteSpace(contract.ValidToStr))
                existing.ValidTo = DateTools.DateTimeFromString(contract.ValidToStr);
            else
                existing.ValidTo = contract.ValidTo;

            if (!string.IsNullOrWhiteSpace(contract.RenewalDateStr))
                existing.RenewalDate = DateTools.DateTimeFromString(contract.RenewalDateStr);
            else
                existing.RenewalDate = contract.RenewalDate;

            await _repo.UpdateAsync(existing, ct);
            await _repo.SaveChangesAsync(ct);

            return new SuccessResultDTO<ContractorContracts>(existing.Id.ToString()) { Data = existing };
        }

        public async Task<ResultDTO<object>> Delete(int id, CancellationToken ct = default)
        {
            var existing = await _repo.GetAsync(id, ct);
            if (existing == null)
                return new ErrorResultDTO<object>(["Nie znaleziono umowy"]);

            await _repo.Remove(existing);
            await _repo.SaveChangesAsync(ct);

            return new SuccessResultDTO<object>(id.ToString());
        }

        private async Task<string> GenerateContractNumber(CancellationToken ct)
        {
            var now = DateTime.Now;
            var countThisYear = await _repo.CountByYearAndMonthAsync(now.Year, now.Month, ct);
            var monthStr = now.Month < 10 ? "0" + now.Month : now.Month.ToString();
            return $"UMW/{now.Year}/{monthStr}/{(countThisYear + 1):D4}";
        }
    }
}