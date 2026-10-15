using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistribution.Application.Dsps;

namespace MusicDistribution.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DspsController(IDspService dspService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<DspResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await dspService.GetAllAsync(cancellationToken));
}
