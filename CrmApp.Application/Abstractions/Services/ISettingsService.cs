using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Services
{
    public interface ISettingsService
    {
        Task<IReadOnlyList<Settings>> GetAll(CancellationToken ct = default);
        Task<string?> GetSettingValueByKey(string key, CancellationToken ct = default);
        Task<ResultDTO<string>> SetSettingValueByKey(string key, string? value, CancellationToken ct = default);
        Task<string?> GetSettingValueById(int id, CancellationToken ct = default);
        Task<ResultDTO<string>> SetSettingValueById(int id, string? value, CancellationToken ct = default);
        Task<IReadOnlyList<SettingsValuesDictionary>> GetValuesDictionaryBySettingId(int settingId, CancellationToken ct = default);
    }
}
