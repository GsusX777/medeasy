// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

namespace MedEasy.Application.Interfaces;

/// <summary>
/// Encryption Service Interface für AES-256-GCM Feldverschlüsselung [SP][EIV]
/// Abstrahiert Verschlüsselungslogik vom Application Layer
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Verschlüsselt einen String mit AES-256-GCM [SP]
    /// </summary>
    Task<byte[]> EncryptAsync(string plaintext);

    /// <summary>
    /// Verschlüsselt binäre Daten (z.B. Audio) mit AES-256-GCM [SP][EIV]
    /// </summary>
    Task<byte[]> EncryptAsync(byte[] data);

    /// <summary>
    /// Entschlüsselt verschlüsselte Daten [SP]
    /// </summary>
    Task<string> DecryptAsync(byte[] encryptedData);

    /// <summary>
    /// Entschlüsselt verschlüsselte binäre Daten [SP][EIV]
    /// </summary>
    Task<byte[]> DecryptBinaryAsync(byte[] encryptedData);

    /// <summary>
    /// Erstellt Hash der Schweizer Versicherungsnummer [SF][AIU]
    /// </summary>
    string HashInsuranceNumber(string insuranceNumber);

    /// <summary>
    /// Generiert einen neuen Verschlüsselungsschlüssel [SP][ZTS]
    /// </summary>
    byte[] GenerateEncryptionKey();

    /// <summary>
    /// Validiert einen Verschlüsselungsschlüssel [SP][ZTS]
    /// </summary>
    bool IsValidEncryptionKey(byte[] key);
    
    // Synchrone Wrapper-Methoden für Controller-Kompatibilität [SP][EIV]
    
    /// <summary>
    /// Verschlüsselt einen String synchron [SP]
    /// </summary>
    byte[] EncryptText(string plaintext);
    
    /// <summary>
    /// Entschlüsselt verschlüsselte binäre Daten synchron [SP][EIV]
    /// </summary>
    byte[] DecryptBinary(byte[] encryptedData);
}
