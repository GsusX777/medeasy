<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy API Contracts

*Letzte Aktualisierung: 25.07.2025*  
*Status: ✅ Nur implementierte APIs*

Diese Dokumentation definiert die **tatsächlich implementierten** API-Endpunkte des MedEasy .NET Backend.

## 🔐 Authentifizierung [ZTS]

**Alle API-Endpunkte erfordern JWT Bearer Token**:
```
Authorization: Bearer <jwt_token>
```

## 📊 JSON Format [ZTS]

**Konfiguration**: `camelCase` für alle JSON-Responses
```json
{
  "firstName": "Max",           // ✅ camelCase
  "lastName": "Mustermann",     // ✅ camelCase
  "dateOfBirth": "1990-01-15T00:00:00Z"
}
```

---

## 📊 API Status Übersicht

| Status | Bedeutung | Beispiel |
|--------|-----------|----------|
| ✅ **Implementiert** | Vollständig funktional | Patients CRUD |
| ⚠️ **Legacy/Dummy** | Funktioniert, aber veraltet | Sessions Controller |
| 🚧 **Geplant** | Noch nicht implementiert | AI Services |

---

## 👥 Patients API ✅

**Base URL**: `/api/v1/patients`  
**Status**: ✅ **Vollständig implementiert** mit DTOs, Services, Validation

### GET /api/v1/patients

**Beschreibung**: Ruft alle Patienten ab

**Response**: `PatientDto[]`
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "firstName": "Max",
    "lastName": "Mustermann",
    "dateOfBirth": "1990-01-15T00:00:00Z",
    "dateOfBirthFormatted": "15.01.1990",
    "insuranceNumberMasked": "756.1234.XXXX.XX",
    "created": "2025-07-25T09:00:00Z",
    "lastModified": "2025-07-25T09:00:00Z",
    "createdBy": "system",
    "lastModifiedBy": "system"
  }
]
```

**Status Codes**:
- `200 OK`: Erfolgreich
- `401 Unauthorized`: Nicht authentifiziert
- `500 Internal Server Error`: Serverfehler

---

### GET /api/v1/patients/{id}

**Beschreibung**: Ruft einen spezifischen Patienten ab

**Parameter**:
- `id` (Guid): Patient ID

**Response**: `PatientDto`
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "firstName": "Max",
  "lastName": "Mustermann",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "dateOfBirthFormatted": "15.01.1990",
  "insuranceNumberMasked": "756.1234.XXXX.XX",
  "created": "2025-07-25T09:00:00Z",
  "lastModified": "2025-07-25T09:00:00Z",
  "createdBy": "system",
  "lastModifiedBy": "system"
}
```

**Status Codes**:
- `200 OK`: Patient gefunden
- `404 Not Found`: Patient nicht gefunden
- `401 Unauthorized`: Nicht authentifiziert
- `500 Internal Server Error`: Serverfehler

---

### POST /api/v1/patients

**Beschreibung**: Erstellt einen neuen Patienten

**Request Body**: `CreatePatientRequest`
```json
{
  "firstName": "Max",
  "lastName": "Mustermann",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "insuranceNumber": "756.1234.5678.90",
  "notes": "Optionale Notizen"
}
```

**Validierung**:
- `firstName`: Required, 1-100 Zeichen
- `lastName`: Required, 1-100 Zeichen
- `dateOfBirth`: Required, gültiges Datum (0-150 Jahre)
- `insuranceNumber`: Required, Format `XXX.XXXX.XXXX.XX`

**Response**: `PatientDto` (wie GET)

**Status Codes**:
- `201 Created`: Patient erfolgreich erstellt
- `400 Bad Request`: Ungültige Eingabedaten
- `401 Unauthorized`: Nicht authentifiziert
- `500 Internal Server Error`: Serverfehler

---

### PUT /api/v1/patients/{id}

**Beschreibung**: Aktualisiert einen Patienten

**Parameter**:
- `id` (Guid): Patient ID

**Request Body**: `UpdatePatientRequest`
```json
{
  "firstName": "Max",                    // Optional
  "lastName": "Mustermann",              // Optional
  "dateOfBirth": "1990-01-15T00:00:00Z", // Optional
  "insuranceNumber": "756.1234.5678.90", // Optional
  "notes": "Aktualisierte Notizen"       // Optional
}
```

