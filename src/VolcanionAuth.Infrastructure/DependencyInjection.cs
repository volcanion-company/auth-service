using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Infrastructure.Caching;
using VolcanionAuth.Infrastructure.Persistence;
using VolcanionAuth.Infrastructure.Persistence.Repositories;
using VolcanionAuth.Infrastructure.Security;

namespace VolcanionAuth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database - PostgreSQL
        var writeConnectionString = configuration.GetConnectionString("WriteDatabase")
            ?? throw new InvalidOperationException("WriteDatabase connection string not found");
        var readConnectionString = configuration.GetConnectionString("ReadDatabase")
            ?? throw new InvalidOperationException("ReadDatabase connection string not found");

        services.AddDbContext<WriteDbContext>(options =>
            options.UseNpgsql(writeConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            }));

        services.AddDbContext<ReadDbContext>(options =>
            options.UseNpgsql(readConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
                npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            }));

        // Redis
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string not found");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "VolcanionAuth:";
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        // Repositories
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<WriteDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));

        // Services
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthorizationService, HybridAuthorizationService>();

        return services;
    }
}
