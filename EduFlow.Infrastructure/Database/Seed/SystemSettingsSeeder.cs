using EduFlow.Application.Constants;
using EduFlow.Domain.Entities;
using EduFlow.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduFlow.Infrastructure.Database.Seed;

public static class SystemSettingsSeeder
{
    private static readonly (string Key, string Description)[] GlobalDefaults =
    [
        (SystemSettingKeys.SmtpHost, "SMTP server host. Leave empty to log emails to the console instead of sending them."),
        (SystemSettingKeys.SmtpPort, "SMTP server port (e.g. 587)."),
        (SystemSettingKeys.SmtpUseSsl, "Whether to use SSL/TLS when connecting to the SMTP server (true/false)."),
        (SystemSettingKeys.SmtpUsername, "SMTP authentication username. Leave empty to connect without authentication."),
        (SystemSettingKeys.SmtpPassword, "SMTP authentication password. Stored encrypted."),
        (SystemSettingKeys.SmtpFromAddress, "The email address emails are sent from."),
        (SystemSettingKeys.SmtpFromName, "The display name emails are sent from.")
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var existingKeys = await dbContext.SystemSettings
            .Where(s => s.TenantId == null)
            .Select(s => s.Key)
            .ToListAsync();

        var missing = GlobalDefaults.Where(d => !existingKeys.Contains(d.Key));

        foreach (var (key, description) in missing)
        {
            dbContext.SystemSettings.Add(new SystemSetting
            {
                Id = Guid.CreateVersion7(),
                TenantId = null,
                Key = key,
                Value = null,
                IsSecret = SystemSettingKeys.Secrets.Contains(key),
                Description = description
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
