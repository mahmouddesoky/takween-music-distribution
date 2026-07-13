using MusicDistribution.Application.DTOs;

namespace MusicDistribution.Application.Interfaces;

public interface IArtistService
{
    Task<IReadOnlyList<ArtistResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ArtistResponse> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default);
}
