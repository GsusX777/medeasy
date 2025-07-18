// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// [SP] SQLCipher Pflicht - Tauri Integration
// [AIU] Anonymisierung ist UNVERÄNDERLICH
// [CT] Cloud-Transparenz - UI zeigt immer Datenverarbeitungsort
// [ATV] Audit-Trail Vollständig
// [ZTS] Zero Tolerance Security - Schlüsselverwaltung
// [FSD] Fail-Safe Defaults - Notfallwiederherstellung

#![cfg_attr(
    all(not(debug_assertions), target_os = "windows"),
    windows_subsystem = "windows"
)]

mod commands;
mod config; // Neu hinzugefügt [CAM][SP]
mod database;
mod repositories;
mod security;

use commands::*;
use database::connection::DatabaseManager;
use repositories::{
    PatientRepository,
    SessionRepository,
    TranscriptRepository,
    AuditRepository
};
use security::key_manager::{KeyManager, KeyStore};
use std::sync::{Arc, Mutex};
use dotenv::dotenv;
use log::{info, warn, error, LevelFilter};
use env_logger::Builder;
use std::io::Write;
use std::path::PathBuf;

// Typdefinitionen für Tauri-States
type DatabaseState = Mutex<DatabaseManager>;
type PatientRepoState = Mutex<PatientRepository>;
type SessionRepoState = Mutex<SessionRepository>;
type TranscriptRepoState = Mutex<TranscriptRepository>;
type AuditRepoState = Mutex<AuditRepository>;

fn main() {
    // Initialize environment variables from .env file
    match dotenv() {
        Ok(_) => info!("Loaded environment variables from .env file"),
        Err(e) => warn!("Failed to load .env file: {}", e),
    }
    
    // Setup logging
    Builder::new()
        .format(|buf, record| {
            writeln!(
                buf,
                "{} [{}] - {}",
                chrono::Local::now().format("%Y-%m-%d %H:%M:%S"),
                record.level(),
                record.args()
            )
        })
        .filter(None, LevelFilter::Info)
        .init();
    
    info!("Starting MedEasy application");
    
    // Initialize database and repositories
    let db_manager = match DatabaseManager::new() {
        Ok(manager) => {
            info!("Database manager initialized successfully");
            manager
        },
        Err(e) => {
            error!("Failed to initialize database manager: {}", e);
            panic!("Database initialization failed: {}", e);
        }
    };
    
    // Initialize schema if needed
    if let Err(e) = db_manager.initialize_schema() {
        error!("Failed to initialize database schema: {}", e);
        panic!("Schema initialization failed: {}", e);
    }
    
    // Create repositories
    let patient_repo = match PatientRepository::new(db_manager.clone()) {
        Ok(repo) => repo,
        Err(e) => {
            error!("Failed to create patient repository: {}", e);
            panic!("Repository initialization failed: {}", e);
        }
    };
    
    let session_repo = match SessionRepository::new(db_manager.clone()) {
        Ok(repo) => repo,
        Err(e) => {
            error!("Failed to create session repository: {}", e);
            panic!("Repository initialization failed: {}", e);
        }
    };
    
    let transcript_repo = match TranscriptRepository::new(db_manager.clone()) {
        Ok(repo) => repo,
        Err(e) => {
            error!("Failed to create transcript repository: {}", e);
            panic!("Repository initialization failed: {}", e);
        }
    };
    
    let audit_repo = AuditRepository::new(db_manager.clone());
    
    // Initialize key manager [ZTS][SP]
    let app_data_dir = std::env::var("MEDEASY_DATA_DIR")
        .map(PathBuf::from)
        .unwrap_or_else(|_| {
            let home = dirs::home_dir().expect("Could not find home directory");
            home.join(".medeasy")
        });
    
    // Ensure directory exists
    std::fs::create_dir_all(&app_data_dir)
        .expect("Failed to create app data directory");
    
    let keystore_path = app_data_dir.join("keystore.json");
    let audit_logger = Arc::new(audit_repo.clone());
    
    let key_manager = match KeyManager::new(keystore_path.clone(), audit_logger.clone()) {
        Ok(manager) => {
            info!("Key manager initialized successfully");
            manager
        },
        Err(e) => {
            error!("Failed to initialize key manager: {}", e);
            panic!("Key manager initialization failed: {}", e);
        }
    };
    
    // Create application state
    let key_store = Arc::new(Mutex::new(None::<KeyStore>));
    let app_state = commands::key_management::AppState {
        key_manager: Arc::new(key_manager),
        key_store,
        audit_logger,
    };
    
    info!("All repositories and security components initialized successfully");
    
    tauri::Builder::default()
        // Register all commands from the commands module
        .invoke_handler(tauri::generate_handler![
            // Existing commands
            get_app_info,
            get_processing_location,
            
            // Database commands
            initialize_database,
            
            // Patient commands
            create_patient,
            get_patient,
            list_patients,
            
            // Session commands
            create_session,
            get_session,
            list_patient_sessions,
            
            // Transcript commands
            create_transcript,
            get_session_transcripts,
            
            // Audit commands
            get_entity_audit_logs,
            
            // Key management commands [ZTS][SP][FSD]
            commands::key_management::initialize_key_manager,
            commands::key_management::get_key_status,
            commands::key_management::rotate_key,
            commands::key_management::change_master_password,
            commands::key_management::create_recovery_data,
            commands::key_management::create_shamir_shares
        ])
        .manage(Mutex::new(db_manager) as DatabaseState)
        .manage(Mutex::new(patient_repo) as PatientRepoState)
        .manage(Mutex::new(session_repo) as SessionRepoState)
        .manage(Mutex::new(transcript_repo) as TranscriptRepoState)
        .manage(Mutex::new(audit_repo) as AuditRepoState)
        .manage(app_state)
        .setup(|app| {
            // [ATV] Audit-Trail Vollständig - Setup logging
            let app_dir = app.path_resolver().app_data_dir().expect("Failed to get app data directory");
            std::fs::create_dir_all(&app_dir).expect("Failed to create app data directory");
            
            // [CT] Cloud-Transparenz - Default to local processing
            let config_file = app_dir.join("config.json");
            if !config_file.exists() {
                let default_config = serde_json::json!({
                    "processing_location": "local", // Default to local processing
                    "cloud_consent": false,         // Require explicit consent for cloud
                    "anonymization_enabled": true,  // [AIU] Cannot be disabled
                    "audit_enabled": true,          // [ATV] Cannot be disabled
                });
                std::fs::write(
                    &config_file,
                    serde_json::to_string_pretty(&default_config).unwrap(),
                ).expect("Failed to write default config");
            }
            
            info!("MedEasy application setup completed");
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("Error while running MedEasy application");
}
