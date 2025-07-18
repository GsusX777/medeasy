// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Konfigurationsmodul [SP][ZTS][CAM]
// Stellt Konfigurationen für die Anwendung bereit

use std::path::PathBuf;
use serde::{Serialize, Deserialize};

/// Konfiguration für die MedEasy-Anwendung [CAM]
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Config {
    /// Basisverzeichnis für Anwendungsdaten
    pub data_dir: PathBuf,
    
    /// Dateiname für die Datenbank
    pub database_name: String,
    
    /// Pfad für Sicherungen
    pub backup_dir: PathBuf,
    
    /// Schlüsselrotations-Intervall in Tagen
    pub key_rotation_interval_days: u32,
    
    /// Aktiviert striktere Sicherheitsprüfungen im Produktionsmodus
    pub production_mode: bool,
}

impl Config {
    /// Erstellt eine neue Standardkonfiguration
    pub fn new(data_dir: PathBuf) -> Self {
        let backup_dir = data_dir.join("backups");
        
        Config {
            data_dir,
            database_name: "medeasy.db".to_string(),
            backup_dir,
            key_rotation_interval_days: 90, // Standardwert: 90 Tage
            production_mode: false,
        }
    }
    
    /// Erstellt eine Testkonfiguration mit temporärem Pfad [SP][TR]
    pub fn for_testing(temp_dir: PathBuf) -> Self {
        let backup_dir = temp_dir.join("backups");
        
        Config {
            data_dir: temp_dir,
            database_name: "medeasy_test.db".to_string(),
            backup_dir,
            key_rotation_interval_days: 1, // Kurzer Zeitraum für Tests
            production_mode: false, // Tests laufen im Nicht-Produktionsmodus
        }
    }
    
    /// Gibt den vollständigen Pfad zur Datenbank zurück
    pub fn database_path(&self) -> PathBuf {
        self.data_dir.join(&self.database_name)
    }
    
    /// Gibt den vollständigen Pfad zum Schlüsselverzeichnis zurück
    pub fn keys_path(&self) -> PathBuf {
        self.data_dir.join("keys")
    }
    
    /// Gibt den vollständigen Pfad zum Audit-Log zurück
    pub fn audit_log_path(&self) -> PathBuf {
        self.data_dir.join("logs").join("audit.log")
    }
}
