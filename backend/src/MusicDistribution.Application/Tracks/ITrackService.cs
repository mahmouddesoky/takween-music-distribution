namespace MusicDistribution.Application.Tracks;

public interface ITrackService
{
    Task<IReadOnlyList<TrackSummaryResponse>> GetFilteredAsync(TrackFilterRequest filter, CancellationToken cancellationToken = default);
    Task<TrackDetailResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TrackDetailResponse> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default);
    Task<TrackDetailResponse> UpdateStatusAsync(int id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default);
    Task<TrackDetailResponse> DistributeAsync(int id, DistributeTrackRequest request, CancellationToken cancellationToken = default);
}
