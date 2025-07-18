// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

//! Vollständig isolierte Datenbanktests [KP100][SP][ZTS]
//! 
//! Diese Tests verwenden ein Fixture-basiertes Testdesign, bei dem jeder Test:
//! - Eine eigene, eindeutig benannte Datenbankdatei verwendet
//! - Seine eigenen Umgebungsvariablen setzt und zurücksetzt
//! - Vollständiges Löschen der Testdaten vor und nach dem Test
//! - Feste Testschlüssel und Konfigurationen verwendet
//! 
//! Implementiert gemäß den MedEasy-Projektregeln für Sicherheit [SP][ZTS]

#[allow(unused_imports)]

use std::env;
use std::fs;
use std::path::Path;
use std::sync::Once;
use uuid::Uuid;
// base64::Engine wird für Dekodierung verwendet [SP][ZTS]

// Importiere die benötigten Module
use crate::database::connection::DatabaseError; // DatabaseManager wird nicht verwendet

// Aktiviere Logging nur einmal
static INIT: Once = Once::new();

/// Test-Fixture-Struktur für isolierte Datenbanktests [SP][ZTS]
struct TestFixture {
    db_path: String,
    original_env: Vec<(String, Option<String>)>,
}

impl TestFixture {
    /// Erstellt eine neue Test-Fixture mit einer isolierten Testumgebung
    fn new() -> Self {
        // Logging nur einmal initialisieren
        INIT.call_once(|| {
            env_logger::init();
        });

        // Eindeutigen Datenbanknamen generieren
        let unique_id = Uuid::new_v4().to_string();
        let db_path = format!("test_db_{}.db", unique_id);

        // Relevante Umgebungsvariablen sichern
        let env_keys = vec![
            "DATABASE_URL",
            "MEDEASY_DB_KEY",
            "MEDEASY_FIELD_ENCRYPTION_KEY",
            "USE_ENCRYPTION"
        ];
        
        let original_env = env_keys.iter()
            .map(|key| (key.to_string(), env::var(key).ok()))
            .collect();

        // Bereinige eventuell existierende Testdatenbankdateien
        Self::cleanup(&db_path);

        // Eine neue Test-Fixture zurückgeben
        Self {
            db_path,
            original_env,
        }
    }

