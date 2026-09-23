using CrmApp.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Enums
{
    public enum ENumeratorParts
    {
        [Description("Symbol dokumentu")]
        Symbol = 0,
        [Description("Autonumeracja")]
        Autonumeration = 1,
        [Description("Miesiąc bez zer")]
        MonthShort = 2,
        [Description("Miesiąc z zerem")]
        MonthLong = 3,
        [Description("Rok krótki")]
        YearShort = 4,
        [Description("Rok długi")]
        YearLong = 5
    }
}
