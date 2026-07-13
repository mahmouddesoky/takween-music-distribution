using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicDistribution.Application.Interfaces;
using MusicDistribution.Infrastructure.Persistence;
using MusicDistribution.Infrastructure.Services;

namespace MusicDistribution.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the persistence layer (EF Core / SQLite) and all application services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<ITrackService, TrackService>();
        services.AddScoped<IDistributionService, DistributionService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
