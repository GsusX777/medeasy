// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Database Tests [KP100] [SP]
// Tests für die Datenbankverbindung und SQLCipher-Integration

#[allow(unused_imports)]

use crate::database::connection::DatabaseManager;
use rusqlite::Result as SqliteResult;
use std::env;
use std::fs;
use std::path::Path;

// Initialisiere die Testumgebung
fn initialize() {
    // Setze die Umgebungsvariablen für jeden Test neu
    env::set_var("DATABASE_URL", "medeasy_test.db");
    env::set_var("MEDEASY_DB_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
    env::set_var("MEDEASY_FIELD_ENCRYPTION_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
    env::set_var("USE_ENCRYPTION", "true");
    
    // Sicherstellen, dass keine alte Testdatenbank existiert
    cleanup();
}

// Bereinige die Testumgebung
fn cleanup() {
    // Lösche die Testdatenbank, falls vorhanden
    if Path::new("medeasy_test.db").exists() {
        fs::remove_file("medeasy_test.db").expect("Testdatenbank konnte nicht gelöscht werden");
    }
    if Path::new("medeasy_test.db-shm").exists() {
        let _ = fs::remove_file("medeasy_test.db-shm"); // WAL-Datei
    }
    if Path::new("medeasy_test.db-wal").exists() {
        let _ = fs::remove_file("medeasy_test.db-wal"); // WAL-Datei
    }
}

#[test]
/// Test, dass eine einfache Datenbankverbindung mit Verschlüsselung hergestellt werden kann [SP]
fn test_database_connection_with_encryption() {
    initialize();
    cleanup();
    
    let db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
    let conn = db_manager.get_connection().expect("Verbindung sollte hergestellt werden können");
    
    // Einfache Abfrage ausführen
    let result: SqliteResult<i32> = conn.query_row("SELECT 1", [], |row| row.get(0));
    assert!(result.is_ok(), "Einfache Abfrage sollte erfolgreich sein");
    assert_eq!(result.unwrap(), 1, "Abfrageergebnis sollte 1 sein");
    
    cleanup();
}

#[test]
/// Test, dass die SQLCipher-Pragmas korrekt gesetzt sind [SP]
fn test_sqlcipher_pragmas() {
    initialize();
    cleanup();
    
    let db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
    let conn = db_manager.get_connection().expect("Verbindung sollte hergestellt werden können");
    
    // Überprüfen der SQLCipher-Pragmas
    // Verwende query_row statt pragma_query_value, da SQLCipher die Werte als Text zurückgibt [ZTS]
    let cipher_page_size: String = conn.query_row("PRAGMA cipher_page_size", [], |row| row.get(0))
        .expect("cipher_page_size sollte abgefragt werden können");
    assert_eq!(cipher_page_size, "4096", "cipher_page_size sollte 4096 sein");
    
    let kdf_iter: String = conn.query_row("PRAGMA kdf_iter", [], |row| row.get(0))
        .expect("kdf_iter sollte abgefragt werden können");
    assert_eq!(kdf_iter, "256000", "kdf_iter sollte 256000 sein");
    
    let memory_security: String = conn.query_row("PRAGMA cipher_memory_security", [], |row| row.get(0))
        .expect("cipher_memory_security sollte abgefragt werden können");
    assert_eq!(memory_security, "1", "cipher_memory_security sollte 1 sein");
    
    // Journal-Modus überprüfen (mit query_row, da es Ergebnisse zurückgibt)
    let journal_mode: String = conn.query_row("PRAGMA journal_mode", [], |row| row.get(0))
        .expect("journal_mode sollte abgefragt werden können");
    assert_eq!(journal_mode.to_uppercase(), "WAL", "journal_mode sollte WAL sein");
    
    cleanup();
}

#[test]
/// Test, dass die Datenbank erstellt wird, wenn sie nicht existiert [SP]
fn test_database_creation() {
    initialize();
    cleanup();
    
    // Sicherstellen, dass die Datenbank nicht existiert
    assert!(!Path::new("medeasy_test.db").exists(), "Testdatenbank sollte nicht existieren");
    
    // Datenbankmanager erstellen (sollte Datenbank erstellen)
    let db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
    let conn = db_manager.get_connection().expect("Verbindung sollte hergestellt werden können");
    
    // Einfache Abfrage ausführen
    let result: SqliteResult<i32> = conn.query_row("SELECT 1", [], |row| row.get(0));
    assert!(result.is_ok(), "Einfache Abfrage sollte erfolgreich sein");
    
    // Überprüfen, ob die Datenbank erstellt wurde
    assert!(Path::new("medeasy_test.db").exists(), "Testdatenbank sollte erstellt worden sein");
    
    cleanup();
}

#[test]
/// Test, dass der Connection-Pool funktioniert [SP]
fn test_connection_pooling() {
    initialize();
    cleanup();
    
    let db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
    
    // Mehrere Verbindungen anfordern
    let conn1 = db_manager.get_connection().expect("Erste Verbindung sollte hergestellt werden können");
    let conn2 = db_manager.get_connection().expect("Zweite Verbindung sollte hergestellt werden können");
    
    // Einfache Abfragen auf beiden Verbindungen ausführen
    let result1: SqliteResult<i32> = conn1.query_row("SELECT 1", [], |row| row.get(0));
    let result2: SqliteResult<i32> = conn2.query_row("SELECT 2", [], |row| row.get(0));
    
    assert!(result1.is_ok(), "Erste Abfrage sollte erfolgreich sein");
    assert!(result2.is_ok(), "Zweite Abfrage sollte erfolgreich sein");
    assert_eq!(result1.unwrap(), 1, "Erstes Abfrageergebnis sollte 1 sein");
    assert_eq!(result2.unwrap(), 2, "Zweites Abfrageergebnis sollte 2 sein");
    
    cleanup();
}

#[test]
/// Test, dass die Migrationen ausgeführt werden können [SP]
fn test_migrations() {
    initialize();
    cleanup();
    
    let mut db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
    
    // Migrationen ausführen
    db_manager.run_migrations().expect("Migrationen sollten ausgeführt werden können");
    
    // Überprüfen, ob die Tabellen existieren
    let conn = db_manager.get_connection().expect("Verbindung sollte hergestellt werden können");
    
    // Liste der erwarteten Tabellen
    let tables = ["patients", "transcripts", "audit_logs"];
    
    for table in tables.iter() {
        let result: SqliteResult<i32> = conn.query_row(
            "SELECT 1 FROM sqlite_master WHERE type='table' AND name=?",
            [table],
            |row| row.get(0)
        );
        assert!(result.is_ok(), "Tabelle {} sollte existieren", table);
    }
    
    cleanup();
}

#[test]
/// Test, dass die Verbindung fehlschlägt, wenn der falsche Schlüssel verwendet wird [SP]
fn test_connection_fails_with_wrong_key() {
    // Vollständige Isolation für diesen Test [ZTS]
    cleanup();
    env::set_var("DATABASE_URL", "wrong_key_test.db");
    env::set_var("MEDEASY_DB_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA="); // Originaler Schlüssel
    env::set_var("USE_ENCRYPTION", "true");
    
    // Datenbank mit dem ersten Schlüssel erstellen
    {
        let db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
        let conn = db_manager.get_connection().expect("Verbindung sollte hergestellt werden können");
        conn.execute("CREATE TABLE test (id INTEGER)", []).expect("Tabelle sollte erstellt werden können");
    }
    
    // Versuchen, mit einem anderen Schlüssel zu verbinden
    env::set_var("MEDEASY_DB_KEY", "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQE="); // Anderer gültiger Base64-Schlüssel
    
    // Einen Verbindungsversuch durchführen
    let db_result = DatabaseManager::new();
    
    // Der Pool sollte zwar erstellt werden, aber bei einem Verbindungsversuch fehlschlagen
    if let Ok(db) = db_result {
        // Erst beim Zugriff wird der Fehler auftreten
        let conn_result = db.get_connection();
        assert!(conn_result.is_err(), "Verbindung sollte mit falschem Schlüssel fehlschlagen");
    } else {
        // In einigen Implementierungen kann es auch direkt beim Pool-Erstellen fehlschlagen
        let err_string = format!("{:?}", db_result.err().unwrap());
        assert!(err_string.contains("file is not a database") || 
               err_string.contains("encryption") || 
               err_string.contains("SQLite"),
               "Fehlermeldung sollte auf Verschlüsselungsproblem hinweisen: {}", err_string);
    }
    
    // Spezielles Cleanup für diesen Test
    if Path::new("wrong_key_test.db").exists() {
        fs::remove_file("wrong_key_test.db").expect("Testdatenbank konnte nicht gelöscht werden");
    }
    if Path::new("wrong_key_test.db-shm").exists() {
        let _ = fs::remove_file("wrong_key_test.db-shm");
    }
    if Path::new("wrong_key_test.db-wal").exists() {
        let _ = fs::remove_file("wrong_key_test.db-wal");
    }
    
    // Umgebung für andere Tests wiederherstellen
    initialize();
}

#[test]
/// Test, dass die Verschlüsselung in Produktion erzwungen wird [SP][ZTS]
fn test_encryption_enforced_in_production() {
    // Vollständige Isolation für diesen Test [ZTS]
    cleanup();
    env::set_var("DATABASE_URL", "prod_test.db");
    
    // Wichtig: Verschlüsselung explizit deaktivieren
    env::set_var("USE_ENCRYPTION", "false");
    
    // Produktionsumgebung mit deaktivierter Verschlüsselung
    // Dies MUSS fehlschlagen, bevor ein Connection-Versuch unternommen wird
    let result = DatabaseManager::new_with_production_flag(true);
    
    // Der Fehler muss bereits bei new_with_production_flag auftreten
    assert!(result.is_err(), "Verbindung sollte fehlschlagen, wenn Verschlüsselung in Produktion deaktiviert ist");
    
    // Überprüfen, dass die Fehlermeldung auf die Verschlüsselungserzwingung hinweist
    let err_string = format!("{:?}", result.err().unwrap());
    assert!(err_string.contains("Unencrypted database not allowed in production") || 
           err_string.contains("Encryption must be enabled in production"),
           "Fehlermeldung sollte eindeutig auf Verschlüsselungserzwingung hinweisen: {}", err_string);
    
    // Spezielles Cleanup für diesen Test
    if Path::new("prod_test.db").exists() {
        fs::remove_file("prod_test.db").expect("Testdatenbank konnte nicht gelöscht werden");
    }
    if Path::new("prod_test.db-shm").exists() {
        let _ = fs::remove_file("prod_test.db-shm");
    }
    if Path::new("prod_test.db-wal").exists() {
        let _ = fs::remove_file("prod_test.db-wal");
    }
    
    // Umgebung für andere Tests wiederherstellen
    initialize();
}

#[test]
/// Test, dass die Verschlüsselung in Entwicklung optional ist [SP]
fn test_encryption_optional_in_development() {
    initialize();
    cleanup();
    
    // Entwicklungsumgebung simulieren
    env::set_var("USE_ENCRYPTION", "false");
    
    // Versuchen, eine unverschlüsselte Verbindung in Entwicklung herzustellen
    let result = DatabaseManager::new_with_production_flag(false);
    
    // Sollte erfolgreich sein
    assert!(result.is_ok(), "Verbindung sollte erfolgreich sein, wenn Verschlüsselung in Entwicklung deaktiviert ist");
    
    cleanup();
}
