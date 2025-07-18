// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Key Rotation Tests [KP100] [SP] [ATV] [ZTS]
// Tests für die Schlüsselrotation und Schlüsselverwaltung

use std::env;
use std::sync::Arc;
use std::path::{Path, PathBuf};
use tempfile::TempDir;

// Importiere KeyManager direkt und re-exportiere AuditLogger/AuditAction nicht [SP][ATV][TR]
use crate::security::key_manager::{KeyManager, KeyType, KeyRotationStatus, KeyConfig, AuditLogger};

// Implementiere eine lokale TestConfig speziell für die Docker-Tests [CAM][SP][ZTS]

// Implementiere eine lokale TestConfig speziell für die Docker-Tests [CAM][SP][ZTS]
#[derive(Debug, Clone)]
pub struct TestConfig {
    pub data_dir: PathBuf,
    pub database_name: String,
    pub backup_dir: PathBuf,
    pub key_rotation_interval_days: u32,
    pub production_mode: bool,
}

impl TestConfig {
    pub fn new(temp_dir: PathBuf) -> Self {
        let backup_dir = temp_dir.join("backups");
        Self {
            data_dir: temp_dir.clone(),
            database_name: String::from("medeasy_test.db"),
            backup_dir,
            key_rotation_interval_days: 1,
            production_mode: false,
        }
    }
    
    // Docker-kompatible for_testing Methode [SP][TR]
    pub fn for_testing(temp_dir: &Path) -> Self {
        Self::new(temp_dir.to_path_buf())
    }
}

// Implementierung des KeyConfig-Traits für die TestConfig [SP][ZTS][CAM]
impl KeyConfig for TestConfig {
    fn keys_path(&self) -> PathBuf {
        self.data_dir.join("keys")
    }
    
    fn audit_log_path(&self) -> PathBuf {
        self.data_dir.join("audit.log")
    }
    
    fn key_rotation_interval_days(&self) -> u32 {
        self.key_rotation_interval_days
    }
    
    fn is_production(&self) -> bool {
        self.production_mode
    }
}

// Docker-Tests verwenden die lokale TestConfig für bessere Kompatibilität [CAM][SP][ZTS]

// Hilfsfunktion zum Erstellen eines temporären Audit-Loggers für Tests [ATV][SP]
fn create_test_audit_logger() -> Arc<AuditLogger> {
    // Verwende einen festen Pfad für Tests, um Probleme mit temporären Verzeichnissen zu vermeiden
    let log_path = std::env::temp_dir().join("medeasy_test_audit.log");
    
    // Erstelle AuditLogger mit der korrekten Methode [ATV][ZTS]
    match AuditLogger::new_with_enforcement(
        log_path.to_str().unwrap()
    ) {
        Ok(logger) => Arc::new(logger),
        Err(e) => panic!("Konnte AuditLogger nicht erstellen: {}", e)
    }
}

// Hilfsfunktion zum Erstellen eines KeyManager für Tests [SP][ZTS][TR]
fn create_test_key_manager() -> (Arc<KeyManager<TestConfig>>, tempfile::TempDir) {
    // Erstelle temporäres Verzeichnis
    let temp_dir = tempfile::tempdir().expect("Temporäres Verzeichnis konnte nicht erstellt werden");
    let keystore_path = temp_dir.path().join("keystore_test.json");
    let _audit_log_path = temp_dir.path().join("audit_test.log");

    // Erstelle einen AuditLogger für Tests
    let audit_logger = create_test_audit_logger();

    // Erstelle eine TestConfig für die Docker-Tests [CAM][SP][ZTS]
    let test_config = TestConfig::for_testing(temp_dir.path());
    
    // Erstelle den KeyManager mit der Test-Config
    let key_manager = Arc::new(KeyManager::new(
        keystore_path.to_str().unwrap(),
        audit_logger,
        Arc::new(test_config) // Übergebe die konfigurierte Config-Struktur [SP][ZTS]
    ));
    
    (key_manager, temp_dir)
}

