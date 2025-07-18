// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Tauri Commands [AIU] [ATV] [SP]
// Implements secure Tauri commands for database operations

use crate::{
    database::connection::DatabaseManager,
    repositories::{
        PatientRepository, 
        SessionRepository, 
        TranscriptRepository, 
        AuditRepository
    }
};
use serde::{Serialize, Deserialize};
use tauri::State;
use uuid::Uuid;
use chrono::Utc;
use log::{info, error, debug};
use std::sync::Mutex;

// Type aliases for Tauri state
pub type DatabaseState = Mutex<DatabaseManager>;
pub type PatientRepoState = Mutex<PatientRepository>;
pub type SessionRepoState = Mutex<SessionRepository>;
pub type TranscriptRepoState = Mutex<TranscriptRepository>;
pub type AuditRepoState = Mutex<AuditRepository>;

// Error type for commands
#[derive(Debug, Serialize)]
pub struct CommandError {
    pub message: String,
    pub code: String,
}

impl From<String> for CommandError {
    fn from(message: String) -> Self {
        Self {
            message,
            code: "UNKNOWN_ERROR".to_string(),
        }
    }
}

// Convert repository errors to CommandError
macro_rules! impl_from_error {
    ($error_type:ty, $code:expr) => {
        impl From<$error_type> for CommandError {
            fn from(err: $error_type) -> Self {
                Self {
                    message: err.to_string(),
                    code: $code.to_string(),
                }
            }
        }
    };
}

impl_from_error!(crate::repositories::patient_repository::PatientRepositoryError, "PATIENT_REPO_ERROR");
impl_from_error!(crate::repositories::session_repository::SessionRepositoryError, "SESSION_REPO_ERROR");
impl_from_error!(crate::repositories::transcript_repository::TranscriptRepositoryError, "TRANSCRIPT_REPO_ERROR");
impl_from_error!(crate::repositories::audit_repository::AuditRepositoryError, "AUDIT_REPO_ERROR");
impl_from_error!(crate::database::connection::DatabaseError, "DATABASE_ERROR");

// Type alias for command results
pub type CommandResult<T> = Result<T, CommandError>;

