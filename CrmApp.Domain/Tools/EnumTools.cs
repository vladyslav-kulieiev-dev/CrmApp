using CrmApp.Domain.DTO;
using CrmApp.Domain.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Tools
{
    public static class EnumTools
    {
        static public string GetDescription(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field != null)
            {
                var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                    return attribute.Description;
            }
            return enumValue.ToString();
        }

        public static IconAttribute? GetIcon<T>(this T source)
        {
            if (source != null && !string.IsNullOrWhiteSpace(source.ToString()))
            {
                FieldInfo? fi = source.GetType().GetField(source.ToString() ?? string.Empty);
                if (fi != null)
                {
                    IconAttribute[] attributes = (IconAttribute[])fi.GetCustomAttributes(typeof(IconAttribute), false);
                    if (attributes != null && attributes.Length > 0) return attributes[0];
                }
            }
            return null;
        }

        static public List<ValueNameDTO<int>> GetEnumValues(Type e)
        {
            var dictionary = new List<ValueNameDTO<int>>();
            foreach (var val in Enum.GetValues(e))
            {
                var iconAttr = val.GetIcon();
                dictionary.Add(new ValueNameDTO<int> ((int)val, ((Enum)val).GetDescription(), iconAttr?.Icon, iconAttr?.IconClass));
            }
            return dictionary;
        }

        static public List<ValueNameDTO<int>> GetEnumValuesWithFieldKey(Type e)
        {
            var dictionary = new List<ValueNameDTO<int>>();
            foreach (var val in Enum.GetValues(e))
            {
                var iconAttr = val.GetIcon();
                dictionary.Add(new ValueNameDTO<int> ((int)val, ((Enum)val).GetDescription(), iconAttr?.Icon, iconAttr?.IconClass, val.ToString()));
            }
            return dictionary;
        }

        public static bool EnumValueMatchesDescription(object? enumValue, string? descriptionOrName)
        {
            if (enumValue == null || string.IsNullOrWhiteSpace(descriptionOrName))
                return false;

            if (enumValue is not Enum e) return false;

            return string.Equals(e.GetDescription(), descriptionOrName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(e.ToString(), descriptionOrName, StringComparison.OrdinalIgnoreCase);
        }

        public static object? GetPropertyValue(object? obj, string propertyName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            return obj.GetType()
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?.GetValue(obj);
        }

        public static string? GetPropertyValueAsString(object? obj, string propertyName)
        {
            var value = GetPropertyValue(obj, propertyName);
            if (value == null) return null;
            if (value is Enum e) return e.GetDescription();
            return value.ToString();
        }
    }
}
