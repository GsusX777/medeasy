// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using System.ComponentModel.DataAnnotations;

namespace MedEasy.Domain.Entities;

/// <summary>
/// Entity für Audio-Aufnahmen [SP][EIV][ATV]
/// Speichert verschlüsselte Audio-Daten für Test- und Benchmark-Zwecke (Admin-Bereich)
/// [PSF] Critical für sichere Audio-Verarbeitung und Anonymisierung
/// </summary>
public class AudioRecord : IHasAuditInfo
{
    /// <summary>
    /// Eindeutige ID des Audio-Records [ATV]
    /// </summary>
    [Key]
    public required string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Original-Dateiname [WMM]
    /// </summary>
    [Required]
    [MaxLength(255)]
    public required string FileName { get; init; }

    /// <summary>
    /// Verschlüsselte Audio-Daten [EIV][SP]
    /// Enthält die kompletten Audio-Daten verschlüsselt mit AES-256-GCM
    /// </summary>
    public required byte[] EncryptedAudioData { get; init; }

    /// <summary>
    /// Dateigröße in Bytes [WMM]
    /// </summary>
    public required long FileSizeBytes { get; init; }

    /// <summary>
    /// Aufnahmedauer in Sekunden [WMM]
    /// </summary>
    public required double DurationSeconds { get; init; }

    /// <summary>
    /// Art der Aufnahme [WMM]
    /// </summary>
    [Required]
    [MaxLength(20)]
    public required string RecordingType { get; init; } // 'upload', 'recording', 'live_chunk'

    /// <summary>
    /// Audio-Format [WMM]
    /// </summary>
    [Required]
    [MaxLength(10)]
    public required string AudioFormat { get; init; } // 'webm', 'wav', 'mp3'

    /// <summary>
    /// Abtastrate in Hz (z.B. 16000 für 16kHz) [WMM]
    /// </summary>
    public required int SampleRate { get; init; }

    /// <summary>
    /// Bitrate in bps (z.B. 256000 für 256kbps) [SF]
    /// </summary>
    public required int BitRate { get; init; }

    /// <summary>
    /// Anzahl Kanäle (1=Mono, 2=Stereo) [WMM]
    /// </summary>
    public required int Channels { get; init; }

    /// <summary>
    /// Flag, ob anonymisiert (immer 1) [AIU]
    /// Anonymisierung ist unveränderlich und kann nie deaktiviert werden
    /// </summary>
    public required bool IsAnonymized { get; init; } = true;

    /// <summary>
    /// Konfidenz der Anonymisierung (0-1) [AIU]
    /// </summary>
    public required double AnonymizationConfidence { get; init; }

    /// <summary>
    /// Flag für manuelle Überprüfung (0/1) [ARQ]
    /// </summary>
    public required bool NeedsReview { get; init; } = false;

    /// <summary>
    /// Verknüpfung zu Benchmark-Ergebnis [WMM]
    /// </summary>
    [MaxLength(50)]
    public string? BenchmarkId { get; init; }

    /// <summary>
    /// Verarbeitungszeit in Millisekunden [PSF]
    /// </summary>
    public int? ProcessingTimeMs { get; init; }

    /// <summary>
    /// Verwendetes Whisper-Modell [WMM]
    /// </summary>
    [MaxLength(20)]
    public string? ModelUsed { get; init; }

    // IHasAuditInfo implementation [ATV]
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string? LastModifiedBy { get; set; }
}

/// <summary>
/// Enum für RecordingType [WMM]
/// </summary>
public static class RecordingType
{
    public const string Upload = "upload";
    public const string Recording = "recording";
    public const string LiveChunk = "live_chunk";
}
