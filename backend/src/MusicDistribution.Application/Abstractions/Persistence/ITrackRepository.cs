using MusicDistribution.Domain.Entities;
using MusicDistribution.Domain.Enums;

namespace MusicDistribution.Application.Abstractions.Persistence;

public record TrackFilter(int? ArtistId, string? Genre, TrackStatus? Status);

public interface ITrackRepository
{
    /// <summary>Read-only list (artist included), ordered by title.</summary>
    Task<IReadOnlyList<Track>> GetFilteredAsync(TrackFilter filter, CancellationToken cancellationToken = default);

    /// <summary>Read-only track with its artist and DSP distributions.</summary>
    Task<Track?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Tracked track with its artist and DSP distributions, for changes saved through <see cref="IUnitOfWork"/>.</summary>
    Task<Track?> GetForUpdateAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> IsrcExistsAsync(string isrc, CancellationToken cancellationToken = default);
    void Add(Track track);
}
