namespace MusicDistribution.Domain.Exceptions;

/// <summary>
/// Raised when an operation would break a business rule owned by the domain model
/// (for example submitting a track to the same DSP twice).
/// </summary>
public class DomainException(string message) : Exception(message);
