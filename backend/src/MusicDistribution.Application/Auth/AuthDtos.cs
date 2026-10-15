namespace MusicDistribution.Application.Auth;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, DateTime ExpiresAt);
