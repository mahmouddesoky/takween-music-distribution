using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistribution.Application.Abstractions.Authentication;
using MusicDistribution.Application.Tracks;

namespace MusicDistribution.Api.Controllers;

/// <summary>
/// HTTP adapter only: binds the request, calls the use case, picks the status code.
/// Validation, business rules and persistence all live behind <see cref="ITrackService"/>.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TracksController(ITrackService trackService) : ControllerBase
{
    /// <summary>List tracks, optionally filtered by artistId, genre and status.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TrackSummaryResponse>>> GetAll(
        [FromQuery] TrackFilterRequest filter,
        CancellationToken cancellationToken) =>
        Ok(await trackService.GetFilteredAsync(filter, cancellationToken));

    /// <summary>Track details including its DSP distribution statuses.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackDetailResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await trackService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TrackDetailResponse>> Create(
        [FromBody] CreateTrackRequest request,
        CancellationToken cancellationToken)
    {
        var track = await trackService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = track.Id }, track);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TrackDetailResponse>> UpdateStatus(
        int id,
        [FromBody] UpdateTrackStatusRequest request,
        CancellationToken cancellationToken) =>
        Ok(await trackService.UpdateStatusAsync(id, request, cancellationToken));

    /// <summary>Submit a track to one or more DSPs.</summary>
    [HttpPost("{id:int}/distribute")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TrackDetailResponse>> Distribute(
        int id,
        [FromBody] DistributeTrackRequest request,
        CancellationToken cancellationToken) =>
        Ok(await trackService.DistributeAsync(id, request, cancellationToken));
}
