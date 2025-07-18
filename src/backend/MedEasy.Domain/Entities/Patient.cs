// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using System;
using System.Collections.Generic;

namespace MedEasy.Domain.Entities
{
    /// <summary>
    /// Repräsentiert einen Patienten im System [MFD][EIV]
    /// </summary>
    public class Patient : IHasAuditInfo
    {
        /// <summary>
        /// Eindeutige ID des Patienten
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Verschlüsselter Name des Patienten [EIV][SP]
        /// </summary>
        public byte[] EncryptedName { get; private set; }

        /// <summary>
        /// Hash der Versicherungsnummer (Format: XXX.XXXX.XXXX.XX) [SF]
        /// </summary>
        public string InsuranceNumberHash { get; private set; }

        /// <summary>
        /// Geburtsdatum des Patienten (Format: DD.MM.YYYY) [SF]
        /// </summary>
        public DateOnly DateOfBirth { get; private set; }

        /// <summary>
        /// Verschlüsseltes Geschlecht des Patienten [EIV]
        /// </summary>
        public byte[] EncryptedGender { get; private set; }

        /// <summary>
        /// Verschlüsselter Name der Krankenkasse [EIV]
        /// </summary>
        public byte[] EncryptedInsuranceProvider { get; private set; }

        /// <summary>
        /// Liste der Konsultationen/Sessions des Patienten [SK]
        /// </summary>
        public ICollection<Session> Sessions { get; private set; } = new List<Session>();

        /// <summary>
        /// Zeitstempel der letzten Änderung für Audit-Trail [ATV]
        /// </summary>
        public DateTime LastModified { get; private set; }

        /// <summary>
        /// ID des Benutzers, der die letzte Änderung vorgenommen hat [ATV]
        /// </summary>
        public string LastModifiedBy { get; private set; }

        /// <summary>
        /// Erstellt einen neuen Patienten mit verschlüsselten Daten
        /// </summary>
        private Patient() 
        {
            // Für EF Core
        }

        /// <summary>
        /// Aktualisiert die Audit-Informationen [ATV]
        /// </summary>
        public void UpdateAuditInfo(string modifiedBy)
        {
            LastModified = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }
    }
}
