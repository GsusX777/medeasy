<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Frontend-Backend Integration Points

*Letzte Aktualisierung: 26.07.2025*  
*Status: 🔄 Analyse der Kommunikationspfade*

Diese Dokumentation zeigt alle Kommunikationspfade zwischen Frontend (Svelte + Tauri) und Backend (.NET API) basierend auf API_CONTRACTS.md und identifiziert fehlende Integrationen.

## 🏗️ Architektur-Übersicht

```
Frontend (Svelte + Tauri)
    ↕️ Tauri Commands (System APIs)
    ↕️ HTTP REST APIs (.NET Backend)
    ↕️ gRPC (AI Service - Python)
Backend (.NET API)
    ↕️ SQLCipher Database
    ↕️ gRPC Client (AI Service)
```

---

## 📊 Integration Status Übersicht

| Kategorie | Frontend Komponenten | Backend APIs | Status | Mock-Daten |
|-----------|---------------------|--------------|--------|------------|
| **Patients** | PatientListView, PatientCreateModal | ✅ `/api/v1/patients/*` | 🔄 **Mock → Real** | ✅ Zu ersetzen |
| **Sessions** | ConsultationListView, ConsultationCreateModal | ⚠️ `/api/v1/sessions/*` (Dummy) | 🔄 **Mock → Real** | ✅ Zu ersetzen |
| **Transcripts** | TranscriptViewer, TranscriptSplitView | 🚧 Geplant | ❌ **Fehlend** | ✅ Nur Mock |
| **Anonymization** | AnonymizationReview, ConfidenceReviewPanel | 🚧 Geplant | ❌ **Fehlend** | ✅ Nur Mock |
| **System Health** | PerformanceMonitor | ✅ `/health`, `/api/system/*` | 🔄 **Mock → Real** | ✅ Zu ersetzen |
| **Security** | SecuritySettings, KeyManagement | 🚧 Geplant | ❌ **Fehlend** | ✅ Nur Mock |
| **Audit** | AuditTrailViewer | 🚧 Geplant | ❌ **Fehlend** | ✅ Nur Mock |

**Legende**:
- ✅ **Implementiert** - Vollständig funktional
- ⚠️ **Legacy/Dummy** - Funktioniert, aber veraltet
- 🚧 **Geplant** - Noch nicht implementiert
- 🔄 **Mock → Real** - Muss von Mock auf echte API umgestellt werden
- ❌ **Fehlend** - Keine Backend-API vorhanden

---

## 👥 Patients Integration

### **Frontend Komponenten:**
- **PatientListView.svelte** - Patientenliste anzeigen
- **PatientCreateModal.svelte** - Neuen Patienten erstellen
- **PatientImportButton.svelte** - Patienten importieren

### **Backend APIs:**
```typescript
✅ GET /api/v1/patients           // Alle Patienten abrufen
✅ GET /api/v1/patients/{id}      // Spezifischen Patienten abrufen
✅ POST /api/v1/patients          // Neuen Patienten erstellen
✅ PUT /api/v1/patients/{id}      // Patienten aktualisieren
✅ DELETE /api/v1/patients/{id}   // Patienten löschen
```

### **Aktuelle Integration:**
```typescript
// PatientListView.svelte - AKTUELL MOCK
import { patientApi } from '$lib/api/database'; // → Mock-API

// ZIEL: Echte REST API Integration
const response = await fetch('/api/v1/patients', {
  headers: { 'Authorization': `Bearer ${token}` }
});
```

### **Fehlende Integration:**
- ❌ **JWT Authentication** - Frontend hat keine Token-Verwaltung
- ❌ **Error Handling** - Keine Backend-Fehlerbehandlung
- ❌ **Validation** - Frontend-Validierung fehlt
- ❌ **Real-time Updates** - Keine Live-Synchronisation

---

## 📅 Sessions Integration

### **Frontend Komponenten:**
- **ConsultationListView.svelte** - Session-Liste anzeigen
- **ConsultationCreateModal.svelte** - Neue Session erstellen
- **SessionRecorder.svelte** - Audio-Aufnahme für Sessions
- **SubHeader.svelte** - Session-Controls und Timer

### **Backend APIs:**
```typescript
⚠️ GET /api/v1/sessions/patient/{patientId}  // Dummy-Implementierung
⚠️ POST /api/v1/sessions                     // Dummy-Implementierung
⚠️ GET /api/v1/sessions/{id}                 // Dummy-Implementierung
⚠️ PUT /api/v1/sessions/{id}                 // Dummy-Implementierung
⚠️ DELETE /api/v1/sessions/{id}              // Dummy-Implementierung
```

### **Aktuelle Integration:**
```typescript
// ConsultationListView.svelte - AKTUELL MOCK
import { sessionApi } from '$lib/api/database'; // → Mock-API

// Backend gibt nur Dummy-Response:
// { "message": "Sessions für Patient {id} abgerufen" }
```

