using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Tools
{
    public static class NumeratorHelper
    {
        public static string GetNumberStructure(List<ENumeratorParts> parts, string delimiter)
            => string.Join(delimiter, parts.Select(p => "@" + p));

        public static List<ENumeratorParts> GetNumeratorParts(string structure, string delimiter)
            => structure
                .Split(delimiter)
                .Select(p => p.TrimStart('@'))
                .Select(p => Enum.TryParse<ENumeratorParts>(p, out var part) ? part : (ENumeratorParts?)null)
                .Where(p => p.HasValue)
                .Select(p => p!.Value)
                .ToList();

        public static string GetNumberStructureParsed(string symbol, string delimiter, string structure, 
            int leadingZeros = 0, DateTime? createdAt = null, int? number = null)
        {
            number ??= 1;
            createdAt ??= DateTime.Now;
            string numberString = number.Value.ToString();
            if (numberString.Length < leadingZeros)
                numberString = '0'.Repeat(leadingZeros - numberString.Length) + numberString;

            return string.Join(delimiter, structure
                .Split(delimiter)
                .Select(p => p.TrimStart('@') switch
                {
                    nameof(ENumeratorParts.Symbol) => symbol,
                    nameof(ENumeratorParts.Autonumeration) => number.Value.ToString(),
                    nameof(ENumeratorParts.MonthShort) => createdAt.Value.Month.ToString(),
                    nameof(ENumeratorParts.MonthLong) => createdAt.Value.Month.ToString("D2"),
                    nameof(ENumeratorParts.YearShort) => createdAt.Value.Year.ToString()[^2..],
                    nameof(ENumeratorParts.YearLong) => createdAt.Value.Year.ToString(),
                    _ => null
                })
                .Where(p => p != null));
        }

        public static string Repeat(this char val, int count)
        {
            return new string(val, count);
        }
    }
}
