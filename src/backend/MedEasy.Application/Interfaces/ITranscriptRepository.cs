// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using MedEasy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedEasy.Application.Interfaces
{
    /// <summary>
    /// Repository-Interface für Transcript-Datenoperationen [CAM][AIU]
    /// Definiert Kontrakt für Clean Architecture mit Anonymisierung [AIU]
    /// </summary>
    public interface ITranscriptRepository
    {
        /// <summary>
        /// Fügt ein neues Transkript hinzu [AIU]
        /// </summary>
        /// <param name="transcript">Transcript-Entity mit verschlüsselten/anonymisierten Daten</param>
        /// <returns>Hinzugefügtes Transkript</returns>
        Task<Transcript> AddAsync(Transcript transcript);

        /// <summary>
        /// Ruft ein Transkript anhand der ID ab [AIU]
        /// </summary>
        /// <param name="id">Transcript-ID</param>
        /// <returns>Transkript oder null wenn nicht gefunden</returns>
        Task<Transcript?> GetByIdAsync(Guid id);

        /// <summary>
        /// Ruft alle Transkripte ab [AIU]
        /// </summary>
        /// <returns>Liste aller Transkripte</returns>
        Task<IEnumerable<Transcript>> GetAllAsync();

        /// <summary>
        /// Ruft alle Transkripte einer Session ab [AIU]
        /// </summary>
        /// <param name="sessionId">Session-ID</param>
        /// <returns>Liste der Transkripte der Session</returns>
        Task<IEnumerable<Transcript>> GetBySessionIdAsync(Guid sessionId);

        /// <summary>
        /// Ruft Transkripte nach Anonymisierungsstatus ab [AIU]
        /// </summary>
        /// <param name="status">Anonymisierungsstatus</param>
        /// <returns>Liste der Transkripte mit dem Status</returns>
        Task<IEnumerable<Transcript>> GetByAnonymizationStatusAsync(string status);

        /// <summary>
        /// Aktualisiert ein Transkript [AIU]
        /// </summary>
        /// <param name="transcript">Aktualisiertes Transkript</param>
        /// <returns>Aktualisiertes Transkript</returns>
        Task<Transcript> UpdateAsync(Transcript transcript);

        /// <summary>
        /// Löscht ein Transkript [ATV]
        /// </summary>
        /// <param name="id">Transcript-ID</param>
        /// <returns>True wenn gelöscht, False wenn nicht gefunden</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Speichert alle Änderungen in der Datenbank [ATV]
        /// </summary>
        /// <returns>Anzahl der betroffenen Datensätze</returns>
        Task<int> SaveChangesAsync();
    }
}
