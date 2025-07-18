// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Database Models [EIV] [MDL]
// Defines the data models with encrypted fields for sensitive data

use serde::{Serialize, Deserialize};
use chrono::Utc; // DateTime wird nicht verwendet
use uuid::Uuid;

/// Patient model with encrypted personal information [EIV]
#[derive(Debug, Serialize, Deserialize)]
pub struct Patient {
    /// Unique identifier
    pub id: String,
    
    /// Encrypted patient name - never stored as plaintext [EIV]
    pub encrypted_name: Vec<u8>,
    
    /// Hashed insurance number - never stored as plaintext [EIV]
    pub insurance_number_hash: String,
    
    /// Date of birth in DD.MM.YYYY format [SC]
    pub date_of_birth: String,
    
    /// Creation timestamp
    pub created: String,
    
    /// User who created the record
    pub created_by: String,
    
    /// Last modification timestamp
    pub last_modified: String,
    
    /// User who last modified the record
    pub last_modified_by: String,
}

/// Session status enum
#[derive(Debug, Serialize, Deserialize, PartialEq)]
pub enum SessionStatus {
    /// Session is in progress
    InProgress,
    
    /// Session is completed
    Completed,
    
    /// Session is archived
    Archived,
}

impl ToString for SessionStatus {
    fn to_string(&self) -> String {
        match self {
            SessionStatus::InProgress => "in_progress".to_string(),
            SessionStatus::Completed => "completed".to_string(),
            SessionStatus::Archived => "archived".to_string(),
        }
    }
}

impl From<String> for SessionStatus {
    fn from(s: String) -> Self {
        match s.as_str() {
            "in_progress" => SessionStatus::InProgress,
            "completed" => SessionStatus::Completed,
            "archived" => SessionStatus::Archived,
            _ => SessionStatus::InProgress, // Default
        }
    }
}

/// Session model representing a medical consultation [SK]
#[derive(Debug, Serialize, Deserialize)]
pub struct Session {
    /// Unique identifier
    pub id: String,
    
    /// Reference to patient
    pub patient_id: String,
    
    /// Session date in DD.MM.YYYY format [SC]
    pub session_date: String,
    
    /// Start time of the session (RFC3339 format) [SC]
    pub start_time: Option<String>,
    
    /// End time of the session (RFC3339 format) [SC]
    pub end_time: Option<String>,
    
    /// Current status of the session
    pub status: String, // Stored as string in DB, converted to enum in code
    
    /// Encrypted doctor's notes [EIV]
    pub encrypted_notes: Option<Vec<u8>>,
    
    /// Encrypted reference to audio file [EIV]
    pub encrypted_audio_reference: Option<Vec<u8>>,
    
    /// Creation timestamp
    pub created: String,
    
    /// User who created the record
    pub created_by: String,
    
    /// Last modification timestamp
    pub last_modified: String,
    
    /// User who last modified the record
    pub last_modified_by: String,
}

/// Transcript model for session recordings [AIU]
#[derive(Debug, Serialize, Deserialize)]
pub struct Transcript {
    /// Unique identifier
    pub id: String,
    
    /// Reference to session
    pub session_id: String,
    
    /// Original encrypted transcript text [EIV]
    pub encrypted_original_text: Vec<u8>,
    
    /// Anonymized encrypted transcript text [AIU] [EIV]
    pub encrypted_anonymized_text: Vec<u8>,
    
    /// Flag indicating if anonymization is complete
    /// This is always true in production [AIU]
    pub is_anonymized: bool,
    
    /// Confidence score of anonymization (0-100%)
    pub anonymization_confidence: Option<f64>,
    
    /// Flag indicating if human review is needed
    pub needs_review: bool,
    
    /// Creation timestamp
    pub created: String,
    
    /// User who created the record
    pub created_by: String,
    
    /// Last modification timestamp
    pub last_modified: String,
    
    /// User who last modified the record
    pub last_modified_by: String,
}

/// Review status enum for anonymization review [ARQ]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum ReviewStatus {
    /// Wartet auf Überprüfung
    Pending,
    /// Wird gerade überprüft
    InReview,
    /// Wurde genehmigt
    Approved,
    /// Wurde abgelehnt
    Rejected,
    /// Wurde auf die Whitelist gesetzt
    Whitelisted,
    /// Wurde automatisch genehmigt (hohe Konfidenz)
    AutoApproved,
}

impl ToString for ReviewStatus {
    fn to_string(&self) -> String {
        match self {
            ReviewStatus::Pending => "pending".to_string(),
            ReviewStatus::InReview => "in_review".to_string(),
            ReviewStatus::Approved => "approved".to_string(),
            ReviewStatus::Rejected => "rejected".to_string(),
            ReviewStatus::Whitelisted => "whitelisted".to_string(),
            ReviewStatus::AutoApproved => "auto_approved".to_string(),
        }
    }
}