### **Fehlende Integration:**
- ❌ **Echte Session-Daten** - Backend gibt nur Dummy-Messages zurück
- ❌ **Audio-Integration** - Keine Verbindung zwischen Recorder und Backend
- ❌ **Session-Timer** - Keine Live-Timer-Synchronisation
- ❌ **Session-Status** - Keine Statusverfolgung (aktiv, pausiert, beendet)

---

## 📝 Transcripts Integration

### **Frontend Komponenten:**
- **TranscriptViewer.svelte** - Transkript anzeigen und bearbeiten
- **TranscriptSplitView.svelte** - Split-Layout für Live-Transkription
- **TranscriptEntry.svelte** - Einzelne Transkript-Einträge
- **LiveAnalysisPanel.svelte** - Live-Analyse während Transkription

### **Backend APIs:**
```typescript
🚧 GET /api/v1/transcripts/session/{sessionId}    // GEPLANT
🚧 POST /api/v1/transcripts                       // GEPLANT
🚧 PUT /api/v1/transcripts/{id}                   // GEPLANT
🚧 DELETE /api/v1/transcripts/{id}                // GEPLANT
🚧 POST /api/v1/transcripts/{id}/analyze          // GEPLANT
```

### **Aktuelle Integration:**
```typescript
// TranscriptViewer.svelte - NUR MOCK
import { transcriptApi } from '$lib/api/database'; // → Mock-API

// Keine Backend-APIs implementiert!
```

### **Fehlende Integration:**
- ❌ **Komplette Backend-API** - Keine Transcript-Endpunkte implementiert
- ❌ **AI-Service Integration** - Keine gRPC-Verbindung zu Python AI Service
- ❌ **Live-Transkription** - Keine WebSocket/Server-Sent Events
- ❌ **Anonymisierung** - Keine Backend-Integration für PII-Erkennung

---

## 🔒 Anonymization Integration

### **Frontend Komponenten:**
- **AnonymizationReview.svelte** - Review-Queue für unsichere Erkennungen
- **AnonymizationNotice.svelte** - Anonymisierungs-Status anzeigen
- **ConfidenceReviewPanel.svelte** - Konfidenz-basierte Reviews
- **ProcessingLocationIndicator.svelte** - Cloud/Local Processing Anzeige

### **Backend APIs:**
```typescript
🚧 GET /api/v1/anonymization/review-queue         // GEPLANT
🚧 POST /api/v1/anonymization/review              // GEPLANT
🚧 GET /api/v1/anonymization/status/{transcriptId} // GEPLANT
🚧 POST /api/v1/anonymization/process             // GEPLANT
```

### **Aktuelle Integration:**
```typescript
// AnonymizationReview.svelte - NUR MOCK
// Verwendet Tauri Commands für lokale Verarbeitung
import { invoke } from '@tauri-apps/api/tauri';

// Keine Backend-APIs implementiert!
```

### **Fehlende Integration:**
- ❌ **Komplette Backend-API** - Keine Anonymization-Endpunkte
- ❌ **AI-Service Integration** - Keine gRPC-Verbindung für PII-Erkennung
- ❌ **Review-Workflow** - Keine Backend-Unterstützung für Review-Queue
- ❌ **Cloud-Processing** - Keine Backend-Integration für Cloud-Anonymisierung

---

## 📊 System Health Integration

### **Frontend Komponenten:**
- **PerformanceMonitor.svelte** - System-Performance anzeigen
- **Header.svelte** - System-Status in Header
- **Sidebar.svelte** - Performance-Monitor in Sidebar

### **Backend APIs:**
```typescript
✅ GET /health                           // Basic Health Check
✅ GET /api/system/performance           // System Performance Metriken
✅ GET /api/system/info                  // System-Informationen
```

### **Aktuelle Integration:**
```typescript
// PerformanceMonitor.svelte - AKTUELL MOCK
async function updatePerformanceMetrics() {
  // TODO: Implement Tauri commands for performance monitoring
  // For now, simulate data
  cpuUsage = Math.floor(Math.random() * 100);
  ramUsage = Math.floor(Math.random() * 100);
  
  // In real implementation:
  // const metrics = await invoke('get_performance_metrics');
  // cpuUsage = metrics.cpu;
}
```

### **Fehlende Integration:**
- ❌ **Tauri Commands** - Keine `get_performance_metrics()` implementiert
- ❌ **REST API Integration** - Keine Verbindung zu `/api/system/performance`
- ❌ **Real-time Updates** - Keine Live-Performance-Daten
- ❌ **Error Handling** - Keine Fallback-Mechanismen

---

## 🔐 Security Integration

