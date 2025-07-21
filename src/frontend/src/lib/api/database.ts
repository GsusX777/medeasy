// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Database API [AIU] [ATV] [SP]
// Temporäre Mock-API bis .NET Backend verfügbar ist

import { 
  patientApi as mockPatientApi,
  sessionApi as mockSessionApi,
  transcriptApi as mockTranscriptApi,
  auditApi as mockAuditApi,
  initializeDatabase as mockInitializeDatabase
} from './database-mock';

// Re-export Mock APIs
export const patientApi = mockPatientApi;
export const sessionApi = mockSessionApi;
export const transcriptApi = mockTranscriptApi;
export const auditApi = mockAuditApi;
export const initializeDatabase = mockInitializeDatabase;

// Helper function (Mock-Version)
function getCurrentUserId(): string {
  return 'mock-user-id';
}
