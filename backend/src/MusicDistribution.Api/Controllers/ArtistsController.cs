using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistribution.Application.DTOs;
using MusicDistribution.Application.Interfaces;

namespace MusicDistribution.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistsController(IArtistService artistService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ArtistResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var artists = await artistService.GetAllAsync(cancellationToken);
        return Ok(artists);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ArtistResponse>> Create(
        [FromBody] CreateArtistRequest request,
        CancellationToken cancellationToken)
    {
        var artist = await artistService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = artist.Id }, artist);
    }
}
