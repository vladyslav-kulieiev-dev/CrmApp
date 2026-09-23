using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmApp.Domain.Entities
{
    public class CatalogItemSupportedSystems
    {
        [Key]
        public int Id { get; set; }
        public int CatalogItemId { get; set; }
        public int SupportedSystemId { get; set; }
        [ForeignKey(nameof(CatalogItemId))]
        public virtual CatalogItems? CatalogItem { get; set; }
    }
}