**Response**: `PatientDto` (wie GET)

**Status Codes**:
- `200 OK`: Patient erfolgreich aktualisiert
- `404 Not Found`: Patient nicht gefunden
- `400 Bad Request`: Ungültige Eingabedaten
- `401 Unauthorized`: Nicht authentifiziert
- `500 Internal Server Error`: Serverfehler

---

## 📊 System APIs ✅

### GET /health

**Beschreibung**: Basis Health Check (öffentlich, keine Authentifizierung)

**Request**: Keine Parameter

**Response**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-07-25T16:38:13.8950012Z"
}
```

**Status Codes**:
- `200 OK`: System ist gesund
- `500 Internal Server Error`: System-Probleme

---

### GET /api/system/performance

**Beschreibung**: Live System-Performance Metriken für medizinische Software-Stabilität [PSF]
**Authentifizierung**: Keine (temporär für Health-Monitor Testing)

**Request**: Keine Parameter

**Response**:
```json
{
  "timestamp": "2025-07-25T16:45:23.1234567Z",
  "cpuUsage": 23.5,
  "cpuName": "Windows CPU (8 cores)",
  "cpuCores": 8,
  "ramUsage": 67.2,
  "totalRamMb": 16384,
  "usedRamMb": 11008,
  "gpuUsage": 0.0,
  "gpuAcceleration": false,
  "gpuName": "Windows GPU (WMI disabled)",
  "diskIo": 0.0,
  "networkLatency": 1
}
```

**Response Schema**:
```typescript
interface SystemPerformanceDto {
  timestamp: string;           // ISO 8601 DateTime
  cpuUsage: number;           // CPU-Auslastung 0-100%
  cpuName: string;            // CPU-Bezeichnung
  cpuCores: number;           // Anzahl CPU-Kerne
  ramUsage: number;           // RAM-Auslastung 0-100%
  totalRamMb: number;         // Gesamter RAM in MB
  usedRamMb: number;          // Verwendeter RAM in MB
  gpuUsage: number | null;    // GPU-Auslastung 0-100% (nullable)
  gpuAcceleration: boolean;   // GPU-Beschleunigung verfügbar
  gpuName: string;            // GPU-Bezeichnung
  diskIo: number;             // Kombinierte Disk I/O in MB/s
  networkLatency: number;     // Netzwerk-Latenz in ms
}
```

**Status Codes**:
- `200 OK`: Performance-Daten erfolgreich abgerufen
- `500 Internal Server Error`: Fehler beim Abrufen der System-Metriken

---

### GET /api/system/info

**Beschreibung**: System-Informationen für Diagnostik
**Authentifizierung**: Keine (temporär für Health-Monitor Testing)

**Request**: Keine Parameter

**Response**:
```json
{
  "operatingSystem": "Microsoft Windows 11.0.22631",
  "architecture": "X64",
  "processorCount": 8,
  "machineName": "DESKTOP-ABC123",
  "userName": "user",
  "workingSet": 123456789,
  "version": "8.0.11",
  "is64BitOperatingSystem": true,
  "is64BitProcess": true
}
```

**Status Codes**:
- `200 OK`: System-Informationen erfolgreich abgerufen
- `500 Internal Server Error`: Fehler beim Abrufen der System-Informationen

---

### GET /api/v1/status (Legacy)

**Beschreibung**: Legacy System-Status (deprecated, wird durch /health ersetzt)
**Authentifizierung**: Keine

---

## 📅 Sessions API (Minimal API) ✅

**Base URL**: `/api/v1/sessions`  
**Status**: ✅ **Implementiert** (Minimal API mit Audit)

### GET /api/v1/sessions

**Beschreibung**: Ruft alle Sessions ab (nur Basis-Informationen)

**Response**:
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "sessionDate": "2025-07-25T09:00:00Z",
    "status": "Completed",
    "patientId": "123e4567-e89b-12d3-a456-426614174001"
  }
]
```

