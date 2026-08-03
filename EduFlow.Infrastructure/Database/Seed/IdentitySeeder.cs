using EduFlow.Application.Constants;
using EduFlow.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EduFlow.Infrastructure.Database.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentitySeeder));

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        var sysAdminEmail = configuration["SysAdmin:Email"] ?? "sysadmin@eduflow.local";
        var sysAdminPassword = configuration["SysAdmin:Password"] ?? "ChangeMe123!";

        if (await userManager.FindByEmailAsync(sysAdminEmail) is not null)
        {
            return;
        }

        var sysAdmin = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = sysAdminEmail,
            Email = sysAdminEmail,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            TenantId = null,
            CreatedOn = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(sysAdmin, sysAdminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(sysAdmin, Roles.SysAdmin);
        }
        else
        {
            logger.LogError(
                "Failed to seed SysAdmin user {Email}: {Errors}",
                sysAdminEmail,
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
