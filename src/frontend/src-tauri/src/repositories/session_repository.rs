// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Session Repository [SK] [ATV] [EIV]
// Implements data access for session records with encryption and audit logging

#[allow(unused_imports)]

use crate::database::{
    connection::{DatabaseManager, DatabaseError},
    encryption::{FieldEncryption, EncryptionError},
    models::{Session, SessionStatus, AuditLog} // AuditLog importiert [ATV]
};
use rusqlite::{params, Result, Error};
use uuid::Uuid;
use chrono::Utc;
use log::{debug, error};
// use serde_json::json;
use thiserror::Error;

#[derive(Error, Debug)]
pub enum SessionRepositoryError {
    #[error("Database error: {0}")]
    DatabaseError(#[from] DatabaseError),
    
    #[error("SQL error: {0}")]
    SqlError(#[from] Error),
    
    #[error("Encryption error: {0}")]
    EncryptionError(#[from] EncryptionError),
    
    #[error("Session not found: {0}")]
    NotFound(String),
    
    #[error("Validation error: {0}")]
    ValidationError(String),
    
    #[error("Patient not found: {0}")]
    PatientNotFound(String),
}

/// Repository for session data operations [SK]
pub struct SessionRepository {
    db: DatabaseManager,
    encryption: FieldEncryption,
}

impl SessionRepository {
    /// Creates a new session repository
    pub fn new(db: DatabaseManager) -> Result<Self, SessionRepositoryError> {
        let encryption = FieldEncryption::new()?;
        Ok(Self { db, encryption })
    }
    
    /// Creates a new session record
    pub fn create_session(
        &self,
        patient_id: &str,
        session_date: &str,
        user_id: &str
    ) -> Result<Session, SessionRepositoryError> {
        debug!("Creating new session for patient: {}", patient_id);
        
        // Validate inputs
        self.validate_session_data(patient_id, session_date)?;
        
        // Verify patient exists
        self.verify_patient_exists(patient_id)?;
        
        // Generate UUID
        let session_id = Uuid::new_v4().to_string();
        let now = Utc::now().to_rfc3339();
        
        // Aktuelle Zeit als start_time verwenden [SC]
        let start_time = now.clone();
        
        // Insert session record
        self.db.with_connection(|conn| {
            conn.execute(
                "INSERT INTO sessions (
                    id, patient_id, session_date, start_time, end_time, status,
                    encrypted_notes, encrypted_audio_reference,
                    created, created_by, last_modified, last_modified_by
                ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8, ?9, ?10, ?11, ?12)",
                params![
                    session_id,
                    patient_id,
                    session_date,
                    start_time,              // Startzeit hinzugefügt [SC]
                    Option::<String>::None,  // end_time ist NULL bei Erstellung
                    SessionStatus::InProgress.to_string(),
                    None::<Vec<u8>>,
                    None::<Vec<u8>>,
                    now,
                    user_id,
                    now,
                    user_id
                ],
            )?;
            
            // Create audit log entry [ATV]
            let audit_log = AuditLog::new(
                "sessions",
                &session_id,
                "INSERT",
                Some(&format!("Created session for patient: {}, date: {}", patient_id, session_date)),
                true, // Contains sensitive data
                user_id,
            );
            
            conn.execute(
                "INSERT INTO audit_logs (
                    id, entity_name, entity_id, action, changes,
                    contains_sensitive_data, timestamp, user_id
                ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8)",
                params![
                    audit_log.id,
                    audit_log.entity_name,
                    audit_log.entity_id,
                    audit_log.action,
                    audit_log.changes,
                    audit_log.contains_sensitive_data as i32,
                    audit_log.timestamp,
                    audit_log.user_id
                ],
            )?;
            
            Ok(())
        })?;
        
        // Return the created session
        Ok(Session {
            id: session_id,
            patient_id: patient_id.to_string(),
            session_date: session_date.to_string(),
            start_time: Some(start_time.clone()),  // Startzeit hinzugefügt [SC]
            end_time: None,                        // Endzeit ist bei Erstellung null [SC]
            status: SessionStatus::InProgress.to_string(),
            encrypted_notes: None,
            encrypted_audio_reference: None,
            created: now.clone(),
            created_by: user_id.to_string(),
            last_modified: now,
            last_modified_by: user_id.to_string(),
        })
    }
    
    /// Gets a session by ID
    pub fn get_session_by_id(&self, id: &str, user_id: &str) -> Result<Session, SessionRepositoryError> {
        debug!("Getting session by ID: {}", id);
        
        let session = self.db.with_connection(|conn| {
            // Query session record
            let mut stmt = conn.prepare(
                "SELECT id, patient_id, session_date, start_time, end_time, status,
                        encrypted_notes, encrypted_audio_reference,
                        created, created_by, last_modified, last_modified_by
                 FROM sessions WHERE id = ?1"
            )?;
            
            let session = stmt.query_row(params![id], |row| {
                Ok(Session {
                    id: row.get(0)?,
                    patient_id: row.get(1)?,
                    session_date: row.get(2)?,
                    start_time: row.get(3)?,
                    end_time: row.get(4)?,
                    status: row.get(5)?,
                    encrypted_notes: row.get(6)?,
                    encrypted_audio_reference: row.get(7)?,
                    created: row.get(8)?,
                    created_by: row.get(9)?,
                    last_modified: row.get(10)?,
                    last_modified_by: row.get(11)?,
                })
            })?;
            
            // Create audit log entry for data access [ATV]
            let audit_log = AuditLog::new(
                "sessions",
                id,
                "SELECT",
                Some("Session data accessed"),
                true, // Contains sensitive data
                user_id,
            );
            
            conn.execute(
                "INSERT INTO audit_logs (
                    id, entity_name, entity_id, action, changes,
                    contains_sensitive_data, timestamp, user_id
                ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8)",
                params![
                    audit_log.id,
                    audit_log.entity_name,
                    audit_log.entity_id,
                    audit_log.action,
                    audit_log.changes,
                    audit_log.contains_sensitive_data as i32,
                    audit_log.timestamp,
                    audit_log.user_id
                ],
            )?;
            
            Ok(session)
        })?;
        
        Ok(session)
    }
    
