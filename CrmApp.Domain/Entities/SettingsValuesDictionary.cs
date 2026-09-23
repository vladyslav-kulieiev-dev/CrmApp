using CrmApp.Domain.Configuration;
using CrmApp.Domain.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CrmApp.Domain.Entities
{
    public class SettingsValuesDictionary
    {
        public SettingsValuesDictionary()
        {
            SettingId = 0;
            SettingKey = string.Empty;
            ValueType = EValueType.Int;
            Value = string.Empty;
            Label = string.Empty;
        }

        [JsonConstructor]
        public SettingsValuesDictionary(
            int settingId,
            string settingKey,
            EValueType valueType,
            string value,
            string label)
        {
            SettingId = settingId;
            SettingKey = settingKey;
            ValueType = valueType;
            Value = value;
            Label = label;
        }

        public SettingsValuesDictionary(Settings setting, string value, string name)
        {
            SettingId = setting.Id;
            SettingKey = setting.Key;
            ValueType = setting.ValueType;
            Value = value;
            Label = name;
        }

        [Key]
        public int Id { get; set; }
        public int SettingId { get; set; }
        public string SettingKey { get; set; }
        public EValueType ValueType { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
    }
}
