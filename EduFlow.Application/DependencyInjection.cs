using System.Reflection;
using EduFlow.Application.Abstractions;
using EduFlow.Application.Extensions;
using EduFlow.Application.Features.PointsFeature;
using EduFlow.Application.Pipelines;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EduFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly);
        services.AddHandlersFromAssembly(assembly);
        services.RegisterApiEndpointsFromAssembly(assembly);

        // Not an IHandler<,> — shared by two handlers (SubmitExamAttempt/ReviewExamAttempt), so it
        // isn't picked up by the assembly scan above and needs its own registration.
        services.AddScoped<IPointsAwardService, PointsAwardService>();

        return services;
    }

    private static IServiceCollection AddHandlersFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                !t.ContainsGenericParameters)
            .ToList();

        foreach (var implementation in handlerTypes)
        {
            var handlerInterfaces = implementation
                .GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IHandler<,>));

            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddScoped(handlerInterface, implementation);
            }
        }

        services.Decorate(typeof(IHandler<,>), typeof(ValidationDecorator<,>));
        services.Decorate(typeof(IHandler<,>), typeof(LoggingDecorator<,>));

        return services;
    }
}
