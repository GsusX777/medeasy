<!-- â€žDer Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein â€“ ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy [PSF][PbD][RA]

MedEasy ist eine sichere medizinische Software für die automatisierte Transkription und Anonymisierung von Arzt-Patienten-Gesprächen, speziell entwickelt für den Schweizer Gesundheitsmarkt.

![MedEasy Logo](docs/assets/logo.png)

## Projektübersicht [MDL]

MedEasy unterstützt Ärzte bei der effizienten Dokumentation von Patientengesprächen durch:

- **Automatische Transkription** von Arzt-Patienten-Gesprächen mit Whisper [WMM]
- **Zuverlässige Anonymisierung** sensibler Patientendaten [AIU]
- **KI-gestützte Analyse** für Diagnosevorschläge (mit Arztbestätigung) [DK]
- **Sichere Datenspeicherung** mit Ende-zu-Ende-Verschlüsselung [SP]
- **Schweizer Compliance** mit nDSG, DSGVO und MDR [SC][DSC]

## Technologie-Stack [TSF]

- **Backend**: .NET 8 mit Clean Architecture
- **KI-Dienste**: Python 3.11 mit FastAPI und gRPC
- **Frontend**: Tauri 1.5 mit Svelte 4
- **Datenbank**: SQLite mit SQLCipher (AES-256)
- **Deployment**: Docker und Kubernetes

## Projektstruktur [CAS]

```
medeasy/
¦
+-- .windsurf/              # Windsurf AI-Assistenten-Konfiguration
+-- docs/                   # Projektdokumentation
+-- src/                    # Quellcode
+-- tests/                  # Tests
+-- config/                 # Konfigurationsdateien
+-- tools/scripts/          # Entwicklungs- und Deployment-Skripte
+-- data/test-samples/      # Testdaten
+-- deployment/             # Deployment-Konfigurationen
+-- database/               # Datenbank-Migrationen und -Schemas
```

Detaillierte Informationen zur Struktur finden Sie in den jeweiligen README-Dateien der Unterverzeichnisse.

## Architektur [CAM][DD]

MedEasy folgt einer strengen Clean Architecture mit klarer Trennung der Verantwortlichkeiten:

1. **Domain Layer**: Enthält die Geschäftslogik und Entitäten ohne externe Abhängigkeiten
2. **Application Layer**: Orchestriert Anwendungsfälle mit CQRS-Pattern
3. **Infrastructure Layer**: Implementiert technische Concerns wie Datenbank und externe Dienste
4. **API Layer**: REST-API mit JWT-Authentifizierung
5. **AI Layer**: Python-basierte KI-Dienste für Transkription und Analyse

Alle Abhängigkeiten zeigen nach innen (Domain Layer).

## Sicherheit [ZTS][PbD]

MedEasy implementiert umfassende Sicherheitsmaßnahmen:

- **Datenverschlüsselung**: AES-256 für alle Patientendaten [SP]
- **Unveränderliche Anonymisierung**: Kann niemals deaktiviert werden [AIU]
- **Audit-Logging**: Alle Datenzugriffe werden protokolliert [ATV]
- **Zero-Trust-Architektur**: Keine impliziten Vertrauensbeziehungen
- **Sichere Authentifizierung**: JWT mit kurzer Lebensdauer und Refresh-Tokens

## Schweiz-spezifische Funktionen [SC][MFD]

- **Spracherkennung**: Optimiert für Hochdeutsch und Schweizerdeutsch [SDH]
- **Schweizer Formate**: Unterstützung für Schweizer Datumsformat (DD.MM.YYYY) und Versicherungsnummern
- **Schweizer Fachbegriffe**: Verwendung korrekter medizinischer Terminologie (DE-CH)
- **Compliance**: Vollständige Einhaltung des nDSG und anderer Schweizer Vorschriften [DSC]

## Entwicklung [DM]

### Voraussetzungen

- .NET SDK 8.0 oder höher
- Python 3.11 oder höher
- Node.js 18 oder höher
- Docker (für lokale Entwicklung)

### Einrichtung der Entwicklungsumgebung

```bash
# Repository klonen
git clone https://github.com/yourusername/medeasy.git
cd medeasy

# Entwicklungsumgebung einrichten
./tools/scripts/development/setup-dev-env.ps1
```

### Ausführen der Anwendung

```bash
# Backend starten
cd src/MedEasy.API
dotnet run

# KI-Dienste starten
cd src/MedEasy.AI
uvicorn api.main:app --reload

# Frontend starten
cd src/MedEasy.Frontend
npm run dev
```

### Tests ausführen [KP100]

```bash
# Alle Tests ausführen
./tools/scripts/testing/run-unit-tests.ps1
./tools/scripts/testing/run-integration-tests.ps1
./tools/scripts/testing/run-e2e-tests.ps1

# Sicherheitstests ausführen
./tools/scripts/testing/security-scan.ps1
```

## Deployment [ZTS]

MedEasy kann in verschiedenen Umgebungen bereitgestellt werden:

```bash
# Entwicklungsumgebung
./deployment/scripts/deploy.sh dev

# Staging-Umgebung
./deployment/scripts/deploy.sh staging

# Produktionsumgebung
./deployment/scripts/deploy.sh prod
```

Weitere Informationen finden Sie in der [Deployment-Dokumentation](deployment/README.md).

## Dokumentation [DM]

Umfassende Dokumentation ist verfügbar unter:

- [Architektur](docs/architecture/README.md)
- [API-Referenz](docs/api/API_REFERENCE.md)
- [Datenbank-Schema](docs/database/DB_SCHEMA.md)
- [Compliance](docs/compliance/README.md)
- [Aktueller Status](docs/CURRENT_STATE.md)
- [Abhängigkeiten](docs/DEPENDENCIES.md)

## Beitragen

Wir freuen uns über Beiträge! Bitte lesen Sie unsere [Beitragsrichtlinien](CONTRIBUTING.md) und den [Verhaltenskodex](CODE_OF_CONDUCT.md).

## Lizenz

Dieses Projekt ist unter der [MIT-Lizenz](LICENSE) lizenziert.

## Sicherheitshinweise [ZTS][NRPD]

- **Keine echten Patientendaten**: Verwenden Sie niemals echte Patientendaten für Entwicklung oder Tests
- **Sicherheitsprobleme**: Melden Sie Sicherheitsprobleme direkt an security@medeasy.ch
- **Geheime Schlüssel**: Speichern Sie niemals API-Schlüssel oder Geheimnisse im Code [NEA]

---

[PSF][PbD][RA][ZTS] MedEasy - Sichere medizinische Transkription und Analyse für Schweizer Ärzte