    /// Gets sessions for a patient
    pub fn get_sessions_for_patient(&self, patient_id: &str, user_id: &str) -> Result<Vec<Session>, SessionRepositoryError> {
        debug!("Getting sessions for patient: {}", patient_id);
        
        let sessions = self.db.with_connection(|conn| {
            // Query session records
            let mut stmt = conn.prepare(
                "SELECT id, patient_id, session_date, start_time, end_time, status,
                        encrypted_notes, encrypted_audio_reference,
                        created, created_by, last_modified, last_modified_by
                 FROM sessions WHERE patient_id = ?1
                 ORDER BY session_date DESC"
            )?;
            
            let session_iter = stmt.query_map(params![patient_id], |row| {
                Ok(Session {
                    id: row.get(0)?,
                    patient_id: row.get(1)?,
                    session_date: row.get(2)?,
                    start_time: row.get(3)?,
                    end_time: row.get(4)?,
                    status: row.get(5)?,
                    encrypted_notes: row.get(6)?,
                    encrypted_audio_reference: row.get(7)?,
                    created: row.get(8)?,
                    created_by: row.get(9)?,
                    last_modified: row.get(10)?,
                    last_modified_by: row.get(11)?,
                })
            })?;
            
            let mut sessions = Vec::new();
            for session in session_iter {
                sessions.push(session?);
            }
            
            // Create audit log entry for data access [ATV]
            let audit_log = AuditLog::new(
                "patients",
                patient_id,
                "SELECT",
                Some(&format!("Listed {} sessions for patient", sessions.len())),
                true, // Contains sensitive data
                user_id,
            );
            
            conn.execute(
                "INSERT INTO audit_logs (
                    id, entity_name, entity_id, action, changes,
                    contains_sensitive_data, timestamp, user_id
                ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8)",
                params![
                    audit_log.id,
                    audit_log.entity_name,
                    audit_log.entity_id,
                    audit_log.action,
                    audit_log.changes,
                    audit_log.contains_sensitive_data as i32,
                    audit_log.timestamp,
                    audit_log.user_id
                ],
            )?;
            
            Ok(sessions)
        })?;
        
