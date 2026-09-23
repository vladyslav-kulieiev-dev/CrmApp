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
    public class TablesAdditionalFields
    {
        [Key]
        public int Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public EValueType FieldType { get; set; }
        public int? DictionaryId { get; set; }
        public bool IsMultiple { get; set; }
        public bool IsShowOnLists { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int SortOrder { get; set; }       
        public bool IsRequired { get; set; }    
        public string? DefaultValue { get; set; }
        public int? RowId { get; set; } 
        public bool PresentInNewRow { get; set; } 
        public short RowSpan { get; set; } 
        public short ColSpan { get; set; } 
        public bool IsMappedFromInitObject { get; set; }
        public string? InitObjectPropertyName { get; set; }

        [ForeignKey(nameof(DictionaryId))]
        public virtual Dictionaries? Dictionary { get; set; }
        [ForeignKey(nameof(CreatedBy))]
        public virtual UsersProfiles? CreatedByUser { get; set; }
        [NotMapped]
        public virtual ICollection<TablesAdditionalFieldsValues>? AdditionalFieldValues { get; set; }

        public virtual ICollection<TablesAdditionalFieldsPermissions>? Permissions { get; set; }
    }
}
