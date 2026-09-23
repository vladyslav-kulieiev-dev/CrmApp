using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.ContractorsDTO
{
    public class ContractorContractsRequest : PagedRequest
    {
        public int? ContractorId { get; set; }
        public int? ContractId { get; set; }
        public List<EEngagementType>? EngagementTypes { get; set; }
        public bool? ActiveOnly { get; set; }
        public bool? HasHoursDebt { get; set; }
        public bool? IsOverLimitCurrentMonth { get; set; }
    }
}
