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
    /// Repository für Transcript-Daten mit SQLCipher-Verschlüsselung [SP][AIU]
    /// Implementiert Clean Architecture Pattern mit Anonymisierung [AIU]
    /// </summary>
    public class TranscriptRepository : ITranscriptRepository
    {
        private readonly SQLCipherContext _context;
        private readonly ILogger<TranscriptRepository> _logger;

        /// <summary>
        /// Konstruktor für TranscriptRepository
        /// </summary>
        /// <param name="context">SQLCipher Datenbankkontext [SP]</param>
        /// <param name="logger">Logger für Audit-Trail [ATV]</param>
        public TranscriptRepository(SQLCipherContext context, ILogger<TranscriptRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Fügt ein neues Transkript hinzu [AIU]
        /// </summary>
        /// <param name="transcript">Transcript-Entity mit verschlüsselten/anonymisierten Daten</param>
        /// <returns>Hinzugefügtes Transkript</returns>
        public async Task<Transcript> AddAsync(Transcript transcript)
        {
            if (transcript == null)
                throw new ArgumentNullException(nameof(transcript));

            try
            {
                _logger.LogInformation("Füge neues Transkript hinzu: Session {SessionId} [ATV]", transcript.SessionId);
                
                // Transkript zur Datenbank hinzufügen
                var entry = await _context.Transcripts.AddAsync(transcript);
                
                // Audit-Log erstellen [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Transcript),
                    EntityId = transcript.Id.ToString(),
                    Action = "CREATE",
                    Changes = $"Transkript erstellt für Session {transcript.SessionId}, Status: {transcript.AnonymizationStatus}",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Transkript erfolgreich hinzugefügt: {TranscriptId} [ATV]", transcript.Id);
                return entry.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Hinzufügen des Transkripts [ECP]");
                throw new InvalidOperationException("Transkript konnte nicht hinzugefügt werden", ex);
            }
        }

        /// <summary>
        /// Ruft ein Transkript anhand der ID ab [AIU]
        /// </summary>
        /// <param name="id">Transcript-ID</param>
        /// <returns>Transkript oder null wenn nicht gefunden</returns>
        public async Task<Transcript?> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Rufe Transkript ab: {TranscriptId} [ATV]", id);
                
                var transcript = await _context.Transcripts
                    .Include(t => t.Session) // Session-Daten mit laden für Referenz
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (transcript != null)
                {
                    // Audit-Log für Zugriff erstellen [ATV]
                    _context.AuditLogs.Add(new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        EntityName = nameof(Transcript),
                        EntityId = id.ToString(),
                        Action = "READ",
                        Changes = "Transkript-Details abgerufen",
                        ContainsSensitiveData = true,
                        Timestamp = DateTime.UtcNow,
                        UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                    });

                    _logger.LogInformation("Transkript erfolgreich abgerufen: {TranscriptId} [ATV]", id);
                }
                else
                {
                    _logger.LogWarning("Transkript nicht gefunden: {TranscriptId} [ATV]", id);
                }

                return transcript;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen des Transkripts: {TranscriptId} [ECP]", id);
                throw new InvalidOperationException($"Transkript {id} konnte nicht abgerufen werden", ex);
            }
        }

        /// <summary>
        /// Ruft alle Transkripte ab [AIU]
        /// </summary>
        /// <returns>Liste aller Transkripte</returns>
        public async Task<IEnumerable<Transcript>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Rufe alle Transkripte ab [ATV]");
                
                var transcripts = await _context.Transcripts
                    .Include(t => t.Session) // Session-Referenz mit laden
                    .OrderByDescending(t => t.Created)
                    .ToListAsync();

                // Audit-Log für Massenzugriff [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Transcript),
                    EntityId = "ALL",
                    Action = "READ_ALL",
                    Changes = $"Alle Transkripte abgerufen (Anzahl: {transcripts.Count})",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Alle Transkripte erfolgreich abgerufen: {Count} [ATV]", transcripts.Count);
                return transcripts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen aller Transkripte [ECP]");
                throw new InvalidOperationException("Transkripte konnten nicht abgerufen werden", ex);
            }
        }

        /// <summary>
        /// Ruft alle Transkripte einer Session ab [AIU]
        /// </summary>
        /// <param name="sessionId">Session-ID</param>
        /// <returns>Liste der Transkripte der Session</returns>
        public async Task<IEnumerable<Transcript>> GetBySessionIdAsync(Guid sessionId)
        {
            try
            {
                _logger.LogInformation("Rufe Transkripte für Session ab: {SessionId} [ATV]", sessionId);
                
                var transcripts = await _context.Transcripts
                    .Where(t => t.SessionId == sessionId)
                    .OrderByDescending(t => t.Created)
                    .ToListAsync();

                // Audit-Log für Session-Zugriff [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Transcript),
                    EntityId = sessionId.ToString(),
                    Action = "READ_BY_SESSION",
                    Changes = $"Transkripte für Session abgerufen (Anzahl: {transcripts.Count})",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Transkripte für Session erfolgreich abgerufen: {SessionId}, Anzahl: {Count} [ATV]", 
                    sessionId, transcripts.Count);
                return transcripts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen der Transkripte für Session: {SessionId} [ECP]", sessionId);
                throw new InvalidOperationException($"Transkripte für Session {sessionId} konnten nicht abgerufen werden", ex);
            }
        }

        /// <summary>
        /// Ruft Transkripte nach Anonymisierungsstatus ab [AIU]
        /// </summary>
        /// <param name="status">Anonymisierungsstatus</param>
        /// <returns>Liste der Transkripte mit dem Status</returns>
        public async Task<IEnumerable<Transcript>> GetByAnonymizationStatusAsync(string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status darf nicht null oder leer sein", nameof(status));

            try
            {
                _logger.LogInformation("Rufe Transkripte nach Anonymisierungsstatus ab: {Status} [ATV]", status);
                
                var transcripts = await _context.Transcripts
                    .Where(t => t.AnonymizationStatus == status)
                    .Include(t => t.Session)
                    .OrderByDescending(t => t.Created)
                    .ToListAsync();

                // Audit-Log für Status-Suche [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Transcript),
                    EntityId = status,
                    Action = "READ_BY_STATUS",
                    Changes = $"Transkripte nach Status '{status}' abgerufen (Anzahl: {transcripts.Count})",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Transkripte nach Status erfolgreich abgerufen: {Status}, Anzahl: {Count} [ATV]", 
                    status, transcripts.Count);
                return transcripts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen der Transkripte nach Status: {Status} [ECP]", status);
                throw new InvalidOperationException($"Transkripte mit Status {status} konnten nicht abgerufen werden", ex);
            }
        }

        /// <summary>
        /// Aktualisiert ein Transkript [AIU]
        /// </summary>
        /// <param name="transcript">Aktualisiertes Transkript</param>
        /// <returns>Aktualisiertes Transkript</returns>
        public async Task<Transcript> UpdateAsync(Transcript transcript)
        {
            if (transcript == null)
                throw new ArgumentNullException(nameof(transcript));

            try
            {
                _logger.LogInformation("Aktualisiere Transkript: {TranscriptId} [ATV]", transcript.Id);
                
                // Prüfen ob Transkript existiert
                var existingTranscript = await _context.Transcripts.FindAsync(transcript.Id);
                if (existingTranscript == null)
                    throw new InvalidOperationException($"Transkript {transcript.Id} nicht gefunden");

                // Transkript aktualisieren
                _context.Entry(existingTranscript).CurrentValues.SetValues(transcript);
                existingTranscript.UpdateAuditInfo("Repository"); // TODO: Aktueller Benutzer

                // Audit-Log erstellen [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Transcript),
                    EntityId = transcript.Id.ToString(),
                    Action = "UPDATE",
                    Changes = $"Transkript aktualisiert, Status: {transcript.AnonymizationStatus}",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Transkript erfolgreich aktualisiert: {TranscriptId} [ATV]", transcript.Id);
                return existingTranscript;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Aktualisieren des Transkripts: {TranscriptId} [ECP]", transcript.Id);
                throw new InvalidOperationException($"Transkript {transcript.Id} konnte nicht aktualisiert werden", ex);
            }
        }

        /// <summary>
        /// Löscht ein Transkript [ATV]
        /// </summary>
        /// <param name="id">Transcript-ID</param>
        /// <returns>True wenn gelöscht, False wenn nicht gefunden</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Lösche Transkript: {TranscriptId} [ATV]", id);
                
                var transcript = await _context.Transcripts.FindAsync(id);
                if (transcript == null)
                {
                    _logger.LogWarning("Transkript zum Löschen nicht gefunden: {TranscriptId} [ATV]", id);
                    return false;
                }

                // Transkript löschen
                _context.Transcripts.Remove(transcript);

                // Audit-Log erstellen [ATV]
                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = nameof(Transcript),
                    EntityId = id.ToString(),
                    Action = "DELETE",
                    Changes = "Transkript gelöscht",
                    ContainsSensitiveData = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = "Repository" // TODO: Aktueller Benutzer aus Context
                });

                _logger.LogInformation("Transkript erfolgreich gelöscht: {TranscriptId} [ATV]", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Löschen des Transkripts: {TranscriptId} [ECP]", id);
                throw new InvalidOperationException($"Transkript {id} konnte nicht gelöscht werden", ex);
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
                _logger.LogDebug("Speichere Transkript-Änderungen in der Datenbank [ATV]");
                
                var result = await _context.SaveChangesAsync();
                
                _logger.LogInformation("Transkript-Änderungen erfolgreich gespeichert: {Count} Datensätze [ATV]", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Speichern der Transkript-Änderungen [ECP]");
                throw new InvalidOperationException("Transkript-Änderungen konnten nicht gespeichert werden", ex);
            }
        }
    }
}
