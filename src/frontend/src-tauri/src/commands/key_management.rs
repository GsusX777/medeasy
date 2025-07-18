// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Schlüsselverwaltungs-Befehle [SP][ZTS][EIV]
// Implementiert Tauri-Befehle für die Schlüsselverwaltung

use std::sync::Arc;
use tauri::{command, State};
use serde::{Serialize, Deserialize};
use chrono::{DateTime, Utc};
use log::{info, error};
use uuid::Uuid;

use crate::security::key_manager::{KeyManager, KeyType, KeyRotationStatus};
use crate::database::audit::{AuditLogger, AuditAction};
use crate::state::AppState;

// Schlüsselstatus für die Benutzeroberfläche [CT]
#[derive(Debug, Clone, Serialize)]
pub struct KeyStatus {
    pub key_type: String,
    pub last_rotated: Option<String>,
    pub days_until_rotation: i64,
    pub status: String,
    pub version: u32,
}

// Ergebnis der Schlüsselrotation [ATV]
#[derive(Debug, Clone, Serialize)]
pub struct KeyRotationResult {
    pub success: bool,
    pub message: String,
    pub timestamp: String,
}

// Parameter für Shamir-Shares [FSD]
#[derive(Debug, Clone, Deserialize)]
pub struct ShamirParams {
    pub threshold: u8,
    pub shares: u8,
}

// Initialisiert die Schlüsselverwaltung mit einem Passwort [ZTS]
#[command]
pub async fn initialize_key_manager(
    password: String,
    state: State<'_, AppState>,
) -> Result<bool, String> {
    let key_manager = state.key_manager.clone();
    
    // Initialisiere Schlüsselmanager
    key_manager.initialize(&password)
        .map_err(|e| e.to_string())
}

// Holt den Status aller Schlüssel [CT]
#[command]
pub async fn get_key_status(
    state: State<'_, AppState>,
) -> Result<Vec<KeyStatus>, String> {
    let key_manager = state.key_manager.clone();
    let now = Utc::now();
    
    // Schlüsseltypen, die wir überprüfen wollen
    let key_types = vec![
        KeyType::Database,
        KeyType::FieldPatient,
        KeyType::FieldSession,
        KeyType::FieldTranscript,
        KeyType::Backup,
    ];
    
    // Sammle Status für jeden Schlüssel
    let mut results = Vec::new();
    
    for key_type in key_types {
        // Hole Rotationsstatus
        let rotation_status = key_manager.check_rotation_status(key_type)
            .map_err(|e| e.to_string())?;
            
        // Konvertiere zu benutzerfreundlichem Status
        let status_str = match rotation_status {
            KeyRotationStatus::UpToDate => "Aktuell".to_string(),
            KeyRotationStatus::DueSoon => "Bald fällig".to_string(),
            KeyRotationStatus::Overdue => "Überfällig".to_string(),
            KeyRotationStatus::Unknown => "Unbekannt".to_string(),
        };
        
        // Hole Metadaten für diesen Schlüssel
        let store = state.key_store.lock().unwrap();
        let key_store = store.as_ref()
            .ok_or("Schlüsselspeicher nicht initialisiert".to_string())?;
            
        let metadata = key_store.key_metadata.get(&key_type)
            .ok_or(format!("Keine Metadaten für Schlüssel {:?}", key_type))?;
            
        // Berechne Tage bis zur Rotation
        let days_until_rotation = (metadata.rotation_due - now).num_days();
        
        // Formatiere letztes Rotationsdatum
        let last_rotated = metadata.last_rotated.map(|dt| {
            dt.format("%d.%m.%Y %H:%M").to_string() // Schweizer Datumsformat [SC]
        });
        
        // Konvertiere Schlüsseltyp zu benutzerfreundlichem String
        let key_type_str = match key_type {
            KeyType::Database => "Datenbank".to_string(),
            KeyType::FieldPatient => "Patient".to_string(),
            KeyType::FieldSession => "Session".to_string(),
            KeyType::FieldTranscript => "Transkript".to_string(),
            KeyType::Backup => "Backup".to_string(),
            _ => format!("{:?}", key_type),
        };
        
        // Füge Status hinzu
        results.push(KeyStatus {
            key_type: key_type_str,
            last_rotated,
            days_until_rotation,
            status: status_str,
            version: metadata.version,
        });
    }
    
    Ok(results)
}

// Rotiert einen Schlüssel [SP][ATV]
#[command]
pub async fn rotate_key(
    key_type_str: String,
    user_id: String,
    state: State<'_, AppState>,
) -> Result<KeyRotationResult, String> {
    let key_manager = state.key_manager.clone();
    let audit_logger = state.audit_logger.clone();
    
    // Konvertiere String zu KeyType
    let key_type = match key_type_str.as_str() {
        "Datenbank" => KeyType::Database,
        "Patient" => KeyType::FieldPatient,
        "Session" => KeyType::FieldSession,
        "Transkript" => KeyType::FieldTranscript,
        "Backup" => KeyType::Backup,
        _ => return Err(format!("Ungültiger Schlüsseltyp: {}", key_type_str)),
    };
    
    // Protokolliere Rotationsversuch [ATV]
    let operation_id = Uuid::new_v4().to_string();
    audit_logger.log(
        "key_rotation",
        &operation_id,
        AuditAction::Update,
        &format!("Schlüsselrotation angefordert: {}", key_type_str),
        false,
    );
    
    // Führe Rotation durch
    match key_manager.rotate_key(key_type, &user_id) {
        Ok(_) => {
            // Erfolgreiche Rotation
            let now = Utc::now();
            let timestamp = now.format("%d.%m.%Y %H:%M:%S").to_string(); // Schweizer Format [SC]
            
            Ok(KeyRotationResult {
                success: true,
                message: format!("Schlüssel '{}' erfolgreich rotiert", key_type_str),
                timestamp,
            })
        },
        Err(e) => {
            // Fehler bei der Rotation
            error!("Fehler bei der Schlüsselrotation: {}", e);
            
            // Protokolliere Fehler [ATV]
            audit_logger.log(
                "key_rotation",
                &operation_id,
                AuditAction::Error,
                &format!("Schlüsselrotation fehlgeschlagen: {}", e),
                false,
            );
            
            Err(e.to_string())
        }
    }
}

