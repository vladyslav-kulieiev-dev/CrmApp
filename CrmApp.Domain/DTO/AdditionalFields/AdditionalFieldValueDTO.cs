using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.AdditionalFields
{
    public class AdditionalFieldValueDTO
    {
        public int Id { get; set; }
        public int TableAdditionalFieldId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public bool IsMultiple { get; set; }
        public bool IsRequired { get; set; }
        public EValueType FieldType { get; set; }
        public string? FieldValue { get; set; }
        public string? DefaultValue { get; set; }
        public bool? FieldValueBool { get; set; }
        public int? DictionaryId { get; set; }
        public int? DictionaryElementId { get; set; }
        public string? DictionaryElementValue { get; set; }
        public bool CanView { get; set; } = true;  
        public bool CanEdit { get; set; } = true;
        public bool IsMappedFromInitObject { get; set; } = false;
        public bool PresentInNewRow { get; set; } = false;
        public short RowSpan { get; set; }
        public short ColSpan { get; set; }
        public int SortOrder { get; set; }
        public List<int> DictionaryElementIds { get; set; } = [];
        public List<DictionaryElementOption> FieldOptions { get; set; } = [];
    }

    public class DictionaryElementOption
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public class AdditionalFieldValueSaveDTO
    {
        public int TableAdditionalFieldId { get; set; }
        public string? FieldValue { get; set; }
        public List<int> DictionaryElementIds { get; set; } = [];
    }

    public class AdditionalFieldValuesBatchSaveDTO
    {
        public string TableName { get; set; } = string.Empty;
        public string RowId { get; set; } = string.Empty;
        public List<AdditionalFieldValueSaveDTO> Values { get; set; } = [];
    }
}
