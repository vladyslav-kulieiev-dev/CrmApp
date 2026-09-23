using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class ContractorsNips
    {
        [Key]
        public int Id { get; set; }
        public int ContractorId { get; set; }
        public string Nip { get; set; } = "";
        public bool IsPrimary { get; set; }
        [ForeignKey(nameof(ContractorId))]
        public virtual Contractors? Contractor { get; set; }
    }
}
