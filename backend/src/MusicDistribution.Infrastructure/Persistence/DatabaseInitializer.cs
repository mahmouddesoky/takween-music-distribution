using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicDistribution.Infrastructure.Persistence.Seeding;

namespace MusicDistribution.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>
    /// Applies pending EF Core migrations and seeds sample data. Keeps the API project
    /// from having to know about <see cref="AppDbContext"/>.
    /// </summary>
    public static async Task InitialiseDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Migrations only exist for relational providers (skips e.g. the EF InMemory provider).
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync(cancellationToken);
        }

        await DbSeeder.SeedAsync(context, cancellationToken);
    }
}
