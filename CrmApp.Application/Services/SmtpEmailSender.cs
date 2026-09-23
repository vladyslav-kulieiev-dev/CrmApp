using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.Configuration;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.Options;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CrmApp.Application.Services
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpClient? _client;
        private readonly string _from;
        private readonly FrontendOptions _opt;

        public SmtpEmailSender(IConfiguration cfg, IOptions<FrontendOptions> opt)
        {
            _opt = opt.Value;
            _from = (cfg["Email:Address"] ?? "").Trim();
            var host = (cfg["Email:Smtp"] ?? "").Trim();
            var portStr = (cfg["Email:Port"] ?? "587").Trim();
            var enableSsl = bool.Parse((cfg["Email:EnableSsl"] ?? "true").Trim()); // always true for Gmail

            var username = (cfg["Email:Username"] ?? "").Trim();
            if (string.IsNullOrWhiteSpace(username)) username = _from;
            var password = (cfg["Email:Password"] ?? "").Replace(" ", "");

            if (string.IsNullOrWhiteSpace(_from) || string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException("Email sender not configured.");

            if (!int.TryParse(portStr, out var port)) port = 587;

            _client = new SmtpClient(host, port)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = enableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password),
                Timeout = 10000
            };
        }

        public async Task SendAsync(List<string> toMails, string subject, string htmlBody,
            CancellationToken ct = default, string? plainTextBody = null)
        {
            if (_client == null) return;

            using var msg = new MailMessage
            {
                From = new MailAddress(_from),
                Subject = subject ?? "",
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = false
            };

            toMails.ForEach((to) => msg.To.Add(new MailAddress(to)));

            var text = plainTextBody ?? HtmlToText(htmlBody);
            var altText = AlternateView.CreateAlternateViewFromString(
                text,
                Encoding.UTF8,
                MediaTypeNames.Text.Plain
            );
            altText.TransferEncoding = TransferEncoding.QuotedPrintable;
            msg.AlternateViews.Add(altText);

            var altHtml = AlternateView.CreateAlternateViewFromString(
                htmlBody ?? "",
                Encoding.UTF8,
                MediaTypeNames.Text.Html
            );
            altHtml.TransferEncoding = TransferEncoding.QuotedPrintable;
            msg.AlternateViews.Add(altHtml);

            await _client.SendMailAsync(msg, ct);
        }

        public (string html, string text) BuildPasswordEmail(string displayName, string resetUrl, bool isInvite)
        {
            var actionLine = isInvite
                ? "Twoje konto zostało utworzone w aplikacji CrmApp. Ustaw swoje hasło, klikając w przycisk poniżej."
                : "Zresetuj swoje hasło do aplikacji CrmApp.";

            var ctaText = isInvite ? "Ustaw hasło" : "Ustaw nowe hasło";
            var preheader = isInvite
                ? "Twoje konto zostało utworzone – ustaw hasło."
                : "Resetowanie hasła – ustaw nowe hasło.";
            string ignoreMsg = !isInvite
                ? "Jeśli to nie Ty inicjowałeś(-aś) tę akcję, zignoruj tę wiadomość."
                : "Jeśli ten e-mail Cię nie dotyczy, zignoruj tę wiadomość.";
            var text = $@"👋 Cześć {displayName},

{actionLine}
Link: {resetUrl}

{ignoreMsg}";


            var html = $@"
<!doctype html>
<html lang=""pl"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta name=""color-scheme"" content=""light dark"">
  <meta name=""supported-color-schemes"" content=""light dark"">
  <title>CrmApp</title>
  <style>
    /* Reset */
    body, table, td, a {{ -ms-text-size-adjust:100%; -webkit-text-size-adjust:100%; }}
    table, td {{ mso-table-lspace:0pt; mso-table-rspace:0pt; }}
    img {{ -ms-interpolation-mode:bicubic; border:0; height:auto; line-height:100%; outline:none; text-decoration:none; }}
    table {{ border-collapse:collapse !important; }}
    body {{ margin:0 !important; padding:0 !important; width:100% !important; }}
    /* Base */
    .wrapper {{ width:100%; background:#f6f7fb; }}
    .container {{ max-width:600px; margin:0 auto; }}
    .card {{ background:#ffffff; border-radius:12px; padding:32px; text-align:center; }}
    h1 {{ margin:0 0 12px; font-size:22px; line-height:1.3; }}
    p {{ margin:0 0 14px; font-size:14px; line-height:1.6; color:#222; }}
    .muted {{ color:#666; font-size:12px; text-align:center; }}
    .btn {{ display:inline-block; padding:12px 20px; border-radius:8px; text-decoration:none; }}
    .btn-primary {{ background:#2563eb; color:#ffffff !important; }}
    .cta-row {{ padding:8px 0 16px; }}
    .footer {{ text-align:center; color:#888; font-size:12px; padding:12px 0 24px; }}
    .preheader {{ display:none !important; visibility:hidden; opacity:0; color:transparent; height:0; width:0; overflow:hidden; }}
    @media (prefers-color-scheme: dark) {{
      .wrapper {{ background:#0b0b0b; }}
      .card {{ background:#121212; }}
      p, h1 {{ color:#eaeaea !important; }}
      .muted {{ color:#aaa !important; }}
    }}
    /* Outlook button fallback */
    .btn-tbl a {{ color:#ffffff !important; text-decoration:none !important; }}
  </style>
</head>
<body>
  <div class=""preheader"">{preheader}</div>
  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""wrapper"" width=""100%"">
    <tr>
      <td align=""center"" style=""padding:24px;"">
        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""container"" width=""600""
             style=""width:600px; max-width:100%; margin:0 auto;"">
          <tr>
            <td style=""padding-bottom:16px; text-align:center; font-weight:700; font-size:16px; color:#111;"">
              CrmApp
            </td>
          </tr>
          <tr>
            <td align=""center"">
              <table role=""presentation"" width=""100%"" class=""card"">
                <tr>
                  <td align=""center"">
                    <h1>👋 Cześć {System.Net.WebUtility.HtmlEncode(displayName)}!</h1>
                    <p>{System.Net.WebUtility.HtmlEncode(actionLine)}</p>

                    <!-- CTA -->
                    <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""cta-row"" style=""margin:8px auto 16px auto;"">
                      <tr>
                        <td align=""center"">
                          <!--[if mso]>
                          <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word""
                            href=""{HtmlEncoder.Default.Encode(resetUrl)}"" style=""height:44px;v-text-anchor:middle;width:230px;"" arcsize=""12%"" stroke=""f"" fillcolor=""#2563eb"">
                            <w:anchorlock/>
                            <center style=""color:#ffffff;font-family:Arial,sans-serif;font-size:16px;font-weight:bold;"">
                              {System.Net.WebUtility.HtmlEncode(ctaText)}
                            </center>
                          </v:roundrect>
                          <![endif]-->
                          <!--[if !mso]><!-- -->
                          <a class=""btn btn-primary""
                             href=""{HtmlEncoder.Default.Encode(resetUrl)}""
                             target=""_blank"" style=""display:inline-block; padding:12px 20px; border-radius:8px; background:#2563eb; color:#ffffff; text-decoration:none;""
                             rel=""noopener"">{System.Net.WebUtility.HtmlEncode(ctaText)}</a>
                          <!--<![endif]-->
                        </td>
                      </tr>
                    </table>

                    <p class=""muted"">Jeśli przycisk nie działa, skopiuj i wklej ten adres w przeglądarce:</p>
                    <p class=""muted"" style=""text-align:center; word-break:break-all; overflow-wrap:anywhere;"">
                        <a href=""{HtmlEncoder.Default.Encode(resetUrl)}"" style=""color:#2563eb;"">{HtmlEncoder.Default.Encode(resetUrl)}</a>
                    </p>

                    <hr style=""border:none;border-top:1px solid #eee;margin:16px 0;"">

                    <p class=""muted"">{ignoreMsg}</p>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <tr>
            <td class=""footer"" align=""center"" style=""text-align:center; color:#888; font-size:12px; padding:12px 0 24px;"">
              © {DateTime.Now.Year} CrmApp • Ten e-mail został wysłany automatycznie.
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            return (html, text);
        }

        private static string HtmlToText(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            html = Regex.Replace(html, @"<(br|BR)\s*/?>", "\n");
            html = Regex.Replace(html, @"</p\s*>", "\n\n", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"</(h[1-6]|div|li|tr|table)\s*>", "\n", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "<.*?>", string.Empty);
            html = Regex.Replace(html, @"[ \t]+", " ");
            html = Regex.Replace(html, @"\n{3,}", "\n\n");
            html = WebUtility.HtmlDecode(html);

            return html.Trim();
        }
    }
}
