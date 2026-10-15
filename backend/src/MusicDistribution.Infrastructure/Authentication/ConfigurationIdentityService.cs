using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MusicDistribution.Application.Abstractions.Authentication;

namespace MusicDistribution.Infrastructure.Authentication;

/// <summary>
/// Validates the login against the admin account from configuration.
/// </summary>
internal sealed class ConfigurationIdentityService(IOptions<AdminCredentialsSettings> options) : IIdentityService
{
    public Task<AuthenticatedUser?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var admin = options.Value;

        // Compare SHA-256 digests in constant time: no early exit on the first wrong
        // character and no leak of the expected length (digests are always 32 bytes).
        var usernameMatches = FixedTimeEquals(username, admin.Username);
        var passwordMatches = FixedTimeEquals(password, admin.Password);

        AuthenticatedUser? user = usernameMatches & passwordMatches
            ? new AuthenticatedUser(admin.Username, Roles.Admin)
            : null;

        return Task.FromResult(user);
    }

    private static bool FixedTimeEquals(string actual, string expected) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(actual)),
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
}
