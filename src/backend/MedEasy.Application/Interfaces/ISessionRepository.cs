// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using MedEasy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedEasy.Application.Interfaces
{
    /// <summary>
    /// Repository-Interface für Session-Datenoperationen [CAM][SK]
    /// Definiert Kontakt für Clean Architecture mit Audit-Trail [ATV]
    /// </summary>
    public interface ISessionRepository
    {
        /// <summary>
        /// Fügt eine neue Session hinzu [SK]
        /// </summary>
        /// <param name="session">Session-Entity mit verschlüsselten Daten</param>
        /// <returns>Hinzugefügte Session</returns>
        Task<Session> AddAsync(Session session);

        /// <summary>
        /// Ruft eine Session anhand der ID ab [SK]
        /// </summary>
        /// <param name="id">Session-ID</param>
        /// <returns>Session oder null wenn nicht gefunden</returns>
        Task<Session?> GetByIdAsync(Guid id);

        /// <summary>
        /// Ruft alle Sessions ab [SK]
        /// </summary>
        /// <returns>Liste aller Sessions</returns>
        Task<IEnumerable<Session>> GetAllAsync();

        /// <summary>
        /// Ruft alle Sessions eines Patienten ab [SK]
        /// </summary>
        /// <param name="patientId">Patient-ID</param>
        /// <returns>Liste der Sessions des Patienten</returns>
        Task<IEnumerable<Session>> GetByPatientIdAsync(Guid patientId);

        /// <summary>
        /// Aktualisiert eine Session [SK]
        /// </summary>
        /// <param name="session">Aktualisierte Session</param>
        /// <returns>Aktualisierte Session</returns>
        Task<Session> UpdateAsync(Session session);

        /// <summary>
        /// Löscht eine Session [ATV]
        /// </summary>
        /// <param name="id">Session-ID</param>
        /// <returns>True wenn gelöscht, False wenn nicht gefunden</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Speichert alle Änderungen in der Datenbank [ATV]
        /// </summary>
        /// <returns>Anzahl der betroffenen Datensätze</returns>
        Task<int> SaveChangesAsync();
    }
}
