namespace MusicDistribution.Application.Abstractions.Authentication;

public record AuthenticatedUser(string Username, string Role);

/// <summary>
/// Checks a username/password pair against the configured user store.
/// </summary>
public interface IIdentityService
{
    Task<AuthenticatedUser?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}
