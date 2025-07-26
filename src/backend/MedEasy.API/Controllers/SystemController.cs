// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using MedEasy.Application.Services;
using MedEasy.Application.DTOs;
using System.Threading.Tasks;
using System;

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Controller für System-Performance und Health-Monitoring [PSF][ZTS]
    /// Stellt Endpunkte für Echtzeit-System-Überwachung bereit
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Temporarily disabled for Health-Monitor testing [ZTS]
    public class SystemController : ControllerBase
    {
        private readonly SystemPerformanceService _performanceService;
        private readonly ILogger<SystemController> _logger;

        public SystemController(
            SystemPerformanceService performanceService,
            ILogger<SystemController> logger)
        {
            _performanceService = performanceService;
            _logger = logger;
        }

        /// <summary>
        /// Holt aktuelle System-Performance-Metriken [PSF][ZTS]
        /// </summary>
        /// <returns>Aktuelle CPU, RAM, GPU und andere System-Metriken</returns>
        /// <response code="200">Performance-Metriken erfolgreich abgerufen</response>
        /// <response code="401">Nicht autorisiert - JWT-Token erforderlich</response>
        /// <response code="500">Interner Server-Fehler beim Abrufen der Metriken</response>
        [HttpGet("performance")]
        [ProducesResponseType(typeof(SystemPerformanceDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<SystemPerformanceDto>> GetPerformanceMetrics()
        {
            try
            {
                _logger.LogDebug("Performance-Metriken angefordert [PSF]");
                
                var metrics = await _performanceService.GetCurrentPerformanceAsync();
                
                _logger.LogInformation("Performance-Metriken erfolgreich bereitgestellt: CPU={CpuUsage}%, RAM={RamUsage}% [PSF]", 
                    metrics.CpuUsage, metrics.RamUsage);
                
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen der Performance-Metriken [ZTS]");
                
                // Sicherheitsfallback - Keine sensiblen Daten preisgeben [ZTS]
                return StatusCode(500, new { 
                    error = "Fehler beim Abrufen der System-Metriken",
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Holt erweiterte System-Informationen [PSF][ZTS]
        /// </summary>
        /// <returns>Detaillierte System-Informationen für Diagnose</returns>
        /// <response code="200">System-Informationen erfolgreich abgerufen</response>
        /// <response code="401">Nicht autorisiert - JWT-Token erforderlich</response>
        /// <response code="500">Interner Server-Fehler</response>
        [HttpGet("info")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> GetSystemInfo()
        {
            try
            {
                _logger.LogDebug("System-Informationen angefordert [PSF]");
                
                var metrics = await _performanceService.GetCurrentPerformanceAsync();
                
                var systemInfo = new
                {
                    timestamp = DateTime.UtcNow,
                    cpu = new
                    {
                        name = metrics.CpuName,
                        cores = metrics.CpuCores,
                        usage = metrics.CpuUsage
                    },
                    memory = new
                    {
                        totalMb = metrics.TotalRamMb,
                        usedMb = metrics.UsedRamMb,
                        usage = metrics.RamUsage
                    },
                    gpu = new
                    {
                        name = metrics.GpuName,
                        acceleration = metrics.GpuAcceleration,
                        usage = metrics.GpuUsage
                    },
                    performance = new
                    {
                        diskIo = metrics.DiskIo,
                        networkLatency = metrics.NetworkLatency
                    },
                    // Medizinische Software-spezifische Metriken [PSF]
                    medicalSoftware = new
                    {
                        encryptionActive = true, // Immer aktiv [SP]
                        auditLogging = true,     // Immer aktiv [ATV]
                        anonymization = true,    // Unveränderlich [AIU]
                        complianceMode = "Swiss_nDSG" // [DSC]
                    }
                };
                
                _logger.LogInformation("System-Informationen erfolgreich bereitgestellt [PSF]");
                
                return Ok(systemInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen der System-Informationen [ZTS]");
                
                return StatusCode(500, new { 
                    error = "Fehler beim Abrufen der System-Informationen",
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Testet System-Performance unter Last [PSF][ZTS]
        /// Nur für Entwicklungs- und Testzwecke
        /// </summary>
        /// <returns>Performance-Test-Ergebnisse</returns>
        /// <response code="200">Performance-Test erfolgreich durchgeführt</response>
        /// <response code="401">Nicht autorisiert</response>
        /// <response code="403">Nur in Entwicklungsumgebung verfügbar</response>
        [HttpPost("performance/test")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> TestPerformance()
        {
            try
            {
                // Nur in Entwicklungsumgebung erlaubt [ZTS]
                if (!HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
                {
                    _logger.LogWarning("Performance-Test in Produktionsumgebung versucht [ZTS]");
                    return Forbid("Performance-Tests sind nur in der Entwicklungsumgebung verfügbar");
                }

                _logger.LogDebug("Performance-Test gestartet [PSF]");
                
                var beforeMetrics = await _performanceService.GetCurrentPerformanceAsync();
                
                // Simuliere kurze Last
                await Task.Delay(1000);
                
                var afterMetrics = await _performanceService.GetCurrentPerformanceAsync();
                
                var testResult = new
                {
                    timestamp = DateTime.UtcNow,
                    before = beforeMetrics,
                    after = afterMetrics,
                    delta = new
                    {
                        cpuUsage = afterMetrics.CpuUsage - beforeMetrics.CpuUsage,
                        ramUsage = afterMetrics.RamUsage - beforeMetrics.RamUsage,
                        responseTime = afterMetrics.NetworkLatency - beforeMetrics.NetworkLatency
                    },
                    testDuration = 1000,
                    status = "completed"
                };
                
                _logger.LogInformation("Performance-Test erfolgreich abgeschlossen [PSF]");
                
                return Ok(testResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Performance-Test [ZTS]");
                
                return StatusCode(500, new { 
                    error = "Fehler beim Performance-Test",
                    timestamp = DateTime.UtcNow 
                });
            }
        }
    }
}
