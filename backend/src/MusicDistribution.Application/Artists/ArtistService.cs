using FluentValidation;
using MusicDistribution.Application.Abstractions.Persistence;
using MusicDistribution.Application.Common.Exceptions;
using MusicDistribution.Application.Common.Validation;
using MusicDistribution.Domain.Entities;

namespace MusicDistribution.Application.Artists;

public class ArtistService(
    IArtistRepository artists,
    IUnitOfWork unitOfWork,
    IValidator<CreateArtistRequest> validator) : IArtistService
{
    public async Task<IReadOnlyList<ArtistResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var all = await artists.GetAllAsync(cancellationToken);
        return all.Select(ToResponse).ToList();
    }

    public async Task<ArtistResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var artist = await artists.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Artist with id {id} was not found.");

        return ToResponse(artist);
    }

    public async Task<ArtistResponse> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default)
    {
        await ValidationHelper.ValidateAndThrowAsync(validator, request, cancellationToken);

        var email = request.Email.Trim().ToLowerInvariant();
        if (await artists.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException($"An artist with email '{email}' already exists.");
        }

        var artist = new Artist
        {
            Name = request.Name.Trim(),
            Email = email,
            Country = request.Country.Trim()
        };

        artists.Add(artist);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(artist);
    }

    private static ArtistResponse ToResponse(Artist artist) =>
        new(artist.Id, artist.Name, artist.Email, artist.Country);
}
