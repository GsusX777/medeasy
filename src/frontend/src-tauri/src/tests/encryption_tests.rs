// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Encryption Tests [KP100] [AIU] [SP]
// Tests für die Feldverschlüsselung mit AES-256-GCM

#[allow(unused_imports)]

use crate::database::encryption::{FieldEncryption, EncryptionError};
use std::env;
use std::sync::Once;
use base64::Engine; // Import für die decode-Methode [ZTS]

// Initialisiere die Testumgebung
static INIT: Once = Once::new();
fn initialize() {
    // Setze die Umgebungsvariablen direkt, ohne Once zu verwenden [ZTS]
    // Dies stellt sicher, dass die Variablen für jeden Test neu gesetzt werden
    env::set_var("MEDEASY_FIELD_ENCRYPTION_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
    env::set_var("USE_ENCRYPTION", "true");
}

#[test]
/// Test, dass die Verschlüsselung mit gültigem Schlüssel initialisiert werden kann [SP]
fn test_field_encryption_initialization() {
    initialize();
    
    let result = FieldEncryption::new();
    assert!(result.is_ok(), "Feldverschlüsselung sollte mit gültigem Schlüssel initialisiert werden können");
}

#[test]
/// Test, dass die Verschlüsselung ohne Schlüssel fehlschlägt [AIU]
fn test_field_encryption_fails_without_key() {
    // WICHTIG: Zuerst initialize() aufrufen, damit die anderen Tests nicht beeinträchtigt werden [ZTS]
    initialize();
    
    // Dann erst die Umgebungsvariable entfernen für diesen speziellen Test
    env::remove_var("MEDEASY_FIELD_ENCRYPTION_KEY");
    env::set_var("USE_ENCRYPTION", "true");
    
    // Verschlüsselung sollte ohne Schlüssel fehlschlagen
    let result = FieldEncryption::new();
    
    // Schlüssel für andere Tests wiederherstellen [SP][ZTS]
    env::set_var("MEDEASY_FIELD_ENCRYPTION_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
    
    // Prüfen, ob der erwartete Fehler auftritt
    assert!(result.is_err(), "Feldverschlüsselung sollte ohne Schlüssel fehlschlagen");
    
    match result {
        Err(EncryptionError::EnvError(msg)) => {
            assert!(msg.contains("Field encryption key not set") || 
                    msg.contains("encryption key"), 
                    "Fehlermeldung sollte auf fehlenden Schlüssel hinweisen: {}", msg);
        },
        Err(e) => {
            println!("Unerwarteter Fehlertyp: {:?}", e);
            panic!("Falscher Fehlertyp: {:?}", e);
        },
        Ok(_) => {
            panic!("Test sollte fehlschlagen, da Verschlüsselung ohne Schlüssel nicht möglich sein sollte");
        }
    }
    
    // Umgebungsvariable wiederherstellen für andere Tests
    initialize();
}

#[test]
/// Test, dass die Verschlüsselung und Entschlüsselung korrekt funktionieren [SP]
fn test_encryption_decryption_roundtrip() {
    initialize();
    
    let encryption = FieldEncryption::new().expect("Feldverschlüsselung sollte initialisiert werden können");
    let plaintext = "Vertrauliche Patientendaten";
    
    // Verschlüsseln
    let ciphertext = encryption.encrypt(plaintext).expect("Verschlüsselung sollte erfolgreich sein");
    
    // Entschlüsseln
    let decrypted = encryption.decrypt(&ciphertext).expect("Entschlüsselung sollte erfolgreich sein");
    
    // Überprüfen, ob der entschlüsselte Text dem Originaltext entspricht
    assert_eq!(decrypted, plaintext, "Entschlüsselter Text sollte dem Originaltext entsprechen");
}

#[test]
/// Test, dass die Verschlüsselung mit zufälligem Nonce unterschiedliche Ergebnisse liefert [SP]
fn test_encryption_randomness() {
    initialize();
    
    let encryption = FieldEncryption::new().expect("Feldverschlüsselung sollte initialisiert werden können");
    let plaintext = "Vertrauliche Patientendaten";
    
    // Zweimal verschlüsseln
    let ciphertext1 = encryption.encrypt(plaintext).expect("Erste Verschlüsselung sollte erfolgreich sein");
    let ciphertext2 = encryption.encrypt(plaintext).expect("Zweite Verschlüsselung sollte erfolgreich sein");
    
    // Überprüfen, ob die Verschlüsselungen unterschiedlich sind (wegen zufälligem Nonce)
    assert_ne!(ciphertext1, ciphertext2, "Verschlüsselungen sollten unterschiedlich sein");
    
    // Beide sollten aber korrekt entschlüsselt werden können
    let decrypted1 = encryption.decrypt(&ciphertext1).expect("Erste Entschlüsselung sollte erfolgreich sein");
    let decrypted2 = encryption.decrypt(&ciphertext2).expect("Zweite Entschlüsselung sollte erfolgreich sein");
    
    assert_eq!(decrypted1, plaintext, "Erste Entschlüsselung sollte korrekt sein");
    assert_eq!(decrypted2, plaintext, "Zweite Entschlüsselung sollte korrekt sein");
}

#[test]
/// Test, dass die Entschlüsselung mit manipulierten Daten fehlschlägt [SP]
fn test_tampered_data_fails_decryption() {
    initialize();
    
    let encryption = FieldEncryption::new().expect("Feldverschlüsselung sollte initialisiert werden können");
    let plaintext = "Vertrauliche Patientendaten";
    
    // Verschlüsseln
    let mut ciphertext = encryption.encrypt(plaintext).expect("Verschlüsselung sollte erfolgreich sein");
    
    // Daten manipulieren (nach dem Nonce)
    if ciphertext.len() > 20 {
        ciphertext[15] ^= 0x01; // Ein Bit ändern
    }
    
    // Entschlüsselung sollte fehlschlagen
    let result = encryption.decrypt(&ciphertext);
    assert!(result.is_err(), "Entschlüsselung manipulierter Daten sollte fehlschlagen");
}

#[test]
/// Test, dass die Entschlüsselung mit ungültigen Daten fehlschlägt [SP]
fn test_invalid_data_fails_decryption() {
    initialize();
    
    let encryption = FieldEncryption::new().expect("Feldverschlüsselung sollte initialisiert werden können");
    
    // Ungültige Daten (zu kurz für Nonce)
    let invalid_data = vec![1, 2, 3, 4, 5];
    
    // Entschlüsselung sollte fehlschlagen
    let result = encryption.decrypt(&invalid_data);
    assert!(result.is_err(), "Entschlüsselung ungültiger Daten sollte fehlschlagen");
}

#[test]
/// Test, dass die Schlüsselgenerierung funktioniert [SP]
fn test_key_generation() {
    let key = FieldEncryption::generate_key().expect("Schlüsselgenerierung sollte erfolgreich sein");
    
    // Schlüssel sollte ein gültiger Base64-String sein
    assert!(!key.is_empty(), "Schlüssel sollte nicht leer sein");
    
    // Dekodieren und Länge überprüfen
    let key_bytes = base64::engine::general_purpose::STANDARD.decode(&key)
        .expect("Schlüssel sollte dekodierbarer Base64-String sein");
    assert_eq!(key_bytes.len(), 32, "Schlüssel sollte 32 Bytes lang sein");
}

#[test]
/// Test, dass die Versicherungsnummern-Validierung funktioniert [SF]
fn test_insurance_number_validation() {
    initialize();
    
    let encryption = FieldEncryption::new().expect("Feldverschlüsselung sollte initialisiert werden können");
    
    // Gültige Schweizer Versicherungsnummer [SF]
    let valid_number = "123.4567.8901.23";
    let result = encryption.hash_insurance_number(valid_number);
    assert!(result.is_ok(), "Gültige Versicherungsnummer sollte akzeptiert werden");
    
    // Ungültige Formate
    let invalid_formats = [
        "123456789012", // Keine Punkte
        "123.456.789.12", // Falsche Gruppierung
        "12.3456.7890.12", // Erste Gruppe zu kurz
        "123.456.7890.12", // Zweite Gruppe zu kurz
        "123.4567.890.12", // Dritte Gruppe zu kurz
        "123.4567.8901.2", // Vierte Gruppe zu kurz
        "abc.defg.hijk.lm", // Keine Zahlen
    ];
    
    for invalid in invalid_formats {
        let result = encryption.hash_insurance_number(invalid);
        assert!(result.is_err(), "Ungültiges Format sollte abgelehnt werden: {}", invalid);
    }
}

#[test]
/// Test, dass das Hashing konsistent ist [SP]
fn test_hash_consistency() {
    initialize();
    
    let encryption = FieldEncryption::new().expect("Feldverschlüsselung sollte initialisiert werden können");
    let value = "Testdaten";
    
    // Zweimal hashen
    let hash1 = encryption.hash_value(value);
    let hash2 = encryption.hash_value(value);
    
    // Hashes sollten identisch sein
    assert_eq!(hash1, hash2, "Hashes sollten für gleiche Eingabe identisch sein");
}
