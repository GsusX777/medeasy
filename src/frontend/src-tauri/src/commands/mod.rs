// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// [CT] Cloud-Transparenz - Zeigt immer Datenverarbeitungsort
// [AIU] Anonymisierung ist UNVERÄNDERLICH - Keine Deaktivierungsmöglichkeit
// [ATV] Audit-Trail Vollständig - Jede Operation wird geloggt
// [ZTS] Zero Tolerance Security - Schlüsselverwaltung
// [FSD] Fail-Safe Defaults - Notfallwiederherstellung

use serde::{Deserialize, Serialize};
use std::path::PathBuf;
use tauri::AppHandle;

// Module exports
pub mod key_management;

#[derive(Serialize, Deserialize, Debug)]
pub struct AppInfo {
    version: String,
    anonymization_enabled: bool, // [AIU] Immer true, kann nicht deaktiviert werden
    processing_location: String, // [CT] "local" oder "cloud"
    audit_enabled: bool,         // [ATV] Immer true, kann nicht deaktiviert werden
}

/// Gibt Informationen über die App zurück
/// [CT] Zeigt immer an, ob lokal oder in der Cloud verarbeitet wird
#[tauri::command]
pub fn get_app_info(app_handle: AppHandle) -> Result<AppInfo, String> {
    // Audit-Log für diese Operation
    log_operation("get_app_info", "Abfrage der App-Informationen").map_err(|e| e.to_string())?;
    
    let config = read_config(&app_handle).map_err(|e| e.to_string())?;
    
    Ok(AppInfo {
        version: env!("CARGO_PKG_VERSION").to_string(),
        // [AIU] Anonymisierung ist UNVERÄNDERLICH - Immer aktiviert
        anonymization_enabled: true,
        // [CT] Cloud-Transparenz - Zeigt Verarbeitungsort
        processing_location: config["processing_location"].as_str().unwrap_or("local").to_string(),
        // [ATV] Audit-Trail Vollständig - Immer aktiviert
        audit_enabled: true,
    })
}

/// [CT] Gibt den aktuellen Verarbeitungsort zurück (lokal oder Cloud)
#[tauri::command]
pub fn get_processing_location(app_handle: AppHandle) -> Result<String, String> {
    // Audit-Log für diese Operation
    log_operation("get_processing_location", "Abfrage des Verarbeitungsorts").map_err(|e| e.to_string())?;
    
    let config = read_config(&app_handle).map_err(|e| e.to_string())?;
    Ok(config["processing_location"].as_str().unwrap_or("local").to_string())
}

// Helper-Funktionen

fn read_config(app_handle: &AppHandle) -> Result<serde_json::Value, std::io::Error> {
    let app_dir = app_handle.path_resolver().app_data_dir().expect("Failed to get app data directory");
    let config_file = app_dir.join("config.json");
    
    let config_content = std::fs::read_to_string(config_file)?;
    Ok(serde_json::from_str(&config_content)?)
}

// [ATV] Audit-Trail Vollständig - Jede Operation wird geloggt
fn log_operation(operation: &str, description: &str) -> Result<(), std::io::Error> {
    // In einer echten Implementierung würde hier ein strukturiertes Logging-System verwendet
    // und die Logs würden verschlüsselt gespeichert werden
    println!("[AUDIT] Operation: {}, Description: {}, Time: {:?}", 
             operation, description, std::time::SystemTime::now());
    Ok(())
}
