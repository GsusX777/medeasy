// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using System.ComponentModel.DataAnnotations;

namespace MedEasy.Domain.Entities;

/// <summary>
/// Entity für Whisper Benchmark-Ergebnisse [WMM][ATV]
/// Speichert Performance-Metriken für verschiedene Whisper-Modelle
/// [PSF] Critical für Performance-Monitoring und Optimierung
/// </summary>
public class BenchmarkResult
{
    /// <summary>
    /// Eindeutige ID des Benchmark-Ergebnisses [ATV]
    /// </summary>
    [Key]
    public required string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Zeitstempel der Benchmark-Ausführung [ATV]
    /// </summary>
    public required DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Name des getesteten Whisper-Modells [WMM]
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string ModelName { get; init; }

    /// <summary>
    /// Verarbeitungszeit in Millisekunden [PSF]
    /// </summary>
    public required double ProcessingTimeMs { get; init; }

    /// <summary>
    /// CPU-Auslastung während der Verarbeitung (%) [PSF]
    /// </summary>
    public required double CpuUsagePercent { get; init; }

    /// <summary>
    /// RAM-Verbrauch in MB [PSF]
    /// </summary>
    public required double MemoryUsageMb { get; init; }

    /// <summary>
    /// Performance-Score (berechnet) [PSF]
    /// </summary>
    public required double PerformanceScore { get; init; }

    /// <summary>
    /// Länge der Audio-Datei in Sekunden [WMM]
    /// </summary>
    public required double AudioDurationSeconds { get; init; }

    /// <summary>
    /// Größe der Audio-Datei in MB [WMM]
    /// </summary>
    public required double AudioFileSizeMb { get; init; }

    /// <summary>
    /// Name der ursprünglichen Audio-Datei [WMM][ATV]
    /// </summary>
    [Required]
    [MaxLength(255)]
    public required string AudioFilename { get; init; } = "unknown.m4a";

    /// <summary>
    /// Verschlüsselter Transkriptions-Text [SP][AIU]
    /// Enthält den tatsächlichen transkribierten Text, verschlüsselt mit AES-256-GCM
    /// Für Benchmarks: '[BENCHMARK_ONLY]' oder echter Text je nach Anforderung
    /// </summary>
    public byte[]? EncryptedTranscriptionText { get; init; }

    /// <summary>
    /// Erkannte Sprache [WMM]
    /// </summary>
    [MaxLength(10)]
    public required string DetectedLanguage { get; init; } = "de";

    /// <summary>
    /// Konfidenz-Score der Transkription (0-1) [WMM]
    /// </summary>
    public required double ConfidenceScore { get; init; }

    /// <summary>
    /// Hardware-Informationen (JSON) [PSF]
    /// </summary>
    [Required]
    public required string HardwareInfo { get; init; }

    /// <summary>
    /// Anzahl der Iterationen für diesen Benchmark [PSF]
    /// </summary>
    public required int Iterations { get; init; } = 1;

    /// <summary>
    /// Durchschnittliche Zeit pro Iteration [PSF]
    /// </summary>
    public required double AverageTimePerIteration { get; init; }

    /// <summary>
    /// Fehler-Informationen (falls aufgetreten) [FSD]
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Status des Benchmarks [ATV]
    /// </summary>
    [Required]
    [MaxLength(20)]
    public required string Status { get; init; } = "Completed";

    /// <summary>
    /// Audit-Informationen: Wer hat den Benchmark ausgeführt [ATV]
    /// </summary>
    [MaxLength(100)]
    public string? UserId { get; init; }

    /// <summary>
    /// Session-ID für Audit-Trail [ATV]
    /// </summary>
    [MaxLength(100)]
    public string? SessionId { get; init; }

    /// <summary>
    /// Soft Delete Flag [ATV]
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Zeitstempel der Löschung [ATV]
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Anonymisierungsstatus [AIU]
    /// </summary>
    [Required]
    [MaxLength(20)]
    public required string AnonymizationStatus { get; init; } = "Anonymized";
}
