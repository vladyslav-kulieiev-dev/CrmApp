using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Enums
{
    public enum ERenewalType
    {
        [Description("Pakiet jednorazowy")]
        None = 0,
        [Description("Miesięczne")]
        Monthly = 1,
        [Description("Roczne")]
        Annually = 2
    }
}
