using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MusicDistribution.Application.Common;
using MusicDistribution.Application.DTOs;
using MusicDistribution.Application.Exceptions;
using MusicDistribution.Application.Interfaces;
using MusicDistribution.Domain.Entities;
using MusicDistribution.Domain.Enums;
using MusicDistribution.Infrastructure.Persistence;

namespace MusicDistribution.Infrastructure.Services;

public class TrackService(
    AppDbContext context,
    IValidator<CreateTrackRequest> createValidator,
    IValidator<UpdateTrackStatusRequest> statusValidator) : ITrackService
{
    public async Task<TrackDetailResponse> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default)
    {
        await ValidationHelper.ValidateAndThrowAsync(createValidator, request, cancellationToken);

        var artistExists = await context.Artists.AnyAsync(artist => artist.Id == request.ArtistId, cancellationToken);
        if (!artistExists)
        {
            throw new NotFoundException($"Artist with id {request.ArtistId} was not found.");
        }

        var normalizedIsrc = request.Isrc.Trim().ToUpperInvariant();
        var isrcExists = await context.Tracks.AnyAsync(track => track.Isrc == normalizedIsrc, cancellationToken);
        if (isrcExists)
        {
            throw new ConflictException($"A track with ISRC '{normalizedIsrc}' already exists.");
        }

        var status = string.IsNullOrWhiteSpace(request.Status)
            ? TrackStatus.Draft
            : Enum.Parse<TrackStatus>(request.Status, true);

        var track = new Track
        {
            Title = request.Title.Trim(),
            ArtistId = request.ArtistId,
            Isrc = normalizedIsrc,
            ReleaseDate = request.ReleaseDate,
            Genre = request.Genre.Trim(),
            Status = status
        };

        context.Tracks.Add(track);
        await context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(track.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<TrackSummaryResponse>> GetFilteredAsync(
        int? artistId,
        string? genre,
        TrackStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = context.Tracks
            .Include(track => track.Artist)
            .AsNoTracking()
            .AsQueryable();

        if (artistId.HasValue)
        {
            query = query.Where(track => track.ArtistId == artistId.Value);
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(track => track.Genre.ToLower() == genre.Trim().ToLower());
        }

        if (status.HasValue)
        {
            query = query.Where(track => track.Status == status.Value);
        }

        var tracks = await query
            .OrderBy(track => track.Title)
            .ToListAsync(cancellationToken);

        return tracks.Select(TrackMapper.ToSummary).ToList();
    }

    public async Task<TrackDetailResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var track = await context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Distributions)
                .ThenInclude(d => d.Dsp)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (track is null)
        {
            throw new NotFoundException($"Track with id {id} was not found.");
        }

        return TrackMapper.ToDetail(track);
    }

    public async Task<TrackDetailResponse> UpdateStatusAsync(
        int id,
        UpdateTrackStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidationHelper.ValidateAndThrowAsync(statusValidator, request, cancellationToken);

        var track = await context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Distributions)
                .ThenInclude(d => d.Dsp)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (track is null)
        {
            throw new NotFoundException($"Track with id {id} was not found.");
        }

        track.Status = Enum.Parse<TrackStatus>(request.Status, true);
        await context.SaveChangesAsync(cancellationToken);

        return TrackMapper.ToDetail(track);
    }
}