### **Frontend Komponenten:**
- **SecuritySettings.svelte** - Sicherheitseinstellungen verwalten
- **KeyManagement.svelte** - Verschlüsselungsschlüssel verwalten
- **DatabaseSecuritySettings.svelte** - Datenbank-Sicherheit konfigurieren
- **SecurityBadge.svelte** - Sicherheitsstatus anzeigen

### **Backend APIs:**
```typescript
🚧 GET /api/v1/security/settings          // GEPLANT
🚧 PUT /api/v1/security/settings          // GEPLANT
🚧 POST /api/v1/security/keys/rotate      // GEPLANT
🚧 GET /api/v1/security/audit             // GEPLANT
```

### **Aktuelle Integration:**
```typescript
// SecuritySettings.svelte - NUR TAURI COMMANDS
import { invoke } from '@tauri-apps/api/tauri';

async function updateSecuritySettings() {
  try {
    // Verwendet lokale Tauri Commands
    await invoke('update_security_settings', { settings });
  } catch (error) {
    console.warn('[ZTS] Tauri API nicht verfügbar, simuliere Verarbeitung');
  }
}
```

### **Fehlende Integration:**
- ❌ **Backend Security APIs** - Keine Sicherheits-Endpunkte implementiert
- ❌ **Key Management** - Keine Backend-Schlüsselverwaltung
- ❌ **Security Audit** - Keine Backend-Sicherheitsüberwachung
- ❌ **Centralized Settings** - Nur lokale Tauri-basierte Einstellungen

---

## 📋 Audit Integration

### **Frontend Komponenten:**
- **AuditTrailViewer.svelte** - Audit-Logs anzeigen und filtern

### **Backend APIs:**
```typescript
🚧 GET /api/v1/audit/logs                 // GEPLANT
🚧 GET /api/v1/audit/entity/{id}          // GEPLANT
🚧 GET /api/v1/audit/user/{userId}        // GEPLANT
🚧 GET /api/v1/audit/search               // GEPLANT
```

### **Aktuelle Integration:**
```typescript
// AuditTrailViewer.svelte - NUR MOCK
import { auditApi } from '$lib/api/database'; // → Mock-API

// Keine Backend-APIs implementiert!
```

### **Fehlende Integration:**
- ❌ **Komplette Backend-API** - Keine Audit-Endpunkte implementiert
- ❌ **Real-time Audit** - Keine Live-Audit-Updates
- ❌ **Advanced Filtering** - Keine Backend-Suchfunktionen
- ❌ **Export Functions** - Keine Audit-Export-Funktionen

---

## 🔄 Mock-APIs die ersetzt werden müssen

### **Aktuelle Mock-Struktur:**
```typescript
// src/frontend/src/lib/api/database.ts
import { 
  patientApi as mockPatientApi,      // ✅ → REST API
  sessionApi as mockSessionApi,      // ⚠️ → REST API (Dummy → Real)
  transcriptApi as mockTranscriptApi, // 🚧 → REST API (Geplant)
  auditApi as mockAuditApi,          // 🚧 → REST API (Geplant)
} from './database-mock';
```

### **Ersetzungs-Priorität:**

#### **🔥 KRITISCH (Sofort):**
1. **patientApi** → `/api/v1/patients/*` (Backend implementiert)
2. **PerformanceMonitor** → `/api/system/performance` (Backend implementiert)

#### **⚠️ WICHTIG (Diese Woche):**
3. **sessionApi** → `/api/v1/sessions/*` (Backend Dummy → Real)
4. **Health Check** → `/health` (Backend implementiert)

#### **📋 GEPLANT (Nächste Wochen):**
5. **transcriptApi** → `/api/v1/transcripts/*` (Backend nicht implementiert)
6. **auditApi** → `/api/v1/audit/*` (Backend nicht implementiert)
7. **anonymizationApi** → `/api/v1/anonymization/*` (Backend nicht implementiert)

---

## 🚀 Implementierungs-Roadmap

### **Phase 1: Basis-Integration (Diese Woche)**
```typescript
// 1. JWT Authentication Service erstellen
class AuthService {
  async login(credentials): Promise<string> { /* JWT Token */ }
  async refreshToken(): Promise<string> { /* Refresh */ }
  getToken(): string | null { /* Current Token */ }
}

// 2. HTTP Client mit Authentication
class ApiClient {
  private baseUrl = 'http://localhost:5000';
  private authService = new AuthService();
  
  async request(endpoint: string, options: RequestInit) {
    const token = this.authService.getToken();
    return fetch(`${this.baseUrl}${endpoint}`, {
      ...options,
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
        ...options.headers
      }
    });
  }
}

// 3. Patients API Integration
export const patientsApi = {
  async getAll(): Promise<PatientDto[]> {
    const response = await apiClient.request('/api/v1/patients');
    return response.json();
  },
  
  async create(patient: CreatePatientRequest): Promise<PatientDto> {
    const response = await apiClient.request('/api/v1/patients', {
      method: 'POST',
      body: JSON.stringify(patient)
    });
    return response.json();
  }
};
```

