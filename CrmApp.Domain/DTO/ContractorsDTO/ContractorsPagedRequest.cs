using CrmApp.Domain.DTO.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.ContractorsDTO
{
    public class ContractorsPagedRequest : PagedRequest
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Nip { get; set; }
        public bool? IsXopero { get; set; }
        public List<int>? EngagementTypes { get; set; }
        public bool? HasHoursRemaining { get; set; }
    }
}
