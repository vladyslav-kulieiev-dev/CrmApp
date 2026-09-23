using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Enums
{
    public enum ELicenceType
    {
        [Description("Serwerowa")]
        Server = 1,
        [Description("Stanowiskowa")]
        PerSeat = 2
    }
}