### **Phase 2: System Health Integration (Diese Woche)**
```typescript
// 4. Performance Monitor Integration
// PerformanceMonitor.svelte
async function updatePerformanceMetrics() {
  try {
    const response = await fetch('/api/system/performance');
    const metrics = await response.json();
    
    cpuUsage = metrics.cpuUsage;
    ramUsage = metrics.ramUsage;
    gpuUsage = metrics.gpuUsage;
    gpuAcceleration = metrics.gpuAcceleration;
  } catch (error) {
    console.error('Failed to get performance metrics:', error);
    // Fallback zu Mock-Daten
  }
}

// 5. Health Check Integration
async function checkSystemHealth() {
  try {
    const response = await fetch('/health');
    const health = await response.json();
    systemStatus = health.status; // "Healthy"
  } catch (error) {
    systemStatus = "Unhealthy";
  }
}
```

### **Phase 3: Sessions Integration (Nächste Woche)**
```typescript
// 6. Sessions API (nach Backend-Implementierung)
export const sessionsApi = {
  async getByPatientId(patientId: string): Promise<SessionDto[]> {
    const response = await apiClient.request(`/api/v1/sessions/patient/${patientId}`);
    return response.json();
  },
  
  async create(session: CreateSessionRequest): Promise<SessionDto> {
    const response = await apiClient.request('/api/v1/sessions', {
      method: 'POST',
      body: JSON.stringify(session)
    });
    return response.json();
  }
};
```

### **Phase 4: Advanced Features (Später)**
```typescript
// 7. WebSocket für Real-time Updates
class RealtimeService {
  private ws: WebSocket;
  
  connect() {
    this.ws = new WebSocket('ws://localhost:5000/ws');
    this.ws.onmessage = (event) => {
      const data = JSON.parse(event.data);
      // Handle real-time updates
    };
  }
}

// 8. AI Service Integration (gRPC-Web)
class AiServiceClient {
  async transcribe(audioData: Uint8Array): Promise<TranscriptionResponse> {
    // gRPC-Web call to Python AI Service
  }
  
  async anonymize(text: string): Promise<AnonymizationResponse> {
    // gRPC-Web call for PII detection
  }
}
```

---

## 🔍 Fehlende Integrationen - Zusammenfassung

### **❌ Komplett fehlende Backend-APIs:**
1. **Transcripts API** - Keine Endpunkte implementiert
2. **Anonymization API** - Keine Endpunkte implementiert  
3. **Audit API** - Keine Endpunkte implementiert
4. **Security API** - Keine Endpunkte implementiert
5. **AI Service gRPC Client** - Keine .NET gRPC Integration

### **⚠️ Dummy-APIs die vervollständigt werden müssen:**
1. **Sessions API** - Nur Dummy-Responses, keine echten Daten
2. **Authentication** - Keine JWT-Implementierung
3. **Error Handling** - Keine strukturierte Fehlerbehandlung

### **🔄 Mock-APIs die ersetzt werden müssen:**
1. **Patient Management** - Mock → REST API (Backend bereit)
2. **System Performance** - Mock → REST API (Backend bereit)
3. **Health Monitoring** - Mock → REST API (Backend bereit)

### **🚀 Tauri Commands die implementiert werden müssen:**
1. **get_performance_metrics()** - System-Performance abrufen
2. **get_system_info()** - System-Informationen abrufen
3. **update_security_settings()** - Sicherheitseinstellungen verwalten
4. **manage_encryption_keys()** - Schlüsselverwaltung

---

## 🎯 Nächste Schritte

### **Sofort (diese Woche):**
1. ✅ **JWT Authentication Service** implementieren
2. ✅ **HTTP Client mit Auth** erstellen
3. ✅ **Patients API Integration** (Mock → REST)
4. ✅ **Performance Monitor Integration** (Mock → REST)

### **Kurzfristig (nächste Woche):**
5. ⚠️ **Sessions API vervollständigen** (Dummy → Real)
6. 🚧 **Transcripts Backend-API** implementieren
7. 🚧 **AI Service gRPC Client** erstellen

### **Mittelfristig (nächste Wochen):**
8. 🚧 **Anonymization Backend-API** implementieren
9. 🚧 **Audit Backend-API** implementieren
10. 🚧 **Security Backend-API** implementieren
11. 🚧 **Real-time Updates** (WebSocket/SSE)

**Diese Dokumentation wird bei jeder API-Änderung aktualisiert [D=C][DSU] und dient als zentrale Referenz für die Frontend-Backend-Integration.**
