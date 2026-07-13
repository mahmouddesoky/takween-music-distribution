using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MusicDistribution.Application.Validators;

namespace MusicDistribution.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all FluentValidation validators found in the Application assembly.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateArtistRequestValidator>();
        return services;
    }
}
