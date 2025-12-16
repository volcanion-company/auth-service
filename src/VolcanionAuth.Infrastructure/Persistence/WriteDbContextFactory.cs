using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VolcanionAuth.Infrastructure.Persistence;

/// <summary>
/// Factory for creating instances of WriteDbContext at design time for Entity Framework Core tools.
/// </summary>
/// <remarks>
/// This factory is used by EF Core tooling (e.g., dotnet ef migrations, dotnet ef database update) 
/// to create a DbContext instance without requiring the full application service provider.
/// It reads the connection string from appsettings.Development.json in the API project.
/// </remarks>
public class WriteDbContextFactory : IDesignTimeDbContextFactory<WriteDbContext>
{
    /// <summary>
    /// Creates a new instance of WriteDbContext for design-time operations.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the factory.</param>
    /// <returns>A configured instance of WriteDbContext.</returns>
    public WriteDbContext CreateDbContext(string[] args)
    {
        // Build configuration from the API project's appsettings
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../VolcanionAuth.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("WriteDatabase")
            ?? throw new InvalidOperationException("WriteDatabase connection string not found in configuration.");

        // Configure DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<WriteDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        });

        return new WriteDbContext(optionsBuilder.Options);
    }
}
