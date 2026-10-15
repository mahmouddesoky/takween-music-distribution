using FluentValidation.TestHelper;
using MusicDistribution.Application.Artists;
using MusicDistribution.Application.Tracks;

namespace MusicDistribution.Tests.Validators;

public class DistributeTrackRequestValidatorTests
{
    private readonly DistributeTrackRequestValidator _validator = new();

    [Fact]
    public void Rejects_empty_dsp_list()
    {
        var result = _validator.TestValidate(new DistributeTrackRequest([]));
        result.ShouldHaveValidationErrorFor(x => x.DspIds);
    }

    [Fact]
    public void Accepts_single_valid_dsp()
    {
        var result = _validator.TestValidate(new DistributeTrackRequest([1]));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Rejects_non_positive_dsp_id()
    {
        var result = _validator.TestValidate(new DistributeTrackRequest([0]));
        result.ShouldHaveValidationErrorFor("DspIds[0]");
    }
}

public class CreateArtistRequestValidatorTests
{
    private readonly CreateArtistRequestValidator _validator = new();

    [Fact]
    public void Rejects_invalid_email()
    {
        var result = _validator.TestValidate(
            new CreateArtistRequest("Amina", "not-an-email", "Egypt"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Accepts_valid_artist()
    {
        var result = _validator.TestValidate(
            new CreateArtistRequest("Amina", "amina@example.com", "Egypt"));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
