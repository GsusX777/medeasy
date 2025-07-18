<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Architektur

*Letzte Aktualisierung: 08.07.2025*

## Clean Architecture [CAM]

MedEasy folgt strikt dem Clean Architecture-Prinzip, wobei die medizinische Logik vollständig von der technischen Implementierung getrennt ist. Die Abhängigkeiten zeigen immer nach innen: UI → Application → Domain ← Infrastructure.

![Clean Architecture](https://raw.githubusercontent.com/jasontaylordev/CleanArchitecture/main/docs/imgs/clean-architecture.png)

## Schichten [CAS]

### Domain Layer (`MedEasy.Domain`)

Der Domain Layer ist das Herzstück der Anwendung und enthält die Geschäftslogik und Entitäten. Er hat **keine Abhängigkeiten** zu anderen Schichten oder externen Bibliotheken.

- **Entitäten**: Patient, Session, Transcript
- **Enums**: ReviewStatus, SessionStatus
- **Wert-Objekte**: Medizinische Messwerte, Versicherungsinformationen
- **Domain Services**: Medizinische Berechnungen, Validierungsregeln

### Application Layer (`MedEasy.Application`)

Der Application Layer orchestriert den Datenfluss zwischen UI und Domain und implementiert die Anwendungsfälle.

- **CQRS mit MediatR**: Trennung von Befehlen und Abfragen [CQA]
- **DTOs**: Datenübertragungsobjekte für die API
- **Validierung**: Validierung von Eingabedaten
- **Mapper**: AutoMapper-Profile für Entity-DTO-Konvertierung

### Infrastructure Layer (`MedEasy.Infrastructure`)

Der Infrastructure Layer implementiert die technischen Details und Integrationen mit externen Systemen.

- **Datenbank**: SQLCipher mit AES-256-Verschlüsselung [SP]
- **Verschlüsselung**: AES-256-CBC für Patientendaten [ES]
- **Audit-Logging**: Vollständige Protokollierung aller Änderungen [ATV]
- **Anonymisierung**: NER-basierte Erkennung und Maskierung von PII [AR]

### API Layer (`MedEasy.API`)

Der API Layer stellt die Schnittstelle für Frontend-Anwendungen bereit.

- **Minimal API**: Leichtgewichtige REST-API mit .NET 8
- **JWT-Authentifizierung**: Sichere Authentifizierung und Autorisierung [ZTS]
- **Swagger/OpenAPI**: API-Dokumentation
- **Health Checks**: Überwachung der Systemgesundheit [MPR]

### AI Layer (`MedEasy.AI`)

Der AI Layer implementiert die KI-Funktionen und kommuniziert über gRPC mit dem Backend.

- **Python 3.11**: Basis für KI-Funktionen
- **FastAPI**: REST-API für KI-Dienste
- **gRPC**: Hochperformante Kommunikation zwischen .NET und Python [MLB]
- **Multi-Provider**: Fallback-Mechanismen für KI-Dienste [PK]

### Frontend Layer (`src/frontend`)

Der Frontend Layer implementiert die Benutzeroberfläche als Desktop-Anwendung mit Tauri und Svelte.

#### Tauri Backend (`src-tauri`)

- **Rust**: Sicheres Backend für die Desktop-Anwendung
- **SQLCipher**: Verschlüsselte lokale Datenspeicherung [SP]
- **Tauri Commands**: Bridge zwischen Frontend und Backend [MLB]
- **Audit-Logging**: Protokollierung aller Benutzeraktionen [ATV]

#### Svelte Frontend (`src`)

- **Svelte 4**: Reaktives UI-Framework
- **TypeScript**: Typsichere Entwicklung [TR]
- **Stores**: Zustandsverwaltung mit Svelte Stores [SK]
- **Komponenten**: Wiederverwendbare UI-Komponenten
  - `ProcessingLocationIndicator`: Zeigt Verarbeitungsort (lokal/cloud) [CT]
  - `AnonymizationNotice`: Informiert über unveränderliche Anonymisierung [AIU]
  - `SwissGermanAlert`: Warnt bei Schweizerdeutsch-Erkennung [SDH]
  - `SessionRecorder`: Steuert Aufnahme mit Audit-Logging [ATV]
  - `TranscriptViewer`: Zeigt anonymisierte Transkripte [AIU]
  - `SecuritySettings`: Sicherheitseinstellungen ohne Anonymisierungs-Toggle [AIU]

## Querschnittliche Belange

### Sicherheit [ZTS][PbD]

- **Verschlüsselung**: AES-256 für alle sensiblen Daten [SP]
- **Authentifizierung**: JWT mit sicheren Validierungsparametern
- **Autorisierung**: Rollenbasierte Zugriffssteuerung
- **Audit-Logging**: Vollständige Protokollierung aller Zugriffe [ATV]
- **Rate-Limiting**: Schutz vor Brute-Force und DoS-Angriffen

### Datenschutz [PbD][DSC]

- **Anonymisierung**: Automatische Erkennung und Maskierung von PII [AIU]
- **Review-Prozess**: Manuelle Überprüfung unsicherer Anonymisierungen [ARQ]
- **Datensparsamkeit**: Nur notwendige Daten werden gespeichert
- **Einwilligung**: Explizite Einwilligung für Cloud-Verarbeitung [CT]

### Fehlerbehandlung [ECP][NSF]

- **Zentrale Fehlerbehandlung**: Middleware für einheitliche Fehlerbehandlung
- **Kontexterhaltung**: Vollständiger Kontext für Diagnose
- **Sichere Fehlerantworten**: Keine sensiblen Informationen in Fehlermeldungen
- **Korrelations-IDs**: Nachverfolgung von Fehlern über Systemgrenzen hinweg

### Resilience [MPR]

- **Health Checks**: Überwachung der Systemgesundheit
- **Circuit Breaker**: Schutz vor Kaskadenfehlern
- **Retry-Policies**: Automatische Wiederholungsversuche bei transienten Fehlern
- **Multi-Provider**: Fallback-Mechanismen für KI-Dienste [PK]

## Kommunikationsfluss

```
Frontend (Tauri + Svelte)
       ↓ ↑
       HTTP/REST + JWT
       ↓ ↑
Backend (.NET 8 Minimal API)
       ↓ ↑
       gRPC
       ↓ ↑
AI Services (Python + FastAPI)
```

## Datenfluss

1. **Aufnahme**: Audio wird lokal aufgezeichnet
2. **Transkription**: Audio wird zu Text transkribiert (lokal oder Cloud) [WMM]
3. **Anonymisierung**: PII wird automatisch erkannt und maskiert [AIU]
4. **Review**: Unsichere Anonymisierungen werden in die Review-Queue gestellt [ARQ]
5. **Speicherung**: Anonymisierte Daten werden verschlüsselt gespeichert [SP]
6. **Analyse**: Medizinische Informationen werden extrahiert und strukturiert
7. **Präsentation**: Aufbereitete Daten werden dem Arzt präsentiert

## Technologie-Stack [TSF]

- **Backend**: .NET 8 mit Minimal API
- **Datenbank**: SQLite mit SQLCipher (AES-256)
- **ORM**: Entity Framework Core
- **API**: REST mit JWT-Authentifizierung
- **Frontend**: 
  - **Desktop**: Tauri 1.5 (Rust) + Svelte 4 (TypeScript)
  - **UI-Framework**: Svelte 4 mit TypeScript
  - **State Management**: Svelte Stores
  - **Build-System**: Vite + SvelteKit
- **KI**: Python 3.11 + FastAPI + gRPC
- **Deployment**: Docker + Kubernetes

## Compliance [RA][DSC]

Die Architektur wurde entwickelt, um folgende Vorschriften zu erfüllen:
- Schweizer nDSG (Datenschutzgesetz)
- DSGVO/GDPR (für EU-Kompatibilität)
- Medizinprodukteverordnung (MDR) für medizinische Software
