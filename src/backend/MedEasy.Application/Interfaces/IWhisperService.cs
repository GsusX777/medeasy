// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using MedEasy.Application.DTOs.AI;

namespace MedEasy.Application.Interfaces;

/// <summary>
/// Interface for Whisper transcription service.
/// 
/// [MLB] Multi-Language Bridge via gRPC to Python AI Service
/// [WMM] Whisper Multi-Model support with benchmarking
/// [AIU] Anonymization is mandatory and cannot be disabled
/// [SP] Secure local processing only
/// [ATV] Audit logging for all operations
/// </summary>
public interface IWhisperService
{
    /// <summary>
    /// Transcribe audio using Whisper model.
    /// 
    /// [AIU] Anonymization is mandatory and cannot be disabled
    /// [WMM] Uses specified or default Whisper model
    /// [SP] Local processing only
    /// [ATV] Logs all transcription operations
    /// </summary>
    /// <param name="request">Transcription request with audio data</param>
    /// <returns>Transcribed and anonymized text</returns>
    Task<TranscriptionResponse> TranscribeAsync(TranscriptionRequest request);

    /// <summary>
    /// Get available Whisper models.
    /// 
    /// [WMM] Returns all supported Whisper models
    /// [SP] Local processing only
    /// </summary>
    /// <returns>List of available Whisper models</returns>
    Task<AvailableModelsResponse> GetAvailableModelsAsync();

    /// <summary>
    /// Get hardware information for optimal model selection.
    /// 
    /// [WMM] Hardware-based model recommendations
    /// [PSF] System hardware analysis
    /// </summary>
    /// <returns>Hardware information and model recommendations</returns>
    Task<HardwareInfoResponse> GetHardwareInfoAsync();

    /// <summary>
    /// Benchmark Whisper models performance.
    /// 
    /// [WMM] Performance comparison of all models
    /// [PB] Performance baseline establishment
    /// [ATV] Logs benchmarking operations
    /// </summary>
    /// <param name="request">Benchmark request with test audio</param>
    /// <returns>Benchmark results for all tested models</returns>
    Task<BenchmarkModelsResponse> BenchmarkModelsAsync(BenchmarkModelsRequest request);
}
