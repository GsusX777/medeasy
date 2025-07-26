// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MedEasy.Application.DTOs;

/// <summary>
/// Session Response DTO - Entschlüsselte Daten für API-Antworten [SP][AIU]
/// Folgt Schweizer Compliance-Anforderungen [SF]
/// </summary>
public class SessionDto
{
    /// <summary>
    /// Session ID als string (Guid-Konvertierung) [ZTS]
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Patient ID als string (Guid-Konvertierung) [ZTS]
    /// </summary>
    public string PatientId { get; set; } = string.Empty;

    /// <summary>
    /// Sitzungsdatum im ISO-8601 Format für API [SF]
    /// </summary>
    public DateTime SessionDate { get; set; }

    /// <summary>
    /// Formatiertes Sitzungsdatum für UI (DD.MM.YYYY) [SF]
    /// </summary>
    [JsonIgnore]
    public string SessionDateFormatted => SessionDate.ToString("dd.MM.yyyy");

    /// <summary>
    /// Startzeit der Sitzung [SF]
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Endzeit der Sitzung (optional) [SF]
    /// </summary>
    public TimeSpan? EndTime { get; set; }

    /// <summary>
    /// Formatierte Startzeit für UI (HH:mm) [SF]
    /// </summary>
    [JsonIgnore]
    public string StartTimeFormatted => StartTime.ToString(@"hh\:mm");

    /// <summary>
    /// Formatierte Endzeit für UI (HH:mm) [SF]
    /// </summary>
    [JsonIgnore]
    public string? EndTimeFormatted => EndTime?.ToString(@"hh\:mm");

    /// <summary>
    /// Grund der Konsultation [PSF]
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Status der Sitzung [ATV]
    /// </summary>
    public SessionStatus Status { get; set; }

    /// <summary>
    /// Entschlüsselte Notizen [SP]
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Anonymisierte Notizen für Review [AIU]
    /// </summary>
    public string? AnonymizedNotes { get; set; }

    /// <summary>
    /// Anonymisierungs-Status [AIU]
    /// </summary>
    public AnonymizationStatus AnonymizationStatus { get; set; }

    /// <summary>
    /// Erstellungsdatum [ATV]
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Letzte Änderung [ATV]
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Erstellt von (Benutzer) [ATV]
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Zuletzt geändert von [ATV]
    /// </summary>
    public string LastModifiedBy { get; set; } = string.Empty;
}

/// <summary>
/// Session Request DTO für Erstellung [ZTS][PSF]
/// </summary>
public class CreateSessionRequest
{
    /// <summary>
    /// Patient ID als string (wird zu Guid konvertiert) [ZTS]
    /// </summary>
    [Required(ErrorMessage = "Patient ID ist erforderlich")]
    public string PatientId { get; set; } = string.Empty;

    /// <summary>
    /// Grund der Konsultation [PSF]
    /// </summary>
    [Required(ErrorMessage = "Grund der Konsultation ist erforderlich")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Grund muss zwischen 5 und 500 Zeichen lang sein")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Sitzungsdatum (optional, Standard: heute) [SF]
    /// </summary>
    public DateTime? SessionDate { get; set; }

    /// <summary>
    /// Startzeit (optional, Standard: jetzt) [SF]
    /// </summary>
    public TimeSpan? StartTime { get; set; }

    /// <summary>
    /// Validiert die Patient ID als Guid [ZTS]
    /// </summary>
    public bool IsValidPatientId()
    {
        return Guid.TryParse(PatientId, out _);
    }
}

/// <summary>
/// Session Update DTO für Änderungen [ZTS][ATV]
/// </summary>
public class UpdateSessionRequest
{
    /// <summary>
    /// Grund der Konsultation (optional bei Update)
    /// </summary>
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Grund muss zwischen 5 und 500 Zeichen lang sein")]
    public string? Reason { get; set; }

    /// <summary>
    /// Notizen zur Sitzung [PSF]
    /// </summary>
    [StringLength(5000, ErrorMessage = "Notizen dürfen maximal 5000 Zeichen lang sein")]
    public string? Notes { get; set; }

    /// <summary>
    /// Endzeit der Sitzung [SF]
    /// </summary>
    public TimeSpan? EndTime { get; set; }

    /// <summary>
    /// Status der Sitzung [ATV]
    /// </summary>
    public SessionStatus? Status { get; set; }
}

/// <summary>
/// Session Status Enumeration [ATV]
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// Sitzung geplant
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Sitzung läuft
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Sitzung abgeschlossen
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Sitzung abgebrochen
    /// </summary>
    Cancelled = 3
}

/// <summary>
/// Anonymization Status Enumeration [AIU]
/// </summary>
public enum AnonymizationStatus
{
    /// <summary>
    /// Noch nicht anonymisiert
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Automatisch anonymisiert (hohe Konfidenz)
    /// </summary>
    AutoAnonymized = 1,

    /// <summary>
    /// Wartet auf manuelle Review (niedrige Konfidenz) [ARQ]
    /// </summary>
    PendingReview = 2,

    /// <summary>
    /// Manuell überprüft und freigegeben [ARQ]
    /// </summary>
    ReviewApproved = 3,

    /// <summary>
    /// Review abgelehnt, erneute Anonymisierung erforderlich [ARQ]
    /// </summary>
    ReviewRejected = 4
}
