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
        /// Verschlüsselter Vorname des Patienten [EIV][SP]
        /// </summary>
        public byte[] EncryptedFirstName { get; private set; }

        /// <summary>
        /// Verschlüsselter Nachname des Patienten [EIV][SP]
        /// </summary>
        public byte[] EncryptedLastName { get; private set; }

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
        /// Zeitstempel der Erstellung [ATV]
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// ID des Benutzers, der die Entity erstellt hat [ATV]
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Zeitstempel der letzten Änderung [ATV]
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// ID des Benutzers, der die letzte Änderung vorgenommen hat [ATV]
        /// </summary>
        public string LastModifiedBy { get; set; } = string.Empty;

        /// <summary>
        /// Erstellt einen neuen Patienten mit verschlüsselten Daten
        /// </summary>
        private Patient() 
        {
            // Für EF Core
        }

        /// <summary>
        /// Factory-Methode: Erstellt einen neuen Patienten mit getrennten Namen [SF][EIV]
        /// </summary>
        public static Patient Create(
            byte[] encryptedFirstName,
            byte[] encryptedLastName,
            string insuranceNumberHash,
            DateOnly dateOfBirth,
            byte[] encryptedGender,
            byte[] encryptedInsuranceProvider,
            string createdBy)
        {
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                EncryptedFirstName = encryptedFirstName ?? throw new ArgumentNullException(nameof(encryptedFirstName)),
                EncryptedLastName = encryptedLastName ?? throw new ArgumentNullException(nameof(encryptedLastName)),
                InsuranceNumberHash = insuranceNumberHash ?? throw new ArgumentNullException(nameof(insuranceNumberHash)),
                DateOfBirth = dateOfBirth,
                EncryptedGender = encryptedGender ?? throw new ArgumentNullException(nameof(encryptedGender)),
                EncryptedInsuranceProvider = encryptedInsuranceProvider ?? throw new ArgumentNullException(nameof(encryptedInsuranceProvider)),
                Created = DateTime.UtcNow,
                CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy)),
                LastModified = DateTime.UtcNow,
                LastModifiedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy))
            };

            return patient;
        }

        /// <summary>
        /// Aktualisiert die verschlüsselten Namen des Patienten [EIV][ATV]
        /// </summary>
        public void UpdateNames(byte[] encryptedFirstName, byte[] encryptedLastName, string modifiedBy)
        {
            EncryptedFirstName = encryptedFirstName ?? throw new ArgumentNullException(nameof(encryptedFirstName));
            EncryptedLastName = encryptedLastName ?? throw new ArgumentNullException(nameof(encryptedLastName));
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