    /// Setzt die Standardtestumgebung
    fn setup(&self) {
        env::set_var("DATABASE_URL", &self.db_path);
        env::set_var("MEDEASY_DB_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA="); // Base64-kodierter 32-Byte-Schlüssel
        env::set_var("MEDEASY_FIELD_ENCRYPTION_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
        env::set_var("USE_ENCRYPTION", "true");
    }

    /// Konfiguriert eine Testumgebung ohne Verschlüsselung
    fn setup_without_encryption(&self) {
        // Zuerst alle vorherigen Umgebungsvariablen entfernen [SP][ZTS]
        for (key, _) in &self.original_env {
            env::remove_var(key);
        }
        
        // Dann die spezifischen Variablen explizit setzen
        env::set_var("DATABASE_URL", &self.db_path);
        env::set_var("MEDEASY_DB_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA="); // Base64-kodierter 32-Byte-Schlüssel
        env::set_var("MEDEASY_FIELD_ENCRYPTION_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
        env::set_var("USE_ENCRYPTION", "false"); // Wichtig: Verschlüsselung ist explizit deaktiviert
    }
    
    /// Konfiguriert eine Testumgebung mit Verschlüsselung [SP][ZTS]
    fn setup_with_encryption(&self) {
        // Zuerst alle vorherigen Umgebungsvariablen entfernen
        for (key, _) in &self.original_env {
            env::remove_var(key);
        }
        
        // Dann die spezifischen Variablen für verschlüsselte Verbindung setzen
        env::set_var("DATABASE_URL", &self.db_path);
        env::set_var("MEDEASY_DB_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA="); // Base64-kodierter 32-Byte-Schlüssel
        env::set_var("MEDEASY_FIELD_ENCRYPTION_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
        env::set_var("USE_ENCRYPTION", "true"); // Verschlüsselung aktivieren
    }

    /// Konfiguriert eine Testumgebung mit falschem Schlüssel
    fn setup_with_wrong_key(&self) {
        // Vorherige Umgebungsvariablen entfernen [SP][ZTS]
        for (key, _) in &self.original_env {
            env::remove_var(key);
        }
        
        // Spezifische Variablen setzen
        env::set_var("DATABASE_URL", &self.db_path);
        env::set_var("MEDEASY_DB_KEY", "ZmFsc2NoZXJTY2hsw7xzc2VsRsO8clRlc3RzRmFsc2NoZXJTY2hsw7w="); // Falscher aber valider Base64-Schlüssel
        env::set_var("MEDEASY_FIELD_ENCRYPTION_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
        env::set_var("USE_ENCRYPTION", "true");
    }

    /// Bereinigt die Testumgebung und stellt den ursprünglichen Zustand wieder her
    fn teardown(&self) {
        // Datenbankdateien löschen
        Self::cleanup(&self.db_path);
        
        // Ursprüngliche Umgebungsvariablen wiederherstellen
        for (key, value) in &self.original_env {
            match value {
                Some(val) => env::set_var(key, val),
                None => env::remove_var(key),
            }
        }
    }

    /// Hilfsmethode zum Löschen aller Datenbankdateien
    fn cleanup(db_path: &str) {
        if Path::new(db_path).exists() {
            fs::remove_file(db_path).expect("Konnte Testdatenbank nicht löschen");
        }
        
        let wal_file = format!("{}-wal", db_path);
        if Path::new(&wal_file).exists() {
            let _ = fs::remove_file(&wal_file);
        }
        
        let shm_file = format!("{}-shm", db_path);
        if Path::new(&shm_file).exists() {
            let _ = fs::remove_file(&shm_file);
        }
    }
}

/// Tests für das KP100-Modul [SP][ZTS]
#[cfg(test)]
mod tests {
    use super::*;
    use crate::database::connection::DatabaseManager;

    #[test]
    /// Test, dass die SQLCipher-PRAGMAs korrekt gesetzt werden [SP]
    fn test_sqlcipher_pragmas() {
        let fixture = TestFixture::new();
        fixture.setup();

        // Erstelle eine verschlüsselte Datenbankverbindung
        let db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
        let conn = db_manager.get_connection().expect("Verbindung sollte hergestellt werden können");
        
        // Überprüfe die wichtigsten SQLCipher-PRAGMAs
        let page_size: String = conn.query_row("PRAGMA cipher_page_size", [], |row| row.get(0))
            .expect("cipher_page_size sollte abgefragt werden können");
        assert_eq!(page_size, "4096", "cipher_page_size sollte 4096 sein");
        
        let kdf_iter: String = conn.query_row("PRAGMA kdf_iter", [], |row| row.get(0))
            .expect("kdf_iter sollte abgefragt werden können");
        assert_eq!(kdf_iter, "256000", "kdf_iter sollte 256000 sein");
        
        let hmac_algo: String = conn.query_row("PRAGMA cipher_hmac_algorithm", [], |row| row.get(0))
            .expect("cipher_hmac_algorithm sollte abgefragt werden können");
        assert_eq!(hmac_algo, "HMAC_SHA256", "cipher_hmac_algorithm sollte HMAC_SHA256 sein");
        
        let kdf_algo: String = conn.query_row("PRAGMA cipher_kdf_algorithm", [], |row| row.get(0))
            .expect("cipher_kdf_algorithm sollte abgefragt werden können");
        assert_eq!(kdf_algo, "PBKDF2_HMAC_SHA256", "cipher_kdf_algorithm sollte PBKDF2_HMAC_SHA256 sein");
        
        let memory_security: String = conn.query_row("PRAGMA cipher_memory_security", [], |row| row.get(0))
            .expect("cipher_memory_security sollte abgefragt werden können");
        assert_eq!(memory_security, "1", "cipher_memory_security sollte 1 sein");

        fixture.teardown();
    }

    #[test]
    /// Test, dass eine verschlüsselte Datenbankverbindung hergestellt werden kann [SP]
    fn test_database_connection_with_encryption() {
        let fixture = TestFixture::new();
        fixture.setup();
        
        // Erstelle eine verschlüsselte Datenbankverbindung
        let db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
        let conn = db_manager.get_connection().expect("Verbindung sollte hergestellt werden können");
        
        // Überprüfe, ob die Verbindung funktioniert
        conn.execute("CREATE TABLE test_connection (id INTEGER)", [])
            .expect("Tabelle sollte erstellt werden können");
            
        let table_exists: i32 = conn.query_row(
            "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='test_connection'",
            [],
            |row| row.get(0)
        ).expect("Tabelle sollte abgefragt werden können");
        
        assert_eq!(table_exists, 1, "Tabelle sollte existieren");

        fixture.teardown();
    }

    #[test]
    /// Test, dass eine Datenbank erstellt wird, wenn sie noch nicht existiert [SP]
    fn test_database_creation() {
        let fixture = TestFixture::new();
        fixture.setup();
        
        // Datenbank sollte noch nicht existieren
        assert!(!Path::new(&fixture.db_path).exists(), "Datenbank sollte noch nicht existieren");
        
        // Datenbankverbindung erstellen
        let db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
        
        // Verbindung abrufen und überprüfen
        let _conn = db_manager.get_connection().expect("Verbindung sollte hergestellt werden können");
        
        // Datenbank sollte jetzt existieren
        assert!(Path::new(&fixture.db_path).exists(), "Testdatenbank sollte erstellt worden sein");

        fixture.teardown();
    }

    #[test]
    /// Test, dass die Verbindung fehlschlägt, wenn der falsche Schlüssel verwendet wird [SP][ZTS]
    fn test_connection_fails_with_wrong_key() {
        let fixture = TestFixture::new();
        
        // Datenbank zuerst bereinigen
        TestFixture::cleanup(&fixture.db_path);
        
        // Datenbank mit korrektem Schlüssel erstellen
        fixture.setup_with_encryption();
        
        // Datenbank initialisieren mit korrektem Schlüssel
        let create_result = DatabaseManager::new();
        assert!(create_result.is_ok(), "Datenbankmanager sollte mit korrektem Schlüssel initialisiert werden können");
        
        // Tabelle erstellen und Verbindung sauber schließen
        {
            let db_manager = create_result.unwrap();
            let conn = db_manager.get_connection().expect("Verbindung sollte hergestellt werden können");
            conn.execute("CREATE TABLE test (id INTEGER)", []).expect("Tabelle sollte erstellt werden können");
            // Verbindung wird beim Verlassen des Blocks automatisch geschlossen
        }
        
        println!("Datenbank wurde erfolgreich mit korrektem Schlüssel erstellt: {}", &fixture.db_path);
        
        // Versuchen, mit einem falschen Schlüssel zu verbinden
        fixture.setup_with_wrong_key();
        
        // Debug-Ausgabe der Umgebungsvariablen
        println!("Test mit falschem Schlüssel - DATABASE_URL={}, USE_ENCRYPTION={}", 
                 env::var("DATABASE_URL").unwrap_or_default(),
                 env::var("USE_ENCRYPTION").unwrap_or_default());
        
        // Der Manager kann je nach Implementierung entweder sofort oder erst beim Verbindungszugriff fehlschlagen
        let db_manager_result = DatabaseManager::new();
        
        // Wenn die Manager-Erstellung fehlschlägt, ist das bereits der erwartete Fehler
        if let Err(err) = db_manager_result {
            let err_string = format!("{:?}", err);
            println!("Manager-Initialisierungsfehler mit falschem Schlüssel: {}", err_string);
            
            // Überprüfen, ob die Fehlermeldung auf einen falschen Schlüssel hinweist
            assert!(err_string.contains("file is not a database") || 
                   err_string.contains("not a database") || 
                   err_string.contains("encryption") ||
                   err_string.contains("decrypt") ||
                   err_string.contains("codec") ||
                   err_string.contains("key"),
                   "Fehlermeldung sollte auf Verschlüsselungsproblem hinweisen: {}", err_string);
                   
            fixture.teardown();
            return;
        }
        
        // Wenn die Manager-Erstellung erfolgreich war, sollte die Verbindungsherstellung fehlschlagen
        let db_manager = db_manager_result.unwrap();
        let conn_result = db_manager.get_connection();
        assert!(conn_result.is_err(), "Verbindung sollte mit falschem Schlüssel fehlschlagen");
        
        // Überprüfen, ob die Fehlermeldung auf einen falschen Schlüssel hinweist
        if let Err(err) = conn_result {
            let err_string = format!("{:?}", err);
            println!("Verbindungsfehler mit falschem Schlüssel: {}", err_string);
            
            assert!(err_string.contains("file is not a database") || 
                   err_string.contains("not a database") || 
                   err_string.contains("encryption") ||
                   err_string.contains("decrypt") ||
                   err_string.contains("codec") ||
                   err_string.contains("key"),
                   "Fehlermeldung sollte auf Verschlüsselungsproblem hinweisen: {}", err_string);
        }
        
        fixture.teardown();
    }

    #[test]
    /// Test, dass die Verschlüsselung in Produktion erzwungen wird [SP][ZTS]
    fn test_encryption_enforced_in_production() {
        // Verwende eine isolierte Fixture
        let fixture = TestFixture::new();
        
        // Datenbank zuerst bereinigen
        TestFixture::cleanup(&fixture.db_path);
        
        // Alle vorhandenen Umgebungsvariablen entfernen und neu setzen
        // um saubere Testumgebung zu gewährleisten
        for (key, _) in env::vars() {
            if key == "DATABASE_URL" || key == "MEDEASY_DB_KEY" || key == "USE_ENCRYPTION" {
                env::remove_var(&key);
            }
        }

        // Testdatenbank erstellen mit deaktivierter Verschlüsselung 
        env::set_var("DATABASE_URL", &fixture.db_path);
        env::set_var("MEDEASY_DB_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA="); // 32 Byte Key [SP]
        
        // Wichtig: Verschlüsselung explizit deaktivieren für den Test
        env::set_var("USE_ENCRYPTION", "false");

        // Debug-Ausgabe zur Fehlerdiagnose
        println!("[TEST] Env-Setup vor Produktionstest: ");
        println!("  - DATABASE_URL={}", env::var("DATABASE_URL").unwrap_or_default());
        println!("  - USE_ENCRYPTION={}", env::var("USE_ENCRYPTION").unwrap_or_default());
        println!("  - MEDEASY_DB_KEY ist gesetzt: {}", env::var("MEDEASY_DB_KEY").is_ok());
        
        // Direkter Test der Produktionsumgebung
        // Bei USE_ENCRYPTION=false muss ein Fehler auftreten
        let result_production = DatabaseManager::new_with_production_flag(true);
        
        // Debug-Ausgabe des Ergebnisses
        println!("[TEST] Produktionsmodus-Test Ergebnis: {:?}", result_production);
        
        // In Produktion muss bei USE_ENCRYPTION=false ein Fehler geworfen werden
        assert!(result_production.is_err(), 
                "[SICHERHEITSBRUCH] new_with_production_flag(true) muss bei USE_ENCRYPTION=false fehlschlagen");
        
        // Prüfen der Fehlermeldung
        if let Err(e) = result_production {
            let error_string = format!("{:?}", e);
            println!("[TEST] Erwarteter Produktionsfehler: {}", error_string);
            
            // Prüfen ob die Fehlermeldung auf Verschlüsselungserzwingung hinweist
            assert!(error_string.contains("Unencrypted") || 
                   error_string.contains("Encryption") || 
                   error_string.contains("production") ||
                   error_string.contains("enabled"),
                   "Fehlermeldung sollte auf Verschlüsselungserzwingung hinweisen: {}", error_string);
        } else {
            panic!("[KRITISCHER FEHLER] Produktion ohne Verschlüsselung wurde nicht verhindert!");
        }
        
        // Jetzt testen wir, ob die Verschlüsselung in Nicht-Produktionsumgebung optional ist
        // (Umgebungsvariable USE_ENCRYPTION sollte immer noch false sein)
        println!("[TEST] USE_ENCRYPTION vor Dev-Test: {}", env::var("USE_ENCRYPTION").unwrap_or_default());
        let dev_result = DatabaseManager::new_with_production_flag(false); // Entwicklungsumgebung
        
        // In Entwicklungsumgebung sollte die Verbindung auch ohne Verschlüsselung klappen
        assert!(dev_result.is_ok(), 
               "In Entwicklungsumgebung sollte Verschlüsselung optional sein: {:?}", dev_result);
        
        fixture.teardown();
    }

    #[test]
    /// Test, dass die Verschlüsselung in Entwicklungsumgebungen optional ist [SP]
    fn test_encryption_optional_in_development() {
        let fixture = TestFixture::new();
        fixture.setup_without_encryption();
        
        // Versuchen, eine unverschlüsselte Verbindung in Entwicklung herzustellen
        let result = DatabaseManager::new_with_production_flag(false);
        
        // Sollte funktionieren
        assert!(result.is_ok(), "Unverschlüsselte Verbindung sollte in Entwicklung möglich sein");

        fixture.teardown();
    }

    #[test]
    /// Test, dass der Connection-Pool funktioniert [SP]
    fn test_connection_pooling() {
        let fixture = TestFixture::new();
        fixture.setup();
        
        // Erstelle eine verschlüsselte Datenbankverbindung mit Pool
        let db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
        
        // Mehrere Verbindungen aus dem Pool abrufen
        let conn1 = db_manager.get_connection().expect("Erste Verbindung sollte hergestellt werden können");
        let conn2 = db_manager.get_connection().expect("Zweite Verbindung sollte hergestellt werden können");
        
        // Überprüfen, ob beide Verbindungen funktionieren
        conn1.execute("CREATE TABLE test_pool1 (id INTEGER)", [])
            .expect("Tabelle 1 sollte erstellt werden können");
        conn2.execute("CREATE TABLE test_pool2 (id INTEGER)", [])
            .expect("Tabelle 2 sollte erstellt werden können");
            
        // Überprüfen, ob beide Tabellen existieren
        let table1_exists: i32 = conn1.query_row(
            "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='test_pool1'",
            [],
            |row| row.get(0)
        ).expect("Tabelle 1 sollte abgefragt werden können");
        
        let table2_exists: i32 = conn2.query_row(
            "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='test_pool2'",
            [],
            |row| row.get(0)
        ).expect("Tabelle 2 sollte abgefragt werden können");
        
        assert_eq!(table1_exists, 1, "Tabelle 1 sollte existieren");
        assert_eq!(table2_exists, 1, "Tabelle 2 sollte existieren");

        fixture.teardown();
    }

    #[test]
    /// Test, dass Migrationen erfolgreich durchgeführt werden können [SP][ATV]
    fn test_migrations() {
        let fixture = TestFixture::new();
        fixture.setup();
        
        // Erstelle eine verschlüsselte Datenbankverbindung
        let db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
        
        // Migrationen ausführen (simuliert, da wir keine echten Migrationen haben)
        let result = db_manager.get_connection()
            .and_then(|conn| {
                conn.execute(
                    "CREATE TABLE migrations_test (
                        id INTEGER PRIMARY KEY,
                        name TEXT NOT NULL,
                        applied_at TEXT NOT NULL
                    )",
                    [],
                ).map_err(|e| DatabaseError::SqlError(e))
            });
            
        assert!(result.is_ok(), "Migrationen sollten erfolgreich ausgeführt werden können");
        
        // Überprüfen, ob die Migrationstabelle existiert
        let conn = db_manager.get_connection().expect("Verbindung sollte hergestellt werden können");
        let table_exists: i32 = conn.query_row(
            "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='migrations_test'",
            [],
            |row| row.get(0)
        ).expect("Migrationstabelle sollte abgefragt werden können");
        
        assert_eq!(table_exists, 1, "Migrationstabelle sollte existieren");

        fixture.teardown();
    }
}
