// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using MedEasy.Domain.Entities;

namespace MedEasy.Domain.Interfaces;

/// <summary>
/// Repository Interface für Benchmark-Ergebnisse [ATV][PSF]
/// [WMM] Whisper Multi-Model Benchmark-Verwaltung
/// </summary>
public interface IBenchmarkResultRepository
{
    /// <summary>
    /// Speichert ein neues Benchmark-Ergebnis [ATV]
    /// </summary>
    Task<BenchmarkResult> AddAsync(BenchmarkResult benchmarkResult);

    /// <summary>
    /// Holt ein Benchmark-Ergebnis anhand der ID [ATV]
    /// </summary>
    Task<BenchmarkResult?> GetByIdAsync(string id);

    /// <summary>
    /// Holt alle Benchmark-Ergebnisse mit Paginierung [PSF]
    /// </summary>
    Task<IEnumerable<BenchmarkResult>> GetAllAsync(int page = 1, int pageSize = 50);

    /// <summary>
    /// Holt Benchmark-Ergebnisse für ein bestimmtes Modell [WMM]
    /// </summary>
    Task<IEnumerable<BenchmarkResult>> GetByModelAsync(string modelName, int limit = 20);

    /// <summary>
    /// Holt die neuesten Benchmark-Ergebnisse [PSF]
    /// </summary>
    Task<IEnumerable<BenchmarkResult>> GetRecentAsync(int limit = 10);

    /// <summary>
    /// Holt die neuesten Benchmark-Ergebnisse mit Paginierung [WMM][ATV]
    /// Für Admin-Panel Benchmark-History
    /// </summary>
    Task<IEnumerable<BenchmarkResult>> GetRecentBenchmarksAsync(int pageSize = 50, int offset = 0);

    /// <summary>
    /// Holt Benchmark-Statistiken für Dashboard [PSF]
    /// </summary>
    Task<BenchmarkStatistics> GetStatisticsAsync();

    /// <summary>
    /// Löscht ein Benchmark-Ergebnis (Soft Delete) [ATV]
    /// </summary>
    Task DeleteAsync(string id);

    /// <summary>
    /// Holt Benchmark-Ergebnisse in einem Zeitraum [ATV]
    /// </summary>
    Task<IEnumerable<BenchmarkResult>> GetByDateRangeAsync(DateTime from, DateTime to);

    /// <summary>
    /// Zählt die Gesamtanzahl der Benchmark-Ergebnisse [PSF]
    /// </summary>
    Task<int> GetTotalCountAsync();
}

/// <summary>
/// Statistiken für Benchmark-Ergebnisse [PSF]
/// </summary>
public class BenchmarkStatistics
{
    public int TotalBenchmarks { get; set; }
    public double AverageProcessingTime { get; set; }
    public double AverageCpuUsage { get; set; }
    public double AverageMemoryUsage { get; set; }
    public string FastestModel { get; set; } = string.Empty;
    public string MostEfficientModel { get; set; } = string.Empty;
    public DateTime LastBenchmark { get; set; }
    public Dictionary<string, int> ModelUsageCount { get; set; } = new();
    public Dictionary<string, double> ModelAveragePerformance { get; set; } = new();
}