impl ReviewStatus {
    /// Konvertiert einen String in einen ReviewStatus
    pub fn from_string(s: &str) -> Result<Self, String> {
        match s {
            "pending" => Ok(ReviewStatus::Pending),
            "in_review" => Ok(ReviewStatus::InReview),
            "approved" => Ok(ReviewStatus::Approved),
            "rejected" => Ok(ReviewStatus::Rejected),
            "whitelisted" => Ok(ReviewStatus::Whitelisted),
            "auto_approved" => Ok(ReviewStatus::AutoApproved),
            _ => Err(format!("Ungültiger ReviewStatus: {}", s)),
        }
    }
}

/// Anonymization review item for human verification [AIU][ARQ]
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AnonymizationReviewItem {
    /// Eindeutige ID des Review-Eintrags
    pub id: String,
    
    /// ID des zugehörigen Transkripts
    pub transcript_id: String,
    
    /// Status des Reviews
    pub status: ReviewStatus,
    
    /// Erkannte persönlich identifizierbare Informationen (JSON)
    pub detected_pii: Option<String>,
    
    /// Konfidenz der Anonymisierung (0-100%)
    pub anonymization_confidence: f64,
    
    /// Grund für die manuelle Überprüfung
    pub review_reason: Option<String>,
    
    /// Notizen des Prüfers
    pub reviewer_notes: Option<String>,
    
    /// Erstellungszeitpunkt
    pub created: String,
    
    /// Benutzer, der den Eintrag erstellt hat
    pub created_by: String,
    
    /// Zeitpunkt der letzten Änderung
    pub last_modified: String,
    
    /// Benutzer, der die letzte Änderung vorgenommen hat
    pub last_modified_by: String,
}

/// Audit log entry for tracking all database operations [ATV]
#[derive(Debug, Serialize, Deserialize)]
pub struct AuditLog {
    /// Unique identifier
    pub id: String,
    
    /// Entity type (table name)
    pub entity_name: String,
    
    /// Entity ID
    pub entity_id: String,
    
    /// Action performed (INSERT, UPDATE, DELETE, SELECT)
    pub action: String,
    
    /// JSON string of changes (for UPDATE)
    pub changes: Option<String>,
    
    /// Flag indicating if sensitive data was accessed
    pub contains_sensitive_data: bool,
    
    /// Timestamp of the action
    pub timestamp: String,
    
    /// User who performed the action
    pub user_id: String,
}

/// Helper functions for creating new entities
impl Patient {
    pub fn new(
        _name: &str, 
        insurance_number_hash: &str, 
        date_of_birth: &str, 
        user_id: &str,
        encrypted_name: Vec<u8>
    ) -> Self {
        let now = Utc::now().to_rfc3339();
        Self {
            id: Uuid::new_v4().to_string(),
            encrypted_name,
            insurance_number_hash: insurance_number_hash.to_string(),
            date_of_birth: date_of_birth.to_string(),
            created: now.clone(),
            created_by: user_id.to_string(),
            last_modified: now,
            last_modified_by: user_id.to_string(),
        }
    }
}

impl Session {
    pub fn new(
        patient_id: &str,
        session_date: &str,
        user_id: &str,
    ) -> Self {
        let now = Utc::now().to_rfc3339();
        Self {
            id: Uuid::new_v4().to_string(),
            patient_id: patient_id.to_string(),
            session_date: session_date.to_string(),
            start_time: Some(now.clone()), // Startzeit als aktuelle Zeit setzen [SC]
            end_time: None,                // Endzeit ist anfangs unbekannt [SC]
            status: SessionStatus::InProgress.to_string(),
            encrypted_notes: None,
            encrypted_audio_reference: None,
            created: now.clone(),
            created_by: user_id.to_string(),
            last_modified: now,
            last_modified_by: user_id.to_string(),
        }
    }
}

impl AnonymizationReviewItem {
    /// Erstellt einen neuen Review-Eintrag [AIU]
    pub fn new(
        transcript_id: &str,
        anonymization_confidence: f64,
        review_reason: Option<String>,
        created_by: &str,
    ) -> Self {
        let now = chrono::Utc::now().to_rfc3339();
        
        // Bestimme den initialen Status basierend auf der Konfidenz [AIU]
        let status = if anonymization_confidence >= 90.0 {
            ReviewStatus::AutoApproved
        } else {
            ReviewStatus::Pending
        };
        
        Self {
            id: uuid::Uuid::new_v4().to_string(),
            transcript_id: transcript_id.to_string(),
            status,
            detected_pii: None,
            anonymization_confidence,
            review_reason,
            reviewer_notes: None,
            created: now.clone(),
            created_by: created_by.to_string(),
            last_modified: now,
            last_modified_by: created_by.to_string(),
        }
    }
}

impl AuditLog {
    pub fn new(
        entity_name: &str,
        entity_id: &str,
        action: &str,
        changes: Option<&str>,
        contains_sensitive_data: bool,
        user_id: &str,
    ) -> Self {
        Self {
            id: Uuid::new_v4().to_string(),
            entity_name: entity_name.to_string(),
            entity_id: entity_id.to_string(),
            action: action.to_string(),
            changes: changes.map(|s| s.to_string()),
            contains_sensitive_data,
            timestamp: Utc::now().to_rfc3339(),
            user_id: user_id.to_string(),
        }
    }
}
