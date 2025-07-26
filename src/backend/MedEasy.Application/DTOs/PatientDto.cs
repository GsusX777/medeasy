// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MedEasy.Application.DTOs;

/// <summary>
/// Patient Response DTO - Entschlüsselte Daten für API-Antworten [SP][AIU]
/// Folgt Schweizer Compliance-Anforderungen [SF]
/// </summary>
public class PatientDto
{
    /// <summary>
    /// Patient ID als string (Guid-Konvertierung) [ZTS]
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Entschlüsselter Vorname [SP]
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Entschlüsselter Nachname [SP]
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Geburtsdatum im ISO-8601 Format für API [SF]
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Formatiertes Geburtsdatum für UI (DD.MM.YYYY) [SF]
    /// </summary>
    [JsonIgnore]
    public string DateOfBirthFormatted => DateOfBirth.ToString("dd.MM.yyyy");

    /// <summary>
    /// Maskierte Versicherungsnummer (XXX.XXXX.XXXX.##) [AIU][SF]
    /// </summary>
    public string InsuranceNumberMasked { get; set; } = string.Empty;

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
/// Patient Request DTO für Erstellung - Validierte Eingabedaten [ZTS][SF]
/// </summary>
public class CreatePatientRequest
{
    /// <summary>
    /// Vorname (erforderlich) [PSF]
    /// </summary>
    [Required(ErrorMessage = "Vorname ist erforderlich")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Vorname muss zwischen 1 und 100 Zeichen lang sein")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Nachname (erforderlich) [PSF]
    /// </summary>
    [Required(ErrorMessage = "Nachname ist erforderlich")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Nachname muss zwischen 1 und 100 Zeichen lang sein")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Schweizer Versicherungsnummer (XXX.XXXX.XXXX.XX) [SF]
    /// </summary>
    [Required(ErrorMessage = "Versicherungsnummer ist erforderlich")]
    [RegularExpression(@"^\d{3}\.\d{4}\.\d{4}\.\d{2}$", 
        ErrorMessage = "Versicherungsnummer muss im Format XXX.XXXX.XXXX.XX sein")]
    public string InsuranceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Geburtsdatum [PSF]
    /// </summary>
    [Required(ErrorMessage = "Geburtsdatum ist erforderlich")]
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Validiert, dass das Geburtsdatum plausibel ist [MV]
    /// </summary>
    public bool IsValidDateOfBirth()
    {
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Year;
        
        // Medizinische Plausibilitätsprüfung: 0-150 Jahre [MV]
        return age >= 0 && age <= 150 && DateOfBirth <= today;
    }
}

/// <summary>
/// Patient Update DTO für Änderungen [ZTS][ATV]
/// </summary>
public class UpdatePatientRequest
{
    /// <summary>
    /// Vorname (optional bei Update)
    /// </summary>
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Vorname muss zwischen 1 und 100 Zeichen lang sein")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Nachname (optional bei Update)
    /// </summary>
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Nachname muss zwischen 1 und 100 Zeichen lang sein")]
    public string? LastName { get; set; }

    /// <summary>
    /// Schweizer Versicherungsnummer (optional bei Update) [SF]
    /// </summary>
    [RegularExpression(@"^\d{3}\.\d{4}\.\d{4}\.\d{2}$", 
        ErrorMessage = "Versicherungsnummer muss im Format XXX.XXXX.XXXX.XX sein")]
    public string? InsuranceNumber { get; set; }

    /// <summary>
    /// Geburtsdatum (optional bei Update)
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Validiert, dass das Geburtsdatum plausibel ist [MV]
    /// </summary>
    public bool IsValidDateOfBirth()
    {
        if (!DateOfBirth.HasValue) return true;
        
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Value.Year;
        
        // Medizinische Plausibilitätsprüfung: 0-150 Jahre [MV]
        return age >= 0 && age <= 150 && DateOfBirth.Value <= today;
    }
}
