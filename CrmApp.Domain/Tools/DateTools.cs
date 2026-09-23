using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Tools
{
    public static class DateTools
    {
        public static DateTime? DateTimeFromOptString(this string? dateStr, string format = "yyyy-MM-dd HH:mm:ss")
        {
            return !string.IsNullOrWhiteSpace(dateStr) ? DateTime.ParseExact(dateStr, format, CultureInfo.InvariantCulture) : null;
        }
        public static DateTime DateTimeFromString(this string dateStr, string format = "yyyy-MM-dd HH:mm:ss")
        {
            return DateTime.ParseExact(dateStr, format, CultureInfo.InvariantCulture);
        }
    }
}
