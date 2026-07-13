using MusicDistribution.Application.DTOs;
using MusicDistribution.Domain.Entities;

namespace MusicDistribution.Infrastructure.Services;

/// <summary>
/// Centralised mapping from domain entities to response DTOs.
/// Keeping it here avoids leaking EF navigation quirks into the API layer.
/// </summary>
internal static class TrackMapper
{
    public static TrackSummaryResponse ToSummary(Track track) =>
        new(
            track.Id,
            track.Title,
            track.ArtistId,
            track.Artist.Name,
            track.Isrc,
            track.ReleaseDate,
            track.Genre,
            track.Status.ToString());

    public static TrackDetailResponse ToDetail(Track track) =>
        new(
            track.Id,
            track.Title,
            track.ArtistId,
            track.Artist.Name,
            track.Isrc,
            track.ReleaseDate,
            track.Genre,
            track.Status.ToString(),
            track.Distributions
                .OrderBy(distribution => distribution.Dsp.Name)
                .Select(distribution => new DistributionStatusDto(
                    distribution.DspId,
                    distribution.Dsp.Name,
                    distribution.Status.ToString(),
                    distribution.SubmittedAt))
                .ToList());
}
