namespace MusicDistribution.Application.DTOs;

public record LoginResponse(string Token, DateTime ExpiresAt);
