using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MusicDistribution.Application.Common;
using MusicDistribution.Application.DTOs;
using MusicDistribution.Application.Exceptions;
using MusicDistribution.Application.Interfaces;
using MusicDistribution.Domain.Entities;
using MusicDistribution.Infrastructure.Persistence;

namespace MusicDistribution.Infrastructure.Services;

public class ArtistService(
    AppDbContext context,
    IValidator<CreateArtistRequest> validator) : IArtistService
{
    public async Task<IReadOnlyList<ArtistResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var artists = await context.Artists
            .AsNoTracking()
            .OrderBy(artist => artist.Name)
            .ToListAsync(cancellationToken);

        return artists
            .Select(artist => new ArtistResponse(artist.Id, artist.Name, artist.Email, artist.Country))
            .ToList();
    }

    public async Task<ArtistResponse> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default)
    {
        await ValidationHelper.ValidateAndThrowAsync(validator, request, cancellationToken);

        var email = request.Email.Trim();
        var emailExists = await context.Artists.AnyAsync(artist => artist.Email == email, cancellationToken);
        if (emailExists)
        {
            throw new ConflictException($"An artist with email '{email}' already exists.");
        }

        var artist = new Artist
        {
            Name = request.Name.Trim(),
            Email = email,
            Country = request.Country.Trim()
        };

        context.Artists.Add(artist);
        await context.SaveChangesAsync(cancellationToken);

        return new ArtistResponse(artist.Id, artist.Name, artist.Email, artist.Country);
    }
}
