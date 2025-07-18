// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Database Connection Manager [SP] [AIU]
// Implements secure database connections with SQLCipher encryption

use rusqlite::{Connection, Result, Error};
use std::env;
use r2d2::{Pool, PooledConnection};
use r2d2_sqlite::SqliteConnectionManager;
use log::{info, error, debug};
use std::sync::Arc;
use thiserror::Error;
use base64::Engine; // Für Base64-Dekodierung des Schlüssels [SP]
// use base64::engine::general_purpose; // Wird nicht verwendet

#[derive(Error, Debug)]
pub enum DatabaseError {
    #[error("Database connection error: {0}")]
    ConnectionError(String),
    
    #[error("SQL error: {0}")]
    SqlError(#[from] rusqlite::Error),
    
    #[error("Pool error: {0}")]
    PoolError(#[from] r2d2::Error),
    
    #[error("Environment error: {0}")]
    EnvError(String),
}

/// Database manager that handles encrypted and unencrypted connections
/// [SP] SQLCipher Pflicht - Ensures all patient data is encrypted at rest
#[derive(Debug)] // Für Debug-Ausgaben in Tests [ZTS]
pub struct DatabaseManager {
    pool: Arc<Pool<SqliteConnectionManager>>,
    is_encrypted: bool,
}

impl Clone for DatabaseManager {
    fn clone(&self) -> Self {
        Self {
            pool: Arc::clone(&self.pool),
            is_encrypted: self.is_encrypted,
        }
    }
}

impl DatabaseManager {
    /// Erstellt einen neuen DatabaseManager mit den Standardeinstellungen
    pub fn new() -> Result<Self, DatabaseError> {
        Self::new_with_production_flag(false)
    }
    
    /// Erstellt einen neuen DatabaseManager mit expliziter Angabe, ob es sich um eine Produktionsumgebung handelt
    /// In Produktionsumgebungen wird die Verschlüsselung erzwungen [SP][ZTS]
    pub fn new_with_production_flag(is_production: bool) -> Result<Self, DatabaseError> {
        // Datenbank-Pfad aus Umgebungsvariablen oder Standard verwenden
        let db_path = env::var("DATABASE_URL")
            .unwrap_or_else(|_| "medeasy.db".to_string());
        
        // Überprüfen, ob die Umgebungsvariable USE_ENCRYPTION gesetzt ist
        let env_encryption_setting = env::var("USE_ENCRYPTION")
            .unwrap_or_else(|_| "false".to_string())
            .parse::<bool>()
            .unwrap_or(false);
            
        // In Produktion muss Verschlüsselung aktiv sein [SP][ZTS]
        if is_production && !env_encryption_setting {
            error!("SECURITY VIOLATION: Attempting to disable encryption in production environment!");
            return Err(DatabaseError::EnvError(
                "Unencrypted database not allowed in production. Set USE_ENCRYPTION=true".to_string()
            ));
        }
        
        // Determine if encryption should be used based on environment
        let use_encryption = if !is_production {
            env_encryption_setting
        } else {
            true // In Produktion immer verschlüsseln
        };
        
        debug!("Initializing database at {} with encryption={}", db_path, use_encryption);
        
        // Create the appropriate connection manager
        let manager = if use_encryption {
            Self::create_encrypted_manager(&db_path)?
        } else {
            if is_production {
                error!("SECURITY WARNING: Attempting to use unencrypted database in production!");
                return Err(DatabaseError::EnvError(
                    "Unencrypted database not allowed in production".to_string()
                ));
            }
            Self::create_unencrypted_manager(&db_path)?
        };
        
        // Create connection pool
        let pool = Pool::builder()
            .max_size(10)
            .build(manager)
            .map_err(|e| DatabaseError::PoolError(e))?;
        
        // Test connection
        let conn = pool.get()
            .map_err(|e| DatabaseError::ConnectionError(e.to_string()))?;
        
        if use_encryption {
            // Verify encryption is working - Verwende query_row statt execute [ZTS]
            let _: String = conn.query_row("PRAGMA cipher_version", [], |row| row.get(0))
                .map_err(|e| DatabaseError::SqlError(e))?;
            info!("SQLCipher encryption verified successfully");
        }
        
        Ok(DatabaseManager {
            pool: Arc::new(pool),
            is_encrypted: use_encryption,
        })
    }
    
    /// Creates a connection manager for encrypted database [SP]
    fn create_encrypted_manager(db_path: &str) -> Result<SqliteConnectionManager, DatabaseError> {
        // Get encryption key from environment
        let key_base64 = env::var("MEDEASY_DB_KEY")
            .map_err(|_| DatabaseError::EnvError("MEDEASY_DB_KEY must be set for encrypted database".to_string()))?;
        
        // Decode Base64-kodierten Schlüssel [SP]
        let key_bytes = base64::engine::general_purpose::STANDARD
            .decode(&key_base64)
            .map_err(|e| DatabaseError::EnvError(format!("Invalid Base64 key: {}", e)))?;
        
        // Überprüfe Schlüssellänge (muss 32 Bytes für AES-256 sein) [SP]
        if key_bytes.len() != 32 {
            return Err(DatabaseError::EnvError(
                format!("Invalid key length: {}, must be 32 bytes", key_bytes.len())
            ));
        }
        
        // Create SQLite connection manager with encryption setup
        let manager = SqliteConnectionManager::file(db_path);
        let manager = manager.with_init(move |conn| {
            // Set encryption key (sicher, ohne SQL-Injection) [ZTS]
            // Konvertiere Bytes zu Hex-String ohne externe Abhängigkeit
            let hex_key = key_bytes.iter()
                .map(|b| format!("{:02x}", b))
                .collect::<String>();
            conn.execute_batch(&format!("PRAGMA key = \"x'{}'\";\n", hex_key))?;
            
            // Configure SQLCipher for strong security
            // Alle PRAGMAs mit pragma_update, um ExecuteReturnedResults-Fehler zu vermeiden [ZTS]
            conn.pragma_update(None, "cipher_page_size", &4096)?;
            conn.pragma_update(None, "kdf_iter", &256000)?;
            conn.pragma_update(None, "cipher_hmac_algorithm", &"HMAC_SHA256")?;
            conn.pragma_update(None, "cipher_kdf_algorithm", &"PBKDF2_HMAC_SHA256")?;
            // Zusätzlich memory_security aktivieren für die Tests
            conn.pragma_update(None, "cipher_memory_security", &1)?;
            
            // Performance settings - PRAGMA journal_mode gibt Ergebnisse zurück [ZTS]
            // Verwende query_row statt pragma_update für PRAGMAs, die Ergebnisse zurückgeben
            let _: String = conn.query_row("PRAGMA journal_mode = WAL", [], |row| row.get(0))?;
            conn.pragma_update(None, "synchronous", &"NORMAL")?;
            
            Ok(())
        });
        
        Ok(manager)
    }
    
    /// Creates a connection manager for unencrypted database (development only)
    fn create_unencrypted_manager(db_path: &str) -> Result<SqliteConnectionManager, DatabaseError> {
        if !cfg!(debug_assertions) {
            error!("SECURITY WARNING: Attempting to create unencrypted database in production!");
        }
        
        // Create standard SQLite connection manager
        let manager = SqliteConnectionManager::file(db_path);
        let manager = manager.with_init(|conn| {
            // Performance settings - PRAGMA journal_mode gibt Ergebnisse zurück [ZTS]
            // Verwende query_row statt pragma_update für PRAGMAs, die Ergebnisse zurückgeben
            let _: String = conn.query_row("PRAGMA journal_mode = WAL", [], |row| row.get(0))?;
            conn.pragma_update(None, "synchronous", &"NORMAL")?;
            
            Ok(())
        });
        
        Ok(manager)
    }
    
    /// Gets a connection from the pool
    pub fn get_connection(&self) -> Result<PooledConnection<SqliteConnectionManager>, DatabaseError> {
        self.pool.get().map_err(|e| DatabaseError::PoolError(e))
    }
    
    /// Returns whether the database is encrypted
    pub fn is_encrypted(&self) -> bool {
        self.is_encrypted
    }
    
    /// Executes a function with a database connection
    pub fn with_connection<F, T>(&self, f: F) -> Result<T, DatabaseError>
    where
        F: FnOnce(&Connection) -> Result<T, Error>,
    {
        let conn = self.get_connection()?;
        f(&conn).map_err(|e| DatabaseError::SqlError(e))
    }
    
    /// Executes a function with a mutable database connection [ZTS]
    /// Notwendig für Transaktionen und andere Operationen, die eine mutable Verbindung benötigen
    pub fn with_connection_mut<F, T>(&self, f: F) -> Result<T, DatabaseError>
    where
        F: FnOnce(&mut Connection) -> Result<T, Error>,
    {
        let mut conn = self.get_connection()?;
        f(&mut conn).map_err(|e| DatabaseError::SqlError(e))
    }
    
    /// Führt Datenbankmigrationen aus [SP]
    /// Erstellt alle notwendigen Tabellen für die Anwendung
    pub fn run_migrations(&mut self) -> Result<(), DatabaseError> {
        debug!("Führe Datenbankmigrationen aus");
        let conn = self.get_connection()?;
        
        // Erstelle Migrations-Tabelle, falls sie nicht existiert [SP][ATV]
        conn.execute(
            "CREATE TABLE IF NOT EXISTS __migrations (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                applied_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )",
            [],
        ).map_err(|e| DatabaseError::SqlError(e))?;
        
        // Erstelle Patienten-Tabelle mit Verschlüsselung [EIV][SP]
        conn.execute(
            "CREATE TABLE IF NOT EXISTS patients (
                id TEXT PRIMARY KEY,
                encrypted_name TEXT NOT NULL,
                insurance_number_hash TEXT NOT NULL,
                date_of_birth TEXT NOT NULL,
                created TEXT NOT NULL,
                created_by TEXT NOT NULL,
                last_modified TEXT NOT NULL,
                last_modified_by TEXT NOT NULL
            )",
            [],
        ).map_err(|e| DatabaseError::SqlError(e))?;
        
        // Erstelle Sessions-Tabelle [SP]
        conn.execute(
            "CREATE TABLE IF NOT EXISTS sessions (
                id TEXT PRIMARY KEY,
                patient_id TEXT NOT NULL,
                session_date TEXT NOT NULL,     -- Schweizer Format: DD.MM.YYYY [SF]
                start_time TEXT NOT NULL,        -- Beginn der Konsultation
                end_time TEXT,                   -- Ende der Konsultation (kann NULL sein)
                status TEXT NOT NULL,            -- Scheduled, InProgress, Completed, Cancelled
                encrypted_notes TEXT,            -- Verschlüsselte Konsultationsnotizen [EIV]
                encrypted_audio_reference TEXT,  -- Verschlüsselte Audiodateireferenz [EIV]
                created TEXT NOT NULL,
                created_by TEXT NOT NULL,
                last_modified TEXT NOT NULL,
                last_modified_by TEXT NOT NULL,
                FOREIGN KEY (patient_id) REFERENCES patients (id)
            )",
            [],
        ).map_err(|e| DatabaseError::SqlError(e))?;
        
        // Erstelle Transkript-Tabelle mit Verschlüsselung und Anonymisierung [EIV][AIU]
        conn.execute(
            "CREATE TABLE IF NOT EXISTS transcripts (
                id TEXT PRIMARY KEY,
                session_id TEXT NOT NULL,
                encrypted_original_text TEXT NOT NULL,
                encrypted_anonymized_text TEXT NOT NULL,
                is_anonymized INTEGER NOT NULL DEFAULT 1, -- Flag, ob anonymisiert (sollte immer 1 sein) [AIU]
                anonymization_confidence REAL NOT NULL,
                needs_review INTEGER NOT NULL DEFAULT 0,
                created TEXT NOT NULL,
                created_by TEXT NOT NULL,
                last_modified TEXT NOT NULL,
                last_modified_by TEXT NOT NULL,
                FOREIGN KEY (session_id) REFERENCES sessions (id)
            )",
            [],
        ).map_err(|e| DatabaseError::SqlError(e))?;
        
