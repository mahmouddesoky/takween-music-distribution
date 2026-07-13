using MusicDistribution.Application.DTOs;

namespace MusicDistribution.Application.Interfaces;

public interface IDistributionService
{
    Task<TrackDetailResponse> DistributeAsync(
        int trackId,
        DistributeTrackRequest request,
        CancellationToken cancellationToken = default);
}
