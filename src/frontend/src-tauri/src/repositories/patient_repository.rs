// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Patient Repository [EIV] [ATV]
// Implements data access for patient records with encryption and audit logging

use crate::database::{
    connection::{DatabaseManager, DatabaseError},
    encryption::{FieldEncryption, EncryptionError},
    models::{Patient, AuditLog}
};
use rusqlite::{params, Result, Error};
use uuid::Uuid;
use chrono::Utc;
use log::{debug, error};
use serde_json::json;
use thiserror::Error;

#[derive(Error, Debug)]
pub enum PatientRepositoryError {
    #[error("Database error: {0}")]
    DatabaseError(#[from] DatabaseError),
    
    #[error("SQL error: {0}")]
    SqlError(#[from] Error),
    
    #[error("Encryption error: {0}")]
    EncryptionError(#[from] EncryptionError),
    
    #[error("Patient not found: {0}")]
    NotFound(String),
    
    #[error("Validation error: {0}")]
    ValidationError(String),
}

/// Repository for patient data operations
pub struct PatientRepository {
    db: DatabaseManager,
    encryption: FieldEncryption,
}

impl PatientRepository {
    /// Creates a new patient repository
    pub fn new(db: DatabaseManager) -> Result<Self, PatientRepositoryError> {
        let encryption = FieldEncryption::new()?;
        Ok(Self { db, encryption })
    }
    
    /// Creates a new patient record
    pub fn create_patient(
        &self,
        name: &str,
        insurance_number: &str,
        date_of_birth: &str,
        user_id: &str
    ) -> Result<Patient, PatientRepositoryError> {
        debug!("Creating new patient record");
        
        // Validate inputs
        self.validate_patient_data(name, insurance_number, date_of_birth)?;
        
        // Generate UUID
        let patient_id = Uuid::new_v4().to_string();
        
        // Encrypt sensitive data [EIV]
        let encrypted_name = self.encryption.encrypt(name)?;
        
        // Hash insurance number (never store in plaintext) [EIV]
        let insurance_hash = self.encryption.hash_value(insurance_number);
        
        let now = Utc::now().to_rfc3339();
        
        // Insert patient record
        self.db.with_connection(|conn| {
            conn.execute(
                "INSERT INTO patients (
                    id, encrypted_name, insurance_number_hash, date_of_birth,
                    created, created_by, last_modified, last_modified_by
                ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8)",
                params![
                    patient_id,
                    encrypted_name,
                    insurance_hash,
                    date_of_birth,
                    now,
                    user_id,
                    now,
                    user_id
                ],
            )?;
            
            // Create audit log entry [ATV]
            let audit_log = AuditLog::new(
                "patients",
                &patient_id,
                "INSERT",
                Some(&format!("Created patient with insurance hash: {}", insurance_hash)),
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
        
        // Return the created patient
        Ok(Patient {
            id: patient_id,
            encrypted_name,
            insurance_number_hash: insurance_hash,
            date_of_birth: date_of_birth.to_string(),
            created: now.clone(),
            created_by: user_id.to_string(),
            last_modified: now,
            last_modified_by: user_id.to_string(),
        })
    }
    
    /// Gets a patient by ID
    pub fn get_patient_by_id(&self, id: &str, user_id: &str) -> Result<Patient, PatientRepositoryError> {
        debug!("Getting patient by ID: {}", id);
        
        let patient = self.db.with_connection(|conn| {
            // Query patient record
            let mut stmt = conn.prepare(
                "SELECT id, encrypted_name, insurance_number_hash, date_of_birth,
                        created, created_by, last_modified, last_modified_by
                 FROM patients WHERE id = ?1"
            )?;
            
            let patient = stmt.query_row(params![id], |row| {
                Ok(Patient {
                    id: row.get(0)?,
                    encrypted_name: row.get(1)?,
                    insurance_number_hash: row.get(2)?,
                    date_of_birth: row.get(3)?,
                    created: row.get(4)?,
                    created_by: row.get(5)?,
                    last_modified: row.get(6)?,
                    last_modified_by: row.get(7)?,
                })
            })?;
            
            // Create audit log entry for data access [ATV]
            let audit_log = AuditLog::new(
                "patients",
                id,
                "SELECT",
                Some("Patient data accessed"),
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
            
            Ok(patient)
        })?;
        
        Ok(patient)
    }
    
    /// Gets a patient by insurance number hash
    pub fn get_patient_by_insurance(&self, insurance_number: &str, user_id: &str) -> Result<Patient, PatientRepositoryError> {
        debug!("Looking up patient by insurance number");
        
        // Hash the insurance number for lookup
        let insurance_hash = self.encryption.hash_value(insurance_number);
        
        let patient = self.db.with_connection(|conn| {
            // Query patient record
            let mut stmt = conn.prepare(
                "SELECT id, encrypted_name, insurance_number_hash, date_of_birth,
                        created, created_by, last_modified, last_modified_by
                 FROM patients WHERE insurance_number_hash = ?1"
            )?;
            
            let patient = stmt.query_row(params![insurance_hash], |row| {
                Ok(Patient {
                    id: row.get(0)?,
                    encrypted_name: row.get(1)?,
                    insurance_number_hash: row.get(2)?,
                    date_of_birth: row.get(3)?,
                    created: row.get(4)?,
                    created_by: row.get(5)?,
                    last_modified: row.get(6)?,
                    last_modified_by: row.get(7)?,
                })
            })?;
            
            // Create audit log entry for data access [ATV]
            let audit_log = AuditLog::new(
                "patients",
                &patient.id,
                "SELECT",
                Some("Patient looked up by insurance number"),
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
            
            Ok(patient)
        })?;
        
        Ok(patient)
    }
    
    /// Updates a patient record
    pub fn update_patient(
        &self,
        id: &str,
        name: Option<&str>,
        insurance_number: Option<&str>,
        date_of_birth: Option<&str>,
        user_id: &str
    ) -> Result<Patient, PatientRepositoryError> {
        debug!("Updating patient record: {}", id);
        
        // Get current patient data for comparison and audit
        let current_patient = self.get_patient_by_id(id, user_id)?;
        
        // Track changes for audit log
        let mut changes = json!({});
        let mut has_changes = false;
        
        // Prepare update parameters
        let mut params_vec: Vec<Box<dyn rusqlite::ToSql>> = Vec::new();
        let mut set_clauses = Vec::new();
        
        // Handle name update
        let encrypted_name = if let Some(new_name) = name {
            has_changes = true;
            changes["name"] = json!("updated");
            let enc_name = self.encryption.encrypt(new_name)?;
            set_clauses.push("encrypted_name = ?");
            params_vec.push(Box::new(enc_name.clone()));
            enc_name
        } else {
            // Wichtig: Klonen, um Ownership-Probleme zu vermeiden [ZTS]
            current_patient.encrypted_name.clone()
        };
        
        // Handle insurance number update
        let insurance_hash = if let Some(new_insurance) = insurance_number {
            has_changes = true;
            changes["insurance_number"] = json!("updated");
            let hash = self.encryption.hash_value(new_insurance);
            set_clauses.push("insurance_number_hash = ?");
            params_vec.push(Box::new(hash.clone()));
            hash
        } else {
            // Wichtig: Klonen, um Ownership-Probleme zu vermeiden [ZTS]
            current_patient.insurance_number_hash.clone()
        };
        
        // Handle date of birth update
        let date_of_birth = if let Some(new_dob) = date_of_birth {
            has_changes = true;
            changes["date_of_birth"] = json!(new_dob);
            set_clauses.push("date_of_birth = ?");
            params_vec.push(Box::new(new_dob.to_string()));
            new_dob.to_string()
        } else {
            // Wichtig: Klonen, um Ownership-Probleme zu vermeiden [ZTS]
            current_patient.date_of_birth.clone()
        };
        
        // If no changes, return current patient
        if !has_changes {
            return Ok(current_patient);
        }
        
        // Add timestamp and user info
        let now = Utc::now().to_rfc3339();
        set_clauses.push("last_modified = ?");
        params_vec.push(Box::new(now.clone()));
        set_clauses.push("last_modified_by = ?");
        params_vec.push(Box::new(user_id.to_string()));
        
        // Add ID for WHERE clause
        params_vec.push(Box::new(id.to_string()));
        
        // Build the SQL query
        let sql = format!(
            "UPDATE patients SET {} WHERE id = ?",
            set_clauses.join(", ")
        );
        
        // Execute update
        self.db.with_connection(|conn| {
            // Convert params_vec to a slice of &dyn ToSql
            let params: Vec<&dyn rusqlite::ToSql> = params_vec
                .iter()
                .map(|p| p.as_ref())
                .collect();
            
            conn.execute(&sql, params.as_slice())?;
            
            // Create audit log entry [ATV]
            let audit_log = AuditLog::new(
                "patients",
                id,
                "UPDATE",
                Some(&changes.to_string()),
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
        
        // Return updated patient
        Ok(Patient {
            id: id.to_string(),
            encrypted_name,
            insurance_number_hash: insurance_hash,
            date_of_birth: date_of_birth,
            created: current_patient.created,
            created_by: current_patient.created_by,
            last_modified: now,
            last_modified_by: user_id.to_string(),
        })
    }
    
    /// Decrypts a patient's name
    pub fn decrypt_patient_name(&self, patient: &Patient) -> Result<String, PatientRepositoryError> {
        Ok(self.encryption.decrypt(&patient.encrypted_name)?)
    }
    
    /// Validates patient data
    fn validate_patient_data(
        &self,
        name: &str,
        insurance_number: &str,
        date_of_birth: &str,
    ) -> Result<(), PatientRepositoryError> {
        // Name validation
        if name.trim().is_empty() {
            return Err(PatientRepositoryError::ValidationError("Name cannot be empty".to_string()));
        }
        
        // Insurance number validation (Swiss format: XXX.XXXX.XXXX.XX) [SC]
        let insurance_regex = regex::Regex::new(r"^\d{3}\.\d{4}\.\d{4}\.\d{2}$").unwrap();
        if !insurance_regex.is_match(insurance_number) {
            return Err(PatientRepositoryError::ValidationError(
                "Invalid insurance number format. Expected: XXX.XXXX.XXXX.XX".to_string()
            ));
        }
        
        // Date of birth validation (Swiss format: DD.MM.YYYY) [SC]
        let date_regex = regex::Regex::new(r"^(0[1-9]|[12][0-9]|3[01])\.(0[1-9]|1[0-2])\.\d{4}$").unwrap();
        if !date_regex.is_match(date_of_birth) {
            return Err(PatientRepositoryError::ValidationError(
                "Invalid date format. Expected: DD.MM.YYYY".to_string()
            ));
        }
        
        Ok(())
    }
}
