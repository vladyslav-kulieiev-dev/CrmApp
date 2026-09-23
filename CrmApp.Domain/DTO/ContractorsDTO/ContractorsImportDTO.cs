using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.ContractorsDTO
{
    public class ContractorsImportDTO
    {
        public int? Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public required string DisplayName { get; set; }
        public string? Nip { get; set; }
        public string? EuVAT { get; set; }

    }
}