**Status Codes**:
- `200 OK`: Sessions erfolgreich abgerufen
- `401 Unauthorized`: Nicht authentifiziert

---

### GET /api/v1/sessions/{id}

**Beschreibung**: Ruft eine spezifische Session ab (mit Audit-Log)

**Parameter**:
- `id` (Guid): Session ID

**Response**: `Session` Entity
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "patientId": "123e4567-e89b-12d3-a456-426614174001",
  "sessionDate": "2025-07-25T09:00:00Z",
  "startTime": "09:00:00",
  "endTime": "09:30:00",
  "status": "Completed",
  "encryptedNotes": "[ENCRYPTED_BLOB]",
  "created": "2025-07-25T09:00:00Z",
  "lastModified": "2025-07-25T09:00:00Z"
}
```

**Status Codes**:
- `200 OK`: Session gefunden
- `404 Not Found`: Session nicht gefunden
- `401 Unauthorized`: Nicht authentifiziert

---

## 📝 Transcripts API ✅

**Base URL**: `/api/v1/transcripts`  
**Status**: ✅ **Implementiert** (Minimal API mit Audit)

### GET /api/v1/transcripts/{id}

**Beschreibung**: Ruft ein spezifisches Transkript ab (mit Audit-Log)

**Parameter**:
- `id` (Guid): Transcript ID

**Response**: `Transcript` Entity
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "sessionId": "123e4567-e89b-12d3-a456-426614174001",
  "encryptedContent": "[ENCRYPTED_BLOB]",
  "anonymizedContent": "Patient berichtet über Kopfschmerzen...",
  "anonymizationStatus": "Reviewed",
  "confidence": 0.95,
  "created": "2025-07-25T09:00:00Z",
  "lastModified": "2025-07-25T09:00:00Z"
}
```

**Status Codes**:
- `200 OK`: Transkript gefunden
- `404 Not Found`: Transkript nicht gefunden
- `401 Unauthorized`: Nicht authentifiziert

---

## 🔍 Anonymization Reviews API ✅

**Base URL**: `/api/v1/anonymization-reviews`  
**Status**: ✅ **Implementiert** (Minimal API)

### GET /api/v1/anonymization-reviews

**Beschreibung**: Ruft alle pendenten Review-Items ab

**Response**:
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "transcriptId": "123e4567-e89b-12d3-a456-426614174001",
    "anonymizationConfidence": 0.85,
    "created": "2025-07-25T09:00:00Z"
  }
]
```

**Status Codes**:
- `200 OK`: Review-Items erfolgreich abgerufen
- `401 Unauthorized`: Nicht authentifiziert

---

## 📅 Sessions API (Legacy Controller) ⚠️

**Base URL**: `/api/sessions`  
**Status**: ⚠️ **Legacy/Dummy** - Wird durch Minimal API ersetzt

**Base URL**: `/api/sessions`

### GET /api/sessions/patient/{patientId}

**Beschreibung**: Ruft Sessions eines Patienten ab

**⚠️ Status**: Dummy-Implementation

**Response**:
```json
{
  "message": "Sessions für Patient {patientId} abgerufen"
}
```

### POST /api/sessions

**Request**: `CreateSessionRequest`
```json
{
  "patientId": "123e4567-e89b-12d3-a456-426614174000",
  "reason": "Routinekontrolle"
}
```

**Response**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "message": "Session erfolgreich erstellt"
}
```

---

## 🚧 Geplante APIs (Noch nicht implementiert)

### Sessions CRUD ✅→🚧
```
🚧 POST /api/v1/sessions              - Create mit SessionDto
🚧 PUT /api/v1/sessions/{id}          - Update Session
🚧 DELETE /api/v1/sessions/{id}       - Delete Session
🚧 GET /api/v1/sessions/patient/{id}  - Sessions by Patient (v1)
```

### Transcripts CRUD 🚧
```
🚧 POST /api/v1/transcripts           - Upload/Create Transcript
🚧 PUT /api/v1/transcripts/{id}       - Update Transcript
🚧 DELETE /api/v1/transcripts/{id}    - Delete Transcript
```

