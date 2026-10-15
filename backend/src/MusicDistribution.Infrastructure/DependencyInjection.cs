using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicDistribution.Application.Abstractions.Authentication;
using MusicDistribution.Application.Abstractions.Persistence;
using MusicDistribution.Infrastructure.Authentication;
using MusicDistribution.Infrastructure.Persistence;
using MusicDistribution.Infrastructure.Persistence.Repositories;

namespace MusicDistribution.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the implementations of the Application abstractions:
    /// EF Core / SQLite persistence, repositories, unit of work, identity and JWT issuing.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IArtistRepository, ArtistRepository>();
        services.AddScoped<ITrackRepository, TrackRepository>();
        services.AddScoped<IDspRepository, DspRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(settings => Encoding.UTF8.GetByteCount(settings.Key) >= 32, "Jwt:Key must be at least 32 bytes.")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Issuer) && !string.IsNullOrWhiteSpace(settings.Audience),
                "Jwt:Issuer and Jwt:Audience are required.")
            .Validate(settings => settings.ExpiryMinutes > 0, "Jwt:ExpiryMinutes must be greater than 0.")
            .ValidateOnStart();

        services.AddOptions<AdminCredentialsSettings>()
            .Bind(configuration.GetSection(AdminCredentialsSettings.SectionName))
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Username) && !string.IsNullOrWhiteSpace(settings.Password),
                "Auth:Username and Auth:Password are required.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IIdentityService, ConfigurationIdentityService>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
