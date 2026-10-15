using MusicDistribution.Application.Abstractions.Persistence;

namespace MusicDistribution.Application.Dsps;

public class DspService(IDspRepository dsps) : IDspService
{
    public async Task<IReadOnlyList<DspResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var all = await dsps.GetAllAsync(cancellationToken);
        return all.Select(dsp => new DspResponse(dsp.Id, dsp.Name)).ToList();
    }
}
