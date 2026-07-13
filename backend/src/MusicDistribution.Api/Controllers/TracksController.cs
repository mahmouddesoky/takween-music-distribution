using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistribution.Application.DTOs;
using MusicDistribution.Application.Interfaces;
using MusicDistribution.Domain.Enums;

namespace MusicDistribution.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TracksController(
    ITrackService trackService,
    IDistributionService distributionService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<TrackSummaryResponse>>> GetAll(
        [FromQuery] int? artistId,
        [FromQuery] string? genre,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        TrackStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<TrackStatus>(status, true, out var trackStatus))
            {
                return BadRequest(new { message = "Status must be Draft, Submitted, or Distributed." });
            }

            parsedStatus = trackStatus;
        }

        var tracks = await trackService.GetFilteredAsync(artistId, genre, parsedStatus, cancellationToken);
        return Ok(tracks);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<TrackDetailResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var track = await trackService.GetByIdAsync(id, cancellationToken);
        return Ok(track);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TrackDetailResponse>> Create(
        [FromBody] CreateTrackRequest request,
        CancellationToken cancellationToken)
    {
        var track = await trackService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = track.Id }, track);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TrackDetailResponse>> UpdateStatus(
        int id,
        [FromBody] UpdateTrackStatusRequest request,
        CancellationToken cancellationToken)
    {
        var track = await trackService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(track);
    }

    [HttpPost("{id:int}/distribute")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TrackDetailResponse>> Distribute(
        int id,
        [FromBody] DistributeTrackRequest request,
        CancellationToken cancellationToken)
    {
        var track = await distributionService.DistributeAsync(id, request, cancellationToken);
        return Ok(track);
    }
}
