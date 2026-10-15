using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MusicDistribution.Application.Artists;
using MusicDistribution.Application.Auth;
using MusicDistribution.Application.Dsps;
using MusicDistribution.Application.Tracks;

namespace MusicDistribution.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the application use-case services and every FluentValidation validator in this assembly.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<ITrackService, TrackService>();
        services.AddScoped<IDspService, DspService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
