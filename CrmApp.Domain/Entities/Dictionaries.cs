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
    public class Dictionaries
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public EDictionaryType DictionaryType { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public bool IsCustom { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual UsersProfiles? CreatedByUser { get; set; }
        [NotMapped]
        public List<DictionariesElementsDTO> DictionariesElements { get; set; } = [];
    }
}
