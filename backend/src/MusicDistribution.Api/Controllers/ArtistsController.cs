using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistribution.Application.Abstractions.Authentication;
using MusicDistribution.Application.Artists;

namespace MusicDistribution.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ArtistsController(IArtistService artistService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ArtistResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await artistService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArtistResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await artistService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArtistResponse>> Create(
        [FromBody] CreateArtistRequest request,
        CancellationToken cancellationToken)
    {
        var artist = await artistService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = artist.Id }, artist);
    }
}
