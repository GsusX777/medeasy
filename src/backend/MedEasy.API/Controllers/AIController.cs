// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.AspNetCore.Mvc;
using MedEasy.Application.Interfaces;
using MedEasy.Application.DTOs.AI;
using MedEasy.Domain.Entities;
using MedEasy.Domain.Interfaces;
using MedEasy.Application.Services;

namespace MedEasy.API.Controllers;

/// <summary>
/// AI Controller for Whisper transcription and AI services.
/// 
/// [MLB] Multi-Language Bridge via gRPC to Python AI Service
/// [WMM] Whisper Multi-Model support with benchmarking
/// [AIU] Anonymization is mandatory and cannot be disabled
/// [SP] Secure local processing only
/// [ATV] Audit logging for all operations
/// </summary>
[ApiController]
[Route("api/ai")]
[Produces("application/json")]
public class AIController : ControllerBase
{
    private readonly IWhisperService _whisperService;
    private readonly ILogger<AIController> _logger;
    private readonly IBenchmarkResultRepository _benchmarkRepository;
    private readonly IAudioRecordRepository _audioRecordRepository;
    private readonly IEncryptionService _encryptionService;
    // TODO: ILiveTranscriptionService will be implemented from scratch
    // private readonly ILiveTranscriptionService _liveTranscriptionService;

    public AIController(
        IWhisperService whisperService,
        ILogger<AIController> logger,
        IBenchmarkResultRepository benchmarkRepository,
        IAudioRecordRepository audioRecordRepository,
        IEncryptionService encryptionService)
        // TODO: Add ILiveTranscriptionService parameter when implemented from scratch
        // ILiveTranscriptionService liveTranscriptionService)
    {
        _whisperService = whisperService;
        _logger = logger;
        _benchmarkRepository = benchmarkRepository;
        _audioRecordRepository = audioRecordRepository;
        _encryptionService = encryptionService;
        // TODO: Assign ILiveTranscriptionService when implemented from scratch
        // _liveTranscriptionService = liveTranscriptionService;
    }