### AI Services 🚧
```
🚧 POST /api/v1/ai/transcribe         - Audio → Text (Whisper)
🚧 POST /api/v1/ai/anonymize          - Text Anonymisierung
🚧 POST /api/v1/ai/analyze            - Medical Analysis
```

### Anonymization Reviews CRUD 🚧
```
🚧 POST /api/v1/anonymization-reviews/{id}/approve
🚧 POST /api/v1/anonymization-reviews/{id}/reject
🚧 PUT /api/v1/anonymization-reviews/{id}
```

### Health Checks 🚧
```
🚧 GET /health/live                   - Liveness Probe
🚧 GET /health/ready                  - Readiness Probe
```

---

## 🔧 Error Handling ✅

### POST /error

**Beschreibung**: Globaler Error Handler (intern)

**Response**: `ProblemDetails`
```json
{
  "title": "Ein interner Serverfehler ist aufgetreten.",
  "status": 500,
  "instance": "/api/v1/patients",
  "detail": null
}
```

---

## 📋 DTO Definitionen

### PatientDto
```typescript
interface PatientDto {
  id: string;                         // Guid als string
  firstName: string;                  // Entschlüsselt
  lastName: string;                   // Entschlüsselt
  dateOfBirth: string;                // ISO-8601
  dateOfBirthFormatted?: string;      // DD.MM.YYYY [SF]
  insuranceNumberMasked: string;      // XXX.XXXX.XXXX.## [AIU]
  notes?: string;
  created: string;                    // ISO-8601
  lastModified: string;               // ISO-8601
  createdBy: string;                  // Username
  lastModifiedBy: string;             // Username
}
```

### CreatePatientRequest
```typescript
interface CreatePatientRequest {
  firstName: string;                  // Required, 1-100 chars
  lastName: string;                   // Required, 1-100 chars
  dateOfBirth: string;                // Required, ISO-8601
  insuranceNumber: string;            // Required, XXX.XXXX.XXXX.XX
  notes?: string;                     // Optional
}
```

### UpdatePatientRequest
```typescript
interface UpdatePatientRequest {
  firstName?: string;                 // Optional, 1-100 chars
  lastName?: string;                  // Optional, 1-100 chars
  dateOfBirth?: string;               // Optional, ISO-8601
  insuranceNumber?: string;           // Optional, XXX.XXXX.XXXX.XX
  notes?: string;                     // Optional
}
```

### SessionDto (Minimal API)
```typescript
interface Session {
  id: string;                         // Guid als string
  patientId: string;                  // Foreign Key
  sessionDate: string;                // ISO-8601
  startTime?: string;                 // HH:MM:SS
  endTime?: string;                   // HH:MM:SS
  status: string;                     // Enum als string
  encryptedNotes?: string;            // Base64 encrypted blob
  created: string;                    // ISO-8601
  lastModified: string;               // ISO-8601
}
```

### TranscriptDto (Minimal API)
```typescript
interface Transcript {
  id: string;                         // Guid als string
  sessionId: string;                  // Foreign Key
  encryptedContent?: string;          // Base64 encrypted blob
  anonymizedContent?: string;         // PII-freier Text
  anonymizationStatus: string;        // Enum: Pending, Reviewed, Approved
  confidence: number;                 // 0.0 - 1.0
  created: string;                    // ISO-8601
  lastModified: string;               // ISO-8601
}
```

### AnonymizationReviewItem (Minimal API)
```typescript
interface AnonymizationReviewItem {
  id: string;                         // Guid als string
  transcriptId: string;               // Foreign Key
  anonymizationConfidence: number;    // 0.0 - 1.0
  created: string;                    // ISO-8601
}
```

---

## 🚨 Fehlerbehandlung

**Standard Error Response**:
```json
{
  "message": "Beschreibung des Fehlers"
}
```

**Validation Error Response**:
```json
{
  "errors": {
    "firstName": ["Vorname ist erforderlich"],
    "insuranceNumber": ["Versicherungsnummer muss im Format XXX.XXXX.XXXX.XX sein"]
  }
}
```

---

**Compliance**: Alle APIs entsprechen den MedEasy-Projektregeln [ZTS], [ATV], [SP], [AIU], [SF] und Schweizer Datenschutzanforderungen.
