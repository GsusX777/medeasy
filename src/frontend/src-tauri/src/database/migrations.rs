// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Database Migrations [SP] [ATV]
// Handles database schema migrations and versioning

use rusqlite::{Connection, Result, params};
use log::{info, warn, debug};
use std::fs;
use std::path::Path;
use std::io;

/// Current database schema version
const CURRENT_SCHEMA_VERSION: i32 = 1;

/// Manages database migrations
pub struct MigrationManager {
    conn: Connection,
}

impl MigrationManager {
    /// Creates a new migration manager
    pub fn new(conn: Connection) -> Self {
        Self { conn }
    }
    
    /// Ensures the migrations table exists
    fn ensure_migrations_table(&self) -> Result<()> {
        self.conn.execute(
            "CREATE TABLE IF NOT EXISTS schema_migrations (
                version INTEGER PRIMARY KEY,
                applied_at TEXT NOT NULL
            )",
            [],
        )?;
        
        Ok(())
    }
    
    /// Gets the current database version
    fn get_current_version(&self) -> Result<i32> {
        // Ensure migrations table exists
        self.ensure_migrations_table()?;
        
        // Get the highest version
        let version: i32 = self.conn.query_row(
            "SELECT COALESCE(MAX(version), 0) FROM schema_migrations",
            [],
            |row| row.get(0),
        )?;
        
        Ok(version)
    }
    
    /// Records a migration as applied
    fn record_migration(&self, version: i32) -> Result<()> {
        self.conn.execute(
            "INSERT INTO schema_migrations (version, applied_at) VALUES (?1, datetime('now'))",
            params![version],
        )?;
        
        Ok(())
    }
    
    /// Runs all pending migrations
    pub fn run_migrations(&mut self) -> Result<()> {
        let current_version = self.get_current_version()?;
        debug!("Current database schema version: {}", current_version);
        
        if current_version >= CURRENT_SCHEMA_VERSION {
            info!("Database schema is up to date (version {})", current_version);
            return Ok(());
        }
        
        info!("Running migrations from version {} to {}", current_version, CURRENT_SCHEMA_VERSION);
        
        // Begin transaction for all migrations
        let tx = self.conn.transaction()?;
        
        // Apply each migration in order
        for version in (current_version + 1)..=CURRENT_SCHEMA_VERSION {
            debug!("Applying migration version {}", version);
            
            match version {
                1 => migration_001(&tx)?,
                // Add future migrations here
                _ => warn!("Unknown migration version: {}", version),
            }
            
            // Record the migration
            tx.execute(
                "INSERT INTO schema_migrations (version, applied_at) VALUES (?1, datetime('now'))",
                params![version],
            )?;
            
            info!("Applied migration version {}", version);
        }
        
        // Commit all migrations
        tx.commit()?;
        
        info!("Database migrated successfully to version {}", CURRENT_SCHEMA_VERSION);
        Ok(())
    }
    
}

/// Initial schema migration [SP]
fn migration_001(_conn: &Connection) -> Result<()> {
    // This is handled by schema.rs create_tables
    // We just need to record the version
    Ok(())
}
    
    /// Backs up the database before migrations
    pub fn backup_database(db_path: &str) -> io::Result<String> {
        let db_path = Path::new(db_path);
        
        // Skip if database doesn't exist yet
        if !db_path.exists() {
            return Ok("Database does not exist yet, no backup needed".to_string());
        }
        
        // Create backup filename with timestamp
        let timestamp = chrono::Local::now().format("%Y%m%d_%H%M%S");
        let backup_filename = format!("{}.backup_{}", 
            db_path.file_name().unwrap().to_string_lossy(),
            timestamp);
        
        let backup_path = db_path.with_file_name(backup_filename);
        
        // Copy the database file
        fs::copy(db_path, &backup_path)?;
        
        Ok(format!("Database backed up to {}", backup_path.to_string_lossy()))
    }
