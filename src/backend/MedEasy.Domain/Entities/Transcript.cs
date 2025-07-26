// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using System;

namespace MedEasy.Domain.Entities
{
    /// <summary>
    /// Repräsentiert ein Transkript einer Arzt-Patienten-Konsultation [MFD]
    /// </summary>
    public class Transcript : IHasAuditInfo
    {
        /// <summary>
        /// Eindeutige ID des Transkripts
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Referenz zur Session
        /// </summary>
        public Guid SessionId { get; private set; }

        /// <summary>
        /// Navigationseigenschaft zur Session
        /// </summary>
        public Session Session { get; private set; }

        /// <summary>
        /// Verschlüsselter Originaltext des Transkripts [EIV]
        /// </summary>
        public byte[] EncryptedOriginalText { get; private set; }

        /// <summary>
        /// Verschlüsselter anonymisierter Text des Transkripts [EIV][AIU]
        /// </summary>
        public byte[] EncryptedAnonymizedText { get; private set; }

        /// <summary>
        /// Verwendetes Whisper-Modell für die Transkription [WMM]
        /// </summary>
        public string WhisperModel { get; private set; }

        /// <summary>
        /// Erkannte Sprache (Hochdeutsch, Schweizerdeutsch) [SDH]
        /// </summary>
        public string DetectedLanguage { get; private set; }

        /// <summary>
        /// Flag für erkanntes Schweizerdeutsch [SDH]
        /// </summary>
        public bool IsSwissGerman { get; private set; }

        /// <summary>
        /// Konfidenz der Spracherkennung in Prozent
        /// </summary>
        public double LanguageConfidence { get; private set; }

        /// <summary>
        /// Flag für Einträge in der Anonymisierungs-Review-Queue [ARQ]
        /// </summary>
        public bool RequiresAnonymizationReview { get; private set; }

        /// <summary>
        /// Zeitstempel der Erstellung für Audit-Trail [ATV]
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// ID des Benutzers, der das Transkript erstellt hat [ATV]
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Zeitstempel der letzten Änderung für Audit-Trail [ATV]
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// ID des Benutzers, der die letzte Änderung vorgenommen hat [ATV]
        /// </summary>
        public string LastModifiedBy { get; set; } = string.Empty;

        /// <summary>
        /// Erstellt ein neues Transkript
        /// </summary>
        private Transcript() 
        {
            // Für EF Core
        }

        /// <summary>
        /// Erstellt ein neues Transkript mit den angegebenen Parametern
        /// </summary>
        public static Transcript Create(
            Guid sessionId,
            byte[] encryptedOriginalText,
            byte[] encryptedAnonymizedText,
            string whisperModel,
            string detectedLanguage,
            bool isSwissGerman,
            double languageConfidence,
            string createdBy)
        {
            var transcript = new Transcript
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                EncryptedOriginalText = encryptedOriginalText,
                EncryptedAnonymizedText = encryptedAnonymizedText,
                WhisperModel = whisperModel,
                DetectedLanguage = detectedLanguage,
                IsSwissGerman = isSwissGerman,
                LanguageConfidence = languageConfidence,
                // Wenn die Konfidenz unter 80% liegt, zur Review-Queue hinzufügen [ARQ]
                RequiresAnonymizationReview = languageConfidence < 80.0,
                Created = DateTime.UtcNow,
                CreatedBy = createdBy,
                LastModified = DateTime.UtcNow,
                LastModifiedBy = createdBy
            };

            return transcript;
        }

        /// <summary>
        /// Aktualisiert die anonymisierte Version des Transkripts [AIU]
        /// </summary>
        public void UpdateAnonymizedText(byte[] encryptedAnonymizedText, string modifiedBy)
        {
            EncryptedAnonymizedText = encryptedAnonymizedText;
            RequiresAnonymizationReview = false;
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
    }
}
