using CrmApp.Domain.Configuration;
using CrmApp.Domain.DTO.AdditionalFields;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.AdditionalFields
{
    public class AdditionalFieldDTO
    {
        public int Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public EValueType FieldType { get; set; } = EValueType.String;
        public int? DictionaryId { get; set; }
        public string? DictionaryName { get; set; }
        public bool IsMultiple { get; set; }
        public bool IsShowOnLists { get; set; }
        public bool IsRequired { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public int SortOrder { get; set; }
        public bool PresentInNewRow { get; set; }
        public short RowSpan { get; set; }
        public short ColSpan { get; set; }
        public string? DefaultValue { get; set; }
        public int? RowId { get; set; }
        public bool IsMappedFromInitObject { get; set; }
        public string? InitObjectPropertyName { get; set; }
        public List<AdditionalFieldPermissionDTO> Permissions { get; set; } = [];
        public List<DictionaryElementOption> FieldOptions { get; set; } = [];
    }

    public class AdditionalFieldCreateDTO
    {
        public string TableName { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public EValueType FieldType { get; set; }
        public int? DictionaryId { get; set; }
        public bool IsMultiple { get; set; }
        public bool IsShowOnLists { get; set; }
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public int? RowId { get; set; }
        public int SortOrder { get; set; }
        public bool PresentInNewRow { get; set; }
        public short RowSpan { get; set; }
        public short ColSpan { get; set; }
        public bool IsMappedFromInitObject { get; set; }
        public string? InitObjectPropertyName { get; set; }
        public List<AdditionalFieldPermissionCreateDTO> Permissions { get; set; } = [];
    }

    public class AdditionalFieldUpdateDTO : AdditionalFieldCreateDTO
    {
        public int Id { get; set; }
    }

    public class AdditionalFieldReorderDTO
    {
        public string TableName { get; set; } = string.Empty;
        public Dictionary<int, int> Orders { get; set; } = [];
    }

    public class AdditionalFieldsBatchSaveDTO
    {
        public string TableName { get; set; } = string.Empty;
        public int? RowId { get; set; }
        public List<AdditionalFieldUpdateDTO> Fields { get; set; } = [];
    }

    public static class AdditionalFieldsHelper
    {
        public static AdditionalFieldValuesBatchSaveDTO GetAdditionalFieldValueSaveDTO(string tableName, int rowId, List<AdditionalFieldValueDTO> fields)
        {
            return new AdditionalFieldValuesBatchSaveDTO
            {
                TableName = tableName,
                RowId = rowId.ToString(),
                Values = fields.Select(v =>
                    new AdditionalFieldValueSaveDTO
                    {
                        TableAdditionalFieldId = v.TableAdditionalFieldId,
                        FieldValue = v.FieldType != EValueType.Boolean ? v.FieldValue : (v.FieldValueBool == true ? "true" : "false"),
                        DictionaryElementIds = !v.IsMultiple && v.DictionaryElementId != null ? [(int)v.DictionaryElementId] : v.DictionaryElementIds
                    }).ToList()
            };
        }
    }
}
