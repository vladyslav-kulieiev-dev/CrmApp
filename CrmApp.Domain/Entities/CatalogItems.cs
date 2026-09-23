using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class CatalogItems
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public int Type { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public int BillingUnitId { get; set; }
        public string BillingUnitName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public decimal VatRate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int? ParentItemId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? TechnicalSupervisorId { get; set; }
        public int? ImplementationManagerId { get; set; }

        [ForeignKey(nameof(Type))]
        public virtual DictionariesElements? TypeObj { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual DictionariesElements? Category { get; set; }

        [ForeignKey(nameof(UnitId))]
        public virtual DictionariesElements? Unit { get; set; }

        [ForeignKey(nameof(BillingUnitId))]
        public virtual DictionariesElements? BillingUnit { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual UsersProfiles? CreatedByUser { get; set; }


        [NotMapped]
        public List<int>? SupportedSystems { get; set; }
        [NotMapped]
        public bool CanDelete { get; set; }
    }
}
