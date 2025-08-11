// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MedEasy.Application.Interfaces;
using MedEasy.Application.Services;
using MedEasy.Domain.Interfaces;

namespace MedEasy.API.Controllers;

/// <summary>
/// Admin Controller for system monitoring and administration.
/// 
/// [PSF] Patient Safety First - System monitoring for reliability
/// [ATV] Audit Trail - All admin actions logged
/// [ZTS] Zero Tolerance Security - Admin access control
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IBenchmarkResultRepository _benchmarkRepository;
    private readonly IEncryptionService _encryptionService;

    public AdminController(
        ILogger<AdminController> logger,
        IBenchmarkResultRepository benchmarkRepository,
        IEncryptionService encryptionService)
    {
        _logger = logger;
        _benchmarkRepository = benchmarkRepository;
        _encryptionService = encryptionService;
    }

    // ❌ REMOVED: Redundant system/performance endpoint
    // Use SystemController.GetPerformance() instead (/api/system/performance)
    // [CAM] Clean Architecture - avoid controller responsibility overlap

    // ❌ REMOVED: Redundant system/health endpoint
    // Use SystemController.GetHealth() instead (/api/system/health)
    // [CAM] Clean Architecture - avoid controller responsibility overlap

    // ❌ REMOVED: Redundant system/stats endpoint  
    // Use SystemController.GetStats() instead (/api/system/stats)
    // [CAM] Clean Architecture - SystemController handles all system monitoring

    /// <summary>
    /// Get system logs with filtering
    /// [ATV] Audit trail log access
    /// </summary>
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? level = null,
        [FromQuery] string? service = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        try
        {
            // [ATV] Log access to logs for audit trail
            _logger.LogInformation("System logs requested with filters: Level={Level}, Service={Service}, From={From}, To={To}, Search={Search}", 
                level, service, from, to, search);

            // TODO: Implement real log aggregation
            // For now, return mock data structure
            var logs = new object[]
            {
                new
                {
                    id = Guid.NewGuid().ToString(),
                    timestamp = DateTime.UtcNow.AddMinutes(-5).ToString("O"),
                    level = "Info",
                    service = "Backend",
                    message = "System performance metrics collected",
                    requestId = Guid.NewGuid().ToString(),
                    userId = "admin",
                    details = new { cpuUsage = 45.2, ramUsage = 8.1 }
                },
                new
                {
                    id = Guid.NewGuid().ToString(),
                    timestamp = DateTime.UtcNow.AddMinutes(-3).ToString("O"),
                    level = "Warning",
                    service = "AI-Service",
                    message = "High memory usage detected during benchmark",
                    requestId = Guid.NewGuid().ToString(),
                    userId = "system",
                    details = new { memoryUsage = 15.8, threshold = 12.0 }
                }
            };

            return Ok(new
            {
                logs = logs,
                totalCount = logs.Length,
                page = page,
                pageSize = pageSize,
                hasMore = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system logs");
            return StatusCode(500, new { error = "Failed to retrieve logs" });
        }
    }

    /// <summary>
    /// Get benchmark history
    /// [WMM] Whisper benchmark history tracking
    /// </summary>
    [HttpGet("benchmarks/history")]
    public async Task<IActionResult> GetBenchmarkHistory(
        [FromQuery] string? period = "7d",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            // [ATV] Log benchmark history access
            _logger.LogInformation("Benchmark history requested for period: {Period}", period);

            // Get real benchmark history from database [ATV][PSF]
            var benchmarkResults = await _benchmarkRepository.GetRecentBenchmarksAsync(
                pageSize, 
                (page - 1) * pageSize
            );
            
            var totalCount = await _benchmarkRepository.GetTotalCountAsync();
            
            // Transform database entities to API response format with decryption
            // [SP][AIU] Create tasks for async decryption mapping
            var historyTasks = benchmarkResults.Select(async br =>
            {
                // [SP][AIU] Decrypt transcribed text for frontend display with workaround for legacy entries
                string? transcribedText = null;
                if (br.EncryptedTranscriptionText != null && br.EncryptedTranscriptionText.Length > 0)
                {
                    try
                    {
                        transcribedText = await _encryptionService.DecryptAsync(br.EncryptedTranscriptionText);
                        _logger.LogDebug("Transcribed text decrypted for benchmark {BenchmarkId}: {TextLength} chars", 
                            br.Id, transcribedText?.Length ?? 0);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to decrypt transcribed text for benchmark {BenchmarkId}", br.Id);
                        transcribedText = "[DECRYPTION_FAILED]";
                    }
                }
                else
                {
                    // [WORKAROUND] Handle legacy entries without encrypted transcription text
                    transcribedText = "[LEGACY_ENTRY_NO_TRANSCRIPTION]";
                    _logger.LogDebug("Legacy benchmark entry {BenchmarkId} has no encrypted transcription text", br.Id);
                }

                return new
                {
                    benchmarkId = br.Id,
                    requestId = br.SessionId ?? br.Id,
                    timestamp = br.Timestamp.ToString("O"),
                    
                    // Frontend expects these exact field names
                    status = br.Status.ToLower() == "completed" ? "completed" : "failed",
                    totalTimeMs = (int)br.ProcessingTimeMs, 
                    modelsCount = 1, 
                    
                    // Model information for frontend display
                    fastestModel = br.ModelName,
                    fastestTimeMs = (int)br.ProcessingTimeMs,
                    slowestModel = br.ModelName,
                    slowestTimeMs = (int)br.ProcessingTimeMs,
                    
                    // Performance data at top level for frontend
                    averageRamUsageMb = (int)br.MemoryUsageMb,
                    performanceScore = br.PerformanceScore, 
                    
                    // [NEW] Transcribed text for comparison and display
                    transcribedText = transcribedText,
                    
                    // Confidence fields moved to top of response object
                    confidenceScore = br.ConfidenceScore,
                    
                    // Hardware information for frontend
                    cpuUsage = br.CpuUsagePercent,
                    ramUsage = br.MemoryUsageMb,
                    
                    // Audio file information
                    audioFileName = br.AudioFilename,
                    audioFileSizeMb = br.AudioFileSizeMb,
                    
                    // Detailed results array (for future use)
                    results = new[]
                    {
                        new
                        {
                            modelName = br.ModelName,
                            averageProcessingTimeMs = (int)br.ProcessingTimeMs,
                            minProcessingTimeMs = (int)br.AverageTimePerIteration,
                            maxProcessingTimeMs = (int)br.ProcessingTimeMs,
                            averageAccuracy = br.ConfidenceScore,
                            averageConfidence = br.ConfidenceScore,
                            cudaUsed = false,
                            averageCpuUsage = br.CpuUsagePercent,
                            averageGpuUsage = 0.0,
                            averageRamUsageMb = br.MemoryUsageMb,
                            averageVramUsageMb = 0.0,
                            performanceScore = br.PerformanceScore,
                            accuracyScore = br.ConfidenceScore * 100,
                            success = br.Status == "Completed",
                            errorMessage = br.ErrorMessage ?? ""
                        }
                    }
                };
            }).ToArray();

            // [SP] Await all decryption tasks to complete
            var history = await Task.WhenAll(historyTasks);

            return Ok(new
            {
                history = history,
                totalCount = totalCount,
                page = page,
                pageSize = pageSize,
                hasMore = (page * pageSize) < totalCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get benchmark history");
            return StatusCode(500, new { error = "Failed to retrieve benchmark history" });
        }
    }
}
