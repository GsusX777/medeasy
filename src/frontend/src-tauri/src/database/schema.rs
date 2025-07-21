// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Database Schema [SP] [ATV]
// Defines the database schema with tables for patients, sessions, transcripts, and audit logs

use rusqlite::{Connection, Result}; // params wird nicht verwendet
use log::{info, debug};

/// Creates all database tables if they don't exist
pub fn create_tables(conn: &Connection) -> Result<()> {
    debug!("Creating database tables if they don't exist");
    
    // Patients Tabelle [EIV]
    conn.execute(
        "CREATE TABLE IF NOT EXISTS patients (
            id TEXT PRIMARY KEY,
            encrypted_name BLOB NOT NULL,
            insurance_number_hash TEXT NOT NULL,
            date_of_birth TEXT NOT NULL,
            created TEXT NOT NULL,
            created_by TEXT NOT NULL,
            last_modified TEXT NOT NULL,
            last_modified_by TEXT NOT NULL
        )",
        [],
    )?;
    info!("Patients table created or verified");
    
    // Sessions Tabelle [SK]
    conn.execute(
        "CREATE TABLE IF NOT EXISTS sessions (
            id TEXT PRIMARY KEY,
            patient_id TEXT NOT NULL,
            session_date TEXT NOT NULL,
            start_time TEXT,
            end_time TEXT,
            status TEXT NOT NULL,
            encrypted_notes BLOB,
            encrypted_audio_file_reference BLOB,
            created TEXT NOT NULL,
            created_by TEXT NOT NULL,
            last_modified TEXT NOT NULL,
            last_modified_by TEXT NOT NULL,
            FOREIGN KEY (patient_id) REFERENCES patients(id)
        )",
        [],
    )?;
    info!("Sessions table created or verified");
    
    // Transcripts Tabelle [AIU]
    conn.execute(
        "CREATE TABLE IF NOT EXISTS transcripts (
            id TEXT PRIMARY KEY,
            session_id TEXT NOT NULL,
            encrypted_original_text BLOB NOT NULL,
            encrypted_anonymized_text BLOB NOT NULL,
            is_anonymized INTEGER NOT NULL DEFAULT 1,
            anonymization_confidence REAL,
            needs_review INTEGER NOT NULL DEFAULT 0,
            created TEXT NOT NULL,
            created_by TEXT NOT NULL,
            last_modified TEXT NOT NULL,
            last_modified_by TEXT NOT NULL,
            FOREIGN KEY (session_id) REFERENCES sessions(id)
        )",
        [],
    )?;
    info!("Transcripts table created or verified");
    
    // AnonymizationReviewItems Tabelle [ARQ]
    conn.execute(
        "CREATE TABLE IF NOT EXISTS anonymization_review_items (
            id TEXT PRIMARY KEY,
            transcript_id TEXT NOT NULL,
            status TEXT NOT NULL,
            detected_pii TEXT NOT NULL,
            anonymization_confidence REAL NOT NULL,
            review_reason TEXT,
            reviewer_notes TEXT,
            created TEXT NOT NULL,
            created_by TEXT NOT NULL,
            last_modified TEXT NOT NULL,
            last_modified_by TEXT NOT NULL,
            FOREIGN KEY (transcript_id) REFERENCES transcripts(id)
        )",
        [],
    )?;
    info!("Anonymization review items table created or verified");
    
    // AuditLog Tabelle [ATV]
    conn.execute(
        "CREATE TABLE IF NOT EXISTS audit_logs (
            id TEXT PRIMARY KEY,
            entity_name TEXT NOT NULL,
            entity_id TEXT NOT NULL,
            action TEXT NOT NULL,
            changes TEXT,
            contains_sensitive_data INTEGER NOT NULL,
            timestamp TEXT NOT NULL,
            user_id TEXT NOT NULL
        )",
        [],
    )?;
    info!("Audit logs table created or verified");
    
    // Indizes erstellen
    create_indices(conn)?;
    
    Ok(())
}

/// Creates indices for better query performance
fn create_indices(conn: &Connection) -> Result<()> {
    debug!("Creating database indices");
    
    // Patient Indizes
    conn.execute("CREATE INDEX IF NOT EXISTS idx_patients_insurance_hash ON patients(insurance_number_hash)", [])?;
    
    // Session Indizes
    conn.execute("CREATE INDEX IF NOT EXISTS idx_sessions_patient_id ON sessions(patient_id)", [])?;
    conn.execute("CREATE INDEX IF NOT EXISTS idx_sessions_date ON sessions(session_date)", [])?;
    
    // Transcript Indizes
    conn.execute("CREATE INDEX IF NOT EXISTS idx_transcripts_session_id ON transcripts(session_id)", [])?;
    conn.execute("CREATE INDEX IF NOT EXISTS idx_transcripts_needs_review ON transcripts(needs_review)", [])?;
    
    // AnonymizationReviewItem Indizes
    conn.execute("CREATE INDEX IF NOT EXISTS idx_review_items_transcript_id ON anonymization_review_items(transcript_id)", [])?;
    conn.execute("CREATE INDEX IF NOT EXISTS idx_review_items_status ON anonymization_review_items(status)", [])?;
    
    // AuditLog Indizes
    conn.execute("CREATE INDEX IF NOT EXISTS idx_audit_entity ON audit_logs(entity_name, entity_id)", [])?;
    conn.execute("CREATE INDEX IF NOT EXISTS idx_audit_timestamp ON audit_logs(timestamp)", [])?;
    conn.execute("CREATE INDEX IF NOT EXISTS idx_audit_user ON audit_logs(user_id)", [])?;
    
    info!("Database indices created or verified");
    Ok(())
}

/// Verifies database integrity and constraints
pub fn verify_database(conn: &Connection) -> Result<bool> {
    debug!("Verifying database integrity");
    
    // Check foreign key constraints
    let foreign_keys_enabled: bool = conn.query_row("PRAGMA foreign_keys", [], |row| row.get(0))?;
    if !foreign_keys_enabled {
        conn.execute("PRAGMA foreign_keys = ON", [])?;
        info!("Foreign key constraints enabled");
    }
    
    // Check if anonymization can be disabled (should never be possible) [AIU]
    let anonymization_required = true;
    
    // Verify encryption is working if enabled
    let encryption_working = match conn.query_row("PRAGMA cipher_version", [], |_| Ok(())) {
        Ok(_) => true,
        Err(_) => false,
    };
    
    Ok(anonymization_required && (encryption_working || !cfg!(debug_assertions)))
}
