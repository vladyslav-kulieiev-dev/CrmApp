using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class TablesAdditionalFieldsValues
    {
        [Key]
        public int Id { get; set; }
        public int TableAdditionalFieldId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string RowId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string? FieldValue { get; set; }
        public int? DictionaryElementId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        [ForeignKey(nameof(TableAdditionalFieldId))]
        public virtual TablesAdditionalFields? TableAdditionalField { get; set; }
        [ForeignKey(nameof(DictionaryElementId))]
        public virtual DictionariesElements? DictionaryElement { get; set; }
        [ForeignKey(nameof(CreatedBy))]
        public virtual UsersProfiles? CreatedByUser { get; set; }

    }
}
