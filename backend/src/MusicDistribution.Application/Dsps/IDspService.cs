namespace MusicDistribution.Application.Dsps;

public interface IDspService
{
    Task<IReadOnlyList<DspResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}
