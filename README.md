<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy [PSF][PbD][RA]

MedEasy ist eine sichere medizinische Software f�r die automatisierte Transkription und Anonymisierung von Arzt-Patienten-Gespr�chen, speziell entwickelt f�r den Schweizer Gesundheitsmarkt.

![MedEasy Logo](docs/assets/logo.png)

## Projekt�bersicht [MDL]

MedEasy unterst�tzt �rzte bei der effizienten Dokumentation von Patientengespr�chen durch:

- **Automatische Transkription** von Arzt-Patienten-Gespr�chen mit Whisper [WMM]
- **Zuverl�ssige Anonymisierung** sensibler Patientendaten [AIU]
- **KI-gest�tzte Analyse** f�r Diagnosevorschl�ge (mit Arztbest�tigung) [DK]
- **Sichere Datenspeicherung** mit Ende-zu-Ende-Verschl�sselung [SP]
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
�
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

1. **Domain Layer**: Enth�lt die Gesch�ftslogik und Entit�ten ohne externe Abh�ngigkeiten
2. **Application Layer**: Orchestriert Anwendungsf�lle mit CQRS-Pattern
3. **Infrastructure Layer**: Implementiert technische Concerns wie Datenbank und externe Dienste
4. **API Layer**: REST-API mit JWT-Authentifizierung
5. **AI Layer**: Python-basierte KI-Dienste f�r Transkription und Analyse

Alle Abh�ngigkeiten zeigen nach innen (Domain Layer).

## Sicherheit [ZTS][PbD]

MedEasy implementiert umfassende Sicherheitsma�nahmen:

- **Datenverschl�sselung**: AES-256 f�r alle Patientendaten [SP]
- **Unver�nderliche Anonymisierung**: Kann niemals deaktiviert werden [AIU]
- **Audit-Logging**: Alle Datenzugriffe werden protokolliert [ATV]
- **Zero-Trust-Architektur**: Keine impliziten Vertrauensbeziehungen
- **Sichere Authentifizierung**: JWT mit kurzer Lebensdauer und Refresh-Tokens

## Schweiz-spezifische Funktionen [SC][MFD]

- **Spracherkennung**: Optimiert f�r Hochdeutsch und Schweizerdeutsch [SDH]
- **Schweizer Formate**: Unterst�tzung f�r Schweizer Datumsformat (DD.MM.YYYY) und Versicherungsnummern
- **Schweizer Fachbegriffe**: Verwendung korrekter medizinischer Terminologie (DE-CH)
- **Compliance**: Vollst�ndige Einhaltung des nDSG und anderer Schweizer Vorschriften [DSC]

## Entwicklung [DM]

### Voraussetzungen

- .NET SDK 8.0 oder h�her
- Python 3.11 oder h�her
- Node.js 18 oder h�her
- Docker (f�r lokale Entwicklung)

### Einrichtung der Entwicklungsumgebung

```bash
# Repository klonen
git clone https://github.com/yourusername/medeasy.git
cd medeasy

# Entwicklungsumgebung einrichten
./tools/scripts/development/setup-dev-env.ps1
```

### Ausf�hren der Anwendung

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

### Tests ausf�hren [KP100]

```bash
# Alle Tests ausf�hren
./tools/scripts/testing/run-unit-tests.ps1
./tools/scripts/testing/run-integration-tests.ps1
./tools/scripts/testing/run-e2e-tests.ps1

# Sicherheitstests ausf�hren
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

Umfassende Dokumentation ist verf�gbar unter:

- [Architektur](docs/architecture/README.md)
- [API-Referenz](docs/api/API_REFERENCE.md)
- [Datenbank-Schema](docs/database/DB_SCHEMA.md)
- [Compliance](docs/compliance/README.md)
- [Aktueller Status](docs/CURRENT_STATE.md)
- [Abh�ngigkeiten](docs/DEPENDENCIES.md)

## Beitragen

Wir freuen uns �ber Beitr�ge! Bitte lesen Sie unsere [Beitragsrichtlinien](CONTRIBUTING.md) und den [Verhaltenskodex](CODE_OF_CONDUCT.md).

## Lizenz

Dieses Projekt ist unter der [MIT-Lizenz](LICENSE) lizenziert.

## Sicherheitshinweise [ZTS][NRPD]

- **Keine echten Patientendaten**: Verwenden Sie niemals echte Patientendaten f�r Entwicklung oder Tests
- **Sicherheitsprobleme**: Melden Sie Sicherheitsprobleme direkt an security@medeasy.ch
- **Geheime Schl�ssel**: Speichern Sie niemals API-Schl�ssel oder Geheimnisse im Code [NEA]

---

[PSF][PbD][RA][ZTS] MedEasy - Sichere medizinische Transkription und Analyse f�r Schweizer �rzte
