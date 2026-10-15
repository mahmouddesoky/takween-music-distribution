namespace MusicDistribution.Application.Abstractions.Authentication;

public record AccessToken(string Token, DateTime ExpiresAt);

public interface IJwtTokenGenerator
{
    AccessToken Generate(AuthenticatedUser user);
}
