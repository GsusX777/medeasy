// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using MedEasy.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MedEasy.Infrastructure.Services
{
    /// <summary>
    /// AES-256-GCM Verschlüsselungsservice für Patientendaten [SP][EIV]
    /// Implementiert sichere Verschlüsselung nach Schweizer nDSG-Standards [DSC]
    /// </summary>
    public class EncryptionService : IEncryptionService, IDisposable
    {
        private readonly ILogger<EncryptionService> _logger;
        private readonly byte[] _encryptionKey;
        private readonly Regex _swissInsuranceRegex;
        private bool _disposed = false;

        /// <summary>
        /// Konstruktor für EncryptionService
        /// </summary>
        /// <param name="configuration">Konfiguration für Verschlüsselungsschlüssel</param>
        /// <param name="logger">Logger für Audit-Trail [ATV]</param>
        public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Verschlüsselungsschlüssel aus Umgebungsvariablen laden [ZTS]
            var keyString = configuration["Encryption:AES_KEY"] 
                ?? throw new InvalidOperationException("Encryption:AES_KEY nicht in Konfiguration gefunden [ZTS]");
            
            _encryptionKey = ValidateAndParseKey(keyString);
            
            // Schweizer Versicherungsnummer Regex [SF]
            _swissInsuranceRegex = new Regex(@"^\d{3}\.\d{4}\.\d{4}\.\d{2}$", RegexOptions.Compiled);
            
            _logger.LogInformation("EncryptionService initialisiert mit AES-256-GCM [SP]");
        }

        /// <summary>
        /// Verschlüsselt Text mit AES-256-GCM [SP]
        /// </summary>
        /// <param name="plainText">Zu verschlüsselnder Text</param>
        /// <returns>Verschlüsselte Daten als byte[]</returns>
        public byte[] Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("Plaintext darf nicht null oder leer sein", nameof(plainText));

            try
            {
                using var aes = new AesGcm(_encryptionKey);
                
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var nonce = new byte[12]; // 96-bit nonce für GCM
                var ciphertext = new byte[plainBytes.Length];
                var tag = new byte[16]; // 128-bit authentication tag
                
                // Sichere Zufallszahl für Nonce generieren [ZTS]
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(nonce);
                }
                
                // AES-256-GCM Verschlüsselung [SP]
                aes.Encrypt(nonce, plainBytes, ciphertext, tag);
                
                // Format: [nonce(12)] + [tag(16)] + [ciphertext(variable)]
                var result = new byte[nonce.Length + tag.Length + ciphertext.Length];
                Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
                Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
                Buffer.BlockCopy(ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length);
                
                _logger.LogDebug("Text erfolgreich verschlüsselt (Länge: {Length} bytes) [ATV]", result.Length);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler bei Verschlüsselung [ECP]");
                throw new InvalidOperationException("Verschlüsselung fehlgeschlagen", ex);
            }
        }

        /// <summary>
        /// Entschlüsselt Daten mit AES-256-GCM [SP]
        /// </summary>
        /// <param name="encryptedData">Verschlüsselte Daten</param>
        /// <returns>Entschlüsselter Text</returns>
        public string Decrypt(byte[] encryptedData)
        {
            if (encryptedData == null || encryptedData.Length < 28) // 12 + 16 = 28 minimum
                throw new ArgumentException("Verschlüsselte Daten ungültig", nameof(encryptedData));

            try
            {
                using var aes = new AesGcm(_encryptionKey);
                
                // Format parsen: [nonce(12)] + [tag(16)] + [ciphertext(variable)]
                var nonce = new byte[12];
                var tag = new byte[16];
                var ciphertext = new byte[encryptedData.Length - 28];
                
                Buffer.BlockCopy(encryptedData, 0, nonce, 0, 12);
                Buffer.BlockCopy(encryptedData, 12, tag, 0, 16);
                Buffer.BlockCopy(encryptedData, 28, ciphertext, 0, ciphertext.Length);
                
                var plainBytes = new byte[ciphertext.Length];
                
                // AES-256-GCM Entschlüsselung mit Authentifizierung [SP]
                aes.Decrypt(nonce, ciphertext, tag, plainBytes);
                
                var result = Encoding.UTF8.GetString(plainBytes);
                _logger.LogDebug("Text erfolgreich entschlüsselt [ATV]");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler bei Entschlüsselung [ECP]");
                throw new InvalidOperationException("Entschlüsselung fehlgeschlagen", ex);
            }
        }

        /// <summary>
        /// Erstellt SHA-256 Hash einer Schweizer Versicherungsnummer [SF]
        /// </summary>
        /// <param name="insuranceNumber">Versicherungsnummer im Format XXX.XXXX.XXXX.XX</param>
        /// <returns>SHA-256 Hash als Hex-String</returns>
        public string HashInsuranceNumber(string insuranceNumber)
        {
            if (string.IsNullOrEmpty(insuranceNumber))
                throw new ArgumentException("Versicherungsnummer darf nicht null oder leer sein", nameof(insuranceNumber));

            // Schweizer Format validieren [SF]
            if (!_swissInsuranceRegex.IsMatch(insuranceNumber))
                throw new ArgumentException("Versicherungsnummer muss im Format XXX.XXXX.XXXX.XX sein [SF]", nameof(insuranceNumber));

            try
            {
                using var sha256 = SHA256.Create();
                var inputBytes = Encoding.UTF8.GetBytes(insuranceNumber);
                var hashBytes = sha256.ComputeHash(inputBytes);
                var result = Convert.ToHexString(hashBytes).ToLowerInvariant();
                
                _logger.LogDebug("Versicherungsnummer erfolgreich gehasht [ATV]");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Hashen der Versicherungsnummer [ECP]");
                throw new InvalidOperationException("Hashing fehlgeschlagen", ex);
            }
        }

        /// <summary>
        /// Generiert einen neuen 256-bit AES-Schlüssel [ZTS]
        /// </summary>
        /// <returns>Base64-kodierter Schlüssel</returns>
        public string GenerateNewKey()
        {
            try
            {
                var key = new byte[32]; // 256 bits
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(key);
                }
                
                var result = Convert.ToBase64String(key);
                _logger.LogInformation("Neuer AES-256 Schlüssel generiert [ZTS]");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler bei Schlüsselgenerierung [ECP]");
                throw new InvalidOperationException("Schlüsselgenerierung fehlgeschlagen", ex);
            }
        }

        /// <summary>
        /// Validiert einen AES-256 Schlüssel [ZTS]
        /// </summary>
        /// <param name="key">Base64-kodierter Schlüssel</param>
        /// <returns>True wenn gültig</returns>
        public bool ValidateKey(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return false;

                var keyBytes = Convert.FromBase64String(key);
                var isValid = keyBytes.Length == 32; // 256 bits
                
                _logger.LogDebug("Schlüssel-Validierung: {IsValid} [ATV]", isValid);
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ungültiger Schlüssel bei Validierung [ECP]");
                return false;
            }
        }

        /// <summary>
        /// Validiert und parst einen Verschlüsselungsschlüssel [ZTS]
        /// </summary>
        /// <param name="keyString">Base64-kodierter Schlüssel</param>
        /// <returns>Schlüssel als byte[]</returns>
        private byte[] ValidateAndParseKey(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
                throw new ArgumentException("Verschlüsselungsschlüssel darf nicht null oder leer sein [ZTS]");

            try
            {
                var keyBytes = Convert.FromBase64String(keyString);
                
                if (keyBytes.Length != 32) // 256 bits
                    throw new ArgumentException($"Verschlüsselungsschlüssel muss genau 32 Bytes (256 bits) haben, erhalten: {keyBytes.Length} [ZTS]");

                return keyBytes;
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Verschlüsselungsschlüssel muss gültiges Base64 sein [ZTS]", ex);
            }
        }

        /// <summary>
        /// Dispose-Pattern für sichere Speicherbereinigung [ZTS]
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Geschützte Dispose-Methode [ZTS]
        /// </summary>
        /// <param name="disposing">True wenn von Dispose() aufgerufen</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Verschlüsselungsschlüssel sicher überschreiben [ZTS]
                    if (_encryptionKey != null)
                    {
                        Array.Clear(_encryptionKey, 0, _encryptionKey.Length);
                    }
                    
                    _logger.LogDebug("EncryptionService disposed [ATV]");
                }
                
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer für Notfall-Cleanup [ZTS]
        /// </summary>
        ~EncryptionService()
        {
            Dispose(false);
        }
    }
}
