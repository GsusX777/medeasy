// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Repository Tests [KP100] [AIU] [EIV]
// Tests für die Repository-Implementierungen mit Fokus auf Anonymisierung

#[allow(unused_imports)]

use crate::database::connection::DatabaseManager;
use crate::repositories::transcript_repository::{TranscriptRepository, TranscriptRepositoryError};
use crate::repositories::patient_repository::PatientRepository;
use crate::repositories::session_repository::SessionRepository;
// use crate::repositories::audit_repository::AuditRepository; // Wird nicht verwendet
// use crate::database::models::{AnonymizationReviewItem, ReviewStatus}; // Wird nicht verwendet
use std::env;
use std::fs;
use std::sync::Once;
// use base64::Engine;
// use base64::engine::general_purpose::STANDARD as BASE64; // Wird nicht verwendet

// Initialisiere die Testumgebung
static INIT: Once = Once::new();
fn initialize() {
    // Zuerst alte Testdatenbank entfernen, um sicherzustellen, dass wir mit einem sauberen Schema beginnen [ZTS]
    let _ = fs::remove_file("medeasy_repo_test.db");
    let _ = fs::remove_file("medeasy_repo_test.db-shm");
    let _ = fs::remove_file("medeasy_repo_test.db-wal");
    
    INIT.call_once(|| {
        // Eindeutigen Datenbanknamen für Tests verwenden, um Konflikte zu vermeiden [ZTS]
        env::set_var("DATABASE_URL", "medeasy_repo_test.db");
        
        // Konsistente Verschlüsselungsschlüssel verwenden [SP][ZTS]
        env::set_var("MEDEASY_DB_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
        env::set_var("MEDEASY_FIELD_ENCRYPTION_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
        
        // Verschlüsselung und Audit-Trail aktivieren [SP][ATV][ZTS]
        env::set_var("USE_ENCRYPTION", "true");
        env::set_var("MEDEASY_ENFORCE_AUDIT", "true");
        
        // Testmodus aktivieren
        env::set_var("MEDEASY_TEST_MODE", "true");
    });
}

// Aufräumen nach Tests
fn cleanup() {
    // Alle SQLite-Dateien entfernen (Hauptdatei und WAL-Dateien) [ZTS]
    let _ = fs::remove_file("medeasy_repo_test.db");
    let _ = fs::remove_file("medeasy_repo_test.db-shm");
    let _ = fs::remove_file("medeasy_repo_test.db-wal");
}

// Hilfsfunktion zum Einrichten der Testdatenbank
fn setup_test_db() -> (DatabaseManager, PatientRepository, SessionRepository, TranscriptRepository) {
    // Testdatenbank initialisieren und alte Dateien entfernen
    initialize();
    
    // Datenbank erstellen und migrieren
    let mut db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
    
    // Explizit Migrationen ausführen, um sicherzustellen, dass alle Tabellen erstellt werden [SP][ATV]
    db_manager.run_migrations().expect("Migrationen sollten erfolgreich ausgeführt werden");
    
    // SQLCipher PRAGMAs prüfen [SP]
    let conn = db_manager.get_connection().expect("Verbindung sollte hergestellt werden können");
    
    // Überprüfe, ob die Verschlüsselung aktiv ist [SP][ZTS]
    let cipher_version: String = conn.pragma_query_value(None, "cipher_version", |row| row.get(0))
        .expect("PRAGMA cipher_version sollte abgefragt werden können");
    assert!(!cipher_version.is_empty(), "SQLCipher sollte aktiv sein");
    
    // Überprüfe Journal-Mode für Leistung und Datensicherheit [SP][ZTS]
    let journal_mode: String = conn.pragma_query_value(None, "journal_mode", |row| row.get(0))
        .expect("PRAGMA journal_mode sollte abgefragt werden können");
        
    // In-Memory-Datenbanken verwenden "memory" als Journal-Mode
    // Dateibasierte Datenbanken verwenden "wal" für bessere Performance und Datensicherheit
    let lower_mode = journal_mode.to_lowercase();
    assert!(lower_mode == "wal" || lower_mode == "memory", 
        "Journal-Mode sollte entweder WAL (für Datei-DBs) oder MEMORY (für In-Memory-DBs) sein, war aber: {}", lower_mode);
    
    // Überprüfe, ob die Tabellen existieren [ZTS]
    let table_count: i32 = conn.query_row(
        "SELECT count(*) FROM sqlite_master WHERE type='table' AND name IN ('patients', 'sessions', 'transcripts', 'audit_logs')",
        [],
        |row| row.get(0)
    ).expect("Tabellen sollten überprüft werden können");
    
    assert_eq!(table_count, 4, "Alle vier Haupttabellen sollten existieren");
    
    // Repositories erstellen
    let patient_repo = PatientRepository::new(db_manager.clone())
        .expect("PatientRepository sollte erstellt werden können");
    let session_repo = SessionRepository::new(db_manager.clone())
        .expect("SessionRepository sollte erstellt werden können");
    let transcript_repo = TranscriptRepository::new(db_manager.clone())
        .expect("TranscriptRepository sollte erstellt werden können");
    
    (db_manager, patient_repo, session_repo, transcript_repo)
}

// Hilfsfunktion zum Erstellen eines Testpatienten
fn create_test_patient(patient_repo: &PatientRepository) -> String {
    let patient = patient_repo.create_patient(
        "Max Mustermann",
        "756.1234.5678.90",
        "01.01.1980",
        "test_user"
    ).expect("Patient sollte erstellt werden können");
    
    patient.id.clone()
}

// Hilfsfunktion zum Erstellen einer Testsession
fn create_test_session(session_repo: &SessionRepository, patient_id: &str) -> String {
    let session = session_repo.create_session(
        patient_id,
        "09.07.2025",
        "test_user"
    ).expect("Session sollte erstellt werden können");
    
    session.id
}

#[test]
/// Test, dass die Anonymisierung erzwungen wird [AIU]
fn test_anonymization_enforced() {
    let (_, patient_repo, session_repo, transcript_repo) = setup_test_db();
    
    // Testdaten erstellen
    let patient_id = create_test_patient(&patient_repo);
    let session_id = create_test_session(&session_repo, &patient_id);
    
    // Versuch, ein Transkript ohne Anonymisierung zu erstellen
    let result = transcript_repo.create_transcript(
        &session_id,
        "Dies ist ein Test mit dem Patienten Max Mustermann.",
        "", // Leerer anonymisierter Text
        1.0,
        "test_user"
    );
    
    // Sollte mit AnonymizationRequired-Fehler fehlschlagen
    assert!(result.is_err(), "Transkript ohne Anonymisierung sollte abgelehnt werden");
    match result {
        Err(TranscriptRepositoryError::AnonymizationRequired(_)) => (),
        _ => panic!("Falscher Fehlertyp, AnonymizationRequired erwartet"),
    }
    
    cleanup();
}

#[test]
/// Test, dass die Anonymisierung korrekt funktioniert [AIU][ZTS][TR]
fn test_anonymization_success() {
    use crate::tests::test_helpers::TestDatabaseFixture;
    use crate::repositories::transcript_repository::TranscriptRepository;
    
    // Isolierte Testdatenbank für diesen Test erstellen [ZTS][TR]
    let fixture = TestDatabaseFixture::new();
    
    // Stelle sicher, dass keine vorherigen Daten vorhanden sind [ZTS]
    fixture.clean_tables().expect("Tabellen sollten geleert werden können");
    
    // Testdaten erstellen
    let user_id = "test_user";
    
    // Patient erstellen
    let patient = fixture.create_test_patient(user_id)
        .expect("Testpatient sollte erstellt werden können");
    
    // Session für diesen Patienten erstellen
    let session = fixture.create_test_session(&patient.id, user_id)
        .expect("Testsession sollte erstellt werden können");
    
    // TranscriptRepository für unsere Testdatenbank erstellen
    // Verwende .as_ref().clone(), um einen DatabaseManager statt Arc<DatabaseManager> zu erhalten [ZTS]
    let db_manager = fixture.db_manager.as_ref().clone();
    let transcript_repo = TranscriptRepository::new(db_manager)
        .expect("TranscriptRepository sollte erstellt werden können");
    
    // Transkript mit Anonymisierung erstellen
    let original_text = "Dies ist ein Test mit dem Patienten Max Mustermann.";
    let anonymized_text = "Dies ist ein Test mit dem Patienten [NAME].";
    
    // 0.95 ist über dem Schwellenwert von 0.8, daher sollte KEIN Review benötigt werden [AIU]
    let transcript = transcript_repo.create_transcript(
        &session.id,
        original_text,
        anonymized_text,
        0.95, // Hohe Konfidenz
        user_id
    ).expect("Transkript mit Anonymisierung sollte erstellt werden können");
    
    // Transkript abrufen und überprüfen
    let retrieved = transcript_repo.get_transcript_by_id(&transcript.id, user_id)
        .expect("Transkript sollte abgerufen werden können");
    
    assert_eq!(retrieved.id, transcript.id, "Transkript-ID sollte übereinstimmen");
    assert_eq!(retrieved.session_id, session.id, "Session-ID sollte übereinstimmen");
    assert!(retrieved.is_anonymized, "Transkript sollte als anonymisiert markiert sein");
    assert_eq!(retrieved.anonymization_confidence.unwrap(), 0.95, "Anonymisierungskonfidenz sollte 0.95 sein");
    
    // KRITISCHE ÜBERPRÜFUNG: Bei 0.95 Konfidenz (> 0.8) darf KEIN Review benötigt werden [AIU][ZTS]
    assert!(!retrieved.needs_review, "Transkript sollte kein Review benötigen");
    
    // Entschlüsselten Text überprüfen
    let decrypted_original = transcript_repo.decrypt_original_text(&retrieved)
        .expect("Originaltext sollte entschlüsselt werden können");
    let decrypted_anonymized = transcript_repo.decrypt_anonymized_text(&retrieved)
        .expect("Anonymisierter Text sollte entschlüsselt werden können");
    
    assert_eq!(decrypted_original, original_text, "Entschlüsselter Originaltext sollte übereinstimmen");
    assert_eq!(decrypted_anonymized, anonymized_text, "Entschlüsselter anonymisierter Text sollte übereinstimmen");
    
    // Keine explizite Cleanup nötig - TestDatabaseFixture.drop() erledigt das
}

#[test]
/// Test, dass niedrige Anonymisierungskonfidenz zu Review-Markierung führt [ARQ][ZTS][AIU]
fn test_low_confidence_review() {
    use crate::tests::test_helpers::TestDatabaseFixture;
    use crate::repositories::transcript_repository::TranscriptRepository;
    
    // Isolierte Testdatenbank für diesen Test erstellen [ZTS][TR]
    let fixture = TestDatabaseFixture::new();
    
    // Stelle sicher, dass keine vorherigen Daten vorhanden sind [ZTS]
    fixture.clean_tables().expect("Tabellen sollten geleert werden können");
    
    // Testdaten erstellen
    let user_id = "test_user";
    
    // Patient erstellen
    let patient = fixture.create_test_patient(user_id)
        .expect("Testpatient sollte erstellt werden können");
    
    // Session für diesen Patienten erstellen
    let session = fixture.create_test_session(&patient.id, user_id)
        .expect("Testsession sollte erstellt werden können");
    
    // TranscriptRepository für unsere Testdatenbank erstellen
    let db_manager = fixture.db_manager.as_ref().clone();
    let transcript_repo = TranscriptRepository::new(db_manager)
        .expect("TranscriptRepository sollte erstellt werden können");
    
    // Transkript mit niedriger Konfidenz erstellen
    let original_text = "Dies ist ein Test mit dem Patienten Max Mustermann und Dr. Schmidt.";
    let anonymized_text = "Dies ist ein Test mit dem Patienten [NAME] und [NAME].";
    
    // WICHTIG: 0.65 ist unter dem Schwellenwert von 0.8, daher sollte Review benötigt werden [AIU][ZTS]
    let transcript = transcript_repo.create_transcript(
        &session.id,
        original_text,
        anonymized_text,
        0.65, // Niedrige Konfidenz
        user_id
    ).expect("Transkript mit niedriger Konfidenz sollte erstellt werden können");
    
    // Überprüfen, ob das Transkript als zu überprüfend markiert ist
    let retrieved = transcript_repo.get_transcript_by_id(&transcript.id, user_id)
        .expect("Transkript sollte abgerufen werden können");
    
    // KRITISCHE ÜBERPRÜFUNG: Bei 0.65 Konfidenz (< 0.8) MUSS Review benötigt werden [AIU][ZTS]
    assert!(retrieved.needs_review, "Transkript mit niedriger Konfidenz sollte als zu überprüfend markiert sein");
    assert_eq!(retrieved.anonymization_confidence.unwrap(), 0.65, "Anonymisierungskonfidenz sollte 0.65 sein");
    
    // Keine explizite Cleanup nötig - TestDatabaseFixture.drop() erledigt das
}

#[test]
/// Test, dass Transkripte nach Session abgerufen werden können [AIU][ZTS][TR]
fn test_get_transcripts_by_session() {
    use crate::tests::test_helpers::TestDatabaseFixture;
    use crate::repositories::transcript_repository::TranscriptRepository;
    
    // Isolierte Testdatenbank für diesen Test erstellen [ZTS][TR]
    let fixture = TestDatabaseFixture::new();
    
    // Stelle sicher, dass keine vorherigen Daten vorhanden sind [ZTS]
    fixture.clean_tables().expect("Tabellen sollten geleert werden können");
    
    // Testdaten erstellen
    let user_id = "test_user";
    
    // Patient erstellen
    let patient = fixture.create_test_patient(user_id)
        .expect("Testpatient sollte erstellt werden können");
    
    // Session für diesen Patienten erstellen
    let session = fixture.create_test_session(&patient.id, user_id)
        .expect("Testsession sollte erstellt werden können");
    
    // TranscriptRepository für unsere Testdatenbank erstellen
    let db_manager = fixture.db_manager.as_ref().clone();
    let transcript_repo = TranscriptRepository::new(db_manager)
        .expect("TranscriptRepository sollte erstellt werden können");
    
    // Transkripte für die Session erstellen
    for i in 1..=3 {
        let original_text = format!("Dies ist Transkript {} mit dem Patienten Max Mustermann.", i);
        let anonymized_text = format!("Dies ist Transkript {} mit dem Patienten [NAME].", i);
        
        transcript_repo.create_transcript(
            &session.id,
            &original_text,
            &anonymized_text,
            0.95, // Hohe Konfidenz
            user_id
        ).expect("Transkript sollte erstellt werden können");
    }
    
    // Transkripte für die Session abrufen
    let transcripts = transcript_repo.get_transcripts_for_session(&session.id, user_id)
        .expect("Transkripte sollten abgerufen werden können");
    
    assert_eq!(transcripts.len(), 3, "Es sollten drei Transkripte für die Session vorhanden sein");
    
    for transcript in transcripts {
        assert_eq!(transcript.session_id, session.id, "Transkript sollte zur richtigen Session gehören");
        assert!(transcript.is_anonymized, "Transkript sollte anonymisiert sein");
    }
    
    // Keine explizite Cleanup nötig - TestDatabaseFixture.drop() erledigt das
}

#[test]
/// Test, dass zu überprüfende Transkripte abgerufen werden können [ARQ][ZTS][TR]
fn test_get_transcripts_needing_review() {
    use crate::tests::test_helpers::TestDatabaseFixture;
    use crate::repositories::transcript_repository::TranscriptRepository;
    
    // Isolierte Testdatenbank für diesen Test erstellen [ZTS][TR]
    let fixture = TestDatabaseFixture::new();
    
    // Stelle sicher, dass keine vorherigen Daten vorhanden sind [ZTS]
    fixture.clean_tables().expect("Tabellen sollten geleert werden können");
    
    // Testdaten erstellen
    let user_id = "test_user";
    
    // Patient erstellen
    let patient = fixture.create_test_patient(user_id)
        .expect("Testpatient sollte erstellt werden können");
    
    // Session für diesen Patienten erstellen
    let session = fixture.create_test_session(&patient.id, user_id)
        .expect("Testsession sollte erstellt werden können");
    
    // TranscriptRepository für unsere Testdatenbank erstellen
    let db_manager = fixture.db_manager.as_ref().clone();
    let transcript_repo = TranscriptRepository::new(db_manager)
        .expect("TranscriptRepository sollte erstellt werden können");
    
    // Genau 2 Transkripte erstellen, die Review benötigen (niedrige Konfidenz)
    for i in 1..=2 {
        let original_text = format!("Review-Transkript {}", i);
        let anonymized_text = format!("Review-Transkript {}", i);
        
        transcript_repo.create_transcript(
            &session.id,
            &original_text,
            &anonymized_text,
            0.6, // Niedrige Konfidenz
            user_id
        ).expect("Transkript sollte erstellt werden können");
    }
    
    // Genau 1 Transkript erstellen, das kein Review benötigt (hohe Konfidenz)
    transcript_repo.create_transcript(
        &session.id,
        "Kein Review nötig",
        "Kein Review nötig",
        0.98, // Hohe Konfidenz
        user_id
    ).expect("Transkript sollte erstellt werden können");
    
    // Zu überprüfende Transkripte abrufen
    let review_transcripts = transcript_repo.get_transcripts_needing_review(user_id)
        .expect("Zu überprüfende Transkripte sollten abgerufen werden können");
    
    // EXAKT 2 Transkripte sollten Review benötigen [ZTS][AIU]
    assert_eq!(review_transcripts.len(), 2, "Es sollten genau 2 zu überprüfende Transkripte vorhanden sein");
    
    // Validiere die Eigenschaften der zurückgegebenen Transkripte
    for transcript in review_transcripts {
        assert!(transcript.needs_review, "Transkript sollte als zu überprüfend markiert sein");
        assert!(transcript.anonymization_confidence.unwrap() < 0.8, 
                "Transkript sollte niedrige Konfidenz haben");
    }
    
    // Keine explizite Cleanup nötig - TestDatabaseFixture.drop() erledigt das
}

// Wir erstellen einen vereinfachten Test für die Low-Confidence-Review-Funktionalität ohne AnonymizationReviewRepository [ARQ][ATV]
#[test]
fn test_low_confidence_detection() {
    initialize();
    
    let mut db_manager = DatabaseManager::new().expect("Datenbankmanager sollte initialisiert werden können");
    let _ = db_manager.run_migrations();
    
    let patient_repo = PatientRepository::new(db_manager.clone())
        .expect("Patient-Repository sollte initialisiert werden können");
    
    let session_repo = SessionRepository::new(db_manager.clone())
        .expect("Session-Repository sollte initialisiert werden können");
    
    let transcript_repo = TranscriptRepository::new(db_manager.clone())
        .expect("Transcript-Repository sollte initialisiert werden können");
    
    // Testdaten erstellen
    let patient_id = create_test_patient(&patient_repo);
    let session_id = create_test_session(&session_repo, &patient_id);
    
    // Transkript mit niedrigem Konfidenzwert erstellen (löst Review aus)
    let transcript = transcript_repo.create_transcript(
        &session_id,
        "Dies ist ein Test mit sensiblen Daten",
        "Dies ist ein Test mit [ANONYMISIERT] Daten",
        0.4, // Niedrige Konfidenz
        "test_user"
    ).expect("Transkript sollte erstellt werden können");
    
    let transcript_id = &transcript.id; // ID aus dem Transcript-Objekt extrahieren
    
    // Transkripte abrufen, die überprüft werden müssen
    let needing_review = transcript_repo.get_transcripts_needing_review("test_user")
        .expect("Transkripte, die Überprüfung benötigen, sollten abgerufen werden können");
    
    // Prüfen, ob das Transkript mit niedriger Konfidenz in der Liste ist
    assert!(!needing_review.is_empty(), "Es sollte mindestens ein Transkript geben, das Überprüfung benötigt");
    assert!(needing_review.iter().any(|t| t.id == *transcript_id), 
           "Das Transkript mit niedriger Konfidenz sollte in der Liste der zu überprüfenden Transkripte sein");
    
    // Prüfen, ob das richtige Transkript abgerufen wird
    let retrieved = transcript_repo.get_transcript_by_id(transcript_id, "test_user")
        .expect("Das Transkript sollte abgerufen werden können");
    
    assert_eq!(retrieved.anonymization_confidence, Some(0.4), "Die Anonymisierungskonfidenz sollte korrekt gespeichert sein");
    assert_eq!(retrieved.session_id, session_id, "Die Session-ID sollte korrekt gespeichert sein");
    
    cleanup();
}

#[test]
/// Test, dass ein nicht existierendes Transkript einen Fehler zurückgibt [AIU]
fn test_nonexistent_transcript() {
    let (_, _, _, transcript_repo) = setup_test_db();
    
    // Versuch, ein nicht existierendes Transkript abzurufen
    let result = transcript_repo.get_transcript_by_id("nonexistent_id", "test_user");
    
    // Der Test sollte fehlschlagen, aber der genaue Fehlertyp kann variieren:
    // - NotFound: Wenn die Tabelle existiert, aber kein Eintrag gefunden wird
    // - DatabaseError/SqlError: Wenn die Tabelle nicht existiert oder ein anderes DB-Problem vorliegt
    assert!(result.is_err(), "Abruf eines nicht existierenden Transkripts sollte fehlschlagen");
    match result {
        Err(TranscriptRepositoryError::NotFound(_)) => (),
        Err(TranscriptRepositoryError::DatabaseError(_)) => (),
        Err(TranscriptRepositoryError::SqlError(_)) => (),
        _ => panic!("Unerwarteter Fehlertyp, NotFound oder DatabaseError erwartet"),
    }
    
    cleanup();
}

#[test]
/// Test, dass ein Transkript für eine nicht existierende Session nicht erstellt werden kann [AIU]
fn test_transcript_for_nonexistent_session() {
    let (_, _, _, transcript_repo) = setup_test_db();
    
    // Versuch, ein Transkript für eine nicht existierende Session zu erstellen
    let result = transcript_repo.create_transcript(
        "nonexistent_session_id",
        "Test",
        "Test",
        0.95,
        "test_user"
    );
    
    assert!(result.is_err(), "Erstellung eines Transkripts für eine nicht existierende Session sollte fehlschlagen");
    match result {
        Err(TranscriptRepositoryError::SessionNotFound(_)) => (),
        _ => panic!("Falscher Fehlertyp, SessionNotFound erwartet"),
    }
    
    cleanup();
}
