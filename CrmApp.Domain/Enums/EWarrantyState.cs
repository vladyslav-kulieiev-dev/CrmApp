using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Enums
{
    public enum EWarrantyState
    {
        [Description("Brak gwarancji")]
        NoWarranty = 0,
        [Description("Terminowa")]
        TermWarranty = 1,
        [Description("Otwarta")]
        OpenWarranty = 2
    }
}
