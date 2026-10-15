namespace MusicDistribution.Application.Artists;

public interface IArtistService
{
    Task<IReadOnlyList<ArtistResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ArtistResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ArtistResponse> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default);
}
