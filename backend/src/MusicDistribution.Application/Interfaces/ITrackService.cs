using MusicDistribution.Application.DTOs;
using MusicDistribution.Domain.Enums;

namespace MusicDistribution.Application.Interfaces;

public interface ITrackService
{
    Task<IReadOnlyList<TrackSummaryResponse>> GetFilteredAsync(
        int? artistId,
        string? genre,
        TrackStatus? status,
        CancellationToken cancellationToken = default);

    Task<TrackDetailResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TrackDetailResponse> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default);

    Task<TrackDetailResponse> UpdateStatusAsync(
        int id,
        UpdateTrackStatusRequest request,
        CancellationToken cancellationToken = default);
}
