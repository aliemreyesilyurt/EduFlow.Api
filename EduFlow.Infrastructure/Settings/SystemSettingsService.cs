using EduFlow.Application.Abstractions.Security;
using EduFlow.Application.Abstractions.Settings;
using EduFlow.Application.Constants;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Infrastructure.Settings;

internal sealed class SystemSettingsService(
    ApplicationDbContext dbContext,
    ICryptographyService cryptographyService) : ISystemSettingsService
{
    public async Task<string?> GetValueAsync(string key, Guid? tenantId, CancellationToken cancellationToken)
    {
        if (tenantId is { } id)
        {
            var tenantSetting = await dbContext.SystemSettings
                .FirstOrDefaultAsync(s => s.TenantId == id && s.Key == key, cancellationToken);

            if (tenantSetting is not null)
            {
                return Decrypt(tenantSetting);
            }
        }

        var globalSetting = await dbContext.SystemSettings
            .FirstOrDefaultAsync(s => s.TenantId == null && s.Key == key, cancellationToken);

        return globalSetting is null ? null : Decrypt(globalSetting);
    }

    public async Task<IReadOnlyList<SystemSettingItem>> GetAllAsync(Guid? tenantId, CancellationToken cancellationToken)
    {
        var globalSettings = await dbContext.SystemSettings
            .Where(s => s.TenantId == null)
            .ToListAsync(cancellationToken);

        var merged = globalSettings.ToDictionary(s => s.Key);

        if (tenantId is { } id)
        {
            var tenantSettings = await dbContext.SystemSettings
                .Where(s => s.TenantId == id)
                .ToListAsync(cancellationToken);

            foreach (var setting in tenantSettings)
            {
                merged[setting.Key] = setting;
            }
        }

        return merged.Values
            .Select(s => new SystemSettingItem(s.Key, MaskIfSecret(s), s.IsSecret, s.Description))
            .OrderBy(s => s.Key)
            .ToList();
    }

    public async Task<Result> UpsertAsync(string key, string? value, Guid? tenantId, CancellationToken cancellationToken)
    {
        var existing = await dbContext.SystemSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Key == key, cancellationToken);

        var isSecret = SystemSettingKeys.Secrets.Contains(key);
        var storedValue = isSecret && !string.IsNullOrEmpty(value)
            ? cryptographyService.Encrypt(value)
            : value;

        if (existing is not null)
        {
            existing.Value = storedValue;
        }
        else
        {
            dbContext.SystemSettings.Add(new SystemSetting
            {
                Id = Guid.CreateVersion7(),
                TenantId = tenantId,
                Key = key,
                Value = storedValue,
                IsSecret = isSecret
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private string? Decrypt(SystemSetting setting) =>
        setting.IsSecret && !string.IsNullOrEmpty(setting.Value)
            ? cryptographyService.Decrypt(setting.Value)
            : setting.Value;

    private static string? MaskIfSecret(SystemSetting setting) =>
        setting.IsSecret && !string.IsNullOrEmpty(setting.Value) ? "***" : setting.Value;
}
