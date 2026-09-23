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
    public class Settings
    {
        public Settings() 
        {
            Key = string.Empty;
            Label = string.Empty;
            ValueType = EValueType.Int;
        }
        public Settings(string key, string label, EValueType valueType)
        {
            Key = key;
            Label = label;
            ValueType = valueType;
            IsMultiple = false;
        }

        [Key]
        public int Id { get; set; }
        public string Key { get; set; }
        public string Label { get; set; }
        public string? Description { get; set; }
        public EValueType ValueType { get; set; }
        public string? Value { get; set; }
        public bool IsMultiple { get; set; }
        public string? TableName { get; set; }
        public string? ValueFixedPrefix { get; set; }
        public string? ValueFixedSuffix { get; set; }

        [NotMapped]
        public List<SettingsValuesDictionary>? AcceptedValues { get; set; }
        [NotMapped]
        public object? ValueObj
        { 
            get
            {
                if (Value == null)
                    return Value;

                return ValueType switch
                {
                    EValueType.Int => int.Parse(Value),
                    EValueType.Decimal => decimal.Parse(Value),
                    EValueType.String => Value,
                    EValueType.Boolean => Value == "true" ? true : false,
                    EValueType.List => Value,
                    _ => Value
                };
            }
        }
    }
}
