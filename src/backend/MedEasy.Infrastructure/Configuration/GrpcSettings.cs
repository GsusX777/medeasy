// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

namespace MedEasy.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for gRPC communication with Python AI Service.
/// 
/// [MLB] Multi-Language Bridge configuration
/// [SP] Secure communication settings
/// </summary>
public class GrpcSettings
{
    public const string SectionName = "Grpc";

    /// <summary>
    /// URL of the Python AI Service gRPC endpoint.
    /// Default: http://localhost:50051
    /// </summary>
    public string ServiceUrl { get; set; } = "http://localhost:50051";

    /// <summary>
    /// Connection timeout in seconds.
    /// Default: 30 seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum message size for gRPC communication (in bytes).
    /// Default: 100MB for large audio files
    /// </summary>
    public int MaxMessageSize { get; set; } = 100 * 1024 * 1024;

    /// <summary>
    /// Number of retry attempts for failed gRPC calls.
    /// Default: 3 retries
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Enable detailed gRPC logging for debugging.
    /// Default: false (only enable in development)
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
}
