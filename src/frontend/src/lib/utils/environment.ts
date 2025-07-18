// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Environment Utilities [ZTS][CT]
// Hilfsfunktionen zur Erkennung der Ausführungsumgebung

import { getVersion } from '@tauri-apps/api/app';

// Umgebungsvariablen
let _isProduction: boolean | null = null;
let _isDevelopment: boolean | null = null;
let _appVersion: string | null = null;

// Prüft, ob die Anwendung in der Produktionsumgebung läuft [ZTS]
export function isProduction(): boolean {
  if (_isProduction === null) {
    // In der Produktionsumgebung ist process.env.NODE_ENV auf 'production' gesetzt
    // oder wir verwenden import.meta.env.PROD in Vite/SvelteKit
    if (typeof import.meta.env !== 'undefined') {
      _isProduction = import.meta.env.PROD === true;
    } else {
      // Fallback: Prüfe auf bestimmte Produktionsmerkmale
      _isProduction = !window.location.host.includes('localhost') && 
                     !window.location.host.includes('127.0.0.1');
    }
  }
  return _isProduction;
}

// Prüft, ob die Anwendung in der Entwicklungsumgebung läuft
export function isDevelopment(): boolean {
  if (_isDevelopment === null) {
    _isDevelopment = !isProduction();
  }
  return _isDevelopment;
}

// Holt die aktuelle App-Version aus Tauri
export async function getAppVersion(): Promise<string> {
  if (_appVersion === null) {
    try {
      _appVersion = await getVersion();
    } catch (error) {
      console.error('Fehler beim Abrufen der App-Version:', error);
      _appVersion = 'unbekannt';
    }
  }
  return _appVersion;
}

// Prüft, ob bestimmte Sicherheitsfeatures erzwungen werden müssen [ZTS][AIU][ATV]
export function shouldEnforceSecurity(): boolean {
  // In der Produktion IMMER Sicherheitsfeatures erzwingen
  if (isProduction()) {
    return true;
  }
  
  // In der Entwicklung optional, aber standardmäßig aktiviert
  // Kann nur durch explizite Konfiguration deaktiviert werden
  return import.meta.env.VITE_ENFORCE_SECURITY !== 'false';
}

// Prüft, ob die Anonymisierung erzwungen werden muss [AIU]
export function shouldEnforceAnonymization(): boolean {
  // Anonymisierung ist IMMER aktiviert, unabhängig von der Umgebung [AIU]
  return true;
}

// Prüft, ob der Audit-Trail erzwungen werden muss [ATV]
export function shouldEnforceAuditTrail(): boolean {
  // In der Produktion IMMER erzwingen
  if (isProduction()) {
    return true;
  }
  
  // In der Entwicklung optional, aber standardmäßig aktiviert
  return import.meta.env.VITE_ENFORCE_AUDIT !== 'false';
}

// Prüft, ob die Cloud-Features verfügbar sind [CT]
export function isCloudEnabled(): boolean {
  // Cloud-Features sind standardmäßig deaktiviert
  // und müssen explizit aktiviert werden
  return import.meta.env.VITE_ENABLE_CLOUD === 'true';
}

// Gibt die aktuelle Umgebung als String zurück
export function getEnvironmentName(): string {
  return isProduction() ? 'Produktion' : 'Entwicklung';
}
