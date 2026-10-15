using FluentValidation.TestHelper;
using MusicDistribution.Application.Tracks;

namespace MusicDistribution.Tests.Validators;

public class CreateTrackRequestValidatorTests
{
    private readonly CreateTrackRequestValidator _validator = new();

    private static CreateTrackRequest ValidRequest() => new(
        Title: "Midnight Echo",
        ArtistId: 1,
        Isrc: "USRC12345678",
        ReleaseDate: new DateOnly(2025, 1, 1),
        Genre: "Pop");

    [Fact]
    public void Accepts_a_well_formed_request()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Rejects_empty_title()
    {
        var request = ValidRequest() with { Title = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Rejects_non_positive_artist_id()
    {
        var request = ValidRequest() with { ArtistId = 0 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ArtistId);
    }

    [Theory]
    [InlineData("BAD")]
    [InlineData("TOOLONG1234567")]
    [InlineData("12RC1234567")]
    [InlineData("USRC1234567")]   // 11 chars: one digit short of a real ISRC
    public void Rejects_malformed_isrc(string isrc)
    {
        var request = ValidRequest() with { Isrc = isrc };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Isrc);
    }
}
