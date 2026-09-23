using CrmApp.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Enums
{
    public enum EProjectState
    {
        [Icon("error", "error")]
        [Description("Anulowany")]
        Cancelled = -1,
        [Icon("pause_circle", "info")]
        [Description("Nierozpoczęty")]
        Created = 0,
        [Icon("pending", "success")]
        [Description("Aktywny")]
        Active = 1,
        [Icon("success")]
        [Description("Zamknięty")]
        Closed = 2
    }
}
