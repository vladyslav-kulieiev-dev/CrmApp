using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Enums
{
    public enum EBillingType
    {
        [Description("Brak")]
        None = 0,
        [Description("Płatność jednorazowa")]
        OneTimePayment = 1,
        [Description("Co miesiąc")]
        Monthly = 2,
        [Description("Co rok")]
        Annually = 3
    }
}
