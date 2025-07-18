/*
„Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17
*/

// Import der notwendigen Module [ZTS][TR]

#[allow(unused_imports)]
use std::sync::Arc;
use std::env;
// use std::fs;
// use std::path::Path;
use std::thread; // Aktiviert für sleep in Tests [SP]
use std::time::Duration; // Aktiviert für sleep in Tests [SP]
use uuid::Uuid;
// use chrono::{DateTime, Utc}; // Wird nicht verwendet

use crate::database::connection::DatabaseManager;
use crate::database::models::{Session, Patient, Transcript};
use crate::repositories::patient_repository::{PatientRepository, PatientRepositoryError};
use crate::repositories::session_repository::{SessionRepository, SessionRepositoryError};
use crate::repositories::transcript_repository::{TranscriptRepository, TranscriptRepositoryError};

/// Stellt eine isolierte Testdatenbank für jeden Test bereit [ZTS][TR]
pub struct TestDatabaseFixture {
    pub db_path: String,
    pub db_manager: Arc<DatabaseManager>,
}

impl TestDatabaseFixture {
    /// Erstellt eine neue isolierte In-Memory-Datenbankinstanz für Tests [ZTS]
    pub fn new() -> Self {
        // Garantiert einzigartige Datenbank für jeden Test
        let uuid = Uuid::new_v4();
        let db_path = format!("file:memdb_{}?mode=memory&cache=shared", uuid);
        
        // Umgebungsvariable für Tests setzen - MUSS VOR DatabaseManager::new() erfolgen [SP][ZTS]
        env::set_var("DATABASE_URL", &db_path);
        
        // Wichtig: SQLite in-memory Datenbanken müssen "shared cache" verwenden
        // und die Verbindung muss während der gesamten Lebensdauer gehalten werden
        let db_manager = Arc::new(DatabaseManager::new()
            .expect("Fehler beim Erstellen des DatabaseManagers"));
        
        // Migrationen ausführen - Klonen um mutability zu erhalten [ZTS]
        let mut db_manager_clone = db_manager.as_ref().clone();
        db_manager_clone.run_migrations()
            .expect("Migrationen sollten ausgeführt werden können");
        
        Self { db_path, db_manager }
    }
    
    /// Erstellt eine neue isolierte Datenbankinstanz auf dem Dateisystem [ZTS]
    /// Diese ist stabiler als In-Memory, aber langsamer
    pub fn new_file_based() -> Self {
        // Prüfe, ob eine Umgebungsvariable für das Test-Verzeichnis gesetzt ist
        let test_dir = std::env::var("MEDEASY_TEST_DIR").unwrap_or_else(|_| ".".to_string());
        
        // Temporärer Dateipfad für die Testdatenbank
        let db_path = format!("{}/test_db_{}.sqlite", test_dir, Uuid::new_v4());

        // Setze Umgebungsvariable für den Datenbankpfad [SP][ZTS]
        std::env::set_var("DATABASE_URL", &db_path);
        
        // DatabaseManager::new() nimmt keine Parameter mehr [ZTS]
        let db_manager = Arc::new(DatabaseManager::new()
            .expect("Testdatenbank sollte erstellt werden können"));
        
        // Migrationen ausführen - Klonen um mutability zu erhalten [ZTS]
        let mut db_manager_clone = db_manager.as_ref().clone();
        db_manager_clone.run_migrations()
            .expect("Migrationen sollten ausgeführt werden können");
        
        Self { db_path, db_manager }
    }
    
    /// Löscht alle Daten aus den Testtabellen [ZTS][TR]
    pub fn clean_tables(&self) -> Result<(), String> {
        // Verwende String als Fehlertyp, um einfacheres Error-Handling zu ermöglichen [ZTS]
        self.db_manager.with_connection_mut(|conn| {
            // Temporär Foreign Key Constraints deaktivieren
            conn.execute("PRAGMA foreign_keys = OFF", [])?;
            
            // Tabellen leeren
            conn.execute("DELETE FROM transcripts", [])?;
            conn.execute("DELETE FROM sessions", [])?;
            conn.execute("DELETE FROM patients", [])?;
            conn.execute("DELETE FROM audit_logs", [])?;
            
            // Foreign Key Constraints wieder aktivieren
            conn.execute("PRAGMA foreign_keys = ON", [])?;
            
            Ok(())
        }).map_err(|e| format!("Fehler beim Bereinigen der Tabellen: {}", e))
    }
    
    /// Erstellt einen Testpatienten mit Standardwerten
    pub fn create_test_patient(&self, user_id: &str) -> Result<Patient, PatientRepositoryError> {
        // Erstelle einen DatabaseManager ohne Arc für das Repository [ZTS]
        let db_manager = self.db_manager.as_ref().clone();
        
        let patient_repo = PatientRepository::new(db_manager)
            .expect("PatientRepository sollte erstellt werden können");
        
        // Angepasst an die tatsächliche create_patient Methode [ZTS]
        patient_repo.create_patient(
            "Max Mustermann", 
            "123.4567.8901.23",
            "01.01.1980",
            user_id
        )
    }
    
    /// Erstellt eine Testsession mit Standardwerten
    pub fn create_test_session(&self, patient_id: &str, user_id: &str) -> Result<Session, SessionRepositoryError> {
        // Erstelle einen DatabaseManager ohne Arc für das Repository [ZTS]
        let db_manager = self.db_manager.as_ref().clone();
        
        let session_repo = SessionRepository::new(db_manager)
            .expect("SessionRepository sollte erstellt werden können");
        
        // Aktuelles Datum im Format DD.MM.YYYY
        let now = chrono::Local::now().format("%d.%m.%Y").to_string();
        
        // Angepasst an die tatsächliche create_session Methode [ZTS]
        session_repo.create_session(
            patient_id,
            &now,
            user_id
        )
    }
    
    /// Erstellt ein Testtranskript mit standardmäßigen Werten [TR]
    pub fn create_test_transcript(
        &self,
        session_id: &str,
        original_text: &str,
        anonymized_text: &str,
        confidence: f64,
        user_id: &str
    ) -> Result<Transcript, TranscriptRepositoryError> {
        // Erstelle einen DatabaseManager ohne Arc für das Repository [ZTS]
        let db_manager = self.db_manager.as_ref().clone();
        
        let transcript_repo = TranscriptRepository::new(db_manager)
            .expect("TranscriptRepository sollte erstellt werden können");
        
        // Direktes Durchreichen des Ergebnisses [ZTS]
        transcript_repo.create_transcript(
            session_id,
            original_text,
            anonymized_text,
            confidence,
            user_id
        )
    }
}

impl Drop for TestDatabaseFixture {
    fn drop(&mut self) {
        // Explizites Aufräumen und Wartezeit
        // Dies hilft, Race Conditions zwischen Tests zu vermeiden
        thread::sleep(Duration::from_millis(50));
        
        // Bei dateibasierten Datenbanken versuchen, die Datei zu löschen
        if !self.db_path.contains("memdb_") {
            if let Err(e) = std::fs::remove_file(&self.db_path) {
                eprintln!("Konnte Testdatenbank nicht löschen: {} ({})", self.db_path, e);
            }
        }
    }
}
