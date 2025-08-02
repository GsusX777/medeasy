// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

namespace MedEasy.Application.DTOs.AI;

/// <summary>
/// Request for audio transcription using Whisper.
/// 
/// [AIU] Anonymization is mandatory and cannot be disabled
/// [SP] Secure audio processing
/// [WMM] Whisper Multi-Model support
/// </summary>
public class TranscriptionRequest
{
    public required string RequestId { get; init; } = Guid.NewGuid().ToString();
    public required byte[] AudioData { get; init; }
    public string? AudioFormat { get; init; } = "wav";
    public string? Language { get; init; } = "auto";
    public string? WhisperModel { get; init; } = "small";
    public string? SessionId { get; init; }
    public bool DetectSwissGerman { get; init; } = true;
    public string? ConsultationContext { get; init; }
}

/// <summary>
/// Response from audio transcription.
/// 
/// [AIU] Contains anonymized text only
/// [ATV] Includes audit information
/// </summary>
public class TranscriptionResponse
{
    public required string RequestId { get; init; }
    public required string Text { get; init; }
    public required string OriginalText { get; init; }
    public required string LanguageCode { get; init; }
    public bool IsSwissGerman { get; init; }
    public bool SwissGermanWarning { get; init; }
    public double ProcessingTimeSeconds { get; init; }
    public int EntitiesFound { get; init; }
    public bool CloudProcessed { get; init; } = false; // Always false [SP]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Request for benchmarking Whisper models.
/// 
/// [WMM] Performance comparison request
/// [PB] Performance baseline establishment
/// </summary>
public class BenchmarkModelsRequest
{
    public required string RequestId { get; init; } = Guid.NewGuid().ToString();
    public required byte[] TestAudioData { get; init; }
    public string AudioFormat { get; init; } = "wav";
    public string AudioFilename { get; init; } = "unknown.m4a";  // Original filename for tracking
    public string Language { get; init; } = "auto";
    public int Iterations { get; init; } = 1;
    public List<string> ModelsToTest { get; init; } = ["base", "small", "medium", "large-v3"];
    public bool EnableCudaBenchmark { get; init; } = false;
    public bool EnableCpuBenchmark { get; init; } = true;
    public bool EnableSwissGermanDetection { get; init; } = true;
}

/// <summary>
/// Response from model benchmarking.
/// 
/// [WMM] Performance comparison results
/// [PB] Performance baseline data
/// </summary>
public class BenchmarkModelsResponse
{
    public required string RequestId { get; init; }
    public required string BenchmarkId { get; init; }
    public long TotalBenchmarkTimeMs { get; init; }
    public required List<ModelBenchmarkResult> Results { get; init; }
    public required HardwareInfo HardwareInfo { get; init; }
    public required ModelRecommendation Recommendation { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Individual model benchmark result.
/// 
/// [WMM] Single model performance metrics
/// </summary>
public class ModelBenchmarkResult
{
    public required string ModelName { get; init; }
    public long AverageProcessingTimeMs { get; init; }
    public long MinProcessingTimeMs { get; init; }
    public long MaxProcessingTimeMs { get; init; }
    public double AverageAccuracy { get; init; }
    public double AverageConfidence { get; init; }
    public bool CudaUsed { get; init; }
    public double AverageCpuUsage { get; init; }
    public double AverageGpuUsage { get; init; }
    public long AverageRamUsageMb { get; init; }
    public long AverageVramUsageMb { get; init; }
    public int PerformanceScore { get; init; }
    public int AccuracyScore { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? TranscribedText { get; init; }  // [NEW] Transcribed text for comparison and display
}

/// <summary>
/// Request for available Whisper models.
/// 
/// [WMM] Model information request
/// </summary>
public class AvailableModelsRequest
{
    public required string RequestId { get; init; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Response with available Whisper models.
/// 
/// [WMM] Available models with details
/// </summary>
public class AvailableModelsResponse
{
    public required string RequestId { get; init; }
    public required List<WhisperModelInfo> Models { get; init; }
    public required string RecommendedModel { get; init; }
    public required HardwareInfo HardwareInfo { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Whisper model information.
/// 
/// [WMM] Individual model details
/// </summary>
public class WhisperModelInfo
{
    public required string Name { get; init; }
    public int SizeMb { get; init; }
    public List<string> SupportedLanguages { get; init; } = [];
    public double RecommendedVramGb { get; init; }
    public double RecommendedRamGb { get; init; }
    public int PerformanceScore { get; init; }
    public int AccuracyScore { get; init; }
    public bool IsAvailable { get; init; }
    public bool IsDownloaded { get; init; }
    public string? LocalPath { get; init; }
    public double EstimatedSpeedFactor { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Request for hardware information.
/// 
/// [WMM][PSF] Hardware analysis request
/// </summary>
public class HardwareInfoRequest
{
    public required string RequestId { get; init; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Response with hardware information.
/// 
/// [WMM][PSF] Detailed hardware information
/// </summary>
public class HardwareInfoResponse
{
    public required string RequestId { get; init; }
    public bool CudaAvailable { get; init; }
    public string? CudaVersion { get; init; }
    public double VramGb { get; init; }
    public double RamGb { get; init; }
    public required CpuInfo CpuInfo { get; init; }
    public GpuInfo? GpuInfo { get; init; }
    public List<ModelRecommendationForHardware> ModelRecommendations { get; init; } = [];
    public List<ModelPerformanceEstimate> PerformanceEstimates { get; init; } = [];
    public List<string> HardwareLimitations { get; init; } = [];
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Hardware information for model selection.
/// 
/// [PSF] System hardware details
/// </summary>
public class HardwareInfo
{
    public bool CudaAvailable { get; init; }
    public double VramGb { get; init; }
    public double RamGb { get; init; }
    public string? CpuInfo { get; init; }
    public string? GpuInfo { get; init; }
    public int CpuCores { get; init; }
}

/// <summary>
/// CPU information.
/// 
/// [PSF] Detailed CPU specs
/// </summary>
public class CpuInfo
{
    public string? Model { get; init; }
    public int PhysicalCores { get; init; }
    public int LogicalCores { get; init; }
    public double BaseFrequencyGhz { get; init; }
    public double MaxFrequencyGhz { get; init; }
    public string? Architecture { get; init; }
}

/// <summary>
/// GPU information.
/// 
/// [PSF] Detailed GPU specs
/// </summary>
public class GpuInfo
{
    public string? Model { get; init; }
    public string? Vendor { get; init; }
    public double TotalVramGb { get; init; }
    public double AvailableVramGb { get; init; }
    public string? ComputeCapability { get; init; }
    public int CudaCores { get; init; }
    public string? DriverVersion { get; init; }
}

/// <summary>
/// Model recommendation based on hardware.
/// 
/// [WMM] Hardware-based model suggestions
/// </summary>
public class ModelRecommendation
{
    public string? RecommendedForPerformance { get; init; }
    public string? RecommendedForAccuracy { get; init; }
    public string? RecommendedForBalance { get; init; }
    public string? Reasoning { get; init; }
    public List<string> HardwareLimitations { get; init; } = [];
}

/// <summary>
/// Model recommendation for specific hardware.
/// 
/// [WMM] Hardware-specific model advice
/// </summary>
public class ModelRecommendationForHardware
{
    public required string ModelName { get; init; }
    public required string Reason { get; init; }
    public int RecommendationScore { get; init; }
    public string? PerformanceCategory { get; init; }
    public List<string> PotentialIssues { get; init; } = [];
}

/// <summary>
/// Model performance estimate for hardware.
/// 
/// [PB] Estimated performance for hardware
/// </summary>
public class ModelPerformanceEstimate
{
    public required string ModelName { get; init; }
    public double EstimatedSpeedFactor { get; init; }
    public long EstimatedRamUsageMb { get; init; }
    public long EstimatedVramUsageMb { get; init; }
    public bool CanRun { get; init; }
    public string? RecommendedMode { get; init; }
    public List<string> Warnings { get; init; } = [];
}
