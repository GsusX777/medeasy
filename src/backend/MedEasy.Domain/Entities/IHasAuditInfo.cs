// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using System;

namespace MedEasy.Domain.Entities
{
    /// <summary>
    /// Interface für Entities mit Audit-Informationen [ATV]
    /// </summary>
    public interface IHasAuditInfo
    {
        /// <summary>
        /// Zeitstempel der Erstellung
        /// </summary>
        DateTime Created { get; set; }

        /// <summary>
        /// ID des Benutzers, der die Entity erstellt hat
        /// </summary>
        string CreatedBy { get; set; }

        /// <summary>
        /// Zeitstempel der letzten Änderung
        /// </summary>
        DateTime LastModified { get; set; }

        /// <summary>
        /// ID des Benutzers, der die letzte Änderung vorgenommen hat
        /// </summary>
        string LastModifiedBy { get; set; }
    }
}
