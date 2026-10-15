using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MusicDistribution.Application.Common.Exceptions;
using MusicDistribution.Application.Tracks;
using MusicDistribution.Domain.Entities;
using MusicDistribution.Domain.Exceptions;
using MusicDistribution.Infrastructure.Persistence;
using MusicDistribution.Infrastructure.Persistence.Repositories;

namespace MusicDistribution.Tests.Services;

/// <summary>
/// Application use-case tests wired to the real repositories over an in-memory SQLite
/// database, so unique indexes and case rules behave like production.
/// </summary>
public sealed class TrackServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly TrackService _service;

    public TrackServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();
        Seed(_context);

        _service = new TrackService(
            new TrackRepository(_context),
            new ArtistRepository(_context),
            new DspRepository(_context),
            new UnitOfWork(_context),
            TimeProvider.System,
            new TrackFilterRequestValidator(),
            new CreateTrackRequestValidator(),
            new UpdateTrackStatusRequestValidator(),
            new DistributeTrackRequestValidator());
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private static void Seed(AppDbContext context)
    {
        context.Artists.Add(new Artist { Id = 1, Name = "Amina", Email = "a@x.com", Country = "US" });
        context.Dsps.AddRange(new Dsp { Id = 1, Name = "Spotify" }, new Dsp { Id = 2, Name = "Apple Music" });
        context.Tracks.Add(new Track("Existing", 1, "USRC10000000", new DateOnly(2025, 1, 1), "Pop") { Id = 1 });
        context.SaveChanges();
        context.ChangeTracker.Clear();
    }

    private static CreateTrackRequest NewTrack(string isrc, string genre = "Rock", int artistId = 1) =>
        new("New Track", artistId, isrc, new DateOnly(2025, 6, 1), genre);

    [Fact]
    public async Task CreateAsync_normalises_isrc_to_uppercase_and_defaults_to_draft()
    {
        var result = await _service.CreateAsync(NewTrack("usrc20000000"));

        Assert.Equal("USRC20000000", result.Isrc);
        Assert.Equal("Draft", result.Status);
        Assert.Equal("Amina", result.ArtistName);
    }

    [Fact]
    public async Task CreateAsync_throws_conflict_on_duplicate_isrc()
    {
        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(NewTrack("usrc10000000")));
    }

    [Fact]
    public async Task CreateAsync_throws_not_found_for_missing_artist()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(NewTrack("USRC99999999", artistId: 999)));
    }

    [Fact]
    public async Task CreateAsync_throws_validation_for_bad_input()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(NewTrack("BAD")));

        Assert.Contains("Isrc", error.Errors.Keys);
    }

    [Fact]
    public async Task GetFilteredAsync_filters_by_genre_case_insensitively()
    {
        await _service.CreateAsync(NewTrack("USRC30000000", genre: "Afrobeats"));

        var pop = await _service.GetFilteredAsync(new TrackFilterRequest(Genre: "pop"));
        var afro = await _service.GetFilteredAsync(new TrackFilterRequest(Genre: "AFROBEATS"));

        Assert.Single(pop);
        Assert.Single(afro);
    }

    [Fact]
    public async Task GetFilteredAsync_filters_by_status_and_artist()
    {
        var drafts = await _service.GetFilteredAsync(new TrackFilterRequest(ArtistId: 1, Status: "draft"));
        var submitted = await _service.GetFilteredAsync(new TrackFilterRequest(Status: "Submitted"));

        Assert.Single(drafts);
        Assert.Empty(submitted);
    }

    [Fact]
    public async Task GetFilteredAsync_rejects_unknown_status()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GetFilteredAsync(new TrackFilterRequest(Status: "Published")));
    }

    [Fact]
    public async Task GetByIdAsync_throws_not_found_for_missing_track()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(999));
    }

    [Fact]
    public async Task DistributeAsync_promotes_draft_to_submitted_and_returns_dsp_names()
    {
        var result = await _service.DistributeAsync(1, new DistributeTrackRequest([2, 1, 1]));

        Assert.Equal("Submitted", result.Status);
        Assert.Equal(["Apple Music", "Spotify"], result.Distributions.Select(d => d.DspName));
        Assert.All(result.Distributions, d => Assert.Equal("Pending", d.Status));
    }

    [Fact]
    public async Task DistributeAsync_rejects_a_repeat_submission_to_the_same_dsp()
    {
        await _service.DistributeAsync(1, new DistributeTrackRequest([1]));

        await Assert.ThrowsAsync<DomainException>(() => _service.DistributeAsync(1, new DistributeTrackRequest([1])));
    }

    [Fact]
    public async Task DistributeAsync_throws_not_found_for_unknown_dsp()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DistributeAsync(1, new DistributeTrackRequest([1, 999])));

        Assert.Contains("999", error.Message);
    }

    [Fact]
    public async Task UpdateStatusAsync_changes_status_after_distribution()
    {
        await _service.DistributeAsync(1, new DistributeTrackRequest([1]));

        var result = await _service.UpdateStatusAsync(1, new UpdateTrackStatusRequest("Distributed"));

        Assert.Equal("Distributed", result.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_enforces_domain_rules()
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            _service.UpdateStatusAsync(1, new UpdateTrackStatusRequest("Distributed")));
    }

    [Fact]
    public async Task UnitOfWork_turns_a_unique_index_race_into_a_conflict()
    {
        // Two requests that both passed the ISRC pre-check: the second insert hits the unique index.
        await using var other = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _context.Tracks.Add(new Track("First", 1, "USRC40000000", new DateOnly(2025, 1, 1), "Pop"));
        other.Tracks.Add(new Track("Second", 1, "USRC40000000", new DateOnly(2025, 1, 1), "Pop"));

        await new UnitOfWork(_context).SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => new UnitOfWork(other).SaveChangesAsync());
    }
}
