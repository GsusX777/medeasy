<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy API Referenz

*Letzte Aktualisierung: 25.07.2025*  
*Status: ✅ Aktuell implementierte APIs*

## 🎯 Übersicht

Die MedEasy API bietet sichere Endpunkte für medizinische Datenverarbeitung unter strikter Einhaltung der Schweizer Datenschutzbestimmungen (nDSG) und medizinischer Sicherheitsstandards.

## 🔐 Sicherheitsmerkmale [ZTS][PbD]

- **JWT-Authentifizierung**: Alle sensiblen Endpunkte erfordern JWT-Token
- **Rate-Limiting**: Schutz vor Brute-Force (10 req/min für sensible Daten)
- **Audit-Logging**: Vollständige Protokollierung aller Zugriffe [ATV]
- **AES-256-GCM Verschlüsselung**: Alle Patientendaten verschlüsselt [SP]
- **SQLCipher**: Verschlüsselte SQLite-Datenbank [SP]
- **Automatische Anonymisierung**: PII-Erkennung unveränderlich aktiv [AIU]

## 🏗️ Architektur [CAM][MLB]

**Desktop-Anwendung (Lokal)** [CT]:
```
Frontend (Svelte + Tauri) ↔ Backend (.NET 8 API) ↔ SQLCipher DB
```

**Basis-URLs**:
```
# Entwicklung
http://localhost:5000/api/v1

# Produktion (Desktop)
http://127.0.0.1:5000/api/v1
```

**Wichtig**: MedEasy ist eine **Desktop-Anwendung** - keine Cloud-Übertragung von Patientendaten [DSC][CT].

---

## 📊 API Status

| Kategorie | Implementiert | Legacy | Geplant | Gesamt |
|-----------|---------------|--------|---------|--------|
| **Patients** | ✅ 4/4 (100%) | - | - | 4 |
| **Sessions** | ✅ 2/6 (33%) | ⚠️ 3 | 🚧 1 | 6 |
| **Transcripts** | ✅ 1/4 (25%) | - | 🚧 3 | 4 |
| **System** | ✅ 2/4 (50%) | - | 🚧 2 | 4 |

**Legende**: ✅ Implementiert | ⚠️ Legacy/Dummy | 🚧 Geplant

---

## 👥 Patients API ✅

**Vollständig implementiert** mit DTOs, Services, Validation, Encryption

### Authentifizierung
```
Authorization: Bearer <jwt_token>
Rate-Limit: 10 Anfragen/Minute (sensible Daten)
```

### Endpunkte

#### GET /api/v1/patients
**Zweck**: Alle Patienten abrufen  
**Audit**: Vollständig protokolliert [ATV]  
**Verschlüsselung**: Namen entschlüsselt für autorisierte Benutzer [EIV]

**Beispiel-Response**:
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
    "lastModified": "2025-07-25T09:00:00Z"
  }
]
```

#### GET /api/v1/patients/{id}
**Zweck**: Spezifischen Patienten abrufen  
**Parameter**: `id` (Guid) - Patient ID  
**Sicherheit**: Audit-Log für jeden Zugriff [ATV]

#### POST /api/v1/patients
**Zweck**: Neuen Patienten erstellen  
**Validation**: Schweizer Versicherungsnummer (XXX.XXXX.XXXX.XX) [SF]  
**Encryption**: Namen werden automatisch verschlüsselt [EIV]

**Beispiel-Request**:
```json
{
  "firstName": "Max",
  "lastName": "Mustermann",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "insuranceNumber": "756.1234.5678.90",
  "notes": "Optionale Notizen"
}
```

#### PUT /api/v1/patients/{id}
**Zweck**: Patienten aktualisieren  
**Flexibilität**: Alle Felder optional  
**Audit**: Änderungen vollständig protokolliert [ATV]

---

## 📅 Sessions API ✅

**Minimal API implementiert** mit Audit-Logging

### GET /api/v1/sessions
**Zweck**: Alle Sessions abrufen (Basis-Informationen)  
**Sicherheit**: Nur ID, Datum, Status, Patient-ID [PbD]

**Beispiel-Response**:
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

### GET /api/v1/sessions/{id}
**Zweck**: Session-Details abrufen  
**Audit**: Vollständige Protokollierung [ATV]  
**Verschlüsselung**: Notizen verschlüsselt gespeichert [EIV]

**Beispiel-Response**:
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

---

## 📝 Transcripts API ✅

**Read-Only implementiert** mit Anonymisierung

### GET /api/v1/transcripts/{id}
**Zweck**: Transkript abrufen  
**Anonymisierung**: PII automatisch erkannt und maskiert [AIU]  
**Confidence**: KI-Vertrauen in Anonymisierung (0.0-1.0)

**Beispiel-Response**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "sessionId": "123e4567-e89b-12d3-a456-426614174001",
  "encryptedContent": "[ENCRYPTED_BLOB]",
  "anonymizedContent": "Patient berichtet über Kopfschmerzen...",
  "anonymizationStatus": "Reviewed",
  "confidence": 0.95,
  "created": "2025-07-25T09:00:00Z"
}
```

---

## 🔍 Anonymization Reviews API ✅

**Review-Queue implementiert**

### GET /api/v1/anonymization-reviews
**Zweck**: Pendente Review-Items abrufen  
**Filter**: Nur Status "Pending"  
**Workflow**: Manuelle Überprüfung bei niedriger Confidence

