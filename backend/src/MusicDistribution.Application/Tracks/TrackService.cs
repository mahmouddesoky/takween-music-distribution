using FluentValidation;
using MusicDistribution.Application.Abstractions.Persistence;
using MusicDistribution.Application.Common.Exceptions;
using MusicDistribution.Application.Common.Validation;
using MusicDistribution.Domain.Entities;

namespace MusicDistribution.Application.Tracks;

/// <summary>
/// Track use cases. Orchestrates validation, repositories and the <see cref="Track"/> domain rules;
/// knows nothing about EF Core or HTTP.
/// </summary>
public class TrackService(
    ITrackRepository tracks,
    IArtistRepository artists,
    IDspRepository dsps,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IValidator<TrackFilterRequest> filterValidator,
    IValidator<CreateTrackRequest> createValidator,
    IValidator<UpdateTrackStatusRequest> statusValidator,
    IValidator<DistributeTrackRequest> distributeValidator) : ITrackService
{
    public async Task<IReadOnlyList<TrackSummaryResponse>> GetFilteredAsync(
        TrackFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        await ValidationHelper.ValidateAndThrowAsync(filterValidator, filter, cancellationToken);

        var criteria = new TrackFilter(
            filter.ArtistId,
            string.IsNullOrWhiteSpace(filter.Genre) ? null : filter.Genre.Trim(),
            string.IsNullOrWhiteSpace(filter.Status) ? null : TrackRules.ParseStatus(filter.Status));

        var result = await tracks.GetFilteredAsync(criteria, cancellationToken);
        return result.Select(track => track.ToSummary()).ToList();
    }

    public async Task<TrackDetailResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var track = await tracks.GetDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Track with id {id} was not found.");

        return track.ToDetail();
    }

    public async Task<TrackDetailResponse> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default)
    {
        await ValidationHelper.ValidateAndThrowAsync(createValidator, request, cancellationToken);

        if (!await artists.ExistsAsync(request.ArtistId, cancellationToken))
        {
            throw new NotFoundException($"Artist with id {request.ArtistId} was not found.");
        }

        var isrc = request.Isrc.Trim().ToUpperInvariant();
        if (await tracks.IsrcExistsAsync(isrc, cancellationToken))
        {
            throw new ConflictException($"A track with ISRC '{isrc}' already exists.");
        }

        // New tracks always start as Draft; they move on through DistributeTo / ChangeStatus.
        var track = new Track(request.Title.Trim(), request.ArtistId, isrc, request.ReleaseDate, request.Genre.Trim());

        tracks.Add(track);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(track.Id, cancellationToken);
    }

    public async Task<TrackDetailResponse> UpdateStatusAsync(
        int id,
        UpdateTrackStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidationHelper.ValidateAndThrowAsync(statusValidator, request, cancellationToken);

        var track = await tracks.GetForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Track with id {id} was not found.");

        track.ChangeStatus(TrackRules.ParseStatus(request.Status));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return track.ToDetail();
    }

    public async Task<TrackDetailResponse> DistributeAsync(
        int id,
        DistributeTrackRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidationHelper.ValidateAndThrowAsync(distributeValidator, request, cancellationToken);

        var track = await tracks.GetForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Track with id {id} was not found.");

        var dspIds = request.DspIds.Distinct().ToList();
        var existingDspIds = await dsps.GetExistingIdsAsync(dspIds, cancellationToken);
        var unknownDspIds = dspIds.Except(existingDspIds).ToList();
        if (unknownDspIds.Count > 0)
        {
            throw new NotFoundException($"DSP id(s) not found: {string.Join(", ", unknownDspIds)}.");
        }

        track.DistributeTo(dspIds, timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload so the new distributions come back with their DSP names.
        return await GetByIdAsync(track.Id, cancellationToken);
    }
}
