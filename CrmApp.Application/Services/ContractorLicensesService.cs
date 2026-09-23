using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Services
{
    public sealed class ContractorLicensesService(IContractorLicensesRepository repo) : IContractorLicensesService
    {
        private readonly IContractorLicensesRepository _repo = repo;
        
        public Task<IReadOnlyList<ContractorLicenses>> GetContractorLicenses(int contractorId, CancellationToken ct = default) => _repo.ListByContractorId(contractorId, ct);
        
        public async Task<ResultDTO<ContractorLicenses>> Add(ContractorLicenses license, CancellationToken ct = default)
        {
            license.Id = 0;
            license.CreatedAt = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(license.SerialNumber))
                license.SerialNumber = GenerateSerialNumber(license.CatalogItemId);

            await _repo.AddAsync(license, ct);
            await _repo.SaveChangesAsync(ct);

            return new SuccessResultDTO<ContractorLicenses>(license.Id.ToString()) { Data = license };
        }

        private static string GenerateSerialNumber(int catalogItemId)
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = GenerateRandomAlphanumeric(6);
            return $"CI{catalogItemId}-{datePart}-{randomPart}";
        }
        private static readonly char[] _chars =
            "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray(); 
        private static string GenerateRandomAlphanumeric(int length)
        {
            var result = new char[length];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var buffer = new byte[length];
            rng.GetBytes(buffer);
            for (int i = 0; i < length; i++)
                result[i] = _chars[buffer[i] % _chars.Length];
            return new string(result);
        }

        public async Task<ResultDTO<ContractorLicenses>> Update(
            ContractorLicenses license, CancellationToken ct = default)
        {
            var existing = await _repo.GetAsync(license.Id, ct);
            if (existing == null)
                return new ErrorResultDTO<ContractorLicenses>(["Nie znaleziono licencji"]);

            existing.CatalogItemId = license.CatalogItemId;
            existing.LicenseType = license.LicenseType;
            existing.Quantity = license.Quantity;
            existing.SerialNumber = license.SerialNumber;
            existing.IssuedAt = license.IssuedAt;
            existing.ValidFrom = license.ValidFrom;
            existing.ExpiresAt = license.ExpiresAt;
            existing.UpgradeDate = license.UpgradeDate;
            existing.WarrantyStatus = license.WarrantyStatus;
            existing.WarrantyEndDate = license.WarrantyEndDate;
            existing.ImplementationOwnerId = license.ImplementationOwnerId;
            existing.TechnicalOwnerId = license.TechnicalOwnerId;
            existing.ModifiedAt = DateTime.Now;
            existing.ModifiedBy = license.ModifiedBy;

            await _repo.UpdateAsync(existing, ct);
            await _repo.SaveChangesAsync(ct);

            return new SuccessResultDTO<ContractorLicenses>(existing.Id.ToString()) { Data = existing };
        }

        public async Task<ResultDTO<object>> Delete(int id, CancellationToken ct = default)
        {
            var existing = await _repo.GetAsync(id, ct);
            if (existing == null)
                return new ErrorResultDTO<object>(["Nie znaleziono licencji"]);

            await _repo.Remove(existing);
            await _repo.SaveChangesAsync(ct);

            return new SuccessResultDTO<object>(id.ToString());
        }
    }
}
