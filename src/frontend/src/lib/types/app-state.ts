// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// [ZTS] Zero Tolerance Security - Type Definitions
// [CT] Cloud-Transparenz - UI zeigt immer Datenverarbeitungsort
// [AIU] Anonymisierung ist UNVERÄNDERLICH
// [ATV] Audit-Trail Vollständig

// App-Status Interface für strikte Typisierung
export interface AppState {
  initialized: boolean;
  version: string;
  processingLocation: 'local' | 'cloud'; // [CT] Cloud-Transparenz
  anonymizationEnabled: boolean; // [AIU] Immer true, readonly
  auditEnabled: boolean; // [ATV] Immer true, readonly
  cloudConsentGiven: boolean;
}

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
