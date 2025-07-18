/* „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 */

#[allow(unused_imports)]

use std::fs;
use std::path::Path;
use std::env;
use rusqlite::Connection;
use uuid::Uuid;
use crate::database::connection::DatabaseManager;
use crate::services::audit::AuditService;
use crate::repositories::{PatientRepository, SessionRepository, TranscriptRepository, AnonymizationReviewRepository};
use crate::models::{AnonymizationReviewStatus};

// Test-Hilfsfunktionen
mod test_helpers {
    use super::*;
    use std::fs;
    use std::path::Path;
    
    pub fn initialize() {
        // Setze Umgebungsvariablen für Tests
        env::set_var("MEDEASY_DB_KEY", "x'2DD29CA851E7B56E4697B0E1F08507293D761A05CE4D1B628663F411A8086D99'");
        env::set_var("MEDEASY_FIELD_ENCRYPTION_KEY", "JVFha/GwPxMGJ6j3RYqI/3QdRWIcpGXmVigXS7GjDUo=");
        env::set_var("MEDEASY_ENFORCE_ENCRYPTION", "true");
        env::set_var("MEDEASY_ENFORCE_AUDIT", "true");
        
        // Stelle sicher, dass der Test-Ordner existiert
        fs::create_dir_all("./test_data").unwrap_or_else(|_| {
            println!("Konnte test_data-Ordner nicht erstellen");
        });
        
        cleanup();
    }
    
    pub fn cleanup() {
        // Lösche alle Testdatenbankdateien
        let db_path = Path::new("./test_data/test_anonymization_review.db");
        if db_path.exists() {
            fs::remove_file(db_path).unwrap_or_else(|_| {
                println!("Konnte Testdatenbank nicht löschen");
            });
        }
        
        // Lösche auch WAL und SHM Dateien
        let wal_path = Path::new("./test_data/test_anonymization_review.db-wal");
        if wal_path.exists() {
            fs::remove_file(wal_path).unwrap_or_else(|_| {
                println!("Konnte WAL-Datei nicht löschen");
            });
        }
        
        let shm_path = Path::new("./test_data/test_anonymization_review.db-shm");
        if shm_path.exists() {
            fs::remove_file(shm_path).unwrap_or_else(|_| {
                println!("Konnte SHM-Datei nicht löschen");
            });
        }
    }
    
    pub fn setup_test_db() -> DatabaseManager {
        // Initialisiere und bereinige alte Dateien
        initialize();
        
        // Erstelle eine neue Testdatenbank
        let db_manager = DatabaseManager::new("./test_data/test_anonymization_review.db").unwrap();
        
        // Führe Migrationen aus
        db_manager.run_migrations().unwrap();
        
        // Überprüfe SQLCipher-Konfiguration
        db_manager.with_connection(|conn| {
            let cipher_version: String = conn.query_row("PRAGMA cipher_version", [], |row| row.get(0)).unwrap();
            assert!(cipher_version.contains("4."), "SQLCipher nicht korrekt konfiguriert");
            
            let journal_mode: String = conn.query_row("PRAGMA journal_mode", [], |row| row.get(0)).unwrap();
            assert_eq!(journal_mode.to_lowercase(), "wal", "WAL-Modus nicht aktiviert");
            
            // Überprüfe, ob alle Tabellen existieren
            let tables = ["patients", "sessions", "transcripts", "audit_logs", "anonymization_review_items"];
            for table in tables.iter() {
                let count: i32 = conn.query_row(
                    &format!("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{}'", table),
                    [],
                    |row| row.get(0)
                ).unwrap();
                assert_eq!(count, 1, "Tabelle {} existiert nicht", table);
            }
            
            Ok(())
        }).unwrap();
        
        db_manager
    }
    
    pub fn create_test_patient(patient_repo: &PatientRepository) -> String {
        let patient_id = patient_repo.create_patient(
            "Test Patient",
            "123.4567.8901.23",
            "01.01.1990",
            "test_user"
        ).unwrap();
        
        patient_id
    }
    
    pub fn create_test_session(session_repo: &SessionRepository, patient_id: &str) -> String {
        let session_id = session_repo.create_session(
            patient_id,
            "2025-07-11T10:00:00Z",
            None,
            "scheduled",
            "test_user"
        ).unwrap();
        
        session_id
    }
    
