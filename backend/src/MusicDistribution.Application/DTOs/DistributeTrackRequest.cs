namespace MusicDistribution.Application.DTOs;

public record DistributeTrackRequest(IReadOnlyList<int> DspIds);
