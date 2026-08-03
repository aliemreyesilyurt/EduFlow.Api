using System.Text;
using Scalar.AspNetCore;
using EduFlow.Application;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Infrastructure;
using EduFlow.Infrastructure.Authentication;
using EduFlow.Infrastructure.Database;
using EduFlow.Infrastructure.Database.Seed;
using EduFlow.WebApi.Exceptions;
using EduFlow.WebApi.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecksConfiguration();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException($"Missing '{JwtOptions.SectionName}' configuration section.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(PolicyNames.SysAdminOnly, p => p.RequireRole(Roles.SysAdmin))
    .AddPolicy(PolicyNames.TenantAdminOrAbove, p => p.RequireRole(Roles.SysAdmin, Roles.TenantAdmin))
    .AddPolicy(PolicyNames.InstructorOrAbove, p => p.RequireRole(Roles.SysAdmin, Roles.TenantAdmin, Roles.Instructor))
    .AddPolicy(PolicyNames.StudentOrAbove, p => p.RequireRole(Roles.SysAdmin, Roles.TenantAdmin, Roles.Instructor, Roles.Student))
    .AddPolicy(PolicyNames.StudentOnly, p => p.RequireRole(Roles.Student))
    .AddPolicy(PolicyNames.Authenticated, p => p.RequireAuthenticatedUser());

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddExceptionHandler<CustomExceptionHandler>()
            .AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

await IdentitySeeder.SeedAsync(app.Services);

app.MapApiEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHealthChecks();

app.MapScalarApiReference(options =>
{
    options.WithTheme(ScalarTheme.DeepSpace);
    options.WithDefaultHttpClient(
        ScalarTarget.CSharp,
        ScalarClient.HttpClient);
});

app.UseHttpsRedirection();
app.UseCors("VueClient");
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();

app.Run();
