// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace MedEasy.Infrastructure.Database
{
    /// <summary>
    /// Interface für SQLite-Verbindungs-Factory [SP]
    /// </summary>
    public interface ISqliteConnectionFactory
    {
        /// <summary>
        /// Erstellt eine neue SQLite-Verbindung
        /// </summary>
        /// <param name="connectionString">Verbindungsstring</param>
        /// <returns>SQLite-Verbindung</returns>
        DbConnection CreateConnection(string connectionString);
        
        /// <summary>
        /// Erstellt eine neue SQLCipher-Verbindung mit Verschlüsselung [SP]
        /// </summary>
        /// <param name="connectionString">Verbindungsstring</param>
        /// <param name="encryptionKey">Verschlüsselungsschlüssel</param>
        /// <returns>Verschlüsselte SQLite-Verbindung</returns>
        SqliteConnection CreateEncryptedConnection(string connectionString, string encryptionKey);
    }
}
