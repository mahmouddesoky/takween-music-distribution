namespace MusicDistribution.Infrastructure.Authentication;

/// <summary>Bound from the "Jwt" configuration section. Shared by token creation and token validation.</summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>HMAC-SHA256 signing key; must be at least 32 bytes.</summary>
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 480;
}
