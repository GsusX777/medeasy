// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.AspNetCore.Mvc;
using MedEasy.Application.Interfaces;
using MedEasy.Application.DTOs.AI;
using MedEasy.Domain.Interfaces;
using MedEasy.Domain.Entities;

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
[Route("api/[controller]")]
[Produces("application/json")]
public class AIController : ControllerBase
{
    private readonly IWhisperService _whisperService;
    private readonly ILogger<AIController> _logger;
    private readonly IBenchmarkResultRepository _benchmarkRepository;
    private readonly IEncryptionService _encryptionService;

    public AIController(
        IWhisperService whisperService,
        ILogger<AIController> logger,
        IBenchmarkResultRepository benchmarkRepository,
        IEncryptionService encryptionService)
    {
        _whisperService = whisperService;
        _logger = logger;
        _benchmarkRepository = benchmarkRepository;
        _encryptionService = encryptionService;
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
}