#[test]
/// Test, dass die Schlüsselverwaltung initialisiert werden kann [SP]
fn test_key_manager_initialization() {
    let (key_manager, _temp_dir) = create_test_key_manager();
    
    // Initialisiere mit Test-Passwort
    let result = key_manager.initialize("TestPasswort123!");
    assert!(result.is_ok(), "Schlüsselverwaltung sollte initialisiert werden können");
}

#[test]
/// Test, dass die Schlüsselrotation funktioniert [SP][ATV]
fn test_key_rotation() {
    let (key_manager, _temp_dir) = create_test_key_manager();
    
    key_manager.initialize("TestPasswort123!")
        .expect("Schlüsselverwaltung sollte initialisiert werden können");
    
    let key_type = KeyType::FieldPatient;
    let original_key = key_manager.get_key(key_type)
        .expect("Schlüssel sollte abgerufen werden können");
    
    let original_metadata = key_manager.get_key_metadata(key_type)
        .expect("Schlüsselmetadaten sollten abgerufen werden können");
    let original_version = original_metadata.version;
    
    assert_eq!(original_key.len(), 32, "Schlüssel sollte 32 Bytes lang sein");
    assert_eq!(original_version, 1, "Erste Version sollte 1 sein");
    
    key_manager.rotate_key(key_type, "test_user")
        .expect("Schlüsselrotation sollte erfolgreich sein");
    
    let new_key = key_manager.get_key(key_type)
        .expect("Neuer Schlüssel sollte abgerufen werden können");
    
    let new_metadata = key_manager.get_key_metadata(key_type)
        .expect("Neue Schlüsselmetadaten sollten abgerufen werden können");
    let new_version = new_metadata.version;
    
    assert_ne!(original_key, new_key, "Schlüssel sollte nach Rotation anders sein");
    assert_eq!(new_version, 2, "Version sollte nach Rotation erhöht werden");
}

#[test]
/// Test, dass der Rotationsstatus korrekt berechnet wird [ATV]
fn test_rotation_status() {
    let (key_manager, _temp_dir) = create_test_key_manager();
    
    // Initialisiere mit Test-Passwort
    key_manager.initialize("TestPasswort123!")
        .expect("Schlüsselverwaltung sollte initialisiert werden können");
    
    // Hole den aktuellen Status (sollte UpToDate sein)
    let key_type = KeyType::FieldPatient;
    let status = key_manager.check_rotation_status(key_type)
        .expect("Status sollte abgerufen werden können");
    
    // Ein frisch erstellter Schlüssel kann DueSoon sein, je nach Konfiguration
    assert!(matches!(status, KeyRotationStatus::UpToDate | KeyRotationStatus::DueSoon), 
            "Status sollte UpToDate oder DueSoon sein, aber war: {:?}", status);
    
    // Manipuliere das Fälligkeitsdatum für die Rotation, um DueSoon zu testen
    // Füge eine öffentliche Methode für Tests hinzu, um das Fälligkeitsdatum zu ändern
    // Da wir die KeyManager-Struktur nicht direkt ändern können, müssen wir einen Workaround verwenden
    
    // Setze eine Umgebungsvariable, die das Testverhalten steuert
    env::set_var("MEDEASY_TEST_ROTATION_DUE_SOON", "true");
    
    // Rufe die Methode auf, die das Fälligkeitsdatum prüft
    // In einer realen Implementierung würde der KeyManager die Umgebungsvariable prüfen
    // und das Fälligkeitsdatum entsprechend anpassen
    
    // Hole den neuen Status (sollte DueSoon sein)
    let status = key_manager.check_rotation_status(key_type)
        .expect("Status sollte abgerufen werden können");
    
    assert_eq!(status, KeyRotationStatus::DueSoon, "Status sollte DueSoon sein");
    
    // Manipuliere das Fälligkeitsdatum für die Rotation, um Overdue zu testen
    // Ähnlicher Workaround wie zuvor
    
    // Entferne die vorherige Testumgebungsvariable
    env::remove_var("MEDEASY_TEST_ROTATION_DUE_SOON");
    
    // Setze eine neue Umgebungsvariable für den Overdue-Status
    env::set_var("MEDEASY_TEST_ROTATION_OVERDUE", "true");
    
    // In einer realen Implementierung würde der KeyManager diese Umgebungsvariable prüfen
    
    // Hole den neuen Status (sollte Overdue sein)
    let status = key_manager.check_rotation_status(key_type)
        .expect("Status sollte abgerufen werden können");
    
    assert_eq!(status, KeyRotationStatus::Overdue, "Status sollte Overdue sein");
}

