// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MedEasy.Application.DTOs.AI;
using MedEasy.Application.Interfaces;
using MedEasy.Infrastructure.Configuration;

namespace MedEasy.Infrastructure.Services;

/// <summary>
/// Whisper service implementation using gRPC communication with Python AI Service.
/// 
/// [MLB] Multi-Language Bridge via gRPC to Python AI Service
/// [WMM] Whisper Multi-Model support with benchmarking
/// [AIU] Anonymization is mandatory and cannot be disabled
/// [SP] Secure local processing only
/// [ATV] Audit logging for all operations
/// </summary>
public class WhisperService : IWhisperService
{
    private readonly GrpcChannel _channel;
    private readonly Medeasy.MedEasyService.MedEasyServiceClient _grpcClient;
    private readonly ILogger<WhisperService> _logger;
    private readonly GrpcSettings _grpcSettings;

    public WhisperService(
        IOptions<GrpcSettings> grpcSettings,
        ILogger<WhisperService> logger)
    {
        _grpcSettings = grpcSettings.Value;
        _logger = logger;

        // Create gRPC channel to Python AI Service
        var channelOptions = new GrpcChannelOptions
        {
            MaxReceiveMessageSize = 100 * 1024 * 1024, // 100MB for large audio files
            MaxSendMessageSize = 100 * 1024 * 1024,    // 100MB for large audio files
        };

        _channel = GrpcChannel.ForAddress(_grpcSettings.ServiceUrl, channelOptions);
        _grpcClient = new Medeasy.MedEasyService.MedEasyServiceClient(_channel);

        _logger.LogInformation("WhisperService initialized with gRPC endpoint: {ServiceUrl}", 
            _grpcSettings.ServiceUrl);
    }