    pub fn create_test_transcript(
        transcript_repo: &TranscriptRepository,
        session_id: &str,
        confidence: f64
    ) -> String {
        let original_text = "Patient Max Mustermann mit Versicherungsnummer 123.4567.8901.23 hat Kopfschmerzen.";
        let anonymized_text = "Patient [NAME] mit Versicherungsnummer [VERSICHERUNGSNUMMER] hat Kopfschmerzen.";
        
        let transcript = transcript_repo.create_transcript(
            session_id,
            original_text,
            anonymized_text,
            confidence,
            "test_user"
        ).unwrap();
        
        transcript.id
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use super::test_helpers::*;
    
    #[test]
    fn test_anonymization_review_creation() {
        // Setup
        let db_manager = setup_test_db();
        let audit_service = AuditService::new(db_manager.clone());
        let patient_repo = PatientRepository::new(db_manager.clone(), audit_service.clone()).unwrap();
        let session_repo = SessionRepository::new(db_manager.clone(), audit_service.clone()).unwrap();
        let transcript_repo = TranscriptRepository::new(db_manager.clone(), audit_service.clone()).unwrap();
        let review_repo = AnonymizationReviewRepository::new(db_manager.clone(), audit_service.clone());
        
        // Erstelle Testdaten
        let patient_id = create_test_patient(&patient_repo);
        let session_id = create_test_session(&session_repo, &patient_id);
        
        // Erstelle Transkript mit niedriger Konfidenz (sollte Review-Eintrag erstellen)
        let low_confidence = 65.0;
        let transcript_id_low = create_test_transcript(&transcript_repo, &session_id, low_confidence);
        
        // Erstelle Transkript mit hoher Konfidenz (sollte keinen Review-Eintrag erstellen)
        let high_confidence = 95.0;
        let transcript_id_high = create_test_transcript(&transcript_repo, &session_id, high_confidence);
        
        // Überprüfe, ob Review-Einträge korrekt erstellt wurden
        let reviews_for_low = review_repo.get_review_items_by_transcript_id(&transcript_id_low).unwrap();
        let reviews_for_high = review_repo.get_review_items_by_transcript_id(&transcript_id_high).unwrap();
        
        // Für niedrige Konfidenz sollte ein Review-Eintrag existieren
        assert_eq!(reviews_for_low.len(), 1, "Kein Review-Eintrag für niedrige Konfidenz erstellt");
        assert_eq!(reviews_for_low[0].status, AnonymizationReviewStatus::Pending, "Review-Status sollte Pending sein");
        assert_eq!(reviews_for_low[0].anonymization_confidence, low_confidence, "Falsche Konfidenz im Review-Eintrag");
        
        // Für hohe Konfidenz sollte kein Review-Eintrag existieren
        assert_eq!(reviews_for_high.len(), 0, "Review-Eintrag für hohe Konfidenz erstellt, obwohl nicht nötig");
        
        // Cleanup
        cleanup();
    }
    
    #[test]
    fn test_review_status_update() {
        // Setup
        let db_manager = setup_test_db();
        let audit_service = AuditService::new(db_manager.clone());
        let patient_repo = PatientRepository::new(db_manager.clone(), audit_service.clone()).unwrap();
        let session_repo = SessionRepository::new(db_manager.clone(), audit_service.clone()).unwrap();
        let transcript_repo = TranscriptRepository::new(db_manager.clone(), audit_service.clone()).unwrap();
        let review_repo = AnonymizationReviewRepository::new(db_manager.clone(), audit_service.clone());
        
        // Erstelle Testdaten
        let patient_id = create_test_patient(&patient_repo);
        let session_id = create_test_session(&session_repo, &patient_id);
        let transcript_id = create_test_transcript(&transcript_repo, &session_id, 60.0);
        
        // Hole den Review-Eintrag
        let reviews = review_repo.get_review_items_by_transcript_id(&transcript_id).unwrap();
        assert_eq!(reviews.len(), 1, "Review-Eintrag wurde nicht erstellt");
        let review_id = reviews[0].id.clone();
        
        // Aktualisiere den Status auf "Approved"
        let notes = Some("Anonymisierung ist korrekt".to_string());
        let updated_review = review_repo.update_review_status(
            &review_id,
            AnonymizationReviewStatus::Approved,
            notes.clone(),
            "test_reviewer"
        ).unwrap();
        
        // Überprüfe, ob der Status korrekt aktualisiert wurde
        assert_eq!(updated_review.status, AnonymizationReviewStatus::Approved, "Status wurde nicht korrekt aktualisiert");
        assert_eq!(updated_review.reviewer_notes, notes, "Notizen wurden nicht korrekt aktualisiert");
        assert_eq!(updated_review.last_modified_by, "test_reviewer", "Benutzer wurde nicht korrekt aktualisiert");
        
        // Überprüfe, ob ein Audit-Log-Eintrag erstellt wurde
        let audit_logs = db_manager.with_connection(|conn| {
            let mut stmt = conn.prepare(
                "SELECT COUNT(*) FROM audit_logs WHERE record_id = ? AND action = 'UPDATE'"
            ).unwrap();
            let count: i64 = stmt.query_row(params![review_id], |row| row.get(0)).unwrap();
            Ok(count)
        }).unwrap();
        
        assert!(audit_logs > 0, "Kein Audit-Log-Eintrag für die Statusaktualisierung erstellt");
        
        // Cleanup
        cleanup();
    }
    
    #[test]
    fn test_get_reviews_by_status() {
        // Setup
        let db_manager = setup_test_db();
        let audit_service = AuditService::new(db_manager.clone());
        let patient_repo = PatientRepository::new(db_manager.clone(), audit_service.clone()).unwrap();
        let session_repo = SessionRepository::new(db_manager.clone(), audit_service.clone()).unwrap();
        let transcript_repo = TranscriptRepository::new(db_manager.clone(), audit_service.clone()).unwrap();
        let review_repo = AnonymizationReviewRepository::new(db_manager.clone(), audit_service.clone());
        
        // Erstelle mehrere Testdaten
        let patient_id = create_test_patient(&patient_repo);
        let session_id = create_test_session(&session_repo, &patient_id);
        
        // Erstelle 3 Transkripte mit niedriger Konfidenz
        let transcript_id1 = create_test_transcript(&transcript_repo, &session_id, 60.0);
        let transcript_id2 = create_test_transcript(&transcript_repo, &session_id, 65.0);
        let transcript_id3 = create_test_transcript(&transcript_repo, &session_id, 70.0);
        
        // Hole alle Review-Einträge
        let reviews1 = review_repo.get_review_items_by_transcript_id(&transcript_id1).unwrap();
        let reviews2 = review_repo.get_review_items_by_transcript_id(&transcript_id2).unwrap();
        let reviews3 = review_repo.get_review_items_by_transcript_id(&transcript_id3).unwrap();
        
        // Aktualisiere die Status auf verschiedene Werte
        review_repo.update_review_status(
            &reviews1[0].id,
            AnonymizationReviewStatus::Approved,
            Some("Approved".to_string()),
            "test_user"
        ).unwrap();
        
        review_repo.update_review_status(
            &reviews2[0].id,
            AnonymizationReviewStatus::Rejected,
            Some("Rejected".to_string()),
            "test_user"
        ).unwrap();
        
        // Der dritte bleibt im Status "Pending"
        
        // Hole Reviews nach Status
        let pending_reviews = review_repo.get_review_items_by_status(AnonymizationReviewStatus::Pending).unwrap();
        let approved_reviews = review_repo.get_review_items_by_status(AnonymizationReviewStatus::Approved).unwrap();
        let rejected_reviews = review_repo.get_review_items_by_status(AnonymizationReviewStatus::Rejected).unwrap();
        
        // Überprüfe die Anzahl der Reviews pro Status
        assert_eq!(pending_reviews.len(), 1, "Falsche Anzahl von Pending-Reviews");
        assert_eq!(approved_reviews.len(), 1, "Falsche Anzahl von Approved-Reviews");
        assert_eq!(rejected_reviews.len(), 1, "Falsche Anzahl von Rejected-Reviews");
        
        // Überprüfe, ob die Reviews die richtigen Transkript-IDs haben
        assert_eq!(pending_reviews[0].transcript_id, transcript_id3, "Falsches Transkript für Pending-Review");
        assert_eq!(approved_reviews[0].transcript_id, transcript_id1, "Falsches Transkript für Approved-Review");
        assert_eq!(rejected_reviews[0].transcript_id, transcript_id2, "Falsches Transkript für Rejected-Review");
        
        // Cleanup
        cleanup();
    }
}
