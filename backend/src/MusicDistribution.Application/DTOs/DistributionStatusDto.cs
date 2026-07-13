namespace MusicDistribution.Application.DTOs;

public record DistributionStatusDto(
    int DspId,
    string DspName,
    string Status,
    DateTime SubmittedAt);
