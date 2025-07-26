// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Database Types [MDL] [SF]
// TypeScript definitions for database DTOs

/**
 * Patient DTO [ZTS]
 */
export interface PatientDto {
  id: string;
  firstName: string;                
  lastName: string;               
  dateOfBirth: string;            
  dateOfBirthFormatted?: string;  
  insuranceNumberMasked: string;  
  notes?: string;
  created: string;
  lastModified: string;           
  createdBy: string;              
  lastModifiedBy: string;         
}

/**
 * DTO for creating a new patient [ZTS]
 */
export interface CreatePatientDto {
  firstName: string;              
  lastName: string;               
  dateOfBirth: string;            
  insuranceNumber: string;        
  notes?: string;
}

/**
 * Session status enum [ZTS]
 */
export enum SessionStatus {
  Scheduled = 'Scheduled',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

/**
 * Anonymization status enum - ✅ NEW [AIU]
 */
export enum AnonymizationStatus {
  Pending = 'Pending',
  AutoAnonymized = 'AutoAnonymized',
  PendingReview = 'PendingReview',
  ReviewApproved = 'ReviewApproved',
  ReviewRejected = 'ReviewRejected'
}

/**
 * Session DTO [ZTS]
 */
export interface SessionDto {
  id: string;
  patientId: string;              // ✅ patient_id → patientId
  sessionDate: string;            // ✅ session_date → sessionDate (ISO-8601)
  sessionDateFormatted?: string;  // ✅ NEW: DD.MM.YYYY für UI [SF]
  startTime: string;              // ✅ start_time → startTime (TimeSpan)
  startTimeFormatted?: string;    // ✅ NEW: HH:mm für UI [SF]
  endTime?: string;               // ✅ end_time → endTime (TimeSpan, optional)
  endTimeFormatted?: string;      // ✅ NEW: HH:mm für UI [SF]
  status: SessionStatus;          // ✅ Enum statt string [ZTS]
  reason: string;
  notes?: string;
  anonymizedNotes?: string;       // ✅ NEW: Anonymisierte Notizen [AIU]
  anonymizationStatus: AnonymizationStatus; // ✅ NEW: Anonymisierungs-Status [AIU]
  created: string;
  lastModified: string;           // ✅ last_modified → lastModified
  createdBy: string;              // ✅ NEW: Audit-Trail [ATV]
  lastModifiedBy: string;         // ✅ NEW: Audit-Trail [ATV]
}

/**
 * DTO for creating a new session [ZTS]
 */
export interface CreateSessionDto {
  patientId: string;              // ✅ patient_id → patientId
  reason: string;                 // ✅ NEW: Grund der Konsultation [PSF]
  sessionDate?: string;           // ✅ NEW: Optional (ISO-8601)
  startTime?: string;             // ✅ NEW: Optional (TimeSpan)
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