// Ändert das Master-Passwort [ZTS]
#[command]
pub async fn change_master_password(
    old_password: String,
    new_password: String,
    state: State<'_, AppState>,
) -> Result<bool, String> {
    let key_manager = state.key_manager.clone();
    let audit_logger = state.audit_logger.clone();
    
    // Protokolliere Passwortänderungsversuch (ohne Passwörter!) [ATV]
    let operation_id = Uuid::new_v4().to_string();
    audit_logger.log(
        "key_management",
        &operation_id,
        AuditAction::Update,
        "Master-Passwortänderung angefordert",
        false,
    );
    
    // Führe Passwortänderung durch
    match key_manager.change_master_password(&old_password, &new_password) {
        Ok(_) => {
            // Erfolgreiche Änderung
            audit_logger.log(
                "key_management",
                &operation_id,
                AuditAction::Update,
                "Master-Passwort erfolgreich geändert",
                false,
            );
            
            Ok(true)
        },
        Err(e) => {
            // Fehler bei der Änderung
            error!("Fehler bei der Passwortänderung: {}", e);
            
            // Protokolliere Fehler [ATV]
            audit_logger.log(
                "key_management",
                &operation_id,
                AuditAction::Error,
                &format!("Passwortänderung fehlgeschlagen: {}", e),
                false,
            );
            
            Err(e.to_string())
        }
    }
}

// Erstellt Notfall-Wiederherstellungsdaten [FSD]
#[command]
pub async fn create_recovery_data(
    recovery_password: String,
    state: State<'_, AppState>,
) -> Result<String, String> {
    let key_manager = state.key_manager.clone();
    let audit_logger = state.audit_logger.clone();
    
    // Protokolliere Wiederherstellungsdaten-Erstellung [ATV]
    let operation_id = Uuid::new_v4().to_string();
    audit_logger.log(
        "key_management",
        &operation_id,
        AuditAction::Create,
        "Wiederherstellungsdaten angefordert",
        false,
    );
    
    // Erstelle Wiederherstellungsdaten
    match key_manager.create_recovery_data(&recovery_password) {
        Ok(recovery_data) => {
            // Erfolgreiche Erstellung
            audit_logger.log(
                "key_management",
                &operation_id,
                AuditAction::Create,
                "Wiederherstellungsdaten erfolgreich erstellt",
                false,
            );
            
            Ok(recovery_data)
        },
        Err(e) => {
            // Fehler bei der Erstellung
            error!("Fehler bei der Erstellung von Wiederherstellungsdaten: {}", e);
            
            // Protokolliere Fehler [ATV]
            audit_logger.log(
                "key_management",
                &operation_id,
                AuditAction::Error,
                &format!("Wiederherstellungsdaten-Erstellung fehlgeschlagen: {}", e),
                false,
            );
            
            Err(e.to_string())
        }
    }
}

// Erstellt Shamir-Shares für den Master-Schlüssel [FSD]
#[command]
pub async fn create_shamir_shares(
    params: ShamirParams,
    state: State<'_, AppState>,
) -> Result<Vec<String>, String> {
    let key_manager = state.key_manager.clone();
    let audit_logger = state.audit_logger.clone();
    
    // Protokolliere Shamir-Shares-Erstellung [ATV]
    let operation_id = Uuid::new_v4().to_string();
    audit_logger.log(
        "key_management",
        &operation_id,
        AuditAction::Create,
        &format!("Shamir-Shares angefordert: {}/{}", params.threshold, params.shares),
        false,
    );
    
    // Erstelle Shamir-Shares
    match key_manager.create_shamir_shares(params.threshold, params.shares) {
        Ok(shares) => {
            // Erfolgreiche Erstellung
            audit_logger.log(
                "key_management",
                &operation_id,
                AuditAction::Create,
                &format!("Shamir-Shares erfolgreich erstellt: {}/{}", params.threshold, params.shares),
                false,
            );
            
            Ok(shares)
        },
        Err(e) => {
            // Fehler bei der Erstellung
            error!("Fehler bei der Erstellung von Shamir-Shares: {}", e);
            
            // Protokolliere Fehler [ATV]
            audit_logger.log(
                "key_management",
                &operation_id,
                AuditAction::Error,
                &format!("Shamir-Shares-Erstellung fehlgeschlagen: {}", e),
                false,
            );
            
            Err(e.to_string())
        }
    }
}
