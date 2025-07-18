// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Database Module [SP] [EIV] [ATV]
// This module implements the database functionality with SQLCipher encryption

// Audit-Modul zuerst, da es von anderen Modulen verwendet wird [ATV][SP]
pub mod audit;
pub mod connection;
pub mod models;
pub mod schema;
pub mod encryption;
pub mod migrations;

// Re-export commonly used items
pub use connection::DatabaseManager;
pub use models::{Patient, Session, Transcript, AuditLog, AnonymizationReviewItem};
pub use audit::{AuditLogger, AuditAction}; // Expliziter Re-Export [ATV][SP]
pub use encryption::FieldEncryption;
