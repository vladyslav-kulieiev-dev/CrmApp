using CrmApp.Domain.DTO.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.CatalogItemsDTO
{
    public class CatalogItemPagedRequest : PagedRequest
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int? CategoryId { get; set; }
        public int? Type { get; set; }
        public bool ShowNotActive { get; set; } = false;
    }
}
