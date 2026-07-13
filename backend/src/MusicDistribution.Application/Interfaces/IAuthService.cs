using MusicDistribution.Application.DTOs;

namespace MusicDistribution.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
