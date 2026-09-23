using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class DictionariesElements
    {
        [Key]
        public int Id { get; set; }
        public int DictionaryId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public bool IsCustom { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public int? ParentId { get; set; }
        public int OrdinalNumber { get; set; }
        public string? AlternativeValuesForMapping { get; set; }
        public string? Icon { get; set; }
        public string? IconColor { get; set; }

        [ForeignKey(nameof(DictionaryId))]
        public virtual Dictionaries? Dictionary { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual UsersProfiles? CreatedByUser { get; set; }
    }
}