        // Erstelle Audit-Log-Tabelle [ATV]
        conn.execute(
            "CREATE TABLE IF NOT EXISTS audit_logs (
                id TEXT PRIMARY KEY,
                entity_name TEXT NOT NULL,
                entity_id TEXT NOT NULL,
                action TEXT NOT NULL,
                changes TEXT,
                contains_sensitive_data INTEGER NOT NULL DEFAULT 0,
                timestamp TEXT NOT NULL,
                user_id TEXT NOT NULL
            )",
            [],
        ).map_err(|e| DatabaseError::SqlError(e))?;
        
        // Erstelle AnonymizationReviewItem-Tabelle für menschliche Überprüfung [AIU][ARQ]
        conn.execute(
            "CREATE TABLE IF NOT EXISTS anonymization_review_items (
                id TEXT PRIMARY KEY,
                transcript_id TEXT NOT NULL,
                status TEXT NOT NULL,
                detected_pii TEXT,
                anonymization_confidence REAL NOT NULL,
                review_reason TEXT,
                reviewer_notes TEXT,
                created TEXT NOT NULL,
                created_by TEXT NOT NULL,
                last_modified TEXT NOT NULL,
                last_modified_by TEXT NOT NULL,
                FOREIGN KEY (transcript_id) REFERENCES transcripts (id)
            )",
            [],
        ).map_err(|e| DatabaseError::SqlError(e))?;
        
        info!("Datenbankmigrationen erfolgreich ausgeführt");
        Ok(())
    }
}
