using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.ContractorsDTO
{
    public sealed class ContractorsImportMap : ClassMap<ContractorsImportDTO>
    {
        public ContractorsImportMap()
        {
            Map(x => x.Id).Name("Id");
            Map(x => x.Code).Name("Kod");
            Map(x => x.Name).Name("Nazwa");
            Map(x => x.DisplayName).Name("Nazwa wyświetlana", "NazwaWyswietlana");
            Map(x => x.Nip).Name("NIP");
            Map(x => x.EuVAT).Name("EU VAT");
        }
    }
}
