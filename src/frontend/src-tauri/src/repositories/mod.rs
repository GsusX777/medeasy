/* „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 */

// MedEasy Repository Module [ATV] [EIV]
// Implements data access layer with audit logging and encryption

pub mod patient_repository;
pub mod session_repository;
pub mod transcript_repository;
pub mod audit_repository;
pub mod anonymization_review_repository;

// Re-export repositories
pub use patient_repository::PatientRepository;
pub use session_repository::SessionRepository;
pub use transcript_repository::TranscriptRepository;
pub use audit_repository::AuditRepository;
pub use anonymization_review_repository::AnonymizationReviewRepository;

