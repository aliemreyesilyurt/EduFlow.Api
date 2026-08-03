using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Abstractions.Settings;

public interface ISystemSettingsService
{
    /// <summary>Resolves a value for the given tenant, falling back to the global default.</summary>
    Task<string?> GetValueAsync(string key, Guid? tenantId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SystemSettingItem>> GetAllAsync(Guid? tenantId, CancellationToken cancellationToken);

    Task<Result> UpsertAsync(string key, string? value, Guid? tenantId, CancellationToken cancellationToken);
}

public sealed record SystemSettingItem(string Key, string? Value, bool IsSecret, string? Description);
