// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using MedEasy.Application.Interfaces;
using MedEasy.Domain.Entities;
using MedEasy.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Infrastructure.Repositories
{
    /// <summary>
    /// Repository für Patientendaten mit SQLCipher-Verschlüsselung [SP][CAM]
    /// Implementiert Clean Architecture Pattern mit Audit-Logging [ATV]
    /// </summary>
    public class PatientRepository : IPatientRepository
    {
        private readonly SQLCipherContext _context;
        private readonly ILogger<PatientRepository> _logger;

        /// <summary>
        /// Konstruktor für PatientRepository
        /// </summary>
        /// <param name="context">SQLCipher Datenbankkontext [SP]</param>
        /// <param name="logger">Logger für Audit-Trail [ATV]</param>
        public PatientRepository(SQLCipherContext context, ILogger<PatientRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Fügt einen neuen Patienten hinzu [EIV]
        /// </summary>
        /// <param name="patient">Patient-Entity mit verschlüsselten Daten</param>
        public async Task AddAsync(Patient patient)
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient));

            try
            {
                _logger.LogInformation("Füge neuen Patienten hinzu [ATV]");
                
                // Patient zur Datenbank hinzufügen
                var entry = await _context.Patients.AddAsync(patient);
                
                // Audit-Log erstellen [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Patient),
                    EntityId = patient.Id.ToString(),
                    Action = "CREATE",
                    Changes = "Patient erstellt",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Patient erfolgreich hinzugefügt: {PatientId} [ATV]", patient.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Hinzufügen des Patienten [ECP]");
                throw new InvalidOperationException("Patient konnte nicht hinzugefügt werden", ex);
            }
        }

        /// <summary>
        /// Ruft einen Patienten anhand der ID ab [EIV]
        /// </summary>
        /// <param name="id">Patient-ID</param>
        /// <returns>Patient oder null wenn nicht gefunden</returns>
        public async Task<Patient?> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Rufe Patient ab: {PatientId} [ATV]", id);
                
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (patient != null)
                {
                    // Audit-Log für Zugriff erstellen [ATV]
                    _context.AuditLogs.Add(new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        EntityName = nameof(Patient),
                        EntityId = id.ToString(),
                        Action = "READ",
                        Changes = "Patient-Details abgerufen",
                        ContainsSensitiveData = true,
                        Timestamp = DateTime.UtcNow,
                        UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                    });

                    _logger.LogInformation("Patient erfolgreich abgerufen: {PatientId} [ATV]", id);
                }
                else
                {
                    _logger.LogWarning("Patient nicht gefunden: {PatientId} [ATV]", id);
                }

                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen des Patienten: {PatientId} [ECP]", id);
                throw new InvalidOperationException($"Patient {id} konnte nicht abgerufen werden", ex);
            }
        }

        /// <summary>
        /// Ruft alle Patienten ab (nur Basis-Informationen für Privacy) [PbD]
        /// </summary>
        /// <returns>Liste aller Patienten</returns>
        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Rufe alle Patienten ab [ATV]");
                
                var patients = await _context.Patients
                    .OrderBy(p => p.Created)
                    .ToListAsync();

                // Audit-Log für Massenzugriff [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Patient),
                    EntityId = "ALL",
                    Action = "READ_ALL",
                    Changes = $"Alle Patienten abgerufen (Anzahl: {patients.Count})",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Alle Patienten erfolgreich abgerufen: {Count} [ATV]", patients.Count);
                return patients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen aller Patienten [ECP]");
                throw new InvalidOperationException("Patienten konnten nicht abgerufen werden", ex);
            }
        }

        /// <summary>
        /// Aktualisiert einen Patienten [EIV]
        /// </summary>
        /// <param name="patient">Aktualisierter Patient</param>
        /// <returns>Aktualisierter Patient</returns>
        public async Task<Patient> UpdateAsync(Patient patient)
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient));

            try
            {
                _logger.LogInformation("Aktualisiere Patient: {PatientId} [ATV]", patient.Id);
                
                // Prüfen ob Patient existiert
                var existingPatient = await _context.Patients.FindAsync(patient.Id);
                if (existingPatient == null)
                    throw new InvalidOperationException($"Patient {patient.Id} nicht gefunden");

                // Patient aktualisieren
                _context.Entry(existingPatient).CurrentValues.SetValues(patient);
                existingPatient.UpdateAuditInfo("Repository"); // TODO: Aktueller Benutzer

                // Audit-Log erstellen [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Patient),
                    EntityId = patient.Id.ToString(),
                    Action = "UPDATE",
                    Changes = "Patient aktualisiert",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Patient erfolgreich aktualisiert: {PatientId} [ATV]", patient.Id);
                return existingPatient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Aktualisieren des Patienten: {PatientId} [ECP]", patient.Id);
                throw new InvalidOperationException($"Patient {patient.Id} konnte nicht aktualisiert werden", ex);
            }
        }

        /// <summary>
        /// Löscht einen Patienten [ATV]
        /// </summary>
        /// <param name="id">Patient-ID</param>
        /// <returns>True wenn gelöscht, False wenn nicht gefunden</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Lösche Patient: {PatientId} [ATV]", id);
                
                var patient = await _context.Patients.FindAsync(id);
                if (patient == null)
                {
                    _logger.LogWarning("Patient zum Löschen nicht gefunden: {PatientId} [ATV]", id);
                    return false;
                }

                // Patient löschen
                _context.Patients.Remove(patient);

                // Audit-Log erstellen [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Patient),
                    EntityId = id.ToString(),
                    Action = "DELETE",
                    Changes = "Patient gelöscht",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Patient erfolgreich gelöscht: {PatientId} [ATV]", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Löschen des Patienten: {PatientId} [ECP]", id);
                throw new InvalidOperationException($"Patient {id} konnte nicht gelöscht werden", ex);
            }
        }

        /// <summary>
        /// Sucht Patienten anhand der Versicherungsnummer (Hash) [SF]
        /// </summary>
        /// <param name="insuranceNumberHash">Hash der Versicherungsnummer</param>
        /// <returns>Patient oder null wenn nicht gefunden</returns>
        public async Task<Patient?> GetByInsuranceNumberHashAsync(string insuranceNumberHash)
        {
            if (string.IsNullOrEmpty(insuranceNumberHash))
                throw new ArgumentException("Versicherungsnummer-Hash darf nicht null oder leer sein", nameof(insuranceNumberHash));

            try
            {
                _logger.LogInformation("Suche Patient nach Versicherungsnummer-Hash [ATV]");
                
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.InsuranceNumberHash == insuranceNumberHash);

                if (patient != null)
                {
                    // Audit-Log für Suche erstellen [ATV]
                    _context.AuditLogs.Add(new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        EntityName = nameof(Patient),
                        EntityId = patient.Id.ToString(),
                        Action = "SEARCH_BY_INSURANCE",
                        Changes = "Patient über Versicherungsnummer gefunden",
                        ContainsSensitiveData = true,
                        Timestamp = DateTime.UtcNow,
                        UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                    });

                    _logger.LogInformation("Patient über Versicherungsnummer gefunden: {PatientId} [ATV]", patient.Id);
                }
                else
                {
                    _logger.LogInformation("Kein Patient mit dieser Versicherungsnummer gefunden [ATV]");
                }

                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler bei der Suche nach Versicherungsnummer [ECP]");
                throw new InvalidOperationException("Suche nach Versicherungsnummer fehlgeschlagen", ex);
            }
        }

        /// <summary>
        /// Speichert alle Änderungen in der Datenbank [ATV]
        /// </summary>
        /// <returns>Anzahl der betroffenen Datensätze</returns>
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                _logger.LogDebug("Speichere Änderungen in der Datenbank [ATV]");
                
                var result = await _context.SaveChangesAsync();
                
                _logger.LogInformation("Änderungen erfolgreich gespeichert: {Count} Datensätze [ATV]", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Speichern der Änderungen [ECP]");
                throw new InvalidOperationException("Änderungen konnten nicht gespeichert werden", ex);
            }
        }

        /// <summary>
        /// Aktualisiert einen Patienten [ATV]
        /// </summary>
        /// <param name="patient">Patient-Entity mit aktualisierten Daten</param>
        public void Update(Patient patient)
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient));

            try
            {
                _logger.LogInformation("Aktualisiere Patient: {PatientId} [ATV]", patient.Id);
                
                // Patient als modifiziert markieren
                _context.Patients.Update(patient);
                
                // Audit-Log erstellen [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Patient),
                    EntityId = patient.Id.ToString(),
                    Action = "UPDATE",
                    Changes = "Patient aktualisiert",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Patient erfolgreich für Update markiert: {PatientId} [ATV]", patient.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Aktualisieren des Patienten: {PatientId} [ECP]", patient.Id);
                throw new InvalidOperationException($"Patient {patient.Id} konnte nicht aktualisiert werden", ex);
            }
        }

        /// <summary>
        /// Löscht einen Patienten (soft delete) [ATV]
        /// </summary>
        /// <param name="patient">Zu löschender Patient</param>
        public void Delete(Patient patient)
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient));

            try
            {
                _logger.LogInformation("Lösche Patient: {PatientId} [ATV]", patient.Id);
                
                // Soft Delete: Patient als gelöscht markieren
                patient.IsDeleted = true;
                patient.DeletedAt = DateTime.UtcNow;
                
                _context.Patients.Update(patient);
                
                // Audit-Log erstellen [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Patient),
                    EntityId = patient.Id.ToString(),
                    Action = "DELETE",
                    Changes = "Patient gelöscht (soft delete)",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Patient erfolgreich für Löschung markiert: {PatientId} [ATV]", patient.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Löschen des Patienten: {PatientId} [ECP]", patient.Id);
                throw new InvalidOperationException($"Patient {patient.Id} konnte nicht gelöscht werden", ex);
            }
        }
    }
}
