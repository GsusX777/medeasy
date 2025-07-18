// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.Data.Sqlite;
using Microsoft.Data.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;

namespace MedEasy.Infrastructure.Database
{
    /// <summary>
    /// Factory für SQLCipher-Verbindungen mit AES-256 Verschlüsselung [SP]
    /// </summary>
    public class SQLCipherConnectionFactory : ISqliteConnectionFactory
    {
        private readonly IOptions<SQLCipherOptions> _options;

        /// <summary>
        /// Konstruktor
        /// </summary>
        public SQLCipherConnectionFactory(IOptions<SQLCipherOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Erstellt eine neue SQLCipher-Verbindung
        /// </summary>
        public SqliteConnection Create(string connectionString, DbContextOptions options)
        {
            // Erstelle die Verbindung
            var connection = new SqliteConnection(connectionString);
            
            // Öffne die Verbindung
            connection.Open();

            // Hole die SQLCipher-Konfiguration
            var sqlCipherConfig = options.FindExtension<SQLCipherConfiguration>() ?? new SQLCipherConfiguration();
            
            // Hole den Verschlüsselungsschlüssel aus den Optionen
            var encryptionKey = _options.Value.EncryptionKey;
            
            // Validiere den Schlüssel
            if (string.IsNullOrEmpty(encryptionKey))
            {
                throw new InvalidOperationException("SQLCipher encryption key is not configured [SP]");
            }

            // Wende die SQLCipher-Konfiguration an
            sqlCipherConfig.ApplyConfiguration(connection, encryptionKey);

            // Verifiziere die Verschlüsselung (wirft eine Exception, wenn die Datenbank nicht korrekt entschlüsselt werden kann)
            VerifyEncryption(connection);

            return connection;
        }

        /// <summary>
        /// Verifiziert, dass die Datenbank korrekt entschlüsselt werden kann
        /// </summary>
        private void VerifyEncryption(SqliteConnection connection)
        {
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT count(*) FROM sqlite_master";
                command.ExecuteScalar();
            }
            catch (SqliteException ex)
            {
                // Wenn ein Fehler auftritt, ist der Schlüssel wahrscheinlich falsch
                throw new InvalidOperationException("Failed to decrypt database. The encryption key may be incorrect [SP]", ex);
            }
        }
    }

    /// <summary>
    /// Optionen für SQLCipher
    /// </summary>
    public class SQLCipherOptions
    {
        /// <summary>
        /// Verschlüsselungsschlüssel für SQLCipher (AES-256) [SP]
        /// </summary>
        public string EncryptionKey { get; set; }

        /// <summary>
        /// Gibt einen sicheren Schlüssel aus einem Passwort zurück
        /// </summary>
        public static string DeriveKeyFromPassword(string password, string salt)
        {
            // Verwende PBKDF2 zur Schlüsselableitung
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password, 
                Encoding.UTF8.GetBytes(salt), 
                256000, // Hohe Anzahl an Iterationen für bessere Sicherheit
                HashAlgorithmName.SHA256);
            
            // Generiere einen 256-Bit-Schlüssel (32 Bytes) für AES-256 [SP]
            byte[] key = pbkdf2.GetBytes(32);
            
            // Konvertiere den Schlüssel in einen Hex-String für SQLCipher
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }
    }

    /// <summary>
    /// Extension-Methoden für SQLCipher-Konfiguration
    /// </summary>
    public static class SQLCipherExtensions
    {
        /// <summary>
        /// Fügt SQLCipher-Unterstützung zu DbContextOptionsBuilder hinzu
        /// </summary>
        public static DbContextOptionsBuilder UseSQLCipher(
            this DbContextOptionsBuilder optionsBuilder,
            Action<SQLCipherConfiguration> configureOptions = null)
        {
            var extension = optionsBuilder.Options.FindExtension<SQLCipherConfiguration>() 
                ?? new SQLCipherConfiguration();
            
            configureOptions?.Invoke(extension);
            
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            
            return optionsBuilder;
        }
    }
}
