using Ganss.Xss;
using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CrmApp.Domain.Tools
{
    public static class MarkdownToHtml
    {
        private static readonly Regex SrcdocRegex =
            new Regex(@"<iframe\b[^>]*\bsrcdoc\s*=\s*""(?<srcdoc>.*?)""",
                      RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static string ToHtml(string md)
        {
            try
            {
                if (md.StartsWith("<iframe") && md.Contains("srcdoc"))
                    md = GetFirstSrcdoc(md);

                var pipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .UsePipeTables()
                    .UseTaskLists()
                    .Build();
                var raw = Markdown.ToHtml(md ?? "", pipeline);
                if (string.IsNullOrWhiteSpace(raw)) return md;

                var sanitizer = new HtmlSanitizer();
                sanitizer.AllowedAttributes.Add("class");
                sanitizer.AllowedSchemes.Add("data"); // if you embed images/data URIs
                return sanitizer.Sanitize(raw);
            }
            catch (Exception ex)
            {
                return md;
            }
        }

        private static string GetFirstSrcdoc(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            var m = SrcdocRegex.Match(html);
            if (!m.Success) return string.Empty;

            var raw = m.Groups["srcdoc"].Value;
            var decoded = WebUtility.HtmlDecode(raw);
            return decoded.Replace("\r\n", "\n").Trim();
        }
    }
}
