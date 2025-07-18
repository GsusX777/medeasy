// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// [SK] Session-Konzept - Eine Session = Eine Konsultation
// [AIU] Anonymisierung ist UNVERÄNDERLICH
// [CT] Cloud-Transparenz - UI zeigt immer Datenverarbeitungsort
// [ATV] Audit-Trail Vollständig

import { writable, derived } from 'svelte/store';
import { invoke } from '@tauri-apps/api/tauri';
import type { Writable } from 'svelte/store';

// Session-Typen
export interface Session {
  id: string;
  startTime: Date;
  endTime?: Date;
  patientId?: string; // Verschlüsselt gespeichert
  doctorId: string;   // Verschlüsselt gespeichert
  status: 'active' | 'completed' | 'archived';
  processingLocation: 'local' | 'cloud'; // [CT] Cloud-Transparenz
  audioRecorded: boolean;
  transcriptAvailable: boolean;
  anonymized: boolean; // [AIU] Immer true nach Verarbeitung
}

// App-Status
interface AppState {
  initialized: boolean;
  version: string;
  processingLocation: 'local' | 'cloud'; // [CT] Cloud-Transparenz
  anonymizationEnabled: boolean; // [AIU] Immer true, readonly
  auditEnabled: boolean; // [ATV] Immer true, readonly
  cloudConsentGiven: boolean;
}

// Aktuelle Session
export const currentSession: Writable<Session | null> = writable(null);

// App-Status Store
export const appState = writable<AppState>({
  initialized: false,
  version: '',
  processingLocation: 'local', // [CT] Default ist lokal
  anonymizationEnabled: true,  // [AIU] Immer true
  auditEnabled: true,          // [ATV] Immer true
  cloudConsentGiven: false
});

// Abgeleiteter Store für UI-Anzeige des Verarbeitungsorts
export const processingLocationIcon = derived(
  appState,
  $appState => $appState.processingLocation === 'local' ? '🔒 Lokal' : '☁️ Cloud'
);

// Session-Funktionen
export async function startNewSession(): Promise<void> {
  const sessionId = crypto.randomUUID();
  const now = new Date();
  
  // Verarbeitungsort abrufen
  const location = await invoke<string>('get_processing_location');
  
  // Neue Session erstellen
  const session: Session = {
    id: sessionId,
    startTime: now,
    doctorId: 'current-doctor', // In echter Implementierung aus Auth-System
    status: 'active',
    processingLocation: location as 'local' | 'cloud',
    audioRecorded: false,
    transcriptAvailable: false,
    anonymized: false // Wird true, sobald Verarbeitung abgeschlossen
  };
  
  currentSession.set(session);
  return;
}

// App-Initialisierung
export async function initializeApp(): Promise<void> {
  try {
    // App-Info von Tauri Backend abrufen
    const appInfo = await invoke<{
      version: string;
      anonymization_enabled: boolean;
      processing_location: string;
      audit_enabled: boolean;
    }>('get_app_info');
    
    appState.update(state => ({
      ...state,
      initialized: true,
      version: appInfo.version,
      processingLocation: appInfo.processing_location as 'local' | 'cloud',
      anonymizationEnabled: appInfo.anonymization_enabled, // [AIU] Immer true
      auditEnabled: appInfo.audit_enabled // [ATV] Immer true
    }));
  } catch (error) {
    console.error('Fehler bei App-Initialisierung:', error);
    throw error;
  }
}
