using CrmApp.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Services
{
    public interface IEmailSender
    {
        (string html, string text) BuildPasswordEmail(string displayName, string resetUrl, bool isInvite);
        Task SendAsync(List<string> toMails, string subject, string htmlBody, CancellationToken ct = default, string? plainTextBody = null);
    }
}
