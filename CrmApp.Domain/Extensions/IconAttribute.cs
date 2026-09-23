using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Extensions
{
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class IconAttribute : System.Attribute
    {
        readonly string icon;
        readonly string iconClass;
        public IconAttribute(string positionalString, string iconClass = "icon-default")
        {
            this.icon = positionalString;
            this.iconClass = iconClass;
        }
        public string Icon
        {
            get { return icon; }
        }
        public string IconClass
        {
            get { return iconClass; }
        }
    }
}
