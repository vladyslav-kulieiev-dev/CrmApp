using CrmApp.Domain.DTO.AdditionalFields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.ContractorsDTO
{
    public class ContractorListItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Nip { get; set; }
        public bool? IsXopero { get; set; }

        public List<int> ActiveEngagementTypes { get; set; } = [];

        public decimal? HoursLimit { get; set; }
        public decimal? HoursUsed { get; set; }
        public decimal? HoursRemaining { get; set; }

        public List<AdditionalFieldValueDTO> AdditionalFieldValues { get; set; } = [];
    }
}
