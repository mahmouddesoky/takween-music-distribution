using MusicDistribution.Domain.Entities;
using MusicDistribution.Domain.Enums;
using MusicDistribution.Domain.Exceptions;

namespace MusicDistribution.Tests.Domain;

/// <summary>
/// Pure domain tests: no database, no DI — the business rules live on the entity.
/// </summary>
public class TrackTests
{
    private static readonly DateTime Now = new(2025, 8, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Track NewTrack() =>
        new("Draft", artistId: 1, isrc: "USRC10000000", releaseDate: new DateOnly(2025, 1, 1), genre: "Pop") { Id = 1 };

    private static Track SubmittedTrack()
    {
        var track = NewTrack();
        track.DistributeTo([1], Now);
        return track;
    }

    [Fact]
    public void New_track_starts_as_draft_with_no_distributions()
    {
        var track = NewTrack();

        Assert.Equal(TrackStatus.Draft, track.Status);
        Assert.Empty(track.Distributions);
    }

    [Fact]
    public void DistributeTo_adds_pending_submissions_and_promotes_draft_to_submitted()
    {
        var track = NewTrack();

        track.DistributeTo([1, 2, 2], Now);

        Assert.Equal(TrackStatus.Submitted, track.Status);
        Assert.Equal(2, track.Distributions.Count);
        Assert.All(track.Distributions, d =>
        {
            Assert.Equal(DistributionStatus.Pending, d.Status);
            Assert.Equal(Now, d.SubmittedAt);
        });
    }

    [Fact]
    public void DistributeTo_rejects_an_empty_dsp_list()
    {
        var track = NewTrack();

        Assert.Throws<DomainException>(() => track.DistributeTo([], Now));
        Assert.Equal(TrackStatus.Draft, track.Status);
    }

    [Fact]
    public void DistributeTo_keeps_a_distributed_track_distributed()
    {
        var track = SubmittedTrack();
        track.ChangeStatus(TrackStatus.Distributed);

        track.DistributeTo([2], Now);

        Assert.Equal(TrackStatus.Distributed, track.Status);
    }

    [Fact]
    public void DistributeTo_rejects_a_dsp_the_track_was_already_submitted_to()
    {
        var track = SubmittedTrack();

        var error = Assert.Throws<DomainException>(() => track.DistributeTo([2, 1], Now));

        Assert.Contains("1", error.Message);
        Assert.Single(track.Distributions);
    }

    [Theory]
    [InlineData(TrackStatus.Submitted)]
    [InlineData(TrackStatus.Distributed)]
    public void ChangeStatus_forward_requires_at_least_one_submission(TrackStatus target)
    {
        var track = NewTrack();

        Assert.Throws<DomainException>(() => track.ChangeStatus(target));
    }

    [Fact]
    public void ChangeStatus_submitted_to_distributed_is_allowed()
    {
        var track = SubmittedTrack();

        track.ChangeStatus(TrackStatus.Distributed);

        Assert.Equal(TrackStatus.Distributed, track.Status);
    }

    [Theory]
    [InlineData(TrackStatus.Submitted, TrackStatus.Draft)]
    [InlineData(TrackStatus.Distributed, TrackStatus.Draft)]
    [InlineData(TrackStatus.Distributed, TrackStatus.Submitted)]
    public void ChangeStatus_never_moves_backwards(TrackStatus from, TrackStatus to)
    {
        var track = SubmittedTrack();
        track.ChangeStatus(from);

        Assert.Throws<DomainException>(() => track.ChangeStatus(to));
        Assert.Equal(from, track.Status);
    }

    [Fact]
    public void ChangeStatus_to_the_current_status_is_a_no_op()
    {
        var track = NewTrack();

        track.ChangeStatus(TrackStatus.Draft);

        Assert.Equal(TrackStatus.Draft, track.Status);
    }
}
