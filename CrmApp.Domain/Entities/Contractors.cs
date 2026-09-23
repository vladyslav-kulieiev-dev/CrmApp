using CrmApp.Domain.DTO;
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
    public class Contractors : ExtendableClass
    {
        [Key]
        public int Id { get; set; }
        public int? ForeignSystemObjectId { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public required string DisplayName { get; set; }
        public string? Nip { get; set; }
        public string? EuVAT { get; set; }
        public bool? IsXopero { get; set; }
        [NotMapped]
        public List<string>? AlternativeNipNumbers { get; set; }
        [NotMapped]
        public List<int> CurrentEngagementTypes { get; set; } = [];
    }
}
