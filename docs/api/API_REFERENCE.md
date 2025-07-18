<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy API Referenz

*Letzte Aktualisierung: 08.07.2025*

## Übersicht

Die MedEasy API bietet sichere Endpunkte für den Zugriff auf medizinische Daten unter strikter Einhaltung der Schweizer Datenschutzbestimmungen (nDSG) und medizinischer Sicherheitsstandards.

## Sicherheitsmerkmale [ZTS][PbD]

- **JWT-Authentifizierung**: Alle sensiblen Endpunkte erfordern JWT-Token
- **Rate-Limiting**: Schutz vor Brute-Force und DoS-Angriffen
- **Audit-Logging**: Vollständige Protokollierung aller Zugriffe [ATV]
- **Verschlüsselung**: Alle Daten werden mit AES-256 verschlüsselt [SP]
- **Anonymisierung**: Automatische Erkennung und Maskierung von PII [AIU]

## Basis-URL

```
https://api.medeasy.ch/api/v1
```

## Endpunkte

### Status

#### GET /status

Öffentlicher Endpunkt zur Überprüfung des API-Status.

**Antwort**: 200 OK
```json
{
  "status": "Operational",
  "timestamp": "2025-07-08T08:54:12Z"
}
```

### Patienten [PbD][EIV]

#### GET /patients

Liefert eine Liste aller Patienten (nur IDs und Versicherungsnummer-Hash).

**Erforderliche Berechtigung**: Authentifizierter Benutzer
**Rate-Limit**: Sensible Daten (10 Anfragen/Minute)

**Antwort**: 200 OK
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "insuranceNumberHash": "a1b2c3d4e5f6..."
  }
]
```

#### GET /patients/{id}

Liefert Details zu einem bestimmten Patienten.

**Erforderliche Berechtigung**: Authentifizierter Benutzer
**Rate-Limit**: Sensible Daten (10 Anfragen/Minute)
**Audit**: Vollständige Protokollierung des Zugriffs [ATV]

**Antwort**: 200 OK
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "encryptedName": "...",
  "insuranceNumberHash": "a1b2c3d4e5f6...",
  "dateOfBirth": "1980-01-01"
}
```

### Sessions [SK][EIV]

#### GET /sessions

Liefert eine Liste aller Sessions (nur IDs und Datum).

**Erforderliche Berechtigung**: Authentifizierter Benutzer
**Rate-Limit**: Sensible Daten (10 Anfragen/Minute)

**Antwort**: 200 OK
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sessionDate": "2025-07-08T08:30:00Z",
    "status": "Completed",
    "patientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }
]
```

#### GET /sessions/{id}

Liefert Details zu einer bestimmten Session.

**Erforderliche Berechtigung**: Authentifizierter Benutzer
**Rate-Limit**: Sensible Daten (10 Anfragen/Minute)
**Audit**: Vollständige Protokollierung des Zugriffs [ATV]

**Antwort**: 200 OK
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "sessionDate": "2025-07-08T08:30:00Z",
  "status": "Completed",
  "patientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "encryptedNotes": "...",
  "created": "2025-07-08T08:30:00Z",
  "createdBy": "dr.mueller@spital.ch",
  "lastModified": "2025-07-08T09:15:00Z",
  "lastModifiedBy": "dr.mueller@spital.ch"
}
```

### Transkripte [AIU][EIV]

#### GET /transcripts/{id}

Liefert Details zu einem bestimmten Transkript.

**Erforderliche Berechtigung**: Authentifizierter Benutzer
**Rate-Limit**: Sensible Daten (10 Anfragen/Minute)
**Audit**: Vollständige Protokollierung des Zugriffs [ATV]

**Antwort**: 200 OK
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "encryptedOriginalText": "...",
  "encryptedAnonymizedText": "...",
  "created": "2025-07-08T08:35:00Z",
  "createdBy": "system",
  "lastModified": "2025-07-08T08:36:00Z",
  "lastModifiedBy": "system"
}
```

### Anonymisierungs-Review-Queue [ARQ]

#### GET /anonymization-reviews

Liefert eine Liste aller Anonymisierungs-Review-Items mit Status "Pending".

**Erforderliche Berechtigung**: Authentifizierter Benutzer
**Rate-Limit**: Sensible Daten (10 Anfragen/Minute)

**Antwort**: 200 OK
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "transcriptId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "anonymizationConfidence": 0.75,
    "created": "2025-07-08T08:36:00Z"
  }
]
```

## Fehlerbehandlung [ECP][NSF]

Alle Fehler werden im standardisierten Format zurückgegeben:

```json
{
  "error": "Beschreibung des Fehlers",
  "statusCode": 400,
  "timestamp": "2025-07-08T08:54:12Z",
  "path": "/api/v1/patients/invalid-id",
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

## Health Checks [MPR]

### GET /health/live

Überprüft, ob die API aktiv ist.

**Antwort**: 200 OK
```json
{
  "status": "Healthy",
  "totalDuration": 12.34,
  "timestamp": "2025-07-08T08:54:12Z",
  "entries": [
    {
      "name": "self",
      "status": "Healthy",
      "duration": 0.12,
      "description": "API is running"
    }
  ]
}
```

### GET /health/ready

Überprüft, ob die API und alle Abhängigkeiten bereit sind.

**Antwort**: 200 OK
```json
{
  "status": "Healthy",
  "totalDuration": 45.67,
  "timestamp": "2025-07-08T08:54:12Z",
  "entries": [
    {
      "name": "database",
      "status": "Healthy",
      "duration": 12.34,
      "description": "Database connection is active"
    },
    {
      "name": "encryption",
      "status": "Healthy",
      "duration": 5.67,
      "description": "Encryption service is available"
    }
  ]
}
```

## Compliance-Hinweise [RA][DSC]

Diese API entspricht den folgenden Vorschriften:
- Schweizer nDSG (Datenschutzgesetz)
- DSGVO/GDPR (für EU-Kompatibilität)
- Medizinprodukteverordnung (MDR) für medizinische Software

## gRPC AI Service API [MLB][DSC][AIU]

Die MedEasy AI Service API ist über gRPC verfügbar und bietet sichere, hochperformante KI-Funktionen für die MedEasy-Anwendung.

### Basis-URL

```
grpc://ai-service.medeasy.ch:50051
```

### Service-Methoden

#### Transcribe

Transkribiert Audio zu Text mit obligatorischer Anonymisierung.

**Anfrage**:
```protobuf
message TranscriptionRequest {
  string request_id = 1;
  bytes audio_data = 2;
  string language_code = 3;
  bool allow_cloud_processing = 4;
  string session_id = 5;
  AuditInfo audit_info = 6;
}
```

**Antwort**:
```protobuf
message TranscriptionResponse {
  string request_id = 1;
  string text = 2;
  string original_text = 3;
  string language_code = 4;
  bool is_swiss_german = 5;
  bool swiss_german_warning = 6;
  float processing_time_seconds = 7;
  repeated Entity detected_entities = 8;
  bool cloud_processed = 9;
}
```

**Sicherheitsmerkmale**:
- [AIU] Obligatorische Anonymisierung
- [CT] Cloud-Transparenz
- [SDH] Schweizerdeutsch-Erkennung
- [ATV] Vollständiges Audit-Logging

#### AnalyzeText

Analysiert medizinischen Text mit KI-Provider-Kette und automatischen Fallbacks.

**Anfrage**:
```protobuf
message AnalysisRequest {
  string request_id = 1;
  string text = 2;
  string analysis_type = 3;
  map<string, string> options = 4;
  bool allow_cloud_processing = 5;
  string session_id = 6;
  AuditInfo audit_info = 7;
}
```

**Antwort**:
```protobuf
message AnalysisResponse {
  string request_id = 1;
  string result = 2;
  float processing_time_seconds = 3;
  string provider_used = 4;
  bool cloud_processed = 5;
  bool has_disclaimer = 6;
  string disclaimer_text = 7;
  MedicalData medical_data = 8;
}
```

**Sicherheitsmerkmale**:
- [PK] Provider-Kette mit Fallbacks
- [CT] Cloud-Transparenz
- [NDW] Diagnose-Disclaimer
- [ATV] Vollständiges Audit-Logging

#### ReviewAnonymization

Überprüft und genehmigt/lehnt Anonymisierungsentscheidungen ab.

**Anfrage**:
```protobuf
message ReviewRequest {
  string request_id = 1;
  repeated EntityDecision entity_decisions = 2;
  AuditInfo audit_info = 3;
}
```

**Antwort**:
```protobuf
message ReviewResponse {
  string request_id = 1;
  bool success = 2;
  repeated Entity updated_entities = 3;
  int32 remaining_review_count = 4;
}
```

**Sicherheitsmerkmale**:
- [ARQ] Anonymisierungs-Review-Queue
- [ATV] Vollständiges Audit-Logging

#### HealthCheck

Überprüft den Dienststatus und die Gesundheit der Komponenten.

**Anfrage**:
```protobuf
message HealthRequest {
  string request_id = 1;
  bool include_details = 2;
}
```

**Antwort**:
```protobuf
message HealthResponse {
  string request_id = 1;
  Status status = 2;
  repeated ComponentStatus components = 3;
  string timestamp = 4;
  string environment = 5;
  string version = 6;
}
```

**Sicherheitsmerkmale**:
- [ATV] Service-Überwachung und Audit
- [SF] Schweizer Datumsformat
- [DSC] Schweizer Datenschutz-Compliance

#### DetectSwissGerman

Erkennt Schweizerdeutsch-Dialekt in Texten.

**Anfrage**:
```protobuf
message SwissGermanRequest {
  string request_id = 1;
  string text = 2;
  bool include_details = 3;
}
```

**Antwort**:
```protobuf
message SwissGermanResponse {
  string request_id = 1;
  bool is_swiss_german = 2;
  float confidence_score = 3;
  repeated DialectMarker dialect_markers = 4;
  repeated string swiss_medical_terms = 5;
  float processing_time_seconds = 6;
}
```

**Sicherheitsmerkmale**:
- [SDH] Schweizerdeutsch-Erkennung
- [MFD] Schweizer medizinische Terminologie
- [ATV] Vollständiges Audit-Logging

#### GetServiceMetrics

Liefert Dienstmetriken und Audit-Trail-Statistiken.

**Anfrage**:
```protobuf
message MetricsRequest {
  string request_id = 1;
  bool include_provider_metrics = 2;
  bool include_anonymization_metrics = 3;
  bool include_swiss_german_metrics = 4;
}
```

**Antwort**:
```protobuf
message MetricsResponse {
  string request_id = 1;
  string service_version = 2;
  float uptime_seconds = 3;
  int32 total_requests = 4;
  string timestamp = 5;
  repeated ProviderMetrics provider_metrics = 6;
  AnonymizationMetrics anonymization_metrics = 7;
  SwissGermanMetrics swiss_german_metrics = 8;
  AuditMetrics audit_metrics = 9;
}
```

**Sicherheitsmerkmale**:
- [ATV] Audit-Trail für alle Operationen
- [DSC] Schweizer Datenschutz-Compliance
- [SF] Schweizer Datumsformat
- [PK] Provider-Ketten-Metriken