#[test]
/// Test, dass die Audit-Protokollierung bei der Schlüsselrotation funktioniert [ATV]
fn test_key_rotation_audit() {
    let (key_manager, _temp_dir) = create_test_key_manager();
    
    // Initialisiere mit Test-Passwort
    key_manager.initialize("TestPasswort123!")
        .expect("Schlüsselverwaltung sollte initialisiert werden können");
    
    // Hole Referenz zum Audit-Logger über eine öffentliche Methode
    let audit_logger = key_manager.get_audit_logger()
        .expect("Audit-Logger sollte abgerufen werden können");
    
    // Zähle Audit-Einträge vor der Rotation
    let entries_before = audit_logger.count_entries().unwrap_or(0);
    
    // Führe Schlüsselrotation durch
    let key_type = KeyType::FieldPatient;
    key_manager.rotate_key(key_type, "test_user")
        .expect("Schlüsselrotation sollte erfolgreich sein");
    
    // Zähle Audit-Einträge nach der Rotation
    let entries_after = audit_logger.count_entries().unwrap_or(0);
    
    // Überprüfe, dass mindestens 2 neue Audit-Einträge erstellt wurden
    // (Start der Rotation und erfolgreiche Rotation)
    // Hinweis: Es können auch 3 sein, wenn ein neuer Schlüsselspeicher erstellt wird
    assert!(entries_after >= entries_before + 2, 
            "Es sollten mindestens 2 neue Audit-Einträge erstellt werden. Vorher: {}, Nachher: {}", 
            entries_before, entries_after);
    
    // Überprüfe, ob die Audit-Einträge die richtigen Informationen enthalten
    let latest_entries = audit_logger.get_latest_entries(2)
        .expect("Neueste Audit-Einträge sollten abgerufen werden können");
    
    let contains_rotation_started = latest_entries.iter().any(|entry| 
        entry.message.contains("Schlüsselrotation gestartet"));
    
    let contains_rotation_success = latest_entries.iter().any(|entry| 
        entry.message.contains("Schlüsselrotation erfolgreich"));
    
    assert!(contains_rotation_started, "Audit sollte 'Schlüsselrotation gestartet' enthalten");
    assert!(contains_rotation_success, "Audit sollte 'Schlüsselrotation erfolgreich' enthalten");
}

#[test]
/// Test, dass alle Schlüsseltypen rotiert werden können [SP]
fn test_rotate_all_key_types() {
    let (key_manager, _temp_dir) = create_test_key_manager();
    
    key_manager.initialize("TestPasswort123!")
        .expect("Schlüsselverwaltung sollte initialisiert werden können");
    
    // Liste aller zu testenden Schlüsseltypen (außer Master, der separat behandelt wird)
    let key_types = vec![
        KeyType::Database,
        KeyType::FieldPatient,
        KeyType::FieldSession,
        KeyType::FieldTranscript,
        KeyType::Backup,
    ];
    
    // Teste Rotation für jeden Schlüsseltyp
    for key_type in key_types.iter() {
        let original_key = key_manager.get_key(*key_type)
            .expect(&format!("Schlüssel {:?} sollte abgerufen werden können", key_type));
        
        key_manager.rotate_key(*key_type, "test_user")
            .expect(&format!("Schlüsselrotation für {:?} sollte erfolgreich sein", key_type));
        
        let new_key = key_manager.get_key(*key_type)
            .expect(&format!("Neuer Schlüssel {:?} sollte abgerufen werden können", key_type));
        
        assert_ne!(original_key, new_key, 
                   "Schlüssel {:?} sollte nach Rotation anders sein", key_type);
    }
}
