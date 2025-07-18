// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Database Stores [AIU] [ATV] [SP]
// Svelte stores for database state management

import { writable, derived, get } from 'svelte/store';
import { patientApi, sessionApi, transcriptApi, initializeDatabase } from '$lib/api/database';
import type { PatientDto, SessionDto, TranscriptDto } from '$lib/types/database';

// Database initialization state
export const databaseInitialized = writable<boolean>(false);
export const databaseError = writable<string | null>(null);

// Patient stores
export const patients = writable<PatientDto[]>([]);
export const selectedPatientId = writable<string | null>(null);
export const selectedPatient = derived(
  [patients, selectedPatientId],
  ([$patients, $selectedPatientId]) => {
    if (!$selectedPatientId) return null;
    return $patients.find(p => p.id === $selectedPatientId) || null;
  }
);

// Session stores
export const sessions = writable<SessionDto[]>([]);
export const selectedSessionId = writable<string | null>(null);
export const selectedSession = derived(
  [sessions, selectedSessionId],
  ([$sessions, $selectedSessionId]) => {
    if (!$selectedSessionId) return null;
    return $sessions.find(s => s.id === $selectedSessionId) || null;
  }
);

// Transcript stores
export const transcripts = writable<TranscriptDto[]>([]);
export const transcriptsNeedingReview = writable<TranscriptDto[]>([]);
export const selectedTranscriptId = writable<string | null>(null);
export const selectedTranscript = derived(
  [transcripts, selectedTranscriptId],
  ([$transcripts, $selectedTranscriptId]) => {
    if (!$selectedTranscriptId) return null;
    return $transcripts.find(t => t.id === $selectedTranscriptId) || null;
  }
);

// Loading states
export const isLoadingPatients = writable<boolean>(false);
export const isLoadingSessions = writable<boolean>(false);
export const isLoadingTranscripts = writable<boolean>(false);

/**
 * Initialize the database
 */
export async function initDb(): Promise<boolean> {
  try {
    databaseError.set(null);
    const result = await initializeDatabase();
    databaseInitialized.set(result);
    return result;
  } catch (error) {
    console.error('Database initialization failed:', error);
    databaseError.set(error instanceof Error ? error.message : String(error));
    return false;
  }
}

/**
 * Load all patients
 */
export async function loadPatients(): Promise<PatientDto[]> {
  try {
    isLoadingPatients.set(true);
    const patientList = await patientApi.listPatients();
    patients.set(patientList);
    return patientList;
  } catch (error) {
    console.error('Failed to load patients:', error);
    throw error;
  } finally {
    isLoadingPatients.set(false);
  }
}

/**
 * Create a new patient
 */
export async function createPatient(
  firstName: string,
  lastName: string,
  dateOfBirth: string,
  insuranceNumber: string,
  notes?: string
): Promise<PatientDto> {
  try {
    // Validate Swiss insurance number format [SF]
    const insuranceRegex = /^\d{3}\.\d{4}\.\d{4}\.\d{2}$/;
    if (!insuranceRegex.test(insuranceNumber)) {
      throw new Error('Ungültiges Format der Versicherungsnummer. Erwartet: XXX.XXXX.XXXX.XX');
    }
    
    // Validate Swiss date format [SF]
    const dateRegex = /^\d{2}\.\d{2}\.\d{4}$/;
    if (!dateRegex.test(dateOfBirth)) {
      throw new Error('Ungültiges Datumsformat. Erwartet: DD.MM.YYYY');
    }
    
    const newPatient = await patientApi.createPatient({
      first_name: firstName,
      last_name: lastName,
      date_of_birth: dateOfBirth,
      insurance_number: insuranceNumber,
      notes
    });
    
    // Update the patients store
    patients.update(current => [...current, newPatient]);
    
    return newPatient;
  } catch (error) {
    console.error('Failed to create patient:', error);
    throw error;
  }
}

/**
 * Select a patient by ID
 */
export function selectPatient(patientId: string | null): void {
  selectedPatientId.set(patientId);
  if (patientId === null) {
    // Clear related selections
    selectedSessionId.set(null);
    selectedTranscriptId.set(null);
    sessions.set([]);
    transcripts.set([]);
  }
}

/**
 * Load sessions for the selected patient
 */
export async function loadPatientSessions(patientId: string): Promise<SessionDto[]> {
  try {
    isLoadingSessions.set(true);
    const sessionList = await sessionApi.listPatientSessions(patientId);
    sessions.set(sessionList);
    return sessionList;
  } catch (error) {
    console.error(`Failed to load sessions for patient ${patientId}:`, error);
    throw error;
  } finally {
    isLoadingSessions.set(false);
  }
}

/**
 * Create a new session for a patient
 */
export async function createSession(patientId: string, notes?: string): Promise<SessionDto> {
  try {
    const newSession = await sessionApi.createSession({
      patient_id: patientId,
      notes
    });
    
    // Update the sessions store
    sessions.update(current => [...current, newSession]);
    
    return newSession;
  } catch (error) {
    console.error('Failed to create session:', error);
    throw error;
  }
}

/**
 * Select a session by ID
 */
export function selectSession(sessionId: string | null): void {
  selectedSessionId.set(sessionId);
  if (sessionId === null) {
    // Clear related selections
    selectedTranscriptId.set(null);
    transcripts.set([]);
  }
}

/**
 * Load transcripts for the selected session
 */
export async function loadSessionTranscripts(sessionId: string): Promise<TranscriptDto[]> {
  try {
    isLoadingTranscripts.set(true);
    const transcriptList = await transcriptApi.getSessionTranscripts(sessionId);
    transcripts.set(transcriptList);
    return transcriptList;
  } catch (error) {
    console.error(`Failed to load transcripts for session ${sessionId}:`, error);
    throw error;
  } finally {
    isLoadingTranscripts.set(false);
  }
}

/**
 * Create a new transcript with mandatory anonymization [AIU]
 */
export async function createTranscript(
  sessionId: string,
  originalText: string,
  anonymizedText: string,
  anonymizationConfidence: number
): Promise<TranscriptDto> {
  try {
    // Enforce anonymization on client side [AIU]
    if (!anonymizedText || anonymizedText.trim() === '') {
      throw new Error('Anonymisierter Text ist erforderlich und darf nicht leer sein');
    }
    
    const newTranscript = await transcriptApi.createTranscript({
      session_id: sessionId,
      original_text: originalText,
      anonymized_text: anonymizedText,
      anonymization_confidence: anonymizationConfidence
    });
    
    // Update the transcripts store
    transcripts.update(current => [...current, newTranscript]);
    
    // If this transcript needs review, update that store too
    if (newTranscript.needs_review) {
      transcriptsNeedingReview.update(current => [...current, newTranscript]);
    }
    
    return newTranscript;
  } catch (error) {
    console.error('Failed to create transcript:', error);
    throw error;
  }
}

/**
 * Load transcripts that need review due to low anonymization confidence [ARQ]
 */
export async function loadTranscriptsNeedingReview(): Promise<TranscriptDto[]> {
  try {
    const reviewList = await transcriptApi.getTranscriptsNeedingReview();
    transcriptsNeedingReview.set(reviewList);
    return reviewList;
  } catch (error) {
    console.error('Failed to load transcripts needing review:', error);
    throw error;
  }
}

/**
 * Select a transcript by ID
 */
export function selectTranscript(transcriptId: string | null): void {
  selectedTranscriptId.set(transcriptId);
}
