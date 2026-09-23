using CrmApp.Domain.DTO;
using CrmApp.Domain.Enums;
using CrmApp.Domain.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class ContractorLicenses : ExtendableClass
    {
        [Key]
        public int Id { get; set; }
        public int ContractorId { get; set; }
        public int CatalogItemId { get; set; }
        public ELicenceType LicenseType { get; set; }
        public decimal Quantity { get; set; }
        public int? ImplementationOwnerId { get; set; }
        public int? TechnicalOwnerId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? UpgradeDate { get; set; }
        public EWarrantyState? WarrantyStatus { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }

        [ForeignKey(nameof(ContractorId))]
        public virtual Contractors? Contractor { get; set; }
        [ForeignKey(nameof(CatalogItemId))]
        public virtual CatalogItems? CatalogItem { get; set; }
        [ForeignKey(nameof(ImplementationOwnerId))]
        public virtual UsersProfiles? ImplementationOwner { get; set; }
        [ForeignKey(nameof(TechnicalOwnerId))]
        public virtual UsersProfiles? TechnicalOwner { get; set; }
        [ForeignKey(nameof(CreatedBy))]
        public virtual UsersProfiles? CreatedByUser { get; set; }
        [NotMapped]
        public string LicenseTypeName => LicenseType.GetDescription();
        [NotMapped]
        public string WarrantyStatusName => WarrantyStatus.HasValue ? WarrantyStatus.Value.GetDescription() : "";
        [NotMapped]
        public string? ValidFromStr { get; set; }
        [NotMapped]
        public string? ExpiresAtStr { get; set; }
        [NotMapped]
        public string? WarrantyEndDateStr { get; set; }
        [NotMapped]
        public string? UpgradeDateStr { get; set; }
    }
}
