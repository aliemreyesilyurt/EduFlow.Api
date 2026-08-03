using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Abstractions.Notifications;
using EduFlow.Application.Abstractions.Security;
using EduFlow.Application.Abstractions.Settings;
using EduFlow.Application.Options;
using EduFlow.Infrastructure.Authentication;
using EduFlow.Infrastructure.Database;
using EduFlow.Infrastructure.Identity;
using EduFlow.Infrastructure.Interceptors;
using EduFlow.Infrastructure.Multitenancy;
using EduFlow.Infrastructure.Notifications;
using EduFlow.Infrastructure.Repository;
using EduFlow.Infrastructure.Security;
using EduFlow.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddDataProtection();

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ICryptographyService, DataProtectionCryptographyService>();

        services.AddSingleton<AuditInterceptor>();
        services.AddScoped<TenantSaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<TenantSaveChangesInterceptor>());
            options.UseNpgsql(configuration.GetConnectionString("connection"));
        });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<ApplicationRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<ClientAppOptions>(configuration.GetSection(ClientAppOptions.SectionName));
        services.AddScoped<JwtTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ISystemSettingsService, SystemSettingsService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
