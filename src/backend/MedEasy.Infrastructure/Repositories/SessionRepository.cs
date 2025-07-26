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
    /// Repository für Session-Daten mit SQLCipher-Verschlüsselung [SP][SK]
    /// Implementiert Clean Architecture Pattern mit Audit-Logging [ATV]
    /// </summary>
    public class SessionRepository : ISessionRepository
    {
        private readonly SQLCipherContext _context;
        private readonly ILogger<SessionRepository> _logger;

        /// <summary>
        /// Konstruktor für SessionRepository
        /// </summary>
        /// <param name="context">SQLCipher Datenbankkontext [SP]</param>
        /// <param name="logger">Logger für Audit-Trail [ATV]</param>
        public SessionRepository(SQLCipherContext context, ILogger<SessionRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Fügt eine neue Session hinzu [SK]
        /// </summary>
        /// <param name="session">Session-Entity mit verschlüsselten Daten</param>
        /// <returns>Hinzugefügte Session</returns>
        public async Task<Session> AddAsync(Session session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            try
            {
                _logger.LogInformation("Füge neue Session hinzu: Patient {PatientId} [ATV]", session.PatientId);
                
                // Session zur Datenbank hinzufügen
                var entry = await _context.Sessions.AddAsync(session);
                
                // Audit-Log erstellen [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Session),
                    EntityId = session.Id.ToString(),
                    Action = "CREATE",
                    Changes = $"Session erstellt für Patient {session.PatientId}",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Session erfolgreich hinzugefügt: {SessionId} [ATV]", session.Id);
                return entry.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Hinzufügen der Session [ECP]");
                throw new InvalidOperationException("Session konnte nicht hinzugefügt werden", ex);
            }
        }

        /// <summary>
        /// Ruft eine Session anhand der ID ab [SK]
        /// </summary>
        /// <param name="id">Session-ID</param>
        /// <returns>Session oder null wenn nicht gefunden</returns>
        public async Task<Session?> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Rufe Session ab: {SessionId} [ATV]", id);
                
                var session = await _context.Sessions
                    .Include(s => s.Patient) // Patient-Daten mit laden für Referenz
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (session != null)
                {
                    // Audit-Log für Zugriff erstellen [ATV]
                    _context.AuditLogs.Add(new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        EntityName = nameof(Session),
                        EntityId = id.ToString(),
                        Action = "READ",
                        Changes = "Session-Details abgerufen",
                        ContainsSensitiveData = true,
                        Timestamp = DateTime.UtcNow,
                        UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                    });

                    _logger.LogInformation("Session erfolgreich abgerufen: {SessionId} [ATV]", id);
                }
                else
                {
                    _logger.LogWarning("Session nicht gefunden: {SessionId} [ATV]", id);
                }

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen der Session: {SessionId} [ECP]", id);
                throw new InvalidOperationException($"Session {id} konnte nicht abgerufen werden", ex);
            }
        }

        /// <summary>
        /// Ruft alle Sessions ab [SK]
        /// </summary>
        /// <returns>Liste aller Sessions</returns>
        public async Task<IEnumerable<Session>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Rufe alle Sessions ab [ATV]");
                
                var sessions = await _context.Sessions
                    .Include(s => s.Patient) // Patient-Referenz mit laden
                    .OrderByDescending(s => s.SessionDate)
                    .ToListAsync();

                // Audit-Log für Massenzugriff [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Session),
                    EntityId = "ALL",
                    Action = "READ_ALL",
                    Changes = $"Alle Sessions abgerufen (Anzahl: {sessions.Count})",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Alle Sessions erfolgreich abgerufen: {Count} [ATV]", sessions.Count);
                return sessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen aller Sessions [ECP]");
                throw new InvalidOperationException("Sessions konnten nicht abgerufen werden", ex);
            }
        }

        /// <summary>
        /// Ruft alle Sessions eines Patienten ab [SK]
        /// </summary>
        /// <param name="patientId">Patient-ID</param>
        /// <returns>Liste der Sessions des Patienten</returns>
        public async Task<IEnumerable<Session>> GetByPatientIdAsync(Guid patientId)
        {
            try
            {
                _logger.LogInformation("Rufe Sessions für Patient ab: {PatientId} [ATV]", patientId);
                
                var sessions = await _context.Sessions
                    .Where(s => s.PatientId == patientId)
                    .OrderByDescending(s => s.SessionDate)
                    .ToListAsync();

                // Audit-Log für Patientenzugriff [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Session),
                    EntityId = patientId.ToString(),
                    Action = "READ_BY_PATIENT",
                    Changes = $"Sessions für Patient abgerufen (Anzahl: {sessions.Count})",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Sessions für Patient erfolgreich abgerufen: {PatientId}, Anzahl: {Count} [ATV]", 
                    patientId, sessions.Count);
                return sessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen der Sessions für Patient: {PatientId} [ECP]", patientId);
                throw new InvalidOperationException($"Sessions für Patient {patientId} konnten nicht abgerufen werden", ex);
            }
        }

        /// <summary>
        /// Aktualisiert eine Session [SK]
        /// </summary>
        /// <param name="session">Aktualisierte Session</param>
        /// <returns>Aktualisierte Session</returns>
        public async Task<Session> UpdateAsync(Session session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            try
            {
                _logger.LogInformation("Aktualisiere Session: {SessionId} [ATV]", session.Id);
                
                // Prüfen ob Session existiert
                var existingSession = await _context.Sessions.FindAsync(session.Id);
                if (existingSession == null)
                    throw new InvalidOperationException($"Session {session.Id} nicht gefunden");

                // Session aktualisieren
                _context.Entry(existingSession).CurrentValues.SetValues(session);
                existingSession.UpdateAuditInfo("Repository"); // TODO: Aktueller Benutzer

                // Audit-Log erstellen [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Session),
                    EntityId = session.Id.ToString(),
                    Action = "UPDATE",
                    Changes = "Session aktualisiert",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Session erfolgreich aktualisiert: {SessionId} [ATV]", session.Id);
                return existingSession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Aktualisieren der Session: {SessionId} [ECP]", session.Id);
                throw new InvalidOperationException($"Session {session.Id} konnte nicht aktualisiert werden", ex);
            }
        }

        /// <summary>
        /// Löscht eine Session [ATV]
        /// </summary>
        /// <param name="id">Session-ID</param>
        /// <returns>True wenn gelöscht, False wenn nicht gefunden</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Lösche Session: {SessionId} [ATV]", id);
                
                var session = await _context.Sessions.FindAsync(id);
                if (session == null)
                {
                    _logger.LogWarning("Session zum Löschen nicht gefunden: {SessionId} [ATV]", id);
                    return false;
                }

                // Session löschen
                _context.Sessions.Remove(session);

                // Audit-Log erstellen [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Session),
                    EntityId = id.ToString(),
                    Action = "DELETE",
                    Changes = "Session gelöscht",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Session erfolgreich gelöscht: {SessionId} [ATV]", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Löschen der Session: {SessionId} [ECP]", id);
                throw new InvalidOperationException($"Session {id} konnte nicht gelöscht werden", ex);
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
                _logger.LogDebug("Speichere Session-Änderungen in der Datenbank [ATV]");
                
                var result = await _context.SaveChangesAsync();
                
                _logger.LogInformation("Session-Änderungen erfolgreich gespeichert: {Count} Datensätze [ATV]", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Speichern der Session-Änderungen [ECP]");
                throw new InvalidOperationException("Session-Änderungen konnten nicht gespeichert werden", ex);
            }
        }
    }
}
