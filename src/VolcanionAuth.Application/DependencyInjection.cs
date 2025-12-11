using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using VolcanionAuth.Application.Common.Behaviors;

namespace VolcanionAuth.Application;

/// <summary>
/// Provides extension methods for registering application services and dependencies with the dependency injection
/// container.
/// </summary>
/// <remarks>This class contains static methods that configure core application services, such as MediatR,
/// validation, and object mapping, for use within the application's dependency injection system. These methods are
/// intended to be called during application startup to ensure all required services are registered and available
/// throughout the application's lifetime.</remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Configures application-level services, including MediatR, validation, and object mapping, for the specified
    /// service collection.
    /// </summary>
    /// <remarks>This method registers MediatR handlers, validators, and AutoMapper profiles from the calling
    /// assembly. It also adds pipeline behaviors for validation and logging to support request processing. Call this
    /// method during application startup to ensure required services are available for dependency injection.</remarks>
    /// <param name="services">The service collection to which the application services will be added. Cannot be null.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance with application services registered.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Get the assembly containing the application services
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR, FluentValidation, and AutoMapper services
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(assembly);

        // Add pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // Return the configured service collection
        return services;
    }
}
