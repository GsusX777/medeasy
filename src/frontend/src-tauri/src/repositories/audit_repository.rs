// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Audit Repository [ATV]
// Implements data access for audit logs with comprehensive tracking

use crate::database::{
    connection::{DatabaseManager, DatabaseError},
    models::AuditLog
};
use rusqlite::{params, Result, Error};
use log::{debug, error}; // info und warn werden nicht verwendet
use std::env;
use thiserror::Error;
// use chrono::Utc;

#[derive(Error, Debug)]
pub enum AuditRepositoryError {
    #[error("Database error: {0}")]
    DatabaseError(#[from] DatabaseError),
    
    #[error("SQL error: {0}")]
    SqlError(#[from] Error),
    
    #[error("Audit log not found: {0}")]
    NotFound(String),
}

/// Repository for audit log operations [ATV]
pub struct AuditRepository {
    db: DatabaseManager,
    enforce_audit: bool,
}

impl AuditRepository {
    /// Creates a new audit repository
    pub fn new(db: DatabaseManager) -> Self {
        // Prüfe, ob Audit-Logging erzwungen werden soll [ATV]
        let enforce_audit = env::var("MEDEASY_ENFORCE_AUDIT")
            .map(|val| val == "true")
            .unwrap_or(false);
            
        Self { db, enforce_audit }
    }
    
    /// Creates a new audit log entry
    pub fn create_audit_log(
        &self,
        entity_name: &str,
        entity_id: &str,
        action: &str,
        changes: Option<&str>,
        contains_sensitive_data: bool,
        user_id: &str
    ) -> Result<AuditLog, AuditRepositoryError> {
        // In Produktionsumgebung muss Audit-Logging funktionieren [ATV][ZTS]
        if self.enforce_audit {
            debug!("Enforced audit logging for {}/{}: {}", entity_name, entity_id, action);
        }
        debug!("Creating audit log for {}/{}: {}", entity_name, entity_id, action);
        
        let audit_log = AuditLog::new(
            entity_name,
            entity_id,
            action,
            changes,
            contains_sensitive_data,
            user_id,
        );
        
        self.db.with_connection(|conn| {
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
        
        Ok(audit_log)
    }
    
    /// Gets audit logs for an entity
    pub fn get_audit_logs_for_entity(
        &self,
        entity_name: &str,
        entity_id: &str,
        limit: i64,
        offset: i64
    ) -> Result<Vec<AuditLog>, AuditRepositoryError> {
        debug!("Getting audit logs for {}/{}", entity_name, entity_id);
        
        let logs = self.db.with_connection(|conn| {
            let mut stmt = conn.prepare(
                "SELECT id, entity_name, entity_id, action, changes,
                        contains_sensitive_data, timestamp, user_id
                 FROM audit_logs 
                 WHERE entity_name = ?1 AND entity_id = ?2
                 ORDER BY timestamp DESC
                 LIMIT ?3 OFFSET ?4"
            )?;
            
            let log_iter = stmt.query_map(
                params![entity_name, entity_id, limit, offset],
                |row| {
                    Ok(AuditLog {
                        id: row.get(0)?,
                        entity_name: row.get(1)?,
                        entity_id: row.get(2)?,
                        action: row.get(3)?,
                        changes: row.get(4)?,
                        contains_sensitive_data: row.get::<_, i32>(5)? != 0,
                        timestamp: row.get(6)?,
                        user_id: row.get(7)?,
                    })
                }
            )?;
            
            let mut logs = Vec::new();
            for log in log_iter {
                logs.push(log?);
            }
            
            Ok(logs)
        })?;
        
        Ok(logs)
    }
    
    /// Gets audit logs for a user
    pub fn get_audit_logs_for_user(
        &self,
        user_id: &str,
        limit: i64,
        offset: i64
    ) -> Result<Vec<AuditLog>, AuditRepositoryError> {
        debug!("Getting audit logs for user: {}", user_id);
        
        let logs = self.db.with_connection(|conn| {
            let mut stmt = conn.prepare(
                "SELECT id, entity_name, entity_id, action, changes,
                        contains_sensitive_data, timestamp, user_id
                 FROM audit_logs 
                 WHERE user_id = ?1
                 ORDER BY timestamp DESC
                 LIMIT ?2 OFFSET ?3"
            )?;
            
            let log_iter = stmt.query_map(
                params![user_id, limit, offset],
                |row| {
                    Ok(AuditLog {
                        id: row.get(0)?,
                        entity_name: row.get(1)?,
                        entity_id: row.get(2)?,
                        action: row.get(3)?,
                        changes: row.get(4)?,
                        contains_sensitive_data: row.get::<_, i32>(5)? != 0,
                        timestamp: row.get(6)?,
                        user_id: row.get(7)?,
                    })
                }
            )?;
            
            let mut logs = Vec::new();
            for log in log_iter {
                logs.push(log?);
            }
            
            Ok(logs)
        })?;
        
        Ok(logs)
    }
    
    /// Gets recent audit logs
    pub fn get_recent_audit_logs(
        &self,
        limit: i64,
        offset: i64,
        sensitive_data: bool
    ) -> Result<Vec<AuditLog>, AuditRepositoryError> {
        debug!("Getting recent audit logs, include_sensitive={}", sensitive_data);
        
        let logs = self.db.with_connection(|conn| {
            let sql = if sensitive_data {
                // Include all logs
                "SELECT id, entity_name, entity_id, action, changes,
                        contains_sensitive_data, timestamp, user_id
                 FROM audit_logs 
                 ORDER BY timestamp DESC
                 LIMIT ? OFFSET ?"
            } else {
                // Exclude logs with sensitive data
                "SELECT id, entity_name, entity_id, action, changes,
                        contains_sensitive_data, timestamp, user_id
                 FROM audit_logs 
                 WHERE contains_sensitive_data = 0
                 ORDER BY timestamp DESC
                 LIMIT ? OFFSET ?"
            };
            
            let mut stmt = conn.prepare(sql)?;
            
            let log_iter = stmt.query_map(
                params![limit, offset],
                |row| {
                    Ok(AuditLog {
                        id: row.get(0)?,
                        entity_name: row.get(1)?,
                        entity_id: row.get(2)?,
                        action: row.get(3)?,
                        changes: row.get(4)?,
                        contains_sensitive_data: row.get::<_, i32>(5)? != 0,
                        timestamp: row.get(6)?,
                        user_id: row.get(7)?,
                    })
                }
            )?;
            
            let mut logs = Vec::new();
            for log in log_iter {
                logs.push(log?);
            }
            
            Ok(logs)
        })?;
        
        Ok(logs)
    }
    
    /// Prüft, ob Audit-Logging erzwungen wird [ATV][ZTS]
    pub fn is_enforced(&self) -> bool {
        self.enforce_audit
    }
    
    /// Gets audit log statistics
    pub fn get_audit_statistics(&self) -> Result<AuditStatistics, AuditRepositoryError> {
        debug!("Getting audit statistics");
        
        let stats = self.db.with_connection(|conn| {
            // Total count
            let total_count: i64 = conn.query_row(
                "SELECT COUNT(*) FROM audit_logs",
                [],
                |row| row.get(0)
            )?;
            
            // Count by action type
            let mut stmt = conn.prepare(
                "SELECT action, COUNT(*) as count
                 FROM audit_logs
                 GROUP BY action
                 ORDER BY count DESC"
            )?;
            
            let action_iter = stmt.query_map([], |row| {
                Ok((row.get::<_, String>(0)?, row.get::<_, i64>(1)?))
            })?;
            
            let mut actions = Vec::new();
            for action in action_iter {
                actions.push(action?);
            }
            
            // Count by entity type
            let mut stmt = conn.prepare(
                "SELECT entity_name, COUNT(*) as count
                 FROM audit_logs
                 GROUP BY entity_name
                 ORDER BY count DESC"
            )?;
            
            let entity_iter = stmt.query_map([], |row| {
                Ok((row.get::<_, String>(0)?, row.get::<_, i64>(1)?))
            })?;
            
            let mut entities = Vec::new();
            for entity in entity_iter {
                entities.push(entity?);
            }
            
            // Count by user
            let mut stmt = conn.prepare(
                "SELECT user_id, COUNT(*) as count
                 FROM audit_logs
                 GROUP BY user_id
                 ORDER BY count DESC"
            )?;
            
            let user_iter = stmt.query_map([], |row| {
                Ok((row.get::<_, String>(0)?, row.get::<_, i64>(1)?))
            })?;
            
            let mut users = Vec::new();
            for user in user_iter {
                users.push(user?);
            }
            
            Ok(AuditStatistics {
                total_count,
                actions,
                entities,
                users,
            })
        })?;
        
        Ok(stats)
    }
}

/// Statistics about audit logs
pub struct AuditStatistics {
    pub total_count: i64,
    pub actions: Vec<(String, i64)>,
    pub entities: Vec<(String, i64)>,
    pub users: Vec<(String, i64)>,
}