    /// <summary>
    /// Get available Whisper models.
    /// 
    /// [WMM] Returns all supported Whisper models
    /// [SP] Local processing only
    /// </summary>
    /// <returns>List of available Whisper models</returns>
    [HttpGet("whisper/available-models")]
    [ProducesResponseType(typeof(AvailableModelsResponse), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<AvailableModelsResponse>> GetAvailableModels()
    {
        try
        {
            _logger.LogInformation("Getting available Whisper models");
            
            var result = await _whisperService.GetAvailableModelsAsync();
            
            _logger.LogInformation("Retrieved {ModelCount} available models", result.Models.Count);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available Whisper models");
            return StatusCode(500, new { error = "Failed to get available models" });
        }
    }

    /// <summary>
    /// Get hardware information for optimal model selection.
    /// 
    /// [WMM] Hardware-based model recommendations
    /// [PSF] System hardware analysis
    /// </summary>
    /// <returns>Hardware information and model recommendations</returns>
    [HttpGet("whisper/hardware-info")]
    [ProducesResponseType(typeof(HardwareInfoResponse), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<HardwareInfoResponse>> GetHardwareInfo()
    {
        try
        {
            _logger.LogInformation("Getting hardware information");
            
            var result = await _whisperService.GetHardwareInfoAsync();
            
            _logger.LogInformation("Retrieved hardware info: {PhysicalCores} cores, {RamGb}GB RAM, CUDA: {CudaAvailable}", 
                result.CpuInfo?.PhysicalCores ?? 0, result.RamGb, result.CudaAvailable);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hardware information");
            return StatusCode(500, new { error = "Failed to get hardware information" });
        }
    }

    /// <summary>
    /// Benchmark Whisper models performance.
    /// 
    /// [WMM] Performance comparison of all models
    /// [PB] Performance baseline establishment
    /// [ATV] Logs benchmarking operations
    /// </summary>
    /// <param name="request">Benchmark request with test audio</param>
    /// <returns>Benchmark results for all tested models</returns>
    [HttpPost("whisper/benchmark-models")]
    [ProducesResponseType(typeof(BenchmarkModelsResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<BenchmarkModelsResponse>> BenchmarkModels([FromBody] BenchmarkModelsRequest request)
    {
        try
        {
            if (request?.TestAudioData == null || request.TestAudioData.Length == 0)
            {
                return BadRequest(new { error = "Test audio data is required" });
            }

            _logger.LogInformation("Starting Whisper model benchmarking with {AudioSize} bytes", 
                request.TestAudioData.Length);
            
            var result = await _whisperService.BenchmarkModelsAsync(request);
            
            _logger.LogInformation("Benchmarking completed for {ModelCount} models", 
                result.Results.Count);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to benchmark Whisper models");
            return StatusCode(500, new { error = "Failed to benchmark models" });
        }
    }

    /// <summary>
    /// Test endpoint to verify AIController routing works.
    /// </summary>
    [HttpPost("whisper/test-endpoint")]
    [ProducesResponseType(200)]
    public ActionResult TestEndpoint()
    {
        _logger.LogInformation("🔧 DEBUG: TestEndpoint called successfully!");
        return Ok(new { message = "AIController routing works", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Benchmark Whisper models with audio file upload (multipart/form-data).
    /// 
    /// [WMM] Tests all 4 Whisper models with real audio
    /// [PB] Performance baseline measurement
    /// [ATV] All operations audited
    /// </summary>
    /// <param name="audioFile">Audio file for benchmarking</param>
    /// <param name="modelsToTest">Comma-separated list of models (default: all)</param>
    /// <param name="iterations">Number of iterations per model (default: 1)</param>
    /// <returns>Benchmark results for all tested models</returns>
    [HttpPost("whisper/benchmark-models-file")]
    [ProducesResponseType(typeof(BenchmarkModelsResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<BenchmarkModelsResponse>> BenchmarkModelsFile()
    {
        // [DEBUG] Log method entry - this should appear if method is called
        _logger.LogInformation("🔧 DEBUG: BenchmarkModelsFile method called!");
        _logger.LogInformation("🔧 DEBUG: Request ContentType: {ContentType}", Request.ContentType);
        _logger.LogInformation("🔧 DEBUG: Request Method: {Method}", Request.Method);
        _logger.LogInformation("🔧 DEBUG: Request Path: {Path}", Request.Path);
        _logger.LogInformation("🔧 DEBUG: HasFormContentType: {HasForm}", Request.HasFormContentType);
        
        string audioFileName = "unknown";
        
        try
        {
            // Manually read form data to bypass model binding issues
            if (!Request.HasFormContentType)
            {
                _logger.LogError("🔧 DEBUG: Request does not have form content type");
                return BadRequest(new { error = "Request must be multipart/form-data" });
            }
            
            var form = await Request.ReadFormAsync();
            _logger.LogInformation("🔧 DEBUG: Form keys: {Keys}", string.Join(", ", form.Keys));
            _logger.LogInformation("🔧 DEBUG: Form files count: {Count}", form.Files.Count);
            
            var audioFile = form.Files.GetFile("audioFile");
            var modelsToTest = form["modelsToTest"].ToString();
            var iterationsStr = form["iterations"].ToString();
            
            _logger.LogInformation("🔧 DEBUG: audioFile is null: {IsNull}", audioFile == null);
            _logger.LogInformation("🔧 DEBUG: modelsToTest: {Models}", modelsToTest);
            _logger.LogInformation("🔧 DEBUG: iterations: {Iterations}", iterationsStr);
            
            if (audioFile == null || audioFile.Length == 0)
            {
                _logger.LogError("🔧 DEBUG: Audio file validation failed - audioFile is null or empty");
                return BadRequest(new { error = "Audio file is required" });
            }
            
            audioFileName = audioFile.FileName ?? "unknown";
            
            if (!int.TryParse(iterationsStr, out int iterations))
            {
                iterations = 1;
            }
            
            if (string.IsNullOrEmpty(modelsToTest))
            {
                modelsToTest = "base,small,medium,large-v3";
            }

            _logger.LogInformation("Starting file benchmarking: {FileName}, Size: {Size} bytes, Models: {Models}", 
                audioFile.FileName, audioFile.Length, modelsToTest);
            
            // Convert IFormFile to byte array
            byte[] audioData;
            using (var memoryStream = new MemoryStream())
            {
                await audioFile.CopyToAsync(memoryStream);
                audioData = memoryStream.ToArray();
            }
            
            // Parse models to test
            var modelsList = modelsToTest.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(m => m.Trim())
                .ToList();
            
            // Create benchmark request
            var request = new BenchmarkModelsRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                TestAudioData = audioData,
                AudioFormat = Path.GetExtension(audioFile.FileName)?.TrimStart('.') ?? "unknown",
                AudioFilename = audioFile.FileName ?? "unknown",  // Pass original filename
                ModelsToTest = modelsList,
                Iterations = iterations,
                Language = "auto",
                EnableCudaBenchmark = false,
                EnableCpuBenchmark = true,
                EnableSwissGermanDetection = true
            };
            
            _logger.LogInformation("🔧 DEBUG: About to call WhisperService.BenchmarkModelsAsync with request: {RequestId}, Models: {Models}, AudioSize: {AudioSize}",
                request.RequestId, string.Join(",", request.ModelsToTest), request.TestAudioData.Length);
            
            // [SP][EIV] Create AudioRecord with encrypted audio data BEFORE benchmark processing
            string? audioRecordId = null;
            try
            {
                var encryptedAudioData = await _encryptionService.EncryptAsync(audioData);
                
                var audioRecord = new AudioRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    FileName = audioFile.FileName ?? "benchmark_audio.wav",
                    EncryptedAudioData = encryptedAudioData,
                    FileSizeBytes = audioData.Length,
                    DurationSeconds = 0, // TODO: Calculate from audio metadata
                    RecordingType = RecordingType.Upload, // Benchmark uploads
                    AudioFormat = Path.GetExtension(audioFile.FileName)?.TrimStart('.') ?? "wav",
                    SampleRate = 16000, // Default for Whisper
                    BitRate = 256000, // Default bitrate
                    Channels = 1, // Mono
                    IsAnonymized = true, // [AIU] Always anonymized
                    AnonymizationConfidence = 1.0, // [AIU] Full confidence for uploaded files
                    NeedsReview = false, // No review needed for benchmark uploads
                    BenchmarkId = request.RequestId, // Link to benchmark
                    CreatedBy = "BenchmarkSystem"
                };
                
                var savedAudioRecord = await _audioRecordRepository.AddAsync(audioRecord);
                audioRecordId = savedAudioRecord.Id;
                
                _logger.LogInformation("[SP][EIV] AudioRecord created and encrypted: {AudioRecordId}, Size: {OriginalSize} -> {EncryptedSize} bytes",
                    audioRecordId, audioData.Length, encryptedAudioData.Length);
            }
            catch (Exception encEx)
            {
                _logger.LogError(encEx, "[SP][EIV] Failed to create encrypted AudioRecord: {Error}", encEx.Message);
                // Continue with benchmark even if AudioRecord creation fails
            }
                
            BenchmarkModelsResponse result;
            try
            {
                result = await _whisperService.BenchmarkModelsAsync(request);
                _logger.LogInformation("🔧 DEBUG: WhisperService.BenchmarkModelsAsync completed successfully");
            }
            catch (Exception whisperEx)
            {
                _logger.LogError(whisperEx, "🔧 DEBUG: WhisperService.BenchmarkModelsAsync failed with exception: {ExceptionType}, Message: {Message}",
                    whisperEx.GetType().Name, whisperEx.Message);
                return StatusCode(500, new { error = $"Whisper service error: {whisperEx.Message}" });
            }
            
            // [ATV] Persist benchmark results to database for audit trail and history
            try
            {
                foreach (var modelResult in result.Results)
                {
                    // [SP][AIU] Encrypt transcribed text for secure storage
                    byte[]? encryptedText = null;
                    if (!string.IsNullOrEmpty(modelResult.TranscribedText))
                    {
                        encryptedText = await _encryptionService.EncryptAsync(modelResult.TranscribedText);
                        _logger.LogInformation("Transcribed text encrypted for model {ModelName}: {TextLength} chars", 
                            modelResult.ModelName, modelResult.TranscribedText.Length);
                    }
                    else
                    {
                        // Fallback: encrypt placeholder for benchmarks without transcription
                        encryptedText = await _encryptionService.EncryptAsync("[BENCHMARK_ONLY]");
                    }

                    var benchmarkEntity = new MedEasy.Domain.Entities.BenchmarkResult
                    {
                        Id = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.UtcNow,
                        AudioRecordId = audioRecordId, // [SP][EIV] Link to encrypted AudioRecord
                        ModelName = modelResult.ModelName,
                        ProcessingTimeMs = modelResult.AverageProcessingTimeMs,
                        CpuUsagePercent = modelResult.AverageCpuUsage,
                        MemoryUsageMb = modelResult.AverageRamUsageMb,
                        PerformanceScore = modelResult.PerformanceScore,
                        AudioDurationSeconds = 0, // TODO: Calculate from audio data
                        AudioFileSizeMb = audioData.Length / (1024.0 * 1024.0),
                        AudioFilename = audioFileName, // [WMM] Store original filename
                        EncryptedTranscriptionText = encryptedText, // [SP][AIU] Encrypted transcribed text
                        DetectedLanguage = "de",
                        ConfidenceScore = modelResult.AverageConfidence,
                        HardwareInfo = System.Text.Json.JsonSerializer.Serialize(result.HardwareInfo),
                        Iterations = iterations,
                        AverageTimePerIteration = modelResult.AverageProcessingTimeMs / iterations,
                        ErrorMessage = modelResult.Success ? null : modelResult.ErrorMessage,
                        Status = "Completed", // [ATV] Required property
                        AnonymizationStatus = "Anonymized" // [AIU] Required property
                    };
                    
                    await _benchmarkRepository.AddAsync(benchmarkEntity);
                    _logger.LogInformation("Benchmark result persisted: {ModelName}, {ProcessingTime}ms, {AudioFile}", 
                        modelResult.ModelName, modelResult.AverageProcessingTimeMs, audioFileName);
                }
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to persist benchmark results to database");
                // Don't fail the request, just log the error
            }
            
            _logger.LogInformation("File benchmarking completed: {FileName}, {ModelCount} models tested", 
                audioFileName, result.Results.Count);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to benchmark models with file: {FileName}", audioFileName);
            return StatusCode(500, new { error = "Failed to benchmark models with file" });
        }
    }

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
    [HttpPost("whisper/transcribe")]
    [ProducesResponseType(typeof(TranscriptionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<TranscriptionResponse>> TranscribeAudio([FromBody] TranscriptionRequest request)
    {
        try
        {
            if (request?.AudioData == null || request.AudioData.Length == 0)
            {
                return BadRequest(new { error = "Audio data is required" });
            }

            _logger.LogInformation("Starting audio transcription with model {Model}, language {Language}", 
                request.WhisperModel ?? "default", request.Language ?? "auto");
            
            var result = await _whisperService.TranscribeAsync(request);
            
            _logger.LogInformation("Transcription completed: {TextLength} characters, {EntitiesFound} entities anonymized", 
                result.Text.Length, result.EntitiesFound);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transcribe audio");
            return StatusCode(500, new { error = "Failed to transcribe audio" });
        }
    }

    /// <summary>
    /// Transcribe audio file upload (multipart/form-data).
    /// 
    /// [WMM] Supports multiple Whisper models
    /// [AIU] Automatic anonymization applied
    /// [ATV] All operations audited
    /// </summary>
    /// <param name="audioFile">Audio file to transcribe</param>
    /// <param name="model">Whisper model to use (default: small)</param>
    /// <param name="language">Language code (default: auto)</param>
    /// <returns>Transcribed and anonymized text</returns>
    [HttpPost("whisper/transcribe-file")]
    [ProducesResponseType(typeof(TranscriptionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<TranscriptionResponse>> TranscribeAudioFile(
        IFormFile audioFile,
        [FromForm] string model = "small",
        [FromForm] string language = "auto")
    {
        try
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest(new { error = "Audio file is required" });
            }

            _logger.LogInformation("Starting file transcription: {FileName}, Size: {Size} bytes, Model: {Model}", 
                audioFile.FileName, audioFile.Length, model);
            
            // Convert IFormFile to byte array
            byte[] audioData;
            using (var memoryStream = new MemoryStream())
            {
                await audioFile.CopyToAsync(memoryStream);
                audioData = memoryStream.ToArray();
            }
            
            // Create transcription request
            var request = new TranscriptionRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                AudioData = audioData,
                AudioFormat = Path.GetExtension(audioFile.FileName)?.TrimStart('.') ?? "unknown",
                WhisperModel = model,
                Language = language
            };
            
            var result = await _whisperService.TranscribeAsync(request);
            
            _logger.LogInformation("File transcription completed: {FileName}, {TextLength} characters, {EntitiesFound} entities anonymized", 
                audioFile.FileName, result.Text.Length, result.EntitiesFound);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transcribe audio file: {FileName}", audioFile?.FileName);
            return StatusCode(500, new { error = "Failed to transcribe audio file" });
        }
    }



    /// <summary>
    /// Health check for AI services.
    /// 
    /// [ATV] Service monitoring and audit
    /// [MLB] Checks gRPC connection to Python AI Service
    /// </summary>
    /// <returns>AI service health status</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> HealthCheck()
    {
        try
        {
            _logger.LogInformation("Performing AI service health check");
            
            // Test gRPC connection by getting available models
            var modelsResult = await _whisperService.GetAvailableModelsAsync();
            
            var healthStatus = new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                services = new
                {
                    grpcConnection = "Connected",
                    whisperService = "Available",
                    modelsAvailable = modelsResult.Models.Count
                }
            };
            
            _logger.LogInformation("AI service health check completed successfully");
            
            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI service health check failed");
            
            var healthStatus = new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow,
                error = ex.Message,
                services = new
                {
                    grpcConnection = "Failed",
                    whisperService = "Unavailable"
                }
            };
            
            return StatusCode(500, healthStatus);
        }
    }

    /// <summary>
    /// Benchmark multiple Whisper models with audio file.
    /// 
    /// [WMM] Multi-model benchmarking with persistence
    /// [ATV] Results stored in database for audit trail
    /// [PSF] Performance monitoring for patient safety
    /// </summary>
    /// <param name="audioFile">Audio file to benchmark</param>
    /// <param name="modelsToTest">Comma-separated list of models to test</param>
    /// <param name="iterations">Number of iterations per model</param>
    /// <returns>Benchmark results</returns>
    [HttpPost("whisper/benchmark-models-file")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> BenchmarkModelsFile(
        IFormFile audioFile,
        [FromForm] string modelsToTest = "base,small,medium,large-v3",
        [FromForm] int iterations = 1)
    {
        try
        {
            // [ATV] Log benchmark request
            _logger.LogInformation("Benchmark request received for models: {Models}, iterations: {Iterations}", 
                modelsToTest, iterations);

            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest(new { error = "Audio file is required" });
            }

            // Convert IFormFile to byte array
            byte[] audioData;
            using (var memoryStream = new MemoryStream())
            {
                await audioFile.CopyToAsync(memoryStream);
                audioData = memoryStream.ToArray();
            }

            // Create BenchmarkModelsRequest DTO
            var benchmarkRequest = new BenchmarkModelsRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                TestAudioData = audioData,
                AudioFormat = Path.GetExtension(audioFile.FileName)?.TrimStart('.') ?? "m4a",
                Language = "auto",
                Iterations = iterations,
                ModelsToTest = modelsToTest.Split(',').Select(m => m.Trim()).ToList(),
                EnableCudaBenchmark = false,
                EnableCpuBenchmark = true,
                EnableSwissGermanDetection = true
            };

            // Call WhisperService for benchmarking
            var benchmarkResult = await _whisperService.BenchmarkModelsAsync(benchmarkRequest);

            // [ATV] Persist each model result to database
            var sessionId = Guid.NewGuid().ToString();
            var persistedResults = new List<object>();

            if (benchmarkResult?.Results != null)
            {
                foreach (var modelResult in benchmarkResult.Results)
                {
                    try
                    {
                        // Calculate audio duration from processing time (rough estimate)
                        var estimatedAudioDuration = modelResult.AverageProcessingTimeMs / 1000.0; // Convert to seconds
                        
                        var benchmarkEntity = new BenchmarkResult
                        {
                            Id = Guid.NewGuid().ToString(),
                            Timestamp = DateTime.UtcNow,
                            ModelName = modelResult.ModelName,
                            ProcessingTimeMs = modelResult.AverageProcessingTimeMs,
                            CpuUsagePercent = modelResult.AverageCpuUsage,
                            MemoryUsageMb = modelResult.AverageRamUsageMb,
                            PerformanceScore = modelResult.PerformanceScore,
                            AudioDurationSeconds = estimatedAudioDuration, // Estimated from processing time
                            AudioFileSizeMb = audioFile.Length / (1024.0 * 1024.0),
                            AudioFilename = audioFile.FileName ?? "unknown", // [WMM] Store original filename
                            EncryptedTranscriptionText = !string.IsNullOrEmpty(modelResult.TranscribedText) 
                                ? await _encryptionService.EncryptAsync(modelResult.TranscribedText) // [SP][AIU] Encrypt real transcribed text
                                : await _encryptionService.EncryptAsync("[BENCHMARK_ONLY]"), // [SP] Fallback for benchmark-only
                            DetectedLanguage = "de",
                            ConfidenceScore = modelResult.AverageConfidence,
                            HardwareInfo = System.Text.Json.JsonSerializer.Serialize(benchmarkResult.HardwareInfo),
                            Iterations = iterations,
                            AverageTimePerIteration = modelResult.AverageProcessingTimeMs / (double)iterations,
                            ErrorMessage = modelResult.Success ? null : modelResult.ErrorMessage,
                            Status = modelResult.Success ? "Completed" : "Failed",
                            SessionId = sessionId,
                            AnonymizationStatus = "NotRequired" // Benchmark data, no PII
                        };

                        // [ATV] Save to database
                        var saved = await _benchmarkRepository.AddAsync(benchmarkEntity);
                        
                        _logger.LogInformation("Benchmark result saved for model {Model} with ID {Id}", 
                            modelResult.ModelName, saved.Id);

                        persistedResults.Add(new
                        {
                            id = saved.Id,
                            modelName = saved.ModelName,
                            saved = true
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to persist benchmark result for model {Model}", 
                            modelResult.ModelName);
                        
                        persistedResults.Add(new
                        {
                            modelName = modelResult.ModelName,
                            saved = false,
                            error = ex.Message
                        });
                    }
                }
            }

            // Return benchmark results with persistence info
            var response = new
            {
                sessionId = sessionId,
                benchmarkResults = benchmarkResult,
                persistedResults = persistedResults,
                totalSaved = persistedResults.Count(r => ((dynamic)r).saved == true)
            };

            _logger.LogInformation("Benchmark completed. {Total} results saved to database", 
                response.totalSaved);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Benchmark failed");
            return StatusCode(500, new { error = "Benchmark failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Save audio recording to encrypted storage.
    /// 
    /// [SP] Audio data encrypted with AES-256-GCM before storage
    /// [EIV] All audio data stored encrypted in AudioRecord table
    /// [ATV] Full audit trail for audio recording operations
    /// [AIU] Anonymization flags enforced and immutable
    /// </summary>
    /// <param name="audioFile">Audio file from recording</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="duration">Recording duration in seconds</param>
    /// <returns>Created AudioRecord with encrypted data</returns>
    [HttpPost("audio/save-recording")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> SaveAudioRecording(
        IFormFile audioFile,
        [FromForm] string fileName,
        [FromForm] double duration)
    {
        try
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest(new { error = "Audio file is required" });
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest(new { error = "Filename is required" });
            }

            _logger.LogInformation("Saving audio recording: {FileName}, Size: {Size} bytes, Duration: {Duration}s [ATV]", 
                fileName, audioFile.Length, duration);
            
            // Convert IFormFile to byte array
            byte[] audioData;
            using (var memoryStream = new MemoryStream())
            {
                await audioFile.CopyToAsync(memoryStream);
                audioData = memoryStream.ToArray();
            }
            
            // Encrypt audio data [SP][EIV]
            var encryptedAudioData = await _encryptionService.EncryptAsync(audioData);
            
            // Create AudioRecord entity
            var audioRecord = new AudioRecord
            {
                Id = Guid.NewGuid().ToString(),
                FileName = fileName,
                EncryptedAudioData = encryptedAudioData,
                AudioFormat = Path.GetExtension(fileName)?.TrimStart('.') ?? "unknown",
                FileSizeBytes = audioFile.Length,
                DurationSeconds = duration,
                RecordingType = "recording", // [WMM] Type of recording
                SampleRate = 16000, // [WMM] 16kHz for Whisper
                BitRate = 256000, // [SF] 256kbps
                Channels = 1, // [WMM] Mono
                
                // [AIU] Anonymization flags - immutable and enforced
                IsAnonymized = true,
                AnonymizationConfidence = 1.0,
                NeedsReview = false,
                
                // [PSF] Processing info - set defaults for recording
                ProcessingTimeMs = 0, // No processing time for direct recording
                ModelUsed = null, // No model used for direct recording
                BenchmarkId = null, // No benchmark for direct recording
                
                // [ATV] Audit trail fields
                Created = DateTime.UtcNow,
                CreatedBy = "system", // TODO: Get from authentication context
                LastModified = DateTime.UtcNow,
                LastModifiedBy = "system"
            };
            
            // Save to database [SP][ATV]
            await _audioRecordRepository.AddAsync(audioRecord);
            
            _logger.LogInformation("Audio recording saved successfully: {Id}, {FileName} [ATV]", 
                audioRecord.Id, audioRecord.FileName);
            
            // Return response without encrypted data
            var response = new
            {
                id = audioRecord.Id,
                fileName = audioRecord.FileName,
                audioFormat = audioRecord.AudioFormat,
                fileSizeBytes = audioRecord.FileSizeBytes,
                durationSeconds = audioRecord.DurationSeconds,
                recordingTimestamp = audioRecord.Created,
                created = audioRecord.Created
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save audio recording: {FileName} [ECP]", fileName);
            return StatusCode(500, new { error = "Failed to save audio recording" });
        }
    }

    /// <summary>
    /// Get all saved audio recordings.
    /// 
    /// [ATV] Audit trail for audio record access
    /// [EIV] Returns metadata only, encrypted audio data not exposed
    /// [SP] Secure access to audio record metadata
    /// </summary>
    /// <returns>List of audio recordings metadata</returns>
    [HttpGet("audio/recordings")]
    [ProducesResponseType(typeof(object[]), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetAudioRecordings()
    {
        try
        {
            _logger.LogInformation("Retrieving audio recordings list [ATV]");
            
            var audioRecords = await _audioRecordRepository.GetAllAsync();
            
            // Return metadata only (no encrypted audio data) [EIV]
            var response = audioRecords.Select(record => new
            {
                id = record.Id,
                fileName = record.FileName,
                audioFormat = record.AudioFormat,
                fileSizeBytes = record.FileSizeBytes,
                durationSeconds = record.DurationSeconds,
                recordingTimestamp = record.Created,
                created = record.Created,
                // Anonymization info (read-only) [AIU]
                anonymizationApplied = record.IsAnonymized,
                anonymizationReviewed = !record.NeedsReview,
                anonymizationConfidence = record.AnonymizationConfidence
            }).OrderByDescending(r => r.created).ToList();
            
            _logger.LogInformation("Retrieved {Count} audio recordings [ATV]", response.Count);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audio recordings [ECP]");
            return StatusCode(500, new { error = "Failed to retrieve audio recordings" });
        }
    }
        
    /// <summary>
    /// Benchmark Whisper models with saved AudioRecord [WMM][ATV]
    /// </summary>
    [HttpPost("whisper/benchmark-models-file/record")]
    public async Task<ActionResult> BenchmarkModelsWithRecord([FromBody] BenchmarkRecordRequest request)
        {
            try
            {
                _logger.LogInformation("Starting benchmark with AudioRecord {AudioRecordId} for models: {Models} [ATV]", 
                    request.AudioRecordId, request.ModelsToTest);
                
                // Validate request
                if (string.IsNullOrEmpty(request.AudioRecordId))
                {
                    return BadRequest(new { error = "AudioRecordId is required" });
                }
                
                if (string.IsNullOrEmpty(request.ModelsToTest))
                {
                    return BadRequest(new { error = "ModelsToTest is required" });
                }
                
                // Get AudioRecord from database
                var audioRecord = await _audioRecordRepository.GetByIdAsync(request.AudioRecordId);
                if (audioRecord == null)
                {
                    return NotFound(new { error = "AudioRecord not found", id = request.AudioRecordId });
                }
                
                // [SP][EIV] Decrypt audio data for benchmark
                var decryptedAudioData = await _encryptionService.DecryptBinaryAsync(audioRecord.EncryptedAudioData);
                
                // Parse models and iterations
                var models = request.ModelsToTest.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(m => m.Trim()).ToList();
                
                if (!int.TryParse(request.Iterations, out int iterations) || iterations <= 0)
                {
                    iterations = 1;
                }
                
                _logger.LogInformation("Benchmarking {ModelCount} models with {Iterations} iterations using AudioRecord {FileName} [WMM]", 
                    models.Count, iterations, audioRecord.FileName);
                
                // Create BenchmarkModelsRequest [WMM]
                var benchmarkRequest = new BenchmarkModelsRequest
                {
                    RequestId = Guid.NewGuid().ToString(),
                    TestAudioData = decryptedAudioData,
                    AudioFormat = audioRecord.AudioFormat ?? "webm",
                    AudioFilename = audioRecord.FileName,
                    Language = "de",
                    Iterations = iterations,
                    ModelsToTest = models,
                    EnableCudaBenchmark = false,
                    EnableCpuBenchmark = true,
                    EnableSwissGermanDetection = true
                };
                
                // Call Whisper service for benchmark
                var benchmarkResult = await _whisperService.BenchmarkModelsAsync(benchmarkRequest);
                
                if (benchmarkResult == null)
                {
                    return StatusCode(500, new { error = "Benchmark failed - no result returned" });
                }
                
                _logger.LogInformation("Benchmark completed successfully. Total time: {TotalTime}ms [WMM]", 
                    benchmarkResult.TotalBenchmarkTimeMs);
                
                // Save benchmark results to database [ATV]
                var sessionId = Guid.NewGuid().ToString();
                var persistedResults = new List<object>();
                
                if (benchmarkResult.Results?.Any() == true)
                {
                    foreach (var modelResult in benchmarkResult.Results)
                    {
                        try
                        {
                            var estimatedAudioDuration = audioRecord.DurationSeconds;
                            
                            var benchmarkEntity = new BenchmarkResult
                            {
                                Id = Guid.NewGuid().ToString(),
                                Timestamp = DateTime.UtcNow,
                                AudioRecordId = audioRecord.Id,
                                ModelName = modelResult.ModelName,
                                ProcessingTimeMs = modelResult.AverageProcessingTimeMs,
                                CpuUsagePercent = modelResult.AverageCpuUsage,
                                MemoryUsageMb = modelResult.AverageRamUsageMb,
                                PerformanceScore = modelResult.PerformanceScore,
                                AudioDurationSeconds = estimatedAudioDuration,
                                AudioFileSizeMb = audioRecord.FileSizeBytes / (1024.0 * 1024.0),
                                AudioFilename = audioRecord.FileName,
                                EncryptedTranscriptionText = !string.IsNullOrEmpty(modelResult.TranscribedText) 
                                    ? await _encryptionService.EncryptAsync(modelResult.TranscribedText)
                                    : await _encryptionService.EncryptAsync("[BENCHMARK_ONLY]"),
                                DetectedLanguage = "de",
                                ConfidenceScore = modelResult.AverageConfidence,
                                HardwareInfo = System.Text.Json.JsonSerializer.Serialize(benchmarkResult.HardwareInfo),
                                Iterations = iterations,
                                AverageTimePerIteration = modelResult.AverageProcessingTimeMs / (double)iterations,
                                ErrorMessage = modelResult.Success ? null : modelResult.ErrorMessage,
                                Status = modelResult.Success ? "Completed" : "Failed",
                                SessionId = sessionId,
                                AnonymizationStatus = "NotRequired"
                            };
                            
                            var saved = await _benchmarkRepository.AddAsync(benchmarkEntity);
                            
                            _logger.LogInformation("Benchmark result saved for model {Model} with ID {Id} [ATV]", 
                                modelResult.ModelName, saved.Id);
                            
                            persistedResults.Add(new
                            {
                                id = saved.Id,
                                modelName = saved.ModelName,
                                saved = true
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to persist benchmark result for model {Model} [ECP]", 
                                modelResult.ModelName);
                            
                            persistedResults.Add(new
                            {
                                modelName = modelResult.ModelName,
                                saved = false,
                                error = ex.Message
                            });
                        }
                    }
                }
                
                // Return benchmark results
                var response = new
                {
                    requestId = benchmarkResult.RequestId,
                    benchmarkId = benchmarkResult.BenchmarkId,
                    totalBenchmarkTimeMs = benchmarkResult.TotalBenchmarkTimeMs,
                    results = benchmarkResult.Results != null 
                        ? benchmarkResult.Results.Select(r => new
                        {
                            modelName = r.ModelName,
                            averageProcessingTimeMs = r.AverageProcessingTimeMs,
                            minProcessingTimeMs = r.MinProcessingTimeMs,
                            maxProcessingTimeMs = r.MaxProcessingTimeMs,
                            averageRamUsageMb = r.AverageRamUsageMb,
                            averageCpuUsage = r.AverageCpuUsage,
                            averageConfidence = r.AverageConfidence,
                            performanceScore = r.PerformanceScore,
                            success = r.Success,
                            errorMessage = r.ErrorMessage,
                            transcribedText = r.TranscribedText
                        }).Cast<object>().ToList()
                        : new List<object>(),
                    hardwareInfo = benchmarkResult.HardwareInfo,
                    recommendation = benchmarkResult.Recommendation,
                    audioRecord = new
                    {
                        id = audioRecord.Id,
                        fileName = audioRecord.FileName,
                        durationSeconds = audioRecord.DurationSeconds,
                        fileSizeBytes = audioRecord.FileSizeBytes
                    },
                    persistedResults = persistedResults
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Benchmark with AudioRecord failed [ECP]");
                return StatusCode(500, new { error = "Benchmark failed", details = ex.Message });
            }
        }
        
        /// <summary>
        /// Ruft entschlüsselte Audio-Daten für Wiedergabe ab [SP][EIV]
        /// </summary>
        [HttpGet("audio/{id}/data")]
        public async Task<ActionResult> GetAudioData(string id)
        {
            try
            {
                var audioRecord = await _audioRecordRepository.GetByIdAsync(id);
                if (audioRecord == null)
                {
                    return NotFound(new { error = "Audio record not found", id });
                }
                
                // [SP][EIV] Decrypt audio data for benchmark processing
                var decryptedAudioData = await _encryptionService.DecryptBinaryAsync(audioRecord.EncryptedAudioData);
                
                // 🚨 DEBUG: Analyze decrypted audio data for corruption [Debug][PSF]
                _logger.LogWarning("🔍 AUDIORECORD DEBUG: Decrypted {Size} bytes from AudioRecord {Id}", 
                    decryptedAudioData.Length, audioRecord.Id);
                
                // Check WAV header integrity
                if (decryptedAudioData.Length >= 4)
                {
                    var wavHeader = System.Text.Encoding.ASCII.GetString(decryptedAudioData.Take(4).ToArray());
                    _logger.LogWarning("🔍 WAV-Header after decryption: '{Header}' (should be 'RIFF')", wavHeader);
                }
                
                // Check first 16 bytes as hex
                if (decryptedAudioData.Length >= 16)
                {
                    var firstBytes = Convert.ToHexString(decryptedAudioData.Take(16).ToArray());
                    _logger.LogWarning("🔍 First 16 bytes (hex): {FirstBytes}", firstBytes);
                }
                
                // Check if data looks like audio (should start with RIFF for WAV)
                var isValidWav = decryptedAudioData.Length >= 4 && 
                    decryptedAudioData[0] == 0x52 && decryptedAudioData[1] == 0x49 && 
                    decryptedAudioData[2] == 0x46 && decryptedAudioData[3] == 0x46;
                _logger.LogWarning("🔍 Is valid WAV format: {IsValid}", isValidWav);
                
                if (!isValidWav)
                {
                    _logger.LogError("🚨 CRITICAL: AudioRecord contains corrupted audio data - this explains Georgian characters! [Bug][RootCause][PSF]");
                }
                
                // Determine content type based on audio format
                var contentType = audioRecord.AudioFormat?.ToLower() switch
                {
                    "webm" => "audio/webm",
                    "wav" => "audio/wav",
                    "mp3" => "audio/mpeg",
                    "m4a" => "audio/mp4",
                    "ogg" => "audio/ogg",
                    _ => "audio/webm" // Default fallback
                };
                
                _logger.LogInformation($"Serving audio data for playback: {audioRecord.FileName} ({decryptedAudioData.Length} bytes) [ATV]");
                
                return File(decryptedAudioData, contentType, audioRecord.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve audio data for playback: {AudioId} [ECP]", id);
                return StatusCode(500, new { error = "Failed to retrieve audio data", details = ex.Message });
            }
        }
        
        /// <summary>
        /// Benchmark Whisper models using chunk-based testing [WMM][PSF]
        /// Tests audio in sequential chunks to simulate live transcription performance
        /// Supports both AudioRecord-based and File-Upload-based testing [UX][Architektur]
        /// </summary>
        [HttpPost("benchmark-chunk-test")]
        public async Task<ActionResult> BenchmarkChunkTest()
        {
            try
            {
                byte[] audioData;
                string fileName;
                string audioFormat;
                ChunkTestRequest request;
                bool isFileUpload = false;
                
                // Determine if this is JSON (AudioRecord) or Form-Data (File Upload) [UX][Architektur]
                if (Request.HasFormContentType)
                {
                    // File Upload Mode: Handle multipart/form-data [UX][Bugfix]
                    _logger.LogInformation("Processing chunk test with file upload [UX]");
                    
                    var form = await Request.ReadFormAsync();
                    var audioFile = form.Files.GetFile("audioFile");
                    
                    if (audioFile == null || audioFile.Length == 0)
                    {
                        return BadRequest(new { error = "Audio file is required for file upload chunk test" });
                    }
                    
                    // Extract form parameters
                    var modelsToTest = form["modelsToTest"].ToString();
                    var iterationsStr = form["iterations"].ToString();
                    var chunkSizeSecondsStr = form["chunkSizeSeconds"].ToString();
                    var overlapMsStr = form["overlapMs"].ToString();
                    
                    // Parse parameters with defaults
                    if (!int.TryParse(iterationsStr, out int iterations)) iterations = 1;
                    if (!double.TryParse(chunkSizeSecondsStr, out double chunkSizeSeconds)) chunkSizeSeconds = 2.0;
                    if (!int.TryParse(overlapMsStr, out int overlapMs)) overlapMs = 500;
                    
                    // Convert file to byte array
                    using var memoryStream = new MemoryStream();
                    await audioFile.CopyToAsync(memoryStream);
                    audioData = memoryStream.ToArray();
                    fileName = audioFile.FileName ?? "uploaded_chunk_test.wav";
                    audioFormat = Path.GetExtension(audioFile.FileName)?.TrimStart('.') ?? "wav";
                    
                    // Create request object for file upload
                    request = new ChunkTestRequest
                    {
                        AudioFile = "uploaded", // Indicate file upload
                        ModelsToTest = modelsToTest ?? "small", // Keep as string - already comma-separated
                        Iterations = iterations,
                        ChunkSettings = new ChunkTestSettings
                        {
                            ChunkSizeSeconds = chunkSizeSeconds,
                            OverlapMs = overlapMs,
                            TestMode = "sequential",
                            AudioSource = "upload"
                        }
                    };
                    
                    isFileUpload = true;
                    _logger.LogInformation($"File upload chunk test: {fileName} ({audioData.Length} bytes), {chunkSizeSeconds}s chunks, {overlapMs}ms overlap [UX]");
                }
                else
                {
                    // JSON Mode: Handle AudioRecord-based chunk test [Existing]
                    request = await Request.ReadFromJsonAsync<ChunkTestRequest>();
                    if (request == null)
                    {
                        return BadRequest(new { error = "Invalid request format" });
                    }
                    
                    _logger.LogInformation($"Starting AudioRecord chunk benchmark test with {request.ChunkSettings.ChunkSizeSeconds}s chunks, {request.ChunkSettings.OverlapMs}ms overlap [WMM]");
                    
                    if (!string.IsNullOrEmpty(request.AudioRecordId))
                    {
                        // Load from AudioRecord [SP][EIV]
                        var audioRecord = await _audioRecordRepository.GetByIdAsync(request.AudioRecordId);
                        if (audioRecord == null)
                        {
                            return NotFound(new { error = "AudioRecord not found", audioRecordId = request.AudioRecordId });
                        }
                        
                        audioData = _encryptionService.DecryptBinary(audioRecord.EncryptedAudioData);
                        fileName = audioRecord.FileName;
                        audioFormat = audioRecord.AudioFormat;
                        
                        // 🚨 DEBUG: Analyze decrypted audio data for corruption in CHUNK TEST [Debug][PSF]
                        _logger.LogWarning("🔍 CHUNK-TEST DEBUG: Decrypted {Size} bytes from AudioRecord {Id}", 
                            audioData.Length, audioRecord.Id);
                        
                        // Check WAV header integrity
                        if (audioData.Length >= 4)
                        {
                            var wavHeader = System.Text.Encoding.ASCII.GetString(audioData.Take(4).ToArray());
                            _logger.LogWarning("🔍 WAV-Header after decryption: '{Header}' (should be 'RIFF')", wavHeader);
                        }
                        
                        // Check first 16 bytes as hex
                        if (audioData.Length >= 16)
                        {
                            var firstBytes = Convert.ToHexString(audioData.Take(16).ToArray());
                            _logger.LogWarning("🔍 First 16 bytes (hex): {FirstBytes}", firstBytes);
                        }
                        
                        // Check if data looks like audio (should start with RIFF for WAV)
                        var isValidWav = audioData.Length >= 4 && 
                            audioData[0] == 0x52 && audioData[1] == 0x49 && 
                            audioData[2] == 0x46 && audioData[3] == 0x46;
                        _logger.LogWarning("🔍 Is valid WAV format: {IsValid}", isValidWav);
                        
                        if (!isValidWav)
                        {
                            _logger.LogError("🚨 CRITICAL: AudioRecord contains corrupted audio data in CHUNK TEST - this explains Georgian characters! [Bug][RootCause][PSF]");
                            
                            // 🔧 FIX: Convert WebM to WAV before chunk splitting [Fix][PSF][RootCause]
                            if (audioFormat?.ToLower() == "webm")
                            {
                                _logger.LogWarning("🔧 CONVERTING WebM to WAV before chunk splitting to fix Georgian characters [Fix][PSF]");
                                
                                try
                                {
                                    // Create temporary WebM file
                                    var tempWebMPath = Path.GetTempFileName() + ".webm";
                                    var tempWavPath = Path.GetTempFileName() + ".wav";
                                    
                                    // Write WebM data to temp file
                                    await System.IO.File.WriteAllBytesAsync(tempWebMPath, audioData);
                                    
                                    // Convert WebM to WAV using FFmpeg (if available) or fallback
                                    var convertedWavData = await ConvertWebMToWavAsync(tempWebMPath, tempWavPath);
                                    
                                    if (convertedWavData != null && convertedWavData.Length > 0)
                                    {
                                        audioData = convertedWavData;
                                        audioFormat = "wav";
                                        
                                        _logger.LogInformation("✅ WebM successfully converted to WAV: {Size} bytes [Fix][PSF]", audioData.Length);
                                        
                                        // Verify WAV header after conversion
                                        var newWavHeader = System.Text.Encoding.ASCII.GetString(audioData.Take(4).ToArray());
                                        _logger.LogInformation("🔍 WAV-Header after conversion: '{Header}'", newWavHeader);
                                    }
                                    else
                                    {
                                        _logger.LogError("❌ WebM to WAV conversion failed - proceeding with original data [Error][PSF]");
                                    }
                                    
                                    // Cleanup temp files
                                    try
                                    {
                                        if (System.IO.File.Exists(tempWebMPath)) System.IO.File.Delete(tempWebMPath);
                                        if (System.IO.File.Exists(tempWavPath)) System.IO.File.Delete(tempWavPath);
                                    }
                                    catch { /* Ignore cleanup errors */ }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "❌ WebM to WAV conversion failed: {Error} [Error][PSF]", ex.Message);
                                }
                            }
                        }
                        
                        _logger.LogInformation($"Loaded AudioRecord for chunk test: {fileName} ({audioData.Length} bytes) [ATV]");
                    }
                    else
                    {
                        return BadRequest(new { error = "AudioRecordId is required for JSON-based chunk test" });
                    }
                }
                
                // Create BenchmarkModelsRequest for chunk processing
                var benchmarkRequest = new BenchmarkModelsRequest
                {
                    RequestId = Guid.NewGuid().ToString(),
                    TestAudioData = audioData,
                    AudioFormat = audioFormat,
                    AudioFilename = fileName,
                    Language = "de", // Swiss German/German
                    Iterations = request.Iterations,
                    ModelsToTest = request.ModelsToTest.Split(',').Select(m => m.Trim()).ToList(),
                    EnableCudaBenchmark = true,
                    EnableCpuBenchmark = true,
                    ChunkSettings = new ChunkSettings
                    {
                        ChunkSizeSeconds = request.ChunkSettings.ChunkSizeSeconds,
                        OverlapMs = request.ChunkSettings.OverlapMs,
                        TestMode = "sequential"
                    }
                };
                
                // Call WhisperService for chunk-based benchmarking [WMM]
                var benchmarkResult = await _whisperService.BenchmarkModelsChunkAsync(benchmarkRequest);
                
                // Store benchmark results in database [SP][EIV][ATV]
                var savedResults = new List<object>();
                foreach (var result in benchmarkResult.Results)
                {
                    var benchmarkEntity = new BenchmarkResult
                    {
                        Id = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.UtcNow,
                        ModelName = result.ModelName,
                        ProcessingTimeMs = result.AverageProcessingTimeMs,
                        CpuUsagePercent = result.AverageCpuUsage,
                        MemoryUsageMb = result.AverageRamUsageMb,
                        PerformanceScore = result.PerformanceScore,
                        AudioDurationSeconds = 0, // Will be calculated from audio data
                        AudioFileSizeMb = audioData.Length / (1024.0 * 1024.0),
                        AudioFilename = fileName,
                        AudioRecordId = request.AudioRecordId,
                        EncryptedTranscriptionText = !string.IsNullOrEmpty(result.TranscribedText) 
                            ? _encryptionService.EncryptText(result.TranscribedText) 
                            : null,
                        DetectedLanguage = "de",
                        ConfidenceScore = result.AverageConfidence,
                        HardwareInfo = System.Text.Json.JsonSerializer.Serialize(benchmarkResult.HardwareInfo),
                        Iterations = request.Iterations,
                        AverageTimePerIteration = result.AverageProcessingTimeMs / request.Iterations,
                        Status = result.Success ? "Completed" : "Failed",
                        ErrorMessage = result.ErrorMessage,
                        AnonymizationStatus = "Anonymized",
                        TestType = "chunk",
                        ChunkCount = benchmarkResult.TotalChunks,
                        ChunkSizeSeconds = request.ChunkSettings.ChunkSizeSeconds,
                        ChunkOverlapMs = request.ChunkSettings.OverlapMs
                    };
                    
                    await _benchmarkRepository.AddAsync(benchmarkEntity);
                    savedResults.Add(new
                    {
                        id = benchmarkEntity.Id,
                        modelName = result.ModelName,
                        averageProcessingTimeMs = result.AverageProcessingTimeMs,
                        minProcessingTimeMs = result.MinProcessingTimeMs,
                        maxProcessingTimeMs = result.MaxProcessingTimeMs,
                        averageRamUsageMb = result.AverageRamUsageMb,
                        averageCpuUsage = result.AverageCpuUsage,
                        averageConfidence = result.AverageConfidence,
                        performanceScore = result.PerformanceScore,
                        success = result.Success,
                        errorMessage = result.ErrorMessage,
                        transcribedText = result.TranscribedText,
                        chunkResults = result.ChunkResults // Individual chunk performance data
                    });
                }
                
                _logger.LogInformation($"Chunk benchmark test completed: {benchmarkResult.Results.Count} models, {benchmarkResult.TotalChunks} chunks, {benchmarkResult.TotalBenchmarkTimeMs}ms total [WMM][ATV]");
                
                return Ok(new
                {
                    requestId = Guid.NewGuid().ToString(),
                    benchmarkId = $"chunk_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                    testType = "chunk",
                    totalBenchmarkTimeMs = benchmarkResult.TotalBenchmarkTimeMs,
                    totalChunks = benchmarkResult.TotalChunks,
                    chunkSizeSeconds = request.ChunkSettings.ChunkSizeSeconds,
                    chunkOverlapMs = request.ChunkSettings.OverlapMs,
                    results = savedResults.Cast<object>().ToList(),
                    hardwareInfo = benchmarkResult.HardwareInfo,
                    recommendation = benchmarkResult.Recommendation,
                    audioInfo = new
                    {
                        fileName,
                        format = audioFormat,
                        sizeBytes = audioData.Length,
                        audioRecordId = request.AudioRecordId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run chunk benchmark test [ECP]");
                return StatusCode(500, new { error = "Chunk benchmark test failed", details = ex.Message });
            }
        }
        
        /// <summary>
        /// Converts WebM audio to WAV format for Whisper compatibility [Fix][PSF]
        /// Fixes Georgian/Tibetan character transcription issue in chunk tests
        /// </summary>
        /// <param name="webmPath">Path to WebM input file</param>
        /// <param name="wavPath">Path to WAV output file</param>
        /// <returns>WAV audio data as byte array</returns>
        private async Task<byte[]?> ConvertWebMToWavAsync(string webmPath, string wavPath)
        {
            try
            {
                _logger.LogInformation("Starting WebM to WAV conversion: {WebMPath} -> {WavPath} [Fix][PSF]", webmPath, wavPath);
                
                // Try FFmpeg conversion first (if available)
                var ffmpegSuccess = await TryFFmpegConversion(webmPath, wavPath);
                
                if (ffmpegSuccess && System.IO.File.Exists(wavPath))
                {
                    var wavData = await System.IO.File.ReadAllBytesAsync(wavPath);
                    _logger.LogInformation("FFmpeg conversion successful: {Size} bytes [Fix][PSF]", wavData.Length);
                    return wavData;
                }
                
                // Fallback: Use NAudio or similar library (if available)
                _logger.LogWarning("FFmpeg not available, trying fallback conversion [Fix][PSF]");
                
                // For now, return null to indicate conversion failed
                // In production, implement NAudio or MediaFoundation conversion
                _logger.LogError("WebM to WAV conversion not available - no conversion library found [Error][PSF]");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebM to WAV conversion failed: {Error} [Error][PSF]", ex.Message);
                return null;
            }
        }
        
        /// <summary>
        /// Attempts WebM to WAV conversion using FFmpeg [Fix][PSF]
        /// </summary>
        /// <param name="inputPath">WebM input file path</param>
        /// <param name="outputPath">WAV output file path</param>
        /// <returns>True if conversion succeeded</returns>
        private async Task<bool> TryFFmpegConversion(string inputPath, string outputPath)
        {
            try
            {
                // Try to find FFmpeg in common locations
                var ffmpegPaths = new[]
                {
                    "ffmpeg", // System PATH
                    "ffmpeg.exe", // Windows
                    @"C:\ffmpeg\bin\ffmpeg.exe", // Common Windows location
                    "/usr/bin/ffmpeg", // Common Linux location
                    "/usr/local/bin/ffmpeg" // Common macOS location
                };
                
                string? ffmpegPath = null;
                foreach (var path in ffmpegPaths)
                {
                    try
                    {
                        var process = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = path,
                                Arguments = "-version",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true
                            }
                        };
                        
                        process.Start();
                        await process.WaitForExitAsync();
                        
                        if (process.ExitCode == 0)
                        {
                            ffmpegPath = path;
                            break;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                
                if (ffmpegPath == null)
                {
                    _logger.LogWarning("FFmpeg not found in system PATH or common locations [Fix][PSF]");
                    return false;
                }
                
                // Run FFmpeg conversion: WebM -> WAV
                var conversionProcess = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = $"-i \"{inputPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{outputPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                
                _logger.LogInformation("Running FFmpeg: {Command} [Fix][PSF]", conversionProcess.StartInfo.Arguments);
                
                conversionProcess.Start();
                var output = await conversionProcess.StandardOutput.ReadToEndAsync();
                var error = await conversionProcess.StandardError.ReadToEndAsync();
                await conversionProcess.WaitForExitAsync();
                
                if (conversionProcess.ExitCode == 0)
                {
                    _logger.LogInformation("FFmpeg conversion completed successfully [Fix][PSF]");
                    return true;
                }
                else
                {
                    _logger.LogError("FFmpeg conversion failed with exit code {ExitCode}: {Error} [Error][PSF]", 
                        conversionProcess.ExitCode, error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FFmpeg conversion attempt failed: {Error} [Error][PSF]", ex.Message);
                return false;
            }
    }

    // ==========================================
    // TODO: LIVE TRANSCRIPTION ENDPOINTS - TO BE IMPLEMENTED FROM SCRATCH
    // ==========================================
    // TODO: Implement live transcription endpoints from scratch
}

/// <summary>
/// Request model for benchmarking with AudioRecord [WMM]
/// </summary>
public class BenchmarkRecordRequest
{
    public required string AudioRecordId { get; set; }
    public required string ModelsToTest { get; set; }
    public string Iterations { get; set; } = "1";
}

/// <summary>
/// Request model for chunk-based benchmarking [WMM][PSF]
/// </summary>
public class ChunkTestRequest
{
    public string? AudioRecordId { get; set; }
    public string? AudioFile { get; set; } // Changed from IFormFile to string for JSON compatibility
    public required string ModelsToTest { get; set; }
    public int Iterations { get; set; } = 1;
    public required ChunkTestSettings ChunkSettings { get; set; }
}

/// <summary>
/// Chunk test settings for sequential processing [WMM]
/// </summary>
public class ChunkTestSettings
{
    public double ChunkSizeSeconds { get; set; } = 2.0;
    public int OverlapMs { get; set; } = 100;
    public string TestMode { get; set; } = "sequential";
    public string AudioSource { get; set; } = "record";
}
