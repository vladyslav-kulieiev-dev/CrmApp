using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Enums
{
    public enum ESystemType
    {
        [Description("enova365")]
        SonetaEnova = 1,
        [Description("Comarch ERP Optima")]
        ComarchOptima = 2,
        [Description("Comarch ERP XL")]
        ComarchXL = 3
    }
}
