using CrmApp.Domain.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO
{
    public class ValueNameDTO<T>
    {
        public ValueNameDTO(T value, string name, string? icon = null, string? iconClass = null, string? key = null) 
        { 
            this.Value = value;
            this.Name = name;
            this.Icon = icon;
            this.IconClass = iconClass;
            this.Key = key;
        }

        public T Value { get; set; }
        public string Name { get; set; }
        public string? Icon { get; set; }
        public string? IconClass { get; set; }
        public string? Key { get; set; }
    }
}
