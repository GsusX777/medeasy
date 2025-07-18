// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using System;
using System.Collections.Generic;

namespace MedEasy.Domain.Entities
{
    /// <summary>
    /// Repräsentiert eine Konsultation/Session zwischen Arzt und Patient [SK][MFD]
    /// </summary>
    public class Session : IHasAuditInfo
    {
        /// <summary>
        /// Eindeutige ID der Session
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Referenz zum Patienten
        /// </summary>
        public Guid PatientId { get; private set; }

        /// <summary>
        /// Navigationseigenschaft zum Patienten
        /// </summary>
        public Patient Patient { get; private set; }

        /// <summary>
        /// Datum und Uhrzeit der Konsultation (Format: DD.MM.YYYY) [SF]
        /// </summary>
        public DateTime SessionDate { get; private set; }

        /// <summary>
        /// Verschlüsselter Grund der Konsultation [EIV]
        /// </summary>
        public byte[] EncryptedReason { get; private set; }

        /// <summary>
        /// Verschlüsselte Notizen zur Konsultation [EIV]
        /// </summary>
        public byte[] EncryptedNotes { get; private set; }

        /// <summary>
        /// Pfad zur Audiodatei der Konsultation
        /// </summary>
        public string AudioFilePath { get; private set; }

        /// <summary>
        /// Status der Session (Aktiv, Abgeschlossen, etc.)
        /// </summary>
        public SessionStatus Status { get; private set; }

        /// <summary>
        /// Flag, ob die Session mit Cloud-Diensten verarbeitet wurde [CT]
        /// </summary>
        public bool ProcessedInCloud { get; private set; }

        /// <summary>
        /// Verwendeter KI-Provider für die Verarbeitung [PK]
        /// </summary>
        public string AIProvider { get; private set; }

        /// <summary>
        /// Transkriptionen der Session
        /// </summary>
        public ICollection<Transcript> Transcripts { get; private set; } = new List<Transcript>();

        /// <summary>
        /// Zeitstempel der Erstellung für Audit-Trail [ATV]
        /// </summary>
        public DateTime Created { get; private set; }

        /// <summary>
        /// ID des Benutzers, der die Session erstellt hat [ATV]
        /// </summary>
        public string CreatedBy { get; private set; }

        /// <summary>
        /// Zeitstempel der letzten Änderung für Audit-Trail [ATV]
        /// </summary>
        public DateTime LastModified { get; private set; }

        /// <summary>
        /// ID des Benutzers, der die letzte Änderung vorgenommen hat [ATV]
        /// </summary>
        public string LastModifiedBy { get; private set; }

        /// <summary>
        /// Erstellt eine neue Session
        /// </summary>
        private Session() 
        {
            // Für EF Core
        }

        /// <summary>
        /// Erstellt eine neue Session mit den angegebenen Parametern
        /// </summary>
        public static Session Create(
            Guid patientId, 
            DateTime sessionDate, 
            byte[] encryptedReason, 
            string audioFilePath, 
            string createdBy)
        {
            var session = new Session
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                SessionDate = sessionDate,
                EncryptedReason = encryptedReason,
                AudioFilePath = audioFilePath,
                Status = SessionStatus.Active,
                ProcessedInCloud = false,
                Created = DateTime.UtcNow,
                CreatedBy = createdBy,
                LastModified = DateTime.UtcNow,
                LastModifiedBy = createdBy
            };

            return session;
        }

        /// <summary>
        /// Markiert die Session als abgeschlossen und unveränderlich [SK]
        /// </summary>
        public void CompleteSession(string modifiedBy)
        {
            Status = SessionStatus.Completed;
            UpdateAuditInfo(modifiedBy);
        }

        /// <summary>
        /// Aktualisiert die Audit-Informationen [ATV]
        /// </summary>
        public void UpdateAuditInfo(string modifiedBy)
        {
            LastModified = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }

        /// <summary>
        /// Setzt den verwendeten KI-Provider [PK]
        /// </summary>
        public void SetAIProvider(string provider, bool isCloud, string modifiedBy)
        {
            AIProvider = provider;
            ProcessedInCloud = isCloud;
            UpdateAuditInfo(modifiedBy);
        }
    }

    /// <summary>
    /// Status einer Session
    /// </summary>
    public enum SessionStatus
    {
        /// <summary>
        /// Aktive Session, kann bearbeitet werden
        /// </summary>
        Active,

        /// <summary>
        /// Abgeschlossene Session, unveränderlich [SK]
        /// </summary>
        Completed,

        /// <summary>
        /// Archivierte Session
        /// </summary>
        Archived
    }
}