**Beispiel-Response**:
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

---

## 📊 System API ✅

### GET /health
**Zweck**: Basis Health Check  
**Öffentlich**: Keine Authentifizierung erforderlich  
**Monitoring**: Für einfache Verfügbarkeitsprüfung

**Response**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-07-25T16:38:13.8950012Z"
}
```

### GET /api/system/performance
**Zweck**: Live System-Performance Metriken  
**Öffentlich**: Keine Authentifizierung erforderlich (temporär für Health-Monitor Testing)  
**Monitoring**: Detaillierte System-Überwachung für medizinische Software-Stabilität [PSF]

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

**Felder Erklärung**:
- `cpuUsage`: CPU-Auslastung in Prozent (0-100)
- `ramUsage`: RAM-Auslastung in Prozent (0-100)
- `totalRamMb`: Gesamter verfügbarer RAM in MB
- `usedRamMb`: Verwendeter RAM in MB
- `gpuUsage`: GPU-Auslastung in Prozent (nullable)
- `gpuAcceleration`: Ist GPU-Beschleunigung verfügbar
- `diskIo`: Kombinierte Disk I/O in MB/s
- `networkLatency`: Netzwerk-Latenz in ms

### GET /api/system/info
**Zweck**: System-Informationen für Diagnostik  
**Öffentlich**: Keine Authentifizierung erforderlich (temporär)  
**Monitoring**: Erweiterte System-Details

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

### GET /api/v1/status
**Zweck**: Legacy System-Status (deprecated)  
**Status**: Wird durch /health ersetzt  
**Monitoring**: Für Rückwärtskompatibilität

**Response**:
```json
{
  "status": "Operational",
  "timestamp": "2025-07-25T09:38:15Z"
}
```

---

## ⚠️ Legacy APIs (Deprecated)

### Sessions Controller ⚠️
**Base URL**: `/api/sessions` (ohne v1)  
**Status**: Dummy-Implementation, wird durch Minimal API ersetzt  
**Verwendung**: Nicht für Produktion empfohlen

---

## 🚧 Geplante APIs

### Sessions CRUD
- `POST /api/v1/sessions` - Session erstellen mit SessionDto
- `PUT /api/v1/sessions/{id}` - Session aktualisieren
- `DELETE /api/v1/sessions/{id}` - Session löschen

### Transcripts CRUD
- `POST /api/v1/transcripts` - Transkript hochladen/erstellen
- `PUT /api/v1/transcripts/{id}` - Transkript aktualisieren
- `DELETE /api/v1/transcripts/{id}` - Transkript löschen

### AI Services
- `POST /api/v1/ai/transcribe` - Audio → Text (Whisper)
- `POST /api/v1/ai/anonymize` - Text-Anonymisierung
- `POST /api/v1/ai/analyze` - Medizinische Analyse

### Health Checks
- `GET /health/live` - Liveness Probe
- `GET /health/ready` - Readiness Probe

---

## 🚨 Fehlerbehandlung

### Standard HTTP Status Codes
- `200 OK` - Erfolgreich
- `201 Created` - Ressource erstellt
- `400 Bad Request` - Ungültige Eingabe
- `401 Unauthorized` - Nicht authentifiziert
- `404 Not Found` - Ressource nicht gefunden
- `500 Internal Server Error` - Serverfehler

### Error Response Format
```json
{
  "message": "Beschreibung des Fehlers"
}
```

### Validation Errors
```json
{
  "errors": {
    "firstName": ["Vorname ist erforderlich"],
    "insuranceNumber": ["Format: XXX.XXXX.XXXX.XX"]
  }
}
```

---

## 🇨🇭 Schweizer Compliance [SF]

### Versicherungsnummer
- **Format**: `XXX.XXXX.XXXX.XX`
- **Validation**: Regex `^\d{3}\.\d{4}\.\d{4}\.\d{2}$`
- **Beispiel**: `756.1234.5678.90`

### Datumsformate
- **API**: ISO-8601 (`2025-07-25T09:38:15Z`)
- **UI**: Schweizer Format (`25.07.2025`)
- **Database**: DD.MM.YYYY String

### Datenschutz (nDSG)
- **Verschlüsselung**: AES-256-GCM für alle PII
- **Anonymisierung**: Automatisch, nicht deaktivierbar
- **Audit**: Vollständige Nachverfolgung aller Zugriffe
- **Lokale Verarbeitung**: Keine Cloud-Übertragung

---

## 🔧 Entwickler-Hinweise

### JSON Format
**Konfiguration**: `camelCase` für alle JSON-Responses
```json
{
  "firstName": "Max",           // ✅ camelCase
  "lastName": "Mustermann",     // ✅ camelCase
  "dateOfBirth": "1990-01-15T00:00:00Z"
}
```

### Rate Limiting
- **Sensible Endpunkte**: 10 Anfragen/Minute
- **Öffentliche Endpunkte**: 100 Anfragen/Minute
- **Header**: `X-RateLimit-Remaining`, `X-RateLimit-Reset`

### Audit Logging
Alle API-Zugriffe werden protokolliert:
```
API Access: GET /api/v1/patients/123 by user@example.com at 2025-07-25T09:38:15Z
```

---

**Compliance**: Diese API entspricht allen MedEasy-Projektregeln [PSF][ZTS][SF][CAM][EIV][SP][AIU][ATV] und Schweizer Datenschutzanforderungen (nDSG).
