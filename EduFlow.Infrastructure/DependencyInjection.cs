using System.Threading.Channels;
using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Caching;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Abstractions.Notifications;
using EduFlow.Application.Abstractions.Security;
using EduFlow.Application.Abstractions.Settings;
using EduFlow.Application.Abstractions.Storage;
using EduFlow.Application.Options;
using EduFlow.Infrastructure.Authentication;
using EduFlow.Infrastructure.Caching;
using EduFlow.Infrastructure.Database;
using EduFlow.Infrastructure.Identity;
using EduFlow.Infrastructure.Interceptors;
using EduFlow.Infrastructure.Logging;
using EduFlow.Infrastructure.Multitenancy;
using EduFlow.Infrastructure.Notifications;
using EduFlow.Infrastructure.Repository;
using EduFlow.Infrastructure.Security;
using EduFlow.Infrastructure.Settings;
using EduFlow.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

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

        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<TenantSaveChangesInterceptor>();
        services.AddScoped<AuditLogInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditLogInterceptor>());
            options.UseNpgsql(configuration.GetConnectionString("connection"));
        });

        // Separate context on purpose: append-only audit/log tables shouldn't share migrations
        // or query filters with the business schema, even though they use the same database today.
        services.AddDbContext<LoggingDbContext>(options =>
        {
            var loggingConnection = configuration.GetConnectionString("logging")
                ?? configuration.GetConnectionString("connection");

            options.UseNpgsql(loggingConnection,
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Logging"));
        });

        services.AddSingleton(Channel.CreateBounded<AppLog>(new BoundedChannelOptions(10_000)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite
        }));
        services.AddSingleton<ILoggerProvider, DatabaseLoggerProvider>();
        services.AddHostedService<DatabaseLogFlusherService>();

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
        services.Configure<TenantProvisioningOptions>(configuration.GetSection(TenantProvisioningOptions.SectionName));
        services.AddScoped<JwtTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ISystemSettingsService, SystemSettingsService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var redisConnection = configuration.GetConnectionString("redis");
        services.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(redisConnection!));
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "EduFlow:";
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        return services;
    }
}
