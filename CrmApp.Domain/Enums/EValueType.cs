using CrmApp.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Enums
{
    public enum EValueType
    {
        [Description("Liczba całkowita")]
        [Icon("pin")]
        Int = 0,
        [Description("Liczba zmiennoprzecinkowa")]
        [Icon("calculate")]
        Decimal = 1,
        [Description("Tekst")]
        [Icon("text_fields")]
        String = 2,
        [Description("Tak/Nie")]
        [Icon("toggle_on")]
        Boolean = 3,
        [Description("Lista wartości")]
        [Icon("list")]
        List = 4,
        [Description("Data")]
        [Icon("today")]
        Date = 5,
        [Description("Czas")]
        [Icon("schedule")]
        Time = 6
    }
}
