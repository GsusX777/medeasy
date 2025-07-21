// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Database API [AIU] [ATV] [SP]
// Provides type-safe access to the Tauri database commands

// Temporäre Mock-API bis .NET Backend verfügbar ist
import { 
  patientApi as mockPatientApi,
  sessionApi as mockSessionApi,
  transcriptApi as mockTranscriptApi,
  auditApi as mockAuditApi,
  initializeDatabase as mockInitializeDatabase
} from './database-mock';
import type { 
  PatientDto, 
  CreatePatientDto, 
  SessionDto, 
  CreateSessionDto,
  TranscriptDto,
  CreateTranscriptDto,
  AuditLogDto
} from '$lib/types/database';

// Current user ID (should be replaced with actual authentication)
const getCurrentUserId = (): string => {
  // In a real implementation, this would come from authentication
  return localStorage.getItem('medeasy_user_id') || 'default_user';
};

/**
 * Patient API
 */
export const patientApi = mockPatientApi;

/**
 * Session API
 */
export const sessionApi = mockSessionApi;

/**
 * Transcript API
 */
export const transcriptApi = mockTranscriptApi;

/**
 * Audit API
 */
export const auditApi = mockAuditApi;

/**
 * Database initialization
 */
export const initializeDatabase = mockInitializeDatabase;

// Legacy sessionApi object structure (wird durch Mock ersetzt)
const _legacySessionApi = {
  /**
   * Create a new session
   * @param session Session data
   * @returns Created session
   */
  async createSession(session: CreateSessionDto): Promise<SessionDto> {
    try {
      return await invoke('create_session', {
        session,
        userId: getCurrentUserId()
      });
    } catch (error) {
      console.error('Failed to create session:', error);
      throw error;
    }
  },

  /**
   * Get a session by ID
   * @param id Session ID
   * @returns Session data
   */
  async getSession(id: string): Promise<SessionDto> {
    try {
      return await invoke('get_session', {
        id,
        userId: getCurrentUserId()
      });
    } catch (error) {
      console.error(`Failed to get session ${id}:`, error);
      throw error;
    }
  },

  /**
   * List all sessions for a patient
   * @param patientId Patient ID
   * @returns List of sessions
   */
  async listPatientSessions(patientId: string): Promise<SessionDto[]> {
    try {
      return await invoke('list_patient_sessions', {
        patientId,
        userId: getCurrentUserId()
      });
    } catch (error) {
      console.error(`Failed to list sessions for patient ${patientId}:`, error);
      throw error;
    }
  }
};

/**
 * Transcript API
 */
export const transcriptApi = {
  /**
   * Create a new transcript with mandatory anonymization [AIU]
   * @param transcript Transcript data with original and anonymized text
   * @returns Created transcript
   */
  async createTranscript(transcript: CreateTranscriptDto): Promise<TranscriptDto> {
    try {
      // Enforce anonymization on client side as well [AIU]
      if (!transcript.anonymized_text || transcript.anonymized_text.trim() === '') {
        throw new Error('Anonymized text is required and cannot be empty');
      }
      
      return await invoke('create_transcript', {
        transcript,
        userId: getCurrentUserId()
      });
    } catch (error) {
      console.error('Failed to create transcript:', error);
      throw error;
    }
  },

  /**
   * Get all transcripts for a session
   * @param sessionId Session ID
   * @returns List of transcripts
   */
  async getSessionTranscripts(sessionId: string): Promise<TranscriptDto[]> {
    try {
      return await invoke('get_session_transcripts', {
        sessionId,
        userId: getCurrentUserId()
      });
    } catch (error) {
      console.error(`Failed to get transcripts for session ${sessionId}:`, error);
      throw error;
    }
  },
  
  /**
   * Get transcripts that need review due to low anonymization confidence [ARQ]
   * @returns List of transcripts needing review
   */
  async getTranscriptsNeedingReview(): Promise<TranscriptDto[]> {
    try {
      return await invoke('get_transcripts_needing_review', {
        userId: getCurrentUserId()
      });
    } catch (error) {
      console.error('Failed to get transcripts needing review:', error);
      throw error;
    }
  }
};

/**
 * Audit API (restricted to admin users) [ATV]
 */
export const auditApi = {
  /**
   * Get audit logs for an entity
   * @param entityName Entity name (e.g., "patients", "sessions")
   * @param entityId Entity ID
   * @param limit Maximum number of logs to return
   * @param offset Offset for pagination
   * @returns List of audit logs
   */
  async getEntityAuditLogs(
    entityName: string, 
    entityId: string, 
    limit?: number, 
    offset?: number
  ): Promise<AuditLogDto[]> {
    try {
      return await invoke('get_entity_audit_logs', {
        entityName,
        entityId,
        userId: getCurrentUserId(),
        limit,
        offset
      });
    } catch (error) {
      console.error(`Failed to get audit logs for ${entityName} ${entityId}:`, error);
      throw error;
    }
  },
  
  /**
   * Get recent audit logs
   * @param limit Maximum number of logs to return
   * @returns List of recent audit logs
   */
  async getRecentAuditLogs(limit: number = 50): Promise<AuditLogDto[]> {
    try {
      return await invoke('get_recent_audit_logs', {
        limit,
        userId: getCurrentUserId()
      });
    } catch (error) {
      console.error('Failed to get recent audit logs:', error);
      throw error;
    }
  }
};

/**
 * Database initialization
 */
export const initializeDatabase = async (): Promise<boolean> => {
  try {
    return await invoke<boolean>('initialize_database');
  } catch (error) {
    console.error('Failed to initialize database:', error);
    throw error;
  }
};
