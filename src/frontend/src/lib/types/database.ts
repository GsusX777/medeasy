// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Database Types [MDL] [SF]
// TypeScript definitions for database DTOs

/**
 * Patient DTO
 */
export interface PatientDto {
  id: string;
  first_name: string;
  last_name: string;
  date_of_birth: string; // Format: DD.MM.YYYY [SF]
  insurance_number: string; // Format: XXX.XXXX.XXXX.XX [SF]
  notes?: string;
  created: string;
  last_modified: string;
}

/**
 * DTO for creating a new patient
 */
export interface CreatePatientDto {
  first_name: string;
  last_name: string;
  date_of_birth: string; // Format: DD.MM.YYYY [SF]
  insurance_number: string; // Format: XXX.XXXX.XXXX.XX [SF]
  notes?: string;
}

/**
 * Session status enum
 */
export enum SessionStatus {
  Scheduled = 'Scheduled',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

/**
 * Session DTO
 */
export interface SessionDto {
  id: string;
  patient_id: string;
  session_date: string; // Format: DD.MM.YYYY [SF]
  start_time: string; // Format: HH:MM
  end_time?: string; // Format: HH:MM (optional)
  status: string; // SessionStatus as string
  notes?: string;
  audio_file_path?: string;
  created: string;
  last_modified: string;
}

/**
 * DTO for creating a new session
 */
export interface CreateSessionDto {
  patient_id: string;
  notes?: string;
}

/**
 * Transcript DTO
 */
export interface TranscriptDto {
  id: string;
  session_id: string;
  anonymized_text: string; // Only anonymized text is returned [AIU]
  anonymization_confidence?: number; // 0-1 value
  needs_review: boolean;
  created: string;
}

/**
 * DTO for creating a new transcript
 */
export interface CreateTranscriptDto {
  session_id: string;
  original_text: string; // Original text with PII
  anonymized_text: string; // Anonymized text (required) [AIU]
  anonymization_confidence: number; // 0-1 value
}

/**
 * Anonymization review item DTO
 */
export interface AnonymizationReviewItemDto {
  id: string;
  transcript_id: string;
  status: string; // 'Pending', 'InReview', 'Approved', 'Rejected', 'Whitelisted'
  detected_pii?: string; // JSON string of detected PII
  anonymization_confidence: number;
  review_reason?: string;
  reviewer_notes?: string;
  created: string;
  last_modified: string;
}

/**
 * Audit log DTO
 */
export interface AuditLogDto {
  id: string;
  entity_name: string;
  entity_id: string;
  action: string; // 'INSERT', 'UPDATE', 'DELETE', 'READ'
  changes?: string;
  contains_sensitive_data: boolean;
  timestamp: string;
  user_id: string;
}

/**
 * Audit statistics DTO
 */
export interface AuditStatisticsDto {
  action_counts: Record<string, number>;
  entity_counts: Record<string, number>;
  user_counts: Record<string, number>;
  sensitive_data_access_count: number;
  total_logs: number;
}