    /// <summary>
    /// Transcribe audio using Whisper model via gRPC.
    /// 
    /// [AIU] Anonymization is mandatory and cannot be disabled
    /// [MLB] Uses gRPC bridge to Python AI Service
    /// [ATV] Logs all transcription operations
    /// </summary>
    public async Task<TranscriptionResponse> TranscribeAsync(TranscriptionRequest request)
    {
        try
        {
            _logger.LogInformation("Starting audio transcription via gRPC: RequestId={RequestId}, Model={Model}, AudioSize={AudioSize}",
                request.RequestId, request.WhisperModel, request.AudioData.Length);

            // Create gRPC request
            var grpcRequest = new Medeasy.TranscriptionRequest
            {
                RequestId = request.RequestId,
                AudioData = Google.Protobuf.ByteString.CopyFrom(request.AudioData),
                AudioFormat = request.AudioFormat ?? "wav",
                AudioLengthSeconds = 0, // Will be calculated by Python service
                LanguageCode = request.Language ?? "auto",
                SessionId = request.SessionId ?? "",
                DetectSwissGerman = request.DetectSwissGerman,
                AllowCloudProcessing = false, // Always false [SP]
                ConsultationContext = request.ConsultationContext ?? ""
            };

            // Call Python AI Service via gRPC
            var grpcResponse = await _grpcClient.TranscribeAsync(grpcRequest);

            _logger.LogInformation("Audio transcription completed via gRPC: RequestId={RequestId}, TextLength={TextLength}, ProcessingTime={ProcessingTime}s",
                grpcResponse.RequestId, grpcResponse.Text.Length, grpcResponse.ProcessingTimeSeconds);

            // Convert gRPC response to DTO
            return new TranscriptionResponse
            {
                RequestId = grpcResponse.RequestId,
                Text = grpcResponse.Text,
                OriginalText = grpcResponse.OriginalText,
                LanguageCode = grpcResponse.LanguageCode,
                IsSwissGerman = grpcResponse.IsSwissGerman,
                SwissGermanWarning = grpcResponse.SwissGermanWarning,
                ProcessingTimeSeconds = grpcResponse.ProcessingTimeSeconds,
                EntitiesFound = grpcResponse.DetectedEntities.Count,
                CloudProcessed = false // Always false [SP]
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transcribe audio via gRPC: RequestId={RequestId}", request.RequestId);
            throw;
        }
    }

    /// <summary>
    /// Get available Whisper models via gRPC.
    /// 
    /// [WMM] Returns all supported Whisper models
    /// [MLB] Uses gRPC bridge to Python AI Service
    /// </summary>
    public async Task<AvailableModelsResponse> GetAvailableModelsAsync()
    {
        try
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Getting available Whisper models via gRPC: RequestId={RequestId}", requestId);

            // Create gRPC request
            var grpcRequest = new Medeasy.GetAvailableModelsRequest
            {
                RequestId = requestId
            };

            // Call Python AI Service via gRPC with timeout
            _logger.LogInformation("Making gRPC call to GetAvailableModelsAsync: RequestId={RequestId}", requestId);
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // 10 second timeout
            var grpcResponse = await _grpcClient.GetAvailableModelsAsync(grpcRequest, cancellationToken: cts.Token);
            
            _logger.LogInformation("gRPC call completed successfully: RequestId={RequestId}", requestId);

            _logger.LogInformation("Retrieved {ModelCount} available models via gRPC: RequestId={RequestId}",
                grpcResponse.Models.Count, grpcResponse.RequestId);

            // Convert gRPC response to DTO - EXTENDED SCHEMA [WMM]
            var models = grpcResponse.Models.Select(m => new WhisperModelInfo
            {
                Name = m.Name,
                SizeMb = m.SizeMb, // Real value from protobuf
                SupportedLanguages = m.SupportedLanguages.ToList(), // Real value from protobuf
                RecommendedVramGb = 0, // Default value - not in minimal schema
                RecommendedRamGb = m.RecommendedRamGb, // Real value from protobuf
                PerformanceScore = 0, // Default value - not in minimal schema
                AccuracyScore = 0, // Default value - not in minimal schema
                IsAvailable = m.IsAvailable,
                IsDownloaded = m.IsDownloaded,
                LocalPath = "", // Default value - not in minimal schema
                EstimatedSpeedFactor = m.EstimatedSpeedFactor, // Real value from protobuf
                Description = m.Description // Real value from protobuf
            }).ToList();

            return new AvailableModelsResponse
            {
                RequestId = grpcResponse.RequestId,
                Models = models,
                RecommendedModel = grpcResponse.RecommendedModel,
                HardwareInfo = await MapHardwareInfoFromResponse() // Use real hardware info from separate call
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available models via gRPC");
            throw;
        }
    }

    /// <summary>
    /// Get hardware information via gRPC.
    /// 
    /// [WMM] Hardware-based model recommendations
    /// [MLB] Uses gRPC bridge to Python AI Service
    /// </summary>
    public async Task<HardwareInfoResponse> GetHardwareInfoAsync()
    {
        try
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Getting hardware information via gRPC: RequestId={RequestId}", requestId);

            // Create gRPC request
            var grpcRequest = new Medeasy.GetHardwareInfoRequest
            {
                RequestId = requestId
            };

            // Call Python AI Service via gRPC
            var grpcResponse = await _grpcClient.GetHardwareInfoAsync(grpcRequest);

            _logger.LogInformation("Retrieved hardware info via gRPC: RequestId={RequestId}, CudaAvailable={CudaAvailable}, RamGb={RamGb}",
                grpcResponse.RequestId, grpcResponse.CudaAvailable, grpcResponse.RamGb);

            // Convert gRPC response to DTO - MINIMAL SCHEMA [PSF]
            return new HardwareInfoResponse
            {
                RequestId = grpcResponse.RequestId,
                CudaAvailable = grpcResponse.CudaAvailable,
                CudaVersion = "", // Default value - field removed from protobuf
                VramGb = 0, // Default value - field removed from protobuf
                RamGb = grpcResponse.RamGb,
                CpuInfo = new CpuInfo
                {
                    Model = null, // Default value - field removed from protobuf
                    PhysicalCores = grpcResponse.CpuInfo?.PhysicalCores ?? 0,
                    LogicalCores = grpcResponse.CpuInfo?.LogicalCores ?? 0,
                    BaseFrequencyGhz = 0, // Default value - field removed from protobuf
                    MaxFrequencyGhz = 0, // Default value - field removed from protobuf
                    Architecture = null // Default value - field removed from protobuf
                },
                GpuInfo = null, // Default value - field removed from protobuf
                ModelRecommendations = new List<ModelRecommendationForHardware>(), // Empty list - field removed from protobuf
                PerformanceEstimates = new List<ModelPerformanceEstimate>(), // Empty list - field removed from protobuf
                HardwareLimitations = new List<string>() // Empty list - field removed from protobuf
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hardware info via gRPC");
            throw;
        }
    }

    /// <summary>
    /// Benchmark Whisper models via gRPC.
    /// 
    /// [WMM] Performance comparison of all models
    /// [MLB] Uses gRPC bridge to Python AI Service
    /// [ATV] Logs benchmarking operations
    /// </summary>
    public async Task<BenchmarkModelsResponse> BenchmarkModelsAsync(BenchmarkModelsRequest request)
    {
        try
        {
            _logger.LogInformation("Starting model benchmarking via gRPC: RequestId={RequestId}, ModelsCount={ModelsCount}, AudioSize={AudioSize}",
                request.RequestId, request.ModelsToTest.Count, request.TestAudioData.Length);

            // Create gRPC request
            var grpcRequest = new Medeasy.BenchmarkModelsRequest
            {
                RequestId = request.RequestId,
                TestAudioData = Google.Protobuf.ByteString.CopyFrom(request.TestAudioData),
                AudioFormat = request.AudioFormat,
                AudioFilename = request.AudioFilename,  // Pass original filename
                Language = request.Language,
                Iterations = request.Iterations,
                EnableCudaBenchmark = request.EnableCudaBenchmark,
                EnableCpuBenchmark = request.EnableCpuBenchmark,
                EnableSwissGermanDetection = request.EnableSwissGermanDetection
            };

            // Add models to test
            grpcRequest.ModelsToTest.AddRange(request.ModelsToTest);

            // Call Python AI Service via gRPC
            var grpcResponse = await _grpcClient.BenchmarkModelsAsync(grpcRequest);

            _logger.LogInformation("Model benchmarking completed via gRPC: RequestId={RequestId}, ResultsCount={ResultsCount}, TotalTime={TotalTime}ms",
                grpcResponse.RequestId, grpcResponse.Results.Count, grpcResponse.TotalBenchmarkTimeMs);

            // Convert gRPC response to DTO
            var results = grpcResponse.Results.Select(r => new ModelBenchmarkResult
            {
                ModelName = r.ModelName,
                AverageProcessingTimeMs = r.AverageProcessingTimeMs,
                MinProcessingTimeMs = r.MinProcessingTimeMs,
                MaxProcessingTimeMs = r.MaxProcessingTimeMs,
                AverageAccuracy = r.AverageAccuracy,
                AverageConfidence = r.AverageConfidence,
                CudaUsed = r.CudaUsed,
                AverageCpuUsage = r.AverageCpuUsage,
                AverageGpuUsage = r.AverageGpuUsage,
                AverageRamUsageMb = r.AverageRamUsageMb,
                AverageVramUsageMb = r.AverageVramUsageMb,
                PerformanceScore = r.PerformanceScore,
                AccuracyScore = r.AccuracyScore,
                Success = r.Success,
                ErrorMessage = r.ErrorMessage,
                TranscribedText = r.TranscribedText  // [NEW] Extract transcribed text from gRPC
            }).ToList();

            return new BenchmarkModelsResponse
            {
                RequestId = grpcResponse.RequestId,
                BenchmarkId = grpcResponse.BenchmarkId,
                TotalBenchmarkTimeMs = grpcResponse.TotalBenchmarkTimeMs,
                Results = results,
                HardwareInfo = new HardwareInfo
                {
                    CudaAvailable = grpcResponse.HardwareInfo?.CudaAvailable ?? false,
                    VramGb = grpcResponse.HardwareInfo?.VramGb ?? 0,
                    RamGb = grpcResponse.HardwareInfo?.RamGb ?? 0,
                    CpuCores = grpcResponse.HardwareInfo?.CpuCores ?? 0
                },
                Recommendation = new ModelRecommendation
                {
                    RecommendedForPerformance = grpcResponse.Recommendation?.RecommendedForPerformance,
                    RecommendedForAccuracy = grpcResponse.Recommendation?.RecommendedForAccuracy,
                    RecommendedForBalance = grpcResponse.Recommendation?.RecommendedForBalance,
                    Reasoning = grpcResponse.Recommendation?.Reasoning,
                    HardwareLimitations = grpcResponse.Recommendation?.HardwareLimitations.ToList() ?? []
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to benchmark models via gRPC: RequestId={RequestId}", request.RequestId);
            throw;
        }
    }

    /// <summary>
    /// Maps HardwareInfoResponse to HardwareInfo for GetAvailableModels.
    /// </summary>
    private async Task<HardwareInfo> MapHardwareInfoFromResponse()
    {
        var hardwareResponse = await GetHardwareInfoAsync();
        return new HardwareInfo
        {
            CudaAvailable = hardwareResponse.CudaAvailable,
            VramGb = hardwareResponse.VramGb,
            RamGb = hardwareResponse.RamGb,
            CpuCores = hardwareResponse.CpuInfo.PhysicalCores
        };
    }

    /// <summary>
    /// Dispose resources.
    /// </summary>
    public void Dispose()
    {
        _channel?.Dispose();
    }
}