// DTO types for API
#[derive(Debug, Serialize, Deserialize)]
pub struct PatientDto {
    pub id: String,
    pub first_name: String,
    pub last_name: String,
    pub date_of_birth: String,
    pub insurance_number: String,
    pub notes: Option<String>,
    pub created: String,
    pub last_modified: String,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct CreatePatientDto {
    pub first_name: String,
    pub last_name: String,
    pub date_of_birth: String,
    pub insurance_number: String,
    pub notes: Option<String>,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct SessionDto {
    pub id: String,
    pub patient_id: String,
    pub status: String,
    pub notes: Option<String>,
    pub audio_file_path: Option<String>,
    pub created: String,
    pub last_modified: String,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct CreateSessionDto {
    pub patient_id: String,
    pub notes: Option<String>,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct TranscriptDto {
    pub id: String,
    pub session_id: String,
    pub anonymized_text: String,
    pub anonymization_confidence: Option<f64>,
    pub needs_review: bool,
    pub created: String,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct CreateTranscriptDto {
    pub session_id: String,
    pub original_text: String,
    pub anonymized_text: String,
    pub anonymization_confidence: f64,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct AuditLogDto {
    pub id: String,
    pub entity_name: String,
    pub entity_id: String,
    pub action: String,
    pub changes: Option<String>,
    pub timestamp: String,
    pub user_id: String,
}

// Patient commands
#[tauri::command]
pub async fn create_patient(
    patient_repo: State<'_, PatientRepoState>,
    patient: CreatePatientDto,
    user_id: String,
) -> CommandResult<PatientDto> {
    info!("Creating patient");
    
    let repo = patient_repo.lock().map_err(|_| CommandError::from("Failed to lock patient repository".to_string()))?;
    
    let created_patient = repo.create_patient(
        &patient.first_name,
        &patient.last_name,
        &patient.date_of_birth,
        &patient.insurance_number,
        patient.notes.as_deref(),
        &user_id,
    )?;
    
    // Decrypt patient data for response
    let first_name = repo.decrypt_first_name(&created_patient)?;
    let last_name = repo.decrypt_last_name(&created_patient)?;
    let notes = if let Some(encrypted_notes) = &created_patient.encrypted_notes {
        Some(repo.decrypt_notes(encrypted_notes)?)
    } else {
        None
    };
    
    Ok(PatientDto {
        id: created_patient.id,
        first_name,
        last_name,
        date_of_birth: created_patient.date_of_birth,
        insurance_number: patient.insurance_number, // Return the original input, not the hash
        notes,
        created: created_patient.created,
        last_modified: created_patient.last_modified,
    })
}

#[tauri::command]
pub async fn get_patient(
    patient_repo: State<'_, PatientRepoState>,
    id: String,
    user_id: String,
) -> CommandResult<PatientDto> {
    info!("Getting patient: {}", id);
    
    let repo = patient_repo.lock().map_err(|_| CommandError::from("Failed to lock patient repository".to_string()))?;
    
    let patient = repo.get_patient_by_id(&id, &user_id)?;
    
    // Decrypt patient data for response
    let first_name = repo.decrypt_first_name(&patient)?;
    let last_name = repo.decrypt_last_name(&patient)?;
    let notes = if let Some(encrypted_notes) = &patient.encrypted_notes {
        Some(repo.decrypt_notes(encrypted_notes)?)
    } else {
        None
    };
    
    Ok(PatientDto {
        id: patient.id,
        first_name,
        last_name,
        date_of_birth: patient.date_of_birth,
        insurance_number: "•••••••••••••", // Masked for security [AIU]
        notes,
        created: patient.created,
        last_modified: patient.last_modified,
    })
}

#[tauri::command]
pub async fn list_patients(
    patient_repo: State<'_, PatientRepoState>,
    user_id: String,
) -> CommandResult<Vec<PatientDto>> {
    info!("Listing patients");
    
    let repo = patient_repo.lock().map_err(|_| CommandError::from("Failed to lock patient repository".to_string()))?;
    
    let patients = repo.get_all_patients(&user_id)?;
    
    let mut patient_dtos = Vec::new();
    for patient in patients {
        // Decrypt patient data for response
        let first_name = repo.decrypt_first_name(&patient)?;
        let last_name = repo.decrypt_last_name(&patient)?;
        let notes = if let Some(encrypted_notes) = &patient.encrypted_notes {
            Some(repo.decrypt_notes(encrypted_notes)?)
        } else {
            None
        };
        
        patient_dtos.push(PatientDto {
            id: patient.id,
            first_name,
            last_name,
            date_of_birth: patient.date_of_birth,
            insurance_number: "•••••••••••••", // Masked for security [AIU]
            notes,
            created: patient.created,
            last_modified: patient.last_modified,
        });
    }
    
    Ok(patient_dtos)
}

// Session commands
#[tauri::command]
pub async fn create_session(
    session_repo: State<'_, SessionRepoState>,
    session: CreateSessionDto,
    user_id: String,
) -> CommandResult<SessionDto> {
    info!("Creating session for patient: {}", session.patient_id);
    
    let repo = session_repo.lock().map_err(|_| CommandError::from("Failed to lock session repository".to_string()))?;
    
    let created_session = repo.create_session(
        &session.patient_id,
        session.notes.as_deref(),
        &user_id,
    )?;
    
    // Decrypt session data for response
    let notes = if let Some(encrypted_notes) = &created_session.encrypted_notes {
        Some(repo.decrypt_notes(encrypted_notes)?)
    } else {
        None
    };
    
    let audio_file_path = if let Some(encrypted_path) = &created_session.encrypted_audio_file_path {
        Some(repo.decrypt_audio_file_path(encrypted_path)?)
    } else {
        None
    };
    
    Ok(SessionDto {
        id: created_session.id,
        patient_id: created_session.patient_id,
        status: created_session.status.to_string(),
        notes,
        audio_file_path,
        created: created_session.created,
        last_modified: created_session.last_modified,
    })
}

#[tauri::command]
pub async fn get_session(
    session_repo: State<'_, SessionRepoState>,
    id: String,
    user_id: String,
) -> CommandResult<SessionDto> {
    info!("Getting session: {}", id);
    
    let repo = session_repo.lock().map_err(|_| CommandError::from("Failed to lock session repository".to_string()))?;
    
    let session = repo.get_session_by_id(&id, &user_id)?;
    
    // Decrypt session data for response
    let notes = if let Some(encrypted_notes) = &session.encrypted_notes {
        Some(repo.decrypt_notes(encrypted_notes)?)
    } else {
        None
    };
    
    let audio_file_path = if let Some(encrypted_path) = &session.encrypted_audio_file_path {
        Some(repo.decrypt_audio_file_path(encrypted_path)?)
    } else {
        None
    };
    
    Ok(SessionDto {
        id: session.id,
        patient_id: session.patient_id,
        status: session.status.to_string(),
        notes,
        audio_file_path,
        created: session.created,
        last_modified: session.last_modified,
    })
}

#[tauri::command]
pub async fn list_patient_sessions(
    session_repo: State<'_, SessionRepoState>,
    patient_id: String,
    user_id: String,
) -> CommandResult<Vec<SessionDto>> {
    info!("Listing sessions for patient: {}", patient_id);
    
    let repo = session_repo.lock().map_err(|_| CommandError::from("Failed to lock session repository".to_string()))?;
    
    let sessions = repo.get_sessions_for_patient(&patient_id, &user_id)?;
    
    let mut session_dtos = Vec::new();
    for session in sessions {
        // Decrypt session data for response
        let notes = if let Some(encrypted_notes) = &session.encrypted_notes {
            Some(repo.decrypt_notes(encrypted_notes)?)
        } else {
            None
        };
        
        let audio_file_path = if let Some(encrypted_path) = &session.encrypted_audio_file_path {
            Some(repo.decrypt_audio_file_path(encrypted_path)?)
        } else {
            None
        };
        
        session_dtos.push(SessionDto {
            id: session.id,
            patient_id: session.patient_id,
            status: session.status.to_string(),
            notes,
            audio_file_path,
            created: session.created,
            last_modified: session.last_modified,
        });
    }
    
    Ok(session_dtos)
}

// Transcript commands
#[tauri::command]
pub async fn create_transcript(
    transcript_repo: State<'_, TranscriptRepoState>,
    transcript: CreateTranscriptDto,
    user_id: String,
) -> CommandResult<TranscriptDto> {
    info!("Creating transcript for session: {}", transcript.session_id);
    
    // Enforce anonymization [AIU]
    if transcript.anonymized_text.trim().is_empty() {
        return Err(CommandError {
            message: "Anonymized text is required and cannot be empty".to_string(),
            code: "ANONYMIZATION_REQUIRED".to_string(),
        });
    }
    
    let repo = transcript_repo.lock().map_err(|_| CommandError::from("Failed to lock transcript repository".to_string()))?;
    
    let created_transcript = repo.create_transcript(
        &transcript.session_id,
        &transcript.original_text,
        &transcript.anonymized_text,
        transcript.anonymization_confidence,
        &user_id,
    )?;
    
    // Decrypt transcript data for response
    let anonymized_text = repo.decrypt_anonymized_text(&created_transcript)?;
    
    Ok(TranscriptDto {
        id: created_transcript.id,
        session_id: created_transcript.session_id,
        anonymized_text,
        anonymization_confidence: created_transcript.anonymization_confidence,
        needs_review: created_transcript.needs_review,
        created: created_transcript.created,
    })
}

#[tauri::command]
pub async fn get_session_transcripts(
    transcript_repo: State<'_, TranscriptRepoState>,
    session_id: String,
    user_id: String,
) -> CommandResult<Vec<TranscriptDto>> {
    info!("Getting transcripts for session: {}", session_id);
    
    let repo = transcript_repo.lock().map_err(|_| CommandError::from("Failed to lock transcript repository".to_string()))?;
    
    let transcripts = repo.get_transcripts_for_session(&session_id, &user_id)?;
    
    let mut transcript_dtos = Vec::new();
    for transcript in transcripts {
        // Only decrypt anonymized text for response [AIU]
        let anonymized_text = repo.decrypt_anonymized_text(&transcript)?;
        
        transcript_dtos.push(TranscriptDto {
            id: transcript.id,
            session_id: transcript.session_id,
            anonymized_text,
            anonymization_confidence: transcript.anonymization_confidence,
            needs_review: transcript.needs_review,
            created: transcript.created,
        });
    }
    
    Ok(transcript_dtos)
}

// Audit commands (restricted to admin users)
#[tauri::command]
pub async fn get_entity_audit_logs(
    audit_repo: State<'_, AuditRepoState>,
    entity_name: String,
    entity_id: String,
    user_id: String,
    limit: Option<i64>,
    offset: Option<i64>,
) -> CommandResult<Vec<AuditLogDto>> {
    info!("Getting audit logs for {}/{}", entity_name, entity_id);
    
    let repo = audit_repo.lock().map_err(|_| CommandError::from("Failed to lock audit repository".to_string()))?;
    
    let logs = repo.get_audit_logs_for_entity(
        &entity_name,
        &entity_id,
        limit.unwrap_or(100),
        offset.unwrap_or(0),
    )?;
    
    let mut log_dtos = Vec::new();
    for log in logs {
        log_dtos.push(AuditLogDto {
            id: log.id,
            entity_name: log.entity_name,
            entity_id: log.entity_id,
            action: log.action,
            changes: log.changes,
            timestamp: log.timestamp,
            user_id: log.user_id,
        });
    }
    
    Ok(log_dtos)
}

// Database initialization command
#[tauri::command]
pub async fn initialize_database(
    db: State<'_, DatabaseState>,
) -> CommandResult<bool> {
    info!("Initializing database");
    
    let db_manager = db.lock().map_err(|_| CommandError::from("Failed to lock database manager".to_string()))?;
    
    // Initialize database schema
    db_manager.initialize_schema()?;
    
    Ok(true)
}
