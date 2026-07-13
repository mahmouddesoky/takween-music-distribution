using Microsoft.EntityFrameworkCore;
using MusicDistribution.Application.DTOs;
using MusicDistribution.Application.Exceptions;
using MusicDistribution.Application.Validators;
using MusicDistribution.Domain.Entities;
using MusicDistribution.Domain.Enums;
using MusicDistribution.Infrastructure.Persistence;
using MusicDistribution.Infrastructure.Services;

namespace MusicDistribution.Tests.Services;

public class DistributionServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new AppDbContext(options);
        ctx.Artists.Add(new Artist { Id = 1, Name = "Amina", Email = "a@x.com", Country = "US" });
        ctx.Dsps.AddRange(
            new Dsp { Id = 1, Name = "Spotify" },
            new Dsp { Id = 2, Name = "Apple Music" });
        ctx.Tracks.Add(new Track
        {
            Id = 1,
            Title = "Draft Track",
            ArtistId = 1,
            Isrc = "USRC1000000",
            ReleaseDate = new DateOnly(2025, 1, 1),
            Genre = "Pop",
            Status = TrackStatus.Draft
        });
        ctx.SaveChanges();
        return ctx;
    }

    private static DistributionService CreateService(AppDbContext ctx) =>
        new(ctx, new DistributeTrackRequestValidator());

    [Fact]
    public async Task DistributeAsync_promotes_draft_to_submitted()
    {
        await using var ctx = CreateContext();
        var svc = CreateService(ctx);

        var result = await svc.DistributeAsync(1, new DistributeTrackRequest([1]));

        Assert.Equal("Submitted", result.Status);
        Assert.Single(result.Distributions);
        Assert.Equal("Spotify", result.Distributions[0].DspName);
    }

    [Fact]
    public async Task DistributeAsync_throws_conflict_when_already_distributed()
    {
        await using var ctx = CreateContext();
        var svc = CreateService(ctx);

        await svc.DistributeAsync(1, new DistributeTrackRequest([1]));

        await Assert.ThrowsAsync<ConflictException>(() =>
            svc.DistributeAsync(1, new DistributeTrackRequest([1])));
    }

    [Fact]
    public async Task DistributeAsync_throws_not_found_for_unknown_dsp()
    {
        await using var ctx = CreateContext();
        var svc = CreateService(ctx);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            svc.DistributeAsync(1, new DistributeTrackRequest([999])));
    }

    [Fact]
    public async Task DistributeAsync_supports_multiple_dsps_at_once()
    {
        await using var ctx = CreateContext();
        var svc = CreateService(ctx);

        var result = await svc.DistributeAsync(1, new DistributeTrackRequest([1, 2]));

        Assert.Equal(2, result.Distributions.Count);
    }
}
