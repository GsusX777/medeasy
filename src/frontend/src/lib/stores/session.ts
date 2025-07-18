// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// [SK] Session-Konzept - Eine Session = Eine Konsultation
// [AIU] Anonymisierung ist UNVERÄNDERLICH
// [CT] Cloud-Transparenz - UI zeigt immer Datenverarbeitungsort
// [ATV] Audit-Trail Vollständig
// [ZTS] Zero Tolerance Security - Keine Sicherheits-Bypasses

import { writable, derived } from 'svelte/store';
import type { Writable } from 'svelte/store';
import type { AppState, Session } from '$lib/types/app-state';

// [ZTS][TSF] Sichere Tauri-Integration für Desktop-Anwendung
// Verhindert SSR-Build-Probleme durch dynamischen Import zur Laufzeit
let tauriInvoke: ((cmd: string, args?: any) => Promise<any>) | null = null;

// Browser-Check Funktion für Desktop-App (Tauri)
function isBrowser(): boolean {
  return typeof window !== 'undefined';
}

// Initialisiere Tauri API nur zur Laufzeit
if (isBrowser()) {
  // Diese Syntax verhindert statische Analyse/Import während SSR
  const tauriModule = '/@tauri-apps/api/tauri'.replace('/', '');
  // vite-ignore verhindert, dass Vite/Rollup versucht, das Modul zu analysieren
  import(/* @vite-ignore */ tauriModule)
    .then(module => {
      tauriInvoke = module.invoke;
    })
    .catch(err => {
      console.error('[ZTS] Fehler beim Laden der Tauri API:', err);
    });
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
  
  // [ZTS] Verarbeitungsort sicher abrufen mit Fallback
  let location: 'local' | 'cloud' = 'local'; // Default ist lokal für Sicherheit
  try {
    if (tauriInvoke) {
      location = await tauriInvoke('get_processing_location') as 'local' | 'cloud';
    } else {
      console.warn('[ZTS] Tauri API nicht verfügbar, verwende lokale Verarbeitung');
    }
  } catch (error) {
    console.error('[ZTS] Fehler beim Abrufen des Verarbeitungsortes:', error);
  }
  
  // Neue Session erstellen
  // [ZTS] Session mit strikter Typisierung
  const session: Session = {
    id: sessionId,
    startTime: now,
    doctorId: 'current-doctor', // In echter Implementierung aus Auth-System
    status: 'active',
    processingLocation: location, // Bereits als 'local' | 'cloud' typisiert
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
    // [ZTS] App-Info sicher von Tauri Backend abrufen
    // [ZTS] Typsichere Default-Werte
    let appInfo: {
      version: string;
      anonymization_enabled: boolean;
      processing_location: 'local' | 'cloud';
      audit_enabled: boolean;
    } = {
      version: '1.0.0', // Fallback Version
      anonymization_enabled: true, // [AIU] Immer true
      processing_location: 'local', // Default: Lokal für Sicherheit
      audit_enabled: true // [ATV] Immer true
    };
    
    try {
      if (tauriInvoke) {
        const response = await tauriInvoke('get_app_info') as {
          version: string;
          anonymization_enabled: boolean;
          processing_location: 'local' | 'cloud';
          audit_enabled: boolean;
        };
        
        // Überprüfe Response
        if (response) {
          appInfo = response;
          // [AIU] Anonymisierung ist UNVERÄNDERLICH - sicherstellen
          if (!response.anonymization_enabled) {
            console.error('[AIU] Versuchte Deaktivierung der Anonymisierung verhindert');
            appInfo.anonymization_enabled = true;
          }
          // [ATV] Audit ist UNVERÄNDERLICH - sicherstellen
          if (!response.audit_enabled) {
            console.error('[ATV] Versuchte Deaktivierung des Audits verhindert');
            appInfo.audit_enabled = true;
          }
        }
      } else {
        console.warn('[ZTS] Tauri API nicht verfügbar, verwende Default-Werte');
      }
    } catch (error) {
      console.error('[ZTS] Fehler beim Abrufen der App-Info:', error);
    }
    
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
