using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Infrastructure.Caching;
using VolcanionAuth.Infrastructure.Persistence;
using VolcanionAuth.Infrastructure.Persistence.Repositories;
using VolcanionAuth.Infrastructure.Security;
using VolcanionAuth.Infrastructure.Services;

namespace VolcanionAuth.Infrastructure;

/// <summary>
/// Provides extension methods for registering infrastructure services and dependencies in the application's dependency
/// injection container.
/// </summary>
/// <remarks>This class contains methods to configure core infrastructure components such as database contexts,
/// caching, repositories, and authorization services. It is intended to be used during application startup to ensure
/// all required services are registered with the dependency injection system.</remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Adds and configures infrastructure services, including database contexts, caching, repositories, and
    /// authorization, to the specified service collection.
    /// </summary>
    /// <remarks>This method registers both read and write database contexts for PostgreSQL, configures Redis
    /// caching, and adds scoped services for repositories, caching, password hashing, JWT token generation, and
    /// authorization. It is intended to be called during application startup as part of dependency injection
    /// setup.</remarks>
    /// <param name="services">The service collection to which the infrastructure services will be added. Must not be null.</param>
    /// <param name="configuration">The application configuration used to retrieve connection strings and other settings. Must not be null.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance with the infrastructure services registered.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a required connection string ('WriteDatabase', 'ReadDatabase', or 'Redis') is not found in the
    /// configuration.</exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database - PostgreSQL
        // Write and Read DbContexts
        var writeConnectionString = configuration.GetConnectionString("WriteDatabase") ?? throw new InvalidOperationException("WriteDatabase connection string not found");
        // Read DbContext
        var readConnectionString = configuration.GetConnectionString("ReadDatabase") ?? throw new InvalidOperationException("ReadDatabase connection string not found");

        // Write DbContext
        services.AddDbContext<WriteDbContext>(options => options.UseNpgsql(writeConnectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        }));

        // Read DbContext
        services.AddDbContext<ReadDbContext>(options => options.UseNpgsql(readConnectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
            npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }));

        // Caching - Redis
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection string not found");
        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "VolcanionAuth:";
        });
        // Redis Connection Multiplexer
        services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));

        // Repositories
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<WriteDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));

        // Services
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        // Authorization Services (RBAC + PBAC)
        services.AddScoped<IPolicyEngineService, PolicyEngineService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        // End of Infrastructure Services
        return services;
    }
}
