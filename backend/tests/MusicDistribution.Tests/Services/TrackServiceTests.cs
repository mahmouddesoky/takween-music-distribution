using Microsoft.EntityFrameworkCore;
using MusicDistribution.Application.DTOs;
using MusicDistribution.Application.Exceptions;
using MusicDistribution.Application.Validators;
using MusicDistribution.Domain.Entities;
using MusicDistribution.Domain.Enums;
using MusicDistribution.Infrastructure.Persistence;
using MusicDistribution.Infrastructure.Services;

namespace MusicDistribution.Tests.Services;

public class TrackServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new AppDbContext(options);
        Seed(ctx);
        return ctx;
    }

    private static void Seed(AppDbContext ctx)
    {
        ctx.Artists.Add(new Artist { Id = 1, Name = "Amina", Email = "a@x.com", Country = "US" });
        ctx.Dsps.Add(new Dsp { Id = 1, Name = "Spotify" });
        ctx.Dsps.Add(new Dsp { Id = 2, Name = "Apple Music" });
        ctx.Tracks.Add(new Track
        {
            Id = 1,
            Title = "Existing",
            ArtistId = 1,
            Isrc = "USRC1000000",
            ReleaseDate = new DateOnly(2025, 1, 1),
            Genre = "Pop",
            Status = TrackStatus.Draft
        });
        ctx.SaveChanges();
    }

    private static TrackService CreateService(AppDbContext ctx) => new(
        ctx,
        new CreateTrackRequestValidator(),
        new UpdateTrackStatusRequestValidator());

    [Fact]
    public async Task CreateAsync_normalises_isrc_to_uppercase()
    {
        await using var ctx = CreateContext();
        var svc = CreateService(ctx);

        var result = await svc.CreateAsync(new CreateTrackRequest(
            "New Track", 1, "usrc2000000", new DateOnly(2025, 6, 1), "Rock", null));

        Assert.Equal("USRC2000000", result.Isrc);
        Assert.Equal("Draft", result.Status);
    }

    [Fact]
    public async Task CreateAsync_throws_conflict_on_duplicate_isrc()
    {
        await using var ctx = CreateContext();
        var svc = CreateService(ctx);

        await Assert.ThrowsAsync<ConflictException>(() =>
            svc.CreateAsync(new CreateTrackRequest(
                "Dup", 1, "USRC1000000", new DateOnly(2025, 6, 1), "Rock", null)));
    }

    [Fact]
    public async Task CreateAsync_throws_not_found_for_missing_artist()
    {
        await using var ctx = CreateContext();
        var svc = CreateService(ctx);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            svc.CreateAsync(new CreateTrackRequest(
                "Orphan", 999, "USRC9999999", new DateOnly(2025, 6, 1), "Rock", null)));
    }

    [Fact]
    public async Task GetFilteredAsync_filters_by_genre_case_insensitively()
    {
        await using var ctx = CreateContext();
        var svc = CreateService(ctx);

        await svc.CreateAsync(new CreateTrackRequest(
            "Afro", 1, "USRC3000000", new DateOnly(2025, 6, 1), "Afrobeats", null));

        var pop = await svc.GetFilteredAsync(null, "pop", null);
        var afro = await svc.GetFilteredAsync(null, "AFROBEATS", null);

        Assert.Single(pop);
        Assert.Single(afro);
    }

    [Fact]
    public async Task UpdateStatusAsync_changes_status()
    {
        await using var ctx = CreateContext();
        var svc = CreateService(ctx);

        var result = await svc.UpdateStatusAsync(1, new UpdateTrackStatusRequest("Distributed"));

        Assert.Equal("Distributed", result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_throws_not_found_for_missing_track()
    {
        await using var ctx = CreateContext();
        var svc = CreateService(ctx);

        await Assert.ThrowsAsync<NotFoundException>(() => svc.GetByIdAsync(999));
    }
}
