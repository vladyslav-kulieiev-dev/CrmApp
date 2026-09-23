using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Enums
{
    public enum EEngagementType
    {
        [Description("Brak (kontakt przed usługą)")]
        None = 0,
        [Description("Umowa")]
        Contract = 1,
        [Description("Pakiet godzin")]
        HoursPackage = 2,
        [Description("Rozliczenie miesięczne")]
        MonthlyBilling = 3
    }
}