        Ok(sessions)
    }
    
    /// Updates a session's notes
    pub fn update_session_notes(
        &self,
        id: &str,
        notes: &str,
        user_id: &str
    ) -> Result<Session, SessionRepositoryError> {
        debug!("Updating notes for session: {}", id);
        
        // Get current session
        let current_session = self.get_session_by_id(id, user_id)?;
        
        // Encrypt notes [EIV]
        let encrypted_notes = self.encryption.encrypt(notes)?;
        
        let now = Utc::now().to_rfc3339();
        
        // Update session
        self.db.with_connection(|conn| {
            conn.execute(
                "UPDATE sessions SET 
                    encrypted_notes = ?1,
                    last_modified = ?2,
                    last_modified_by = ?3
                 WHERE id = ?4",
                params![
                    encrypted_notes,
                    now,
                    user_id,
                    id
                ],
            )?;
            
            // Create audit log entry [ATV]
            let audit_log = AuditLog::new(
                "sessions",
                id,
                "UPDATE",
                Some("Updated session notes"),
                true, // Contains sensitive data
                user_id,
            );
            
            conn.execute(
                "INSERT INTO audit_logs (
                    id, entity_name, entity_id, action, changes,
                    contains_sensitive_data, timestamp, user_id
                ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8)",
                params![
                    audit_log.id,
                    audit_log.entity_name,
                    audit_log.entity_id,
                    audit_log.action,
                    audit_log.changes,
                    audit_log.contains_sensitive_data as i32,
                    audit_log.timestamp,
                    audit_log.user_id
                ],
            )?;
            
            Ok(())
        })?;
        
        // Return updated session
        Ok(Session {
            id: id.to_string(),
            patient_id: current_session.patient_id,
            session_date: current_session.session_date,
            start_time: current_session.start_time,
            end_time: current_session.end_time,
            status: current_session.status,
            encrypted_notes: Some(encrypted_notes),
            encrypted_audio_reference: current_session.encrypted_audio_reference,
            created: current_session.created,
            created_by: current_session.created_by,
            last_modified: now,
            last_modified_by: user_id.to_string(),
        })
    }
    
    /// Updates a session's audio file reference
    pub fn update_session_audio(
        &self,
        id: &str,
        audio_file_reference: &str,
        user_id: &str
    ) -> Result<Session, SessionRepositoryError> {
        debug!("Updating audio reference for session: {}", id);
        
        // Get current session
        let current_session = self.get_session_by_id(id, user_id)?;
        
        // Encrypt audio file reference [EIV]
        let encrypted_audio = self.encryption.encrypt(audio_file_reference)?;
        
        let now = Utc::now().to_rfc3339();
        
        // Update session
        self.db.with_connection(|conn| {
            conn.execute(
                "UPDATE sessions SET 
                    encrypted_audio_reference = ?1,
                    last_modified = ?2,
                    last_modified_by = ?3
                 WHERE id = ?4",
                params![
                    encrypted_audio,
                    now,
                    user_id,
                    id
                ],
            )?;
            
            // Create audit log entry [ATV]
            let audit_log = AuditLog::new(
                "sessions",
                id,
                "UPDATE",
                Some("Updated session audio reference"),
                true, // Contains sensitive data
                user_id,
            );
            
            conn.execute(
                "INSERT INTO audit_logs (
                    id, entity_name, entity_id, action, changes,
                    contains_sensitive_data, timestamp, user_id
                ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8)",
                params![
                    audit_log.id,
                    audit_log.entity_name,
                    audit_log.entity_id,
                    audit_log.action,
                    audit_log.changes,
                    audit_log.contains_sensitive_data as i32,
                    audit_log.timestamp,
                    audit_log.user_id
                ],
            )?;
            
            Ok(())
        })?;
        
        // Return updated session
        Ok(Session {
            id: id.to_string(),
            patient_id: current_session.patient_id,
            session_date: current_session.session_date,
            start_time: current_session.start_time,
            end_time: current_session.end_time,
            status: current_session.status,
            encrypted_notes: current_session.encrypted_notes,
            encrypted_audio_reference: Some(encrypted_audio),
            created: current_session.created,
            created_by: current_session.created_by,
            last_modified: now,
            last_modified_by: user_id.to_string(),
        })
    }
    
    /// Updates a session's status
    pub fn update_session_status(
        &self,
        id: &str,
        status: SessionStatus,
        user_id: &str
    ) -> Result<Session, SessionRepositoryError> {
        debug!("Updating status for session: {} to {:?}", id, status);
        
        // Get current session
        let current_session = self.get_session_by_id(id, user_id)?;
        
        let now = Utc::now().to_rfc3339();
        let status_str = status.to_string();
        
        // Update session
        self.db.with_connection(|conn| {
            conn.execute(
                "UPDATE sessions SET 
                    status = ?1,
                    last_modified = ?2,
                    last_modified_by = ?3
                 WHERE id = ?4",
                params![
                    status_str,
                    now,
                    user_id,
                    id
                ],
            )?;
            
            // Create audit log entry [ATV]
            let audit_log = AuditLog::new(
                "sessions",
                id,
                "UPDATE",
                Some(&format!("Updated session status to {}", status_str)),
                false, // Status change is not sensitive
                user_id,
            );
            
            conn.execute(
                "INSERT INTO audit_logs (
                    id, entity_name, entity_id, action, changes,
                    contains_sensitive_data, timestamp, user_id
                ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8)",
                params![
                    audit_log.id,
                    audit_log.entity_name,
                    audit_log.entity_id,
                    audit_log.action,
                    audit_log.changes,
                    audit_log.contains_sensitive_data as i32,
                    audit_log.timestamp,
                    audit_log.user_id
                ],
            )?;
            
            Ok(())
        })?;
        
        // Return updated session
        Ok(Session {
            id: id.to_string(),
            patient_id: current_session.patient_id,
            session_date: current_session.session_date,
            start_time: current_session.start_time,
            end_time: current_session.end_time,
            status: status_str,
            encrypted_notes: current_session.encrypted_notes,
            encrypted_audio_reference: current_session.encrypted_audio_reference,
            created: current_session.created,
            created_by: current_session.created_by,
            last_modified: now,
            last_modified_by: user_id.to_string(),
        })
    }
    
    /// Decrypts a session's notes
    pub fn decrypt_session_notes(&self, session: &Session) -> Result<Option<String>, SessionRepositoryError> {
        if let Some(ref encrypted_notes) = session.encrypted_notes {
            Ok(Some(self.encryption.decrypt(encrypted_notes)?))
        } else {
            Ok(None)
        }
    }
    
    /// Decrypts a session's audio file reference
    pub fn decrypt_audio_reference(&self, session: &Session) -> Result<Option<String>, SessionRepositoryError> {
        if let Some(ref encrypted_audio) = session.encrypted_audio_reference {
            Ok(Some(self.encryption.decrypt(encrypted_audio)?))
        } else {
            Ok(None)
        }
    }
    
    /// Validates session data
    fn validate_session_data(
        &self,
        patient_id: &str,
        session_date: &str,
    ) -> Result<(), SessionRepositoryError> {
        // Patient ID validation
        if patient_id.trim().is_empty() {
            return Err(SessionRepositoryError::ValidationError("Patient ID cannot be empty".to_string()));
        }
        
        // Date validation (Swiss format: DD.MM.YYYY) [SC]
        let date_regex = regex::Regex::new(r"^(0[1-9]|[12][0-9]|3[01])\.(0[1-9]|1[0-2])\.\d{4}$").unwrap();
        if !date_regex.is_match(session_date) {
            return Err(SessionRepositoryError::ValidationError(
                "Invalid date format. Expected: DD.MM.YYYY".to_string()
            ));
        }
        
        Ok(())
    }
    
    /// Verifies that a patient exists
    fn verify_patient_exists(&self, patient_id: &str) -> Result<(), SessionRepositoryError> {
        let exists = self.db.with_connection(|conn| {
            let count: i64 = conn.query_row(
                "SELECT COUNT(*) FROM patients WHERE id = ?1",
                params![patient_id],
                |row| row.get(0),
            )?;
            
            Ok(count > 0)
        })?;
        
        if !exists {
            return Err(SessionRepositoryError::PatientNotFound(
                format!("Patient with ID {} not found", patient_id)
            ));
        }
        
        Ok(())
    }
}
