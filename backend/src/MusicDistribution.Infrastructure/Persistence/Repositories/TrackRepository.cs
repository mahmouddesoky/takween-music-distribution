using Microsoft.EntityFrameworkCore;
using MusicDistribution.Application.Abstractions.Persistence;
using MusicDistribution.Domain.Entities;

namespace MusicDistribution.Infrastructure.Persistence.Repositories;

internal sealed class TrackRepository(AppDbContext context) : ITrackRepository
{
    public async Task<IReadOnlyList<Track>> GetFilteredAsync(TrackFilter filter, CancellationToken cancellationToken = default)
    {
        var query = context.Tracks
            .AsNoTracking()
            .Include(track => track.Artist)
            .AsQueryable();

        if (filter.ArtistId.HasValue)
        {
            query = query.Where(track => track.ArtistId == filter.ArtistId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Genre))
        {
            var genre = filter.Genre.ToLower();
            query = query.Where(track => track.Genre.ToLower() == genre);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(track => track.Status == filter.Status.Value);
        }

        return await query
            .OrderBy(track => track.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<Track?> GetDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        WithDetails(context.Tracks.AsNoTracking())
            .FirstOrDefaultAsync(track => track.Id == id, cancellationToken);

    public Task<Track?> GetForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        WithDetails(context.Tracks)
            .FirstOrDefaultAsync(track => track.Id == id, cancellationToken);

    public Task<bool> IsrcExistsAsync(string isrc, CancellationToken cancellationToken = default) =>
        context.Tracks.AnyAsync(track => track.Isrc == isrc, cancellationToken);

    public void Add(Track track) => context.Tracks.Add(track);

    private static IQueryable<Track> WithDetails(IQueryable<Track> query) =>
        query
            .Include(track => track.Artist)
            .Include(track => track.Distributions)
                .ThenInclude(distribution => distribution.Dsp);
}
