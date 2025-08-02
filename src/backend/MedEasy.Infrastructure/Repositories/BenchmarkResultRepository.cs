// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MedEasy.Domain.Entities;
using MedEasy.Domain.Interfaces;
using MedEasy.Infrastructure.Database;

namespace MedEasy.Infrastructure.Repositories;

/// <summary>
/// Repository-Implementierung für Benchmark-Ergebnisse [ATV][PSF]
/// [SP] SQLCipher-verschlüsselte Speicherung
/// [WMM] Whisper Multi-Model Benchmark-Verwaltung
/// </summary>
public class BenchmarkResultRepository : IBenchmarkResultRepository
{
    private readonly SQLCipherContext _context;
    private readonly ILogger<BenchmarkResultRepository> _logger;

    public BenchmarkResultRepository(
        SQLCipherContext context,
        ILogger<BenchmarkResultRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Speichert ein neues Benchmark-Ergebnis [ATV]
    /// </summary>
    public async Task<BenchmarkResult> AddAsync(BenchmarkResult benchmarkResult)
    {
        try
        {
            // [ATV] Audit-Logging für Benchmark-Speicherung
            _logger.LogInformation("Storing benchmark result for model {ModelName} with ID {Id}", 
                benchmarkResult.ModelName, benchmarkResult.Id);

            _context.BenchmarkResults.Add(benchmarkResult);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully stored benchmark result {Id}", benchmarkResult.Id);
            return benchmarkResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store benchmark result {Id}", benchmarkResult.Id);
            throw;
        }
    }

    /// <summary>
    /// Holt ein Benchmark-Ergebnis anhand der ID [ATV]
    /// </summary>
    public async Task<BenchmarkResult?> GetByIdAsync(string id)
    {
        try
        {
            return await _context.BenchmarkResults
                .Where(b => !b.IsDeleted && b.Id == id)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get benchmark result {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Holt alle Benchmark-Ergebnisse mit Paginierung [PSF]
    /// </summary>
    public async Task<IEnumerable<BenchmarkResult>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            return await _context.BenchmarkResults
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.Timestamp)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get benchmark results page {Page}", page);
            throw;
        }
    }

    /// <summary>
    /// Holt Benchmark-Ergebnisse für ein bestimmtes Modell [WMM]
    /// </summary>
    public async Task<IEnumerable<BenchmarkResult>> GetByModelAsync(string modelName, int limit = 20)
    {
        try
        {
            return await _context.BenchmarkResults
                .Where(b => !b.IsDeleted && b.ModelName == modelName)
                .OrderByDescending(b => b.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get benchmark results for model {ModelName}", modelName);
            throw;
        }
    }

    /// <summary>
    /// Holt die neuesten Benchmark-Ergebnisse [PSF]
    /// </summary>
    public async Task<IEnumerable<BenchmarkResult>> GetRecentAsync(int limit = 10)
    {
        try
        {
            return await _context.BenchmarkResults
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent benchmark results");
            throw;
        }
    }

    /// <summary>
    /// Holt die neuesten Benchmark-Ergebnisse mit Paginierung [WMM][ATV]
    /// Für Admin-Panel Benchmark-History
    /// </summary>
    public async Task<IEnumerable<BenchmarkResult>> GetRecentBenchmarksAsync(int pageSize = 50, int offset = 0)
    {
        try
        {
            _logger.LogInformation("Getting recent benchmarks with pageSize {PageSize}, offset {Offset}", pageSize, offset);
            
            return await _context.BenchmarkResults
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.Timestamp)
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent benchmark results with pagination");
            throw;
        }
    }

    /// <summary>
    /// Holt Benchmark-Statistiken für Dashboard [PSF]
    /// </summary>
    public async Task<BenchmarkStatistics> GetStatisticsAsync()
    {
        try
        {
            var benchmarks = await _context.BenchmarkResults
                .Where(b => !b.IsDeleted)
                .ToListAsync();

            if (!benchmarks.Any())
            {
                return new BenchmarkStatistics();
            }

            var modelGroups = benchmarks.GroupBy(b => b.ModelName).ToList();

            return new BenchmarkStatistics
            {
                TotalBenchmarks = benchmarks.Count,
                AverageProcessingTime = benchmarks.Average(b => b.ProcessingTimeMs),
                AverageCpuUsage = benchmarks.Average(b => b.CpuUsagePercent),
                AverageMemoryUsage = benchmarks.Average(b => b.MemoryUsageMb),
                FastestModel = modelGroups
                    .OrderBy(g => g.Average(b => b.ProcessingTimeMs))
                    .FirstOrDefault()?.Key ?? "Unknown",
                MostEfficientModel = modelGroups
                    .OrderByDescending(g => g.Average(b => b.PerformanceScore))
                    .FirstOrDefault()?.Key ?? "Unknown",
                LastBenchmark = benchmarks.Max(b => b.Timestamp),
                ModelUsageCount = modelGroups.ToDictionary(g => g.Key, g => g.Count()),
                ModelAveragePerformance = modelGroups.ToDictionary(
                    g => g.Key, 
                    g => g.Average(b => b.PerformanceScore))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get benchmark statistics");
            throw;
        }
    }

    /// <summary>
    /// Löscht ein Benchmark-Ergebnis (Soft Delete) [ATV]
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        try
        {
            var benchmark = await _context.BenchmarkResults
                .FirstOrDefaultAsync(b => b.Id == id);

            if (benchmark != null)
            {
                benchmark.IsDeleted = true;
                benchmark.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted benchmark result {Id}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete benchmark result {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Holt Benchmark-Ergebnisse in einem Zeitraum [ATV]
    /// </summary>
    public async Task<IEnumerable<BenchmarkResult>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        try
        {
            return await _context.BenchmarkResults
                .Where(b => !b.IsDeleted && b.Timestamp >= from && b.Timestamp <= to)
                .OrderByDescending(b => b.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get benchmark results for date range {From} - {To}", from, to);
            throw;
        }
    }

    /// <summary>
    /// Zählt die Gesamtanzahl der Benchmark-Ergebnisse [PSF]
    /// </summary>
    public async Task<int> GetTotalCountAsync()
    {
        try
        {
            return await _context.BenchmarkResults
                .Where(b => !b.IsDeleted)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get total benchmark count");
            throw;
        }
    }
}
