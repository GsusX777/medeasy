// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

/**
 * Typdefinitionen für MedEasy Frontend
 * [SK][AIU][CT][DSC]
 */

/**
 * Session-Interface gemäß MedEasy-Projektregeln [SK]
 * Eine Session = Eine Konsultation
 */
export interface Session {
  /** Eindeutige ID der Session */
  id: string;
  
  /** Verschlüsselte Patienten-ID [EIV] */
  encryptedPatientId: string;
  
  /** Verschlüsselte Arzt-ID [EIV] */
  encryptedDoctorId: string;
  
  /** Zeitstempel der Erstellung */
  createdAt: Date;
  
  /** Zeitstempel der letzten Änderung */
  updatedAt: Date;
  
  /** Status der Session */
  status: SessionStatus;
  
  /** Anonymisierung ist IMMER aktiviert [AIU] */
  anonymized: boolean;
  
  /** Verarbeitungsort (lokal/cloud) [CT] */
  processingLocation: ProcessingLocation;
  
  /** Einwilligung zur Cloud-Verarbeitung [CT][DSC] */
  cloudConsentGiven: boolean;
  
  /** Schweizerdeutsch erkannt [SDH] */
  swissGermanDetected: boolean;
  
  /** Transkription */
  transcript?: Transcript;
}

/**
 * Status einer Session [SK]
 */
export enum SessionStatus {
  CREATED = 'created',
  RECORDING = 'recording',
  PROCESSING = 'processing',
  COMPLETED = 'completed',
  ERROR = 'error'
}

/**
 * Verarbeitungsort [CT]
 */
export enum ProcessingLocation {
  LOCAL = 'local',
  CLOUD = 'cloud'
}

/**
 * Transkript mit Anonymisierung [AIU]
 */
export interface Transcript {
  /** Eindeutige ID des Transkripts */
  id: string;
  
  /** Anonymisierter Text (immer) [AIU] */
  anonymizedText: string;
  
  /** Konfidenz der Anonymisierung [ARQ] */
  anonymizationConfidence: number;
  
  /** Schweizerdeutsch erkannt [SDH] */
  swissGermanDetected: boolean;
  
  /** Verwendeter KI-Provider [PK] */
  usedProvider: string;
}

/**
 * App-Zustand [CT]
 */
export interface AppState {
  /** Ist die App initialisiert */
  initialized: boolean;
  
  /** Ist die App offline */
  offline: boolean;
  
  /** Verfügbare KI-Provider [PK] */
  availableProviders: string[];
  
  /** Aktueller KI-Provider [PK] */
  currentProvider: string;
  
  /** Ist die Cloud-Verarbeitung verfügbar [CT] */
  cloudProcessingAvailable: boolean;
  
  /** Ist die lokale Verarbeitung verfügbar [CT] */
  localProcessingAvailable: boolean;
}
