// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Audit-Logger Modul [ATV][ZTS][SP]
// Implementiert die Audit-Protokollierung für alle sicherheitsrelevanten Operationen

use std::fs::{self, OpenOptions};
use std::io::Write;
use std::path::Path;
use std::sync::{Arc, Mutex};
use chrono::Utc;
use log::{debug, error};
use serde::{Serialize, Deserialize};
use uuid::Uuid;
use std::fmt;

/// Verfügbare Audit-Aktionstypen [ATV]
#[derive(Debug, Clone, Serialize, Deserialize)]
pub enum AuditAction {
    Login,
    Logout,
    DataAccess,
    DataModification,
    KeyCreation,
    KeyRotation,
    KeyAccess,
    ConfigurationChange,
    SecurityEvent,
    SystemEvent,
}

impl fmt::Display for AuditAction {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        match self {
            AuditAction::Login => write!(f, "LOGIN"),
            AuditAction::Logout => write!(f, "LOGOUT"),
            AuditAction::DataAccess => write!(f, "DATA_ACCESS"),
            AuditAction::DataModification => write!(f, "DATA_MODIFICATION"),
            AuditAction::KeyCreation => write!(f, "KEY_CREATION"),
            AuditAction::KeyRotation => write!(f, "KEY_ROTATION"),
            AuditAction::KeyAccess => write!(f, "KEY_ACCESS"),
            AuditAction::ConfigurationChange => write!(f, "CONFIGURATION_CHANGE"),
            AuditAction::SecurityEvent => write!(f, "SECURITY_EVENT"),
            AuditAction::SystemEvent => write!(f, "SYSTEM_EVENT"),
        }
    }
}

/// AuditLogger für alle sicherheitsrelevanten Aktivitäten [ATV]
#[derive(Debug)]
pub struct AuditLogger {
    log_path: String,
    log_file: Arc<Mutex<Option<std::fs::File>>>,
}

impl AuditLogger {
    /// Erstellt einen neuen AuditLogger mit dem angegebenen Pfad
    pub fn new(log_path: &str) -> Self {
        // Stelle sicher, dass das Verzeichnis existiert
        if let Some(parent) = Path::new(log_path).parent() {
            if !parent.exists() {
                if let Err(e) = fs::create_dir_all(parent) {
                    error!("Fehler beim Erstellen des Audit-Log-Verzeichnisses: {}", e);
                }
            }
        }

        // Öffne die Logdatei im Append-Modus
        let file = match OpenOptions::new()
            .create(true)
            .append(true)
            .open(log_path) 
        {
            Ok(file) => Some(file),
            Err(e) => {
                error!("Fehler beim Öffnen der Audit-Log-Datei: {}", e);
                None
            }
        };

        AuditLogger {
            log_path: log_path.to_string(),
            log_file: Arc::new(Mutex::new(file)),
        }
    }

    /// Protokolliert eine Audit-Aktion [ATV]
    pub fn log(&self, action: AuditAction, message: &str, user_id: Option<&str>) -> Result<(), String> {
        let timestamp = Utc::now();
        let event_id = Uuid::new_v4();
        let user = user_id.unwrap_or("SYSTEM");

        let log_entry = format!(
            "{} | {} | {} | {} | {}\n",
            timestamp.to_rfc3339(),
            event_id,
            user,
            action,
            message
        );

        debug!("Audit-Log-Eintrag: {}", log_entry.trim());

        if let Some(mut file) = self.log_file.lock().unwrap().as_ref() {
            if let Err(e) = file.write_all(log_entry.as_bytes()) {
                let error_msg = format!("Fehler beim Schreiben des Audit-Logs: {}", e);
                error!("{}", error_msg);
                return Err(error_msg);
            }
            
            if let Err(e) = file.flush() {
                let error_msg = format!("Fehler beim Flushen des Audit-Logs: {}", e);
                error!("{}", error_msg);
                return Err(error_msg);
            }
        } else {
            let error_msg = "Audit-Log-Datei ist nicht geöffnet".to_string();
            error!("{}", error_msg);
            return Err(error_msg);
        }

        Ok(())
    }

    /// Gibt den Pfad der Audit-Log-Datei zurück
    pub fn get_log_path(&self) -> &str {
        &self.log_path
    }
}
