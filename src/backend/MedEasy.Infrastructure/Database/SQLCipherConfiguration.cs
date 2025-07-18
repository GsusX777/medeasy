// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MedEasy.Infrastructure.Database
{
    /// <summary>
    /// Konfiguration für SQLCipher mit AES-256 Verschlüsselung [SP]
    /// </summary>
    public class SQLCipherConfiguration : IDbContextOptionsExtension
    {
        /// <summary>
        /// Info für EF Core
        /// </summary>
        private DbContextOptionsExtensionInfo _info;

        /// <summary>
        /// Schlüssel-Ableitungs-Iterationen (PBKDF2)
        /// </summary>
        public int KeyDerivationIterations { get; set; } = 256000; // Hohe Anzahl für bessere Sicherheit

        /// <summary>
        /// Cipher-Modus für SQLCipher
        /// </summary>
        public string CipherMode { get; set; } = "aes-256-cbc"; // AES-256 als Standard [SP]

        /// <summary>
        /// Konstruktor
        /// </summary>
        public SQLCipherConfiguration()
        {
            _info = new SQLCipherConfigurationInfo(this);
        }

        /// <summary>
        /// Gibt die Info für EF Core zurück
        /// </summary>
        public DbContextOptionsExtensionInfo Info => _info;

        /// <summary>
        /// Validiert die Konfiguration
        /// </summary>
        public void Validate(IDbContextOptions options)
        {
            // Keine spezielle Validierung notwendig
        }

        /// <summary>
        /// Wendet die Konfiguration auf die Connection an
        /// </summary>
        public void ApplyConfiguration(SqliteConnection connection, string encryptionKey)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentException("Encryption key cannot be null or empty", nameof(encryptionKey));

            // Stelle sicher, dass die Verbindung offen ist
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            // Führe SQLCipher-Konfigurationsbefehle aus [SP]
            using var command = connection.CreateCommand();
            
            // PRAGMA key mit dem Verschlüsselungsschlüssel
            command.CommandText = "PRAGMA key = @key";
            command.Parameters.AddWithValue("@key", encryptionKey);
            command.ExecuteNonQuery();

            // Setze Cipher auf AES-256 [SP]
            command.CommandText = $"PRAGMA cipher = '{CipherMode}'";
            command.Parameters.Clear();
            command.ExecuteNonQuery();

            // Setze die Anzahl der Schlüsselableitungsiterationen
            command.CommandText = $"PRAGMA kdf_iter = {KeyDerivationIterations}";
            command.Parameters.Clear();
            command.ExecuteNonQuery();

            // Aktiviere Secure Delete (Überschreiben gelöschter Daten) [ZTS]
            command.CommandText = "PRAGMA secure_delete = ON";
            command.Parameters.Clear();
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Info-Klasse für EF Core
        /// </summary>
        private class SQLCipherConfigurationInfo : DbContextOptionsExtensionInfo
        {
            private readonly SQLCipherConfiguration _extension;

            /// <summary>
            /// Konstruktor
            /// </summary>
            public SQLCipherConfigurationInfo(SQLCipherConfiguration extension) 
                : base(extension)
            {
                _extension = extension;
            }

            /// <summary>
            /// Gibt true zurück, wenn die Konfiguration gleich ist
            /// </summary>
            public override bool IsDatabaseProvider => false;

            /// <summary>
            /// Gibt einen eindeutigen Bezeichner zurück
            /// </summary>
            public override string LogFragment => "SQLCipherConfiguration";

            /// <summary>
            /// Gibt einen Hash-Code für die Konfiguration zurück
            /// </summary>
            public override int GetServiceProviderHashCode() => 0;

            /// <summary>
            /// Prüft, ob die Konfiguration gleich ist
            /// </summary>
            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            {
                return other is SQLCipherConfigurationInfo;
            }

            /// <summary>
            /// Gibt eine Debug-Info zurück
            /// </summary>
            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                debugInfo["SQLCipher:CipherMode"] = _extension.CipherMode;
                debugInfo["SQLCipher:KeyDerivationIterations"] = _extension.KeyDerivationIterations.ToString();
            }
        }
    }
}
