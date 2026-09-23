using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface ISettingsRepository
    {
        Task<Settings?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Settings?> GetByIdReadOnlyAsync(int id, CancellationToken ct = default);
        Task<Settings?> GetByKeyAsync(string key, CancellationToken ct = default);
        Task<Settings?> GetByKeyReadOnlyAsync(string key, CancellationToken ct = default);
        Task<IReadOnlyList<Settings>> ListAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<SettingsValuesDictionary>> ListSettingValuesAsync(int settingId, CancellationToken ct = default);
        Task<IReadOnlyList<SettingsValuesDictionary>> ListValuesDictionaryAsync(CancellationToken ct = default);
        Task<string?> GetValueByIdAsync(int settingId, CancellationToken ct = default);
        Task<string?> GetValueByKeyAsync(string key, CancellationToken ct = default);
        Task SetValueByIdAsync(int settingId, string? value, CancellationToken ct = default);
        Task SetValueByKeyAsync(string key, string? value, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
