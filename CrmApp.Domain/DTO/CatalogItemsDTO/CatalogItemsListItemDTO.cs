using CrmApp.Domain.DTO.AdditionalFields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.CatalogItemsDTO
{
    public class CatalogItemsListItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public int Type { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string BillingUnitName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public decimal VatRate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int? TechnicalSupervisorId { get; set; }
        public int? ImplementationManagerId { get; set; }
        public List<AdditionalFieldValueDTO> AdditionalFieldValues { get; set; } = [];
    }
}
