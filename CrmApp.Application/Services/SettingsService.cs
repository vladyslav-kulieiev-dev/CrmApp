using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Services
{
    public sealed class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _repo;
        public SettingsService(ISettingsRepository repo)
        {
            _repo = repo;
        }

        public async Task<IReadOnlyList<Settings>> GetAll(CancellationToken ct = default)
        {
            var settings = await _repo.ListAllAsync(ct);
            var settingsValues = await _repo.ListValuesDictionaryAsync(ct);
            foreach (var setting in settings.Where(x => x.ValueType == EValueType.List && string.IsNullOrWhiteSpace(x.TableName)))
            {
                setting.AcceptedValues = [.. settingsValues.Where(x => x.SettingId == setting.Id)];
            } 

            return settings;
        }

        public async Task<string?> GetSettingValueById(int id, CancellationToken ct)
        {
            return await _repo.GetValueByIdAsync(id, ct);
        }

        public async Task<string?> GetSettingValueByKey(string key, CancellationToken ct)
        {
            return await _repo.GetValueByKeyAsync(key, ct);
        }


        public async Task<IReadOnlyList<SettingsValuesDictionary>> GetValuesDictionaryBySettingId(int settingId, CancellationToken ct)
        {
            return await _repo.ListSettingValuesAsync(settingId, ct);
        }

        public async Task<ResultDTO<string>> SetSettingValueById(int id, string? value, CancellationToken ct)
        {
            await _repo.SetValueByIdAsync(id, value, ct);
            await _repo.SaveChangesAsync(ct);
            return new SuccessResultDTO<string>(id.ToString(), []) { Data = value };
        }

        public async Task<ResultDTO<string>> SetSettingValueByKey(string key, string? value, CancellationToken ct)
        {
            await _repo.SetValueByKeyAsync(key, value, ct);
            await _repo.SaveChangesAsync(ct);
            return new SuccessResultDTO<string>(key, []) { Data = value };
        }
    }
}
