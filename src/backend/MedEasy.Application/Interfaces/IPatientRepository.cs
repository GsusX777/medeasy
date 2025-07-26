// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using MedEasy.Domain.Entities;

namespace MedEasy.Application.Interfaces;

/// <summary>
/// Repository Interface für Patient Entity [CAM][ATV]
/// Definiert Datenzugriff ohne Implementierungsdetails
/// </summary>
public interface IPatientRepository
{
    /// <summary>
    /// Fügt einen neuen Patienten hinzu [ATV]
    /// </summary>
    Task AddAsync(Patient patient);

    /// <summary>
    /// Ruft einen Patienten anhand der ID ab [PbD]
    /// </summary>
    Task<Patient?> GetByIdAsync(Guid id);

    /// <summary>
    /// Ruft alle Patienten ab [PbD]
    /// </summary>
    Task<IEnumerable<Patient>> GetAllAsync();

    /// <summary>
    /// Aktualisiert einen Patienten [ATV]
    /// </summary>
    void Update(Patient patient);

    /// <summary>
    /// Löscht einen Patienten (soft delete) [ATV]
    /// </summary>
    void Delete(Patient patient);

    /// <summary>
    /// Sucht Patienten anhand der Versicherungsnummer [SF]
    /// </summary>
    Task<Patient?> GetByInsuranceNumberHashAsync(string insuranceNumberHash);

    /// <summary>
    /// Speichert Änderungen in der Datenbank [ATV]
    /// </summary>
    Task<int> SaveChangesAsync();
}
