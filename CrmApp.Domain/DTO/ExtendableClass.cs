using CrmApp.Domain.DTO.AdditionalFields;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO
{
    public class ExtendableClass
    {
        [NotMapped]
        public List<AdditionalFieldDTO> AdditionalFields { get; set; } = [];

        [NotMapped]
        public List<AdditionalFieldValueDTO> AdditionalFieldValues { get; set; } = [];
    }
}
