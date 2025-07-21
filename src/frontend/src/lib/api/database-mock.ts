// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Database Mock API [AIU] [ATV] [SP]
// Temporäre Mock-Implementierung für Tests ohne Backend

import type { 
  PatientDto, 
  CreatePatientDto, 
  SessionDto, 
  CreateSessionDto,
  TranscriptDto,
  CreateTranscriptDto,
  AuditLogDto
} from '$lib/types/database';

// Mock-Daten für Tests [SF]
const mockPatients: PatientDto[] = [
  {
    id: '1',
    first_name: 'Hans',
    last_name: 'Müller',
    date_of_birth: '15.03.1975',
    insurance_number: '756.1234.5678.90',
    notes: 'Diabetes Typ 2',
    created: '2024-01-15T10:00:00Z',
    last_modified: '2024-01-15T10:00:00Z'
  },
  {
    id: '2',
    first_name: 'Maria',
    last_name: 'Schmidt',
    date_of_birth: '22.07.1980',
    insurance_number: '756.9876.5432.10',
    notes: 'Hypertonie',
    created: '2024-01-16T14:30:00Z',
    last_modified: '2024-01-16T14:30:00Z'
  }
];

const mockSessions: SessionDto[] = [
  {
    id: '1',
    patient_id: '1',
    session_date: '19.01.2025',
    start_time: '09:00',
    end_time: '09:30',
    status: 'Completed',
    notes: 'Routinekontrolle',
    created: '2024-01-19T09:00:00Z',
    last_modified: '2024-01-19T09:30:00Z'
  },
  {
    id: '2',
    patient_id: '2',
    session_date: '19.01.2025',
    start_time: '10:00',
    end_time: '10:45',
    status: 'Completed',
    notes: 'Blutdruckkontrolle',
    created: '2024-01-19T10:00:00Z',
    last_modified: '2024-01-19T10:45:00Z'
  },
  {
    id: '3',
    patient_id: '1',
    session_date: '20.01.2025',
    start_time: '14:00',
    end_time: undefined,
    status: 'Scheduled',
    notes: 'Nachkontrolle',
    created: '2024-01-19T15:00:00Z',
    last_modified: '2024-01-19T15:00:00Z'
  }
];

// Simuliere Netzwerk-Delay
const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

/**
 * Patient API Mock
 */
export const patientApi = {
  async createPatient(patient: CreatePatientDto): Promise<PatientDto> {
    await delay(200);
    const newPatient: PatientDto = {
      id: Date.now().toString(),
      ...patient,
      created: new Date().toISOString(),
      last_modified: new Date().toISOString()
    };
    mockPatients.push(newPatient);
    return newPatient;
  },

  async getPatient(id: string): Promise<PatientDto> {
    await delay(100);
    const patient = mockPatients.find(p => p.id === id);
    if (!patient) throw new Error(`Patient with ID ${id} not found`);
    return patient;
  },

  async listPatients(): Promise<PatientDto[]> {
    await delay(150);
    return [...mockPatients];
  }
};

/**
 * Session API Mock
 */
export const sessionApi = {
  async createSession(session: CreateSessionDto): Promise<SessionDto> {
    await delay(200);
    const newSession: SessionDto = {
      id: Date.now().toString(),
      session_date: new Date().toLocaleDateString('de-CH'),
      start_time: new Date().toLocaleTimeString('de-CH', { hour: '2-digit', minute: '2-digit' }),
      end_time: undefined,
      status: 'Scheduled',
      ...session,
      created: new Date().toISOString(),
      last_modified: new Date().toISOString()
    };
    mockSessions.push(newSession);
    return newSession;
  },

  async getSession(id: string): Promise<SessionDto> {
    await delay(100);
    const session = mockSessions.find(s => s.id === id);
    if (!session) throw new Error(`Session with ID ${id} not found`);
    return session;
  },

  async listPatientSessions(patientId: string): Promise<SessionDto[]> {
    await delay(150);
    return mockSessions.filter(s => s.patient_id === patientId);
  }
};

/**
 * Transcript API Mock
 */
export const transcriptApi = {
  async createTranscript(transcript: CreateTranscriptDto): Promise<TranscriptDto> {
    await delay(300);
    const newTranscript: TranscriptDto = {
      id: Date.now().toString(),
      needs_review: transcript.anonymization_confidence < 0.8,
      created: new Date().toISOString(),
      ...transcript
    };
    return newTranscript;
  },

  async getSessionTranscripts(sessionId: string): Promise<TranscriptDto[]> {
    await delay(150);
    return []; // Keine Mock-Transkripte für jetzt
  },

  async getTranscriptsNeedingReview(): Promise<TranscriptDto[]> {
    await delay(150);
    return []; // Keine Mock-Transkripte für jetzt
  }
};

/**
 * Audit API Mock
 */
export const auditApi = {
  async getEntityAuditLogs(
    entityName: string, 
    entityId: string, 
    limit?: number, 
    offset?: number
  ): Promise<AuditLogDto[]> {
    await delay(200);
    return []; // Keine Mock-Audit-Logs für jetzt
  },

  async getRecentAuditLogs(limit: number = 50): Promise<AuditLogDto[]> {
    await delay(200);
    return []; // Keine Mock-Audit-Logs für jetzt
  }
};

/**
 * Database initialization Mock
 */
export const initializeDatabase = async (): Promise<boolean> => {
  await delay(500);
  console.log('Mock-Datenbank initialisiert [Mock-Modus aktiv]');
  return true;
};
