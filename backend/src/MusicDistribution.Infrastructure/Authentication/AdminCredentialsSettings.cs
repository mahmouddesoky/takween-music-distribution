namespace MusicDistribution.Infrastructure.Authentication;

/// <summary>
/// Bound from the "Auth" configuration section: the single demo admin account.
/// A real system would use ASP.NET Core Identity (hashed passwords in the database) or an external IdP.
/// </summary>
public class AdminCredentialsSettings
{
    public const string SectionName = "Auth";

    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
