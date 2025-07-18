/* „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 */

use log::{debug, error};
use rusqlite::{params, Connection};
use thiserror::Error;
use uuid::Uuid;

use crate::database::connection::DatabaseError;
use crate::database::connection::DatabaseManager;
use crate::database::models::{AnonymizationReviewItem, ReviewStatus};
use crate::repositories::audit_repository::AuditRepository;

/// Fehler, die im AnonymizationReviewRepository auftreten können [ARQ]
#[derive(Error, Debug)]
pub enum AnonymizationReviewRepositoryError {
    /// Datenbankfehler
    #[error("Datenbankfehler: {0}")]
    DatabaseError(#[from] DatabaseError),

    /// SQL-Fehler
    #[error("SQL-Fehler: {0}")]
    SqlError(#[from] rusqlite::Error),

    /// Review-Eintrag nicht gefunden
    #[error("Review-Eintrag nicht gefunden: {0}")]
    NotFound(String),

    /// Ungültiger Status
    #[error("Ungültiger Status: {0}")]
    InvalidStatus(String),

    /// Allgemeiner Fehler
    #[error("Allgemeiner Fehler: {0}")]
    General(String),
}

/// Repository für die Verwaltung von Anonymisierungs-Review-Einträgen [AIU][ARQ]
pub struct AnonymizationReviewRepository {
    db_manager: DatabaseManager,
    audit_repository: AuditRepository,
}

impl AnonymizationReviewRepository {
    /// Erstellt ein neues AnonymizationReviewRepository
    pub fn new(db_manager: DatabaseManager, audit_repository: AuditRepository) -> Self {
        Self {
            db_manager,
            audit_repository,
        }
    }

    /// Erstellt einen neuen Review-Eintrag [AIU]
    pub fn create_review_item(
        &self,
        transcript_id: &str,
        anonymization_confidence: f64,
        review_reason: Option<String>,
        user_id: &str,
    ) -> Result<AnonymizationReviewItem, AnonymizationReviewRepositoryError> {
        debug!("Erstelle neuen Review-Eintrag für Transkript {}", transcript_id);
        
        let review_item = AnonymizationReviewItem::new(
            transcript_id,
            anonymization_confidence,
            review_reason,
            user_id,
        );
        
        let conn = self.db_manager.get_connection()?;
        
        conn.execute(
            "INSERT INTO anonymization_review_items (
                id, transcript_id, status, detected_pii, anonymization_confidence,
                review_reason, reviewer_notes, created, created_by, last_modified, last_modified_by
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
            params![
                review_item.id,
                review_item.transcript_id,
                review_item.status.to_string(),
                review_item.detected_pii,
                review_item.anonymization_confidence,
                review_item.review_reason,
                review_item.reviewer_notes,
                review_item.created,
                review_item.created_by,
                review_item.last_modified,
                review_item.last_modified_by,
            ],
        )?;
        
        // Audit-Log erstellen [ATV]
        self.audit_repository.create_audit_log(
            "anonymization_review_items",
            &review_item.id,
            "CREATE",
            Some(&format!("Review-Eintrag für Transkript {} erstellt", transcript_id)),
            true, // Enthält sensible Daten
            user_id,
        )?;
        
        Ok(review_item)
    }
    
    /// Holt einen Review-Eintrag anhand seiner ID [AIU]
    pub fn get_review_item_by_id(&self, id: &str) -> Result<AnonymizationReviewItem, AnonymizationReviewRepositoryError> {
        debug!("Hole Review-Eintrag mit ID {}", id);
        
        let conn = self.db_manager.get_connection()?;
        
        let mut stmt = conn.prepare(
            "SELECT id, transcript_id, status, detected_pii, anonymization_confidence,
                    review_reason, reviewer_notes, created, created_by, last_modified, last_modified_by
             FROM anonymization_review_items
             WHERE id = ?"
        )?;
        
        let review_item = stmt.query_row(params![id], |row| {
            let status_str: String = row.get(2)?;
            let status = ReviewStatus::from_string(&status_str)
                .map_err(|e| rusqlite::Error::InvalidColumnType(2, e))?;
            
            Ok(AnonymizationReviewItem {
                id: row.get(0)?,
                transcript_id: row.get(1)?,
                status,
                detected_pii: row.get(3)?,
                anonymization_confidence: row.get(4)?,
                review_reason: row.get(5)?,
                reviewer_notes: row.get(6)?,
                created: row.get(7)?,
                created_by: row.get(8)?,
                last_modified: row.get(9)?,
                last_modified_by: row.get(10)?,
            })
        }).map_err(|e| {
            match e {
                rusqlite::Error::QueryReturnedNoRows => AnonymizationReviewRepositoryError::NotFound(id.to_string()),
                _ => AnonymizationReviewRepositoryError::SqlError(e),
            }
        })?;
        
        // Audit-Log erstellen [ATV]
        self.audit_repository.create_audit_log(
            "anonymization_review_items",
            &review_item.id,
            "READ",
            Some(&format!("Review-Eintrag für Transkript {} gelesen", review_item.transcript_id)),
            true, // Enthält sensible Daten
            "system", // Hier sollte eigentlich der aktuelle Benutzer übergeben werden
        )?;
        
        Ok(review_item)
    }
    
    /// Holt alle Review-Einträge für ein Transkript [AIU]
    pub fn get_review_items_by_transcript_id(&self, transcript_id: &str) -> Result<Vec<AnonymizationReviewItem>, AnonymizationReviewRepositoryError> {
        debug!("Hole Review-Einträge für Transkript {}", transcript_id);
        
        let conn = self.db_manager.get_connection()?;
        
        let mut stmt = conn.prepare(
            "SELECT id, transcript_id, status, detected_pii, anonymization_confidence,
                    review_reason, reviewer_notes, created, created_by, last_modified, last_modified_by
             FROM anonymization_review_items
             WHERE transcript_id = ?
             ORDER BY created DESC"
        )?;
        
        let review_items = stmt.query_map(params![transcript_id], |row| {
            let status_str: String = row.get(2)?;
            let status = ReviewStatus::from_string(&status_str)
                .map_err(|e| rusqlite::Error::InvalidColumnType(2, e))?;
            
            Ok(AnonymizationReviewItem {
                id: row.get(0)?,
                transcript_id: row.get(1)?,
                status,
                detected_pii: row.get(3)?,
                anonymization_confidence: row.get(4)?,
                review_reason: row.get(5)?,
                reviewer_notes: row.get(6)?,
                created: row.get(7)?,
                created_by: row.get(8)?,
                last_modified: row.get(9)?,
                last_modified_by: row.get(10)?,
            })
        })?
        .collect::<Result<Vec<_>, _>>()?;
        
        // Audit-Log erstellen [ATV]
        self.audit_repository.create_audit_log(
            "anonymization_review_items",
            transcript_id,
            "READ",
            Some(&format!("Alle Review-Einträge für Transkript {} gelesen", transcript_id)),
            true, // Enthält sensible Daten
            "system", // Hier sollte eigentlich der aktuelle Benutzer übergeben werden
        )?;
        
        Ok(review_items)
    }
    
    /// Aktualisiert den Status eines Review-Eintrags [AIU]
    pub fn update_review_status(
        &self,
        id: &str,
        new_status: ReviewStatus,
        reviewer_notes: Option<String>,
        user_id: &str,
    ) -> Result<AnonymizationReviewItem, AnonymizationReviewRepositoryError> {
        debug!("Aktualisiere Status für Review-Eintrag {} auf {:?}", id, new_status);
        
        let conn = self.db_manager.get_connection()?;
        let now = chrono::Utc::now().to_rfc3339();
        
        // Prüfe, ob der Review-Eintrag existiert
        let current_item = self.get_review_item_by_id(id)?;
        
        conn.execute(
            "UPDATE anonymization_review_items
             SET status = ?, reviewer_notes = ?, last_modified = ?, last_modified_by = ?
             WHERE id = ?",
            params![
                new_status.to_string(),
                reviewer_notes,
                now,
                user_id,
                id,
            ],
        )?;
        
        // Hole den aktualisierten Eintrag
        let updated_item = self.get_review_item_by_id(id)?;
        
        // Audit-Log erstellen [ATV]
        self.audit_repository.create_audit_log(
            "anonymization_review_items",
            id,
            "UPDATE",
            Some(&format!(
                "Status für Review-Eintrag von {:?} auf {:?} geändert",
                current_item.status, new_status
            )),
            true, // Enthält sensible Daten
            user_id,
        )?;
        
        Ok(updated_item)
    }
    
    /// Aktualisiert die erkannten PIIs eines Review-Eintrags [AIU]
    pub fn update_detected_pii(
        &self,
        id: &str,
        detected_pii: String,
        user_id: &str,
    ) -> Result<AnonymizationReviewItem, AnonymizationReviewRepositoryError> {
        debug!("Aktualisiere erkannte PIIs für Review-Eintrag {}", id);
        
        let conn = self.db_manager.get_connection()?;
        let now = chrono::Utc::now().to_rfc3339();
        
        conn.execute(
            "UPDATE anonymization_review_items
             SET detected_pii = ?, last_modified = ?, last_modified_by = ?
             WHERE id = ?",
            params![
                detected_pii,
                now,
                user_id,
                id,
            ],
        )?;
        
        // Hole den aktualisierten Eintrag
        let updated_item = self.get_review_item_by_id(id)?;
        
        // Audit-Log erstellen [ATV]
        self.audit_repository.create_audit_log(
            "anonymization_review_items",
            id,
            "UPDATE",
            Some("Erkannte PIIs aktualisiert"),
            true, // Enthält sensible Daten
            user_id,
        )?;
        
        Ok(updated_item)
    }
    
    /// Holt alle Review-Einträge mit einem bestimmten Status [AIU]
    pub fn get_review_items_by_status(&self, status: ReviewStatus) -> Result<Vec<AnonymizationReviewItem>, AnonymizationReviewRepositoryError> {
        debug!("Hole Review-Einträge mit Status {:?}", status);
        
        let conn = self.db_manager.get_connection()?;
        
        let mut stmt = conn.prepare(
            "SELECT id, transcript_id, status, detected_pii, anonymization_confidence,
                    review_reason, reviewer_notes, created, created_by, last_modified, last_modified_by
             FROM anonymization_review_items
             WHERE status = ?
             ORDER BY created DESC"
        )?;
        
        let review_items = stmt.query_map(params![status.to_string()], |row| {
            let status_str: String = row.get(2)?;
            let status = ReviewStatus::from_string(&status_str)
                .map_err(|e| rusqlite::Error::InvalidColumnType(2, e))?;
            
            Ok(AnonymizationReviewItem {
                id: row.get(0)?,
                transcript_id: row.get(1)?,
                status,
                detected_pii: row.get(3)?,
                anonymization_confidence: row.get(4)?,
                review_reason: row.get(5)?,
                reviewer_notes: row.get(6)?,
                created: row.get(7)?,
                created_by: row.get(8)?,
                last_modified: row.get(9)?,
                last_modified_by: row.get(10)?,
            })
        })?
        .collect::<Result<Vec<_>, _>>()?;
        
        // Audit-Log erstellen [ATV]
        self.audit_repository.create_audit_log(
            "anonymization_review_items",
            &status.to_string(),
            "READ",
            Some(&format!("Alle Review-Einträge mit Status {:?} gelesen", status)),
            true, // Enthält sensible Daten
            "system", // Hier sollte eigentlich der aktuelle Benutzer übergeben werden
        )?;
        
        Ok(review_items)
    }
}
