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

public class DistributionService(
    AppDbContext context,
    IValidator<DistributeTrackRequest> validator) : IDistributionService
{
    public async Task<TrackDetailResponse> DistributeAsync(
        int trackId,
        DistributeTrackRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidationHelper.ValidateAndThrowAsync(validator, request, cancellationToken);

        var track = await context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Distributions)
                .ThenInclude(d => d.Dsp)
            .FirstOrDefaultAsync(t => t.Id == trackId, cancellationToken);

        if (track is null)
        {
            throw new NotFoundException($"Track with id {trackId} was not found.");
        }

        var distinctDspIds = request.DspIds.Distinct().ToList();
        var existingDspIds = await context.Dsps
            .Where(dsp => distinctDspIds.Contains(dsp.Id))
            .Select(dsp => dsp.Id)
            .ToListAsync(cancellationToken);

        if (existingDspIds.Count != distinctDspIds.Count)
        {
            throw new NotFoundException("One or more DSP ids were not found.");
        }

        var alreadyDistributed = track.Distributions
            .Where(distribution => distinctDspIds.Contains(distribution.DspId))
            .Select(distribution => distribution.DspId)
            .ToList();

        if (alreadyDistributed.Count > 0)
        {
            throw new ConflictException(
                $"Track is already submitted to DSP id(s): {string.Join(", ", alreadyDistributed)}.");
        }

        var submittedAt = DateTime.UtcNow;
        foreach (var dspId in distinctDspIds)
        {
            context.TrackDistributions.Add(new TrackDistribution
            {
                TrackId = track.Id,
                DspId = dspId,
                SubmittedAt = submittedAt,
                Status = DistributionStatus.Pending
            });
        }

        // Submitting a draft track moves it into the pipeline.
        if (track.Status == TrackStatus.Draft)
        {
            track.Status = TrackStatus.Submitted;
        }

        await context.SaveChangesAsync(cancellationToken);

        return await ReloadTrackDetailAsync(track.Id, cancellationToken);
    }

    private async Task<TrackDetailResponse> ReloadTrackDetailAsync(int trackId, CancellationToken cancellationToken)
    {
        var track = await context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Distributions)
                .ThenInclude(d => d.Dsp)
            .AsNoTracking()
            .FirstAsync(t => t.Id == trackId, cancellationToken);

        return TrackMapper.ToDetail(track);
    }
}
