using MusicDistribution.Domain.Entities;

namespace MusicDistribution.Application.Tracks;

/// <summary>
/// Maps domain entities to response DTOs. Expects Artist (and Distributions.Dsp for detail) to be loaded.
/// </summary>
internal static class TrackMappings
{
    public static TrackSummaryResponse ToSummary(this Track track) =>
        new(
            track.Id,
            track.Title,
            track.ArtistId,
            track.Artist.Name,
            track.Isrc,
            track.ReleaseDate,
            track.Genre,
            track.Status.ToString());

    public static TrackDetailResponse ToDetail(this Track track) =>
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
