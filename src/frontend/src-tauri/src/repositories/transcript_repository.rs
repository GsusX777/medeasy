// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Transcript Repository [AIU] [ATV] [ARQ]
// Implements data access for transcript records with mandatory anonymization

#[allow(unused_imports)]

use crate::database::{
    connection::{DatabaseManager, DatabaseError},
    encryption::{FieldEncryption, EncryptionError},
    models::{Transcript, ReviewStatus, AuditLog}
};
use rusqlite::{params, Result, Error};
use uuid::Uuid;
use chrono::Utc;
use log::{debug, error};
use thiserror::Error;

#[derive(Error, Debug)]
pub enum TranscriptRepositoryError {
    #[error("Database error: {0}")]
    DatabaseError(#[from] DatabaseError),
    
    #[error("SQL error: {0}")]
    SqlError(#[from] Error),
    
    #[error("Encryption error: {0}")]
    EncryptionError(#[from] EncryptionError),
    
    #[error("Transcript not found: {0}")]
    NotFound(String),
    
    #[error("Session not found: {0}")]
    SessionNotFound(String),
    
    #[error("Anonymization required: {0}")]
    AnonymizationRequired(String),
}

/// Repository for transcript data operations with mandatory anonymization [AIU]
pub struct TranscriptRepository {
    db: DatabaseManager,
    encryption: FieldEncryption,
}

impl TranscriptRepository {
    /// Creates a new transcript repository
    pub fn new(db: DatabaseManager) -> Result<Self, TranscriptRepositoryError> {
        let encryption = FieldEncryption::new()?;
        Ok(Self { db, encryption })
    }
    
    /// Creates a new transcript record with mandatory anonymization [AIU]
    pub fn create_transcript(
        &self,
        session_id: &str,
        original_text: &str,
        anonymized_text: &str,
        anonymization_confidence: f64,
        user_id: &str
    ) -> Result<Transcript, TranscriptRepositoryError> {
        debug!("Creating new transcript for session: {}", session_id);
        
        // Verify session exists
        self.verify_session_exists(session_id)?;
        
        // Enforce anonymization [AIU]
        if anonymized_text.trim().is_empty() {
            return Err(TranscriptRepositoryError::AnonymizationRequired(
                "Anonymized text is required and cannot be empty".to_string()
            ));
        }
        
        // Generate UUID
        let transcript_id = Uuid::new_v4().to_string();
        
        // Encrypt both original and anonymized text [EIV]
        let encrypted_original = self.encryption.encrypt(original_text)?;
        let encrypted_anonymized = self.encryption.encrypt(anonymized_text)?;
        
        // Determine if review is needed based on confidence (Skala 0.0-1.0) [AIU]
        let needs_review = anonymization_confidence < 0.8;
        
        let now = Utc::now().to_rfc3339();
        
        // Insert transcript record
        self.db.with_connection_mut(|conn| {
            // Begin transaction
            let tx = conn.transaction()?;
            
            // Insert transcript
            tx.execute(
                "INSERT INTO transcripts (
                    id, session_id, encrypted_original_text, encrypted_anonymized_text,
                    is_anonymized, anonymization_confidence, needs_review,
                    created, created_by, last_modified, last_modified_by
                ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8, ?9, ?10, ?11)",
                params![
                    transcript_id,
                    session_id,
                    encrypted_original,
                    encrypted_anonymized,
                    true, // Always anonymized [AIU]
                    anonymization_confidence,
                    needs_review as i32,
                    now,
                    user_id,
                    now,
                    user_id
                ],
            )?;
            
            // Create audit log entry [ATV]
            let audit_log = AuditLog::new(
                "transcripts",
                &transcript_id,
                "INSERT",
                Some(&format!("Created transcript for session: {}, anonymization confidence: {:.1}%", 
                    session_id, anonymization_confidence)),
                true, // Contains sensitive data
                user_id,
            );
            
            tx.execute(
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
            
            // If confidence is low, create review items [ARQ]
            if needs_review {
                let review_id = Uuid::new_v4().to_string();
                
                tx.execute(
                    "INSERT INTO anonymization_review_items (
                        id, transcript_id, status, detected_pii,
                        anonymization_confidence, review_reason,
                        created, created_by, last_modified, last_modified_by
                    ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8, ?9, ?10)",
                    params![
                        review_id,
                        transcript_id,
                        ReviewStatus::Pending.to_string(),
                        "Automatic detection", // Will be replaced with actual PII in production
                        anonymization_confidence,
                        "Low confidence score",
                        now,
                        user_id,
                        now,
                        user_id
                    ],
                )?;
                
                debug!("Created review item for transcript with low confidence");
            }
            
            // Commit transaction
            tx.commit()?;
            
            Ok(())
        })?;
        
        // Return the created transcript
        Ok(Transcript {
            id: transcript_id,
            session_id: session_id.to_string(),
            encrypted_original_text: encrypted_original,
            encrypted_anonymized_text: encrypted_anonymized,
            is_anonymized: true, // Always true [AIU]
            anonymization_confidence: Some(anonymization_confidence),
            needs_review,
            created: now.clone(),
            created_by: user_id.to_string(),
            last_modified: now,
            last_modified_by: user_id.to_string(),
        })
    }
    
    /// Gets a transcript by ID
    pub fn get_transcript_by_id(&self, id: &str, user_id: &str) -> Result<Transcript, TranscriptRepositoryError> {
        debug!("Getting transcript by ID: {}", id);
        
        let transcript = self.db.with_connection(|conn| {
            // Query transcript record
            let mut stmt = conn.prepare(
                "SELECT id, session_id, encrypted_original_text, encrypted_anonymized_text,
                        is_anonymized, anonymization_confidence, needs_review,
                        created, created_by, last_modified, last_modified_by
                 FROM transcripts WHERE id = ?1"
            )?;
            
            let transcript = stmt.query_row(params![id], |row| {
                Ok(Transcript {
                    id: row.get(0)?,
                    session_id: row.get(1)?,
                    encrypted_original_text: row.get(2)?,
                    encrypted_anonymized_text: row.get(3)?,
                    is_anonymized: row.get::<_, i32>(4)? != 0,
                    anonymization_confidence: row.get(5)?,
                    needs_review: row.get::<_, i32>(6)? != 0,
                    created: row.get(7)?,
                    created_by: row.get(8)?,
                    last_modified: row.get(9)?,
                    last_modified_by: row.get(10)?,
                })
            })?;
            
            // Create audit log entry for data access [ATV]
            let audit_log = AuditLog::new(
                "transcripts",
                id,
                "SELECT",
                Some("Transcript data accessed"),
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
            
            Ok(transcript)
        })?;
        
        Ok(transcript)
    }
    
    /// Gets transcripts for a session
    pub fn get_transcripts_for_session(&self, session_id: &str, user_id: &str) -> Result<Vec<Transcript>, TranscriptRepositoryError> {
        debug!("Getting transcripts for session: {}", session_id);
        
        let transcripts = self.db.with_connection(|conn| {
            // Query transcript records
            let mut stmt = conn.prepare(
                "SELECT id, session_id, encrypted_original_text, encrypted_anonymized_text,
                        is_anonymized, anonymization_confidence, needs_review,
                        created, created_by, last_modified, last_modified_by
                 FROM transcripts WHERE session_id = ?1
                 ORDER BY created DESC"
            )?;
            
            let transcript_iter = stmt.query_map(params![session_id], |row| {
                Ok(Transcript {
                    id: row.get(0)?,
                    session_id: row.get(1)?,
                    encrypted_original_text: row.get(2)?,
                    encrypted_anonymized_text: row.get(3)?,
                    is_anonymized: row.get::<_, i32>(4)? != 0,
                    anonymization_confidence: row.get(5)?,
                    needs_review: row.get::<_, i32>(6)? != 0,
                    created: row.get(7)?,
                    created_by: row.get(8)?,
                    last_modified: row.get(9)?,
                    last_modified_by: row.get(10)?,
                })
            })?;
            
            let mut transcripts = Vec::new();
            for transcript in transcript_iter {
                transcripts.push(transcript?);
            }
            
            // Create audit log entry for data access [ATV]
            let audit_log = AuditLog::new(
                "sessions",
                session_id,
                "SELECT",
                Some(&format!("Listed {} transcripts for session", transcripts.len())),
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
            
            Ok(transcripts)
        })?;
        
        Ok(transcripts)
    }
    
    /// Gets transcripts that need review [ARQ]
    pub fn get_transcripts_needing_review(&self, user_id: &str) -> Result<Vec<Transcript>, TranscriptRepositoryError> {
        debug!("Getting transcripts that need review");
        
        let transcripts = self.db.with_connection(|conn| {
            // Query transcript records
            let mut stmt = conn.prepare(
                "SELECT id, session_id, encrypted_original_text, encrypted_anonymized_text,
                        is_anonymized, anonymization_confidence, needs_review,
                        created, created_by, last_modified, last_modified_by
                 FROM transcripts WHERE needs_review = 1
                 ORDER BY created DESC"
            )?;
            
            let transcript_iter = stmt.query_map([], |row| {
                Ok(Transcript {
                    id: row.get(0)?,
                    session_id: row.get(1)?,
                    encrypted_original_text: row.get(2)?,
                    encrypted_anonymized_text: row.get(3)?,
                    is_anonymized: row.get::<_, i32>(4)? != 0,
                    anonymization_confidence: row.get(5)?,
                    needs_review: row.get::<_, i32>(6)? != 0,
                    created: row.get(7)?,
                    created_by: row.get(8)?,
                    last_modified: row.get(9)?,
                    last_modified_by: row.get(10)?,
                })
            })?;
            
            let mut transcripts = Vec::new();
            for transcript in transcript_iter {
                transcripts.push(transcript?);
            }
            
            // Create audit log entry for data access [ATV]
            let audit_log = AuditLog::new(
                "transcripts",
                "batch",
                "SELECT",
                Some(&format!("Listed {} transcripts needing review", transcripts.len())),
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
            
            Ok(transcripts)
        })?;
        
        Ok(transcripts)
    }
    
    /// Decrypts a transcript's original text
    pub fn decrypt_original_text(&self, transcript: &Transcript) -> Result<String, TranscriptRepositoryError> {
        Ok(self.encryption.decrypt(&transcript.encrypted_original_text)?)
    }
    
    /// Decrypts a transcript's anonymized text
    pub fn decrypt_anonymized_text(&self, transcript: &Transcript) -> Result<String, TranscriptRepositoryError> {
        Ok(self.encryption.decrypt(&transcript.encrypted_anonymized_text)?)
    }
    
    /// Verifies that a session exists
    fn verify_session_exists(&self, session_id: &str) -> Result<(), TranscriptRepositoryError> {
        let exists = self.db.with_connection(|conn| {
            let count: i64 = conn.query_row(
                "SELECT COUNT(*) FROM sessions WHERE id = ?1",
                params![session_id],
                |row| row.get(0),
            )?;
            
            Ok(count > 0)
        })?;
        
        if !exists {
            return Err(TranscriptRepositoryError::SessionNotFound(
                format!("Session with ID {} not found", session_id)
            ));
        }
        
        Ok(())
    }
}
