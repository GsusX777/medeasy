# MedEasy Projektstruktur

„Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

Diese Dokumentation bietet einen umfassenden Überblick über die Projektstruktur von MedEasy, einschließlich aller Verzeichnisse und Dateien. Standardbibliotheken wie `node_modules` und andere externe Abhängigkeiten werden nur angedeutet.

## Gesamtstruktur

```
medeasy/
├── .env.example                  # Beispiel-Umgebungsvariablen
├── .git/                         # Git-Repository
├── .github/                      # GitHub-Konfiguration
│   └── workflows/                # GitHub Actions
├── .gitignore                    # Git-Ignore-Datei
├── .windsurf/                    # Windsurf AI Konfiguration
├── .windsurfrules                # Windsurf AI Regeln für das Projekt
├── LICENSE                       # Projektlizenz
├── README.md                     # Haupt-README
├── config/                       # Konfigurationsdateien
│   └── .gitkeep                  # Leeres Verzeichnis
├── data/                         # Datenspeicher (verschlüsselt)
├── docs/                         # Projektdokumentation
│   ├── .gitkeep                  # Git-Platzhalter
│   ├── DOCS-INDEX.md             # Zentrale Dokumentationsreferenz
│   ├── FEATURE_STATUS.md         # Status der Funktionen
│   ├── ai-service/               # AI-Service Dokumentation
│   │   └── README.md             # AI-Service Übersicht
│   ├── api/                      # API-Dokumentation
│   │   └── API_REFERENCE.md      # API-Referenz
│   ├── architecture/             # Architektur-Dokumentation
│   │   ├── AI_SERVICE.md         # AI-Service Architektur
│   │   ├── Dependency_Graph.svg  # Abhängigkeitsdiagramm
│   │   └── README.md             # Architektur-Übersicht
│   ├── compliance/               # Compliance und Sicherheit
│   │   └── SECURITY.md           # Sicherheitskonzept
│   ├── config/                   # Konfigurationsdokumentation
│   │   └── README.md             # Konfigurationsrichtlinien
│   ├── database/                 # Datenbankdokumentation
│   │   ├── IMPLEMENTATION.md     # Implementierungsdetails
│   │   ├── RELATIONSHIPS.mermaid # Entitätsbeziehungen
│   │   ├── SCHEMA.md             # Datenbankschema
│   │   └── [weitere Dateien]     # Zusätzliche DB-Dokumentation
│   ├── frontend/                 # Frontend-Dokumentation
│   │   ├── NOT_USE_ROADMAP.md    # Veraltete Roadmap (nicht verwenden)
│   │   ├── NOT_USE_SECURITY.md   # Veraltete Sicherheitsdoku (nicht verwenden)
│   │   ├── README.md             # Frontend-Übersicht
│   │   └── TESTING.md            # Frontend-Tests
│   ├── project/                  # Projektmanagement
│   │   ├── PROJECT_STRUCTURE.md  # Diese Datei
│   │   └── checkliste.md         # Entwicklungs-Checkliste
│   ├── security/                 # Sicherheitsdokumentation
│   ├── testing/                  # Testdokumentation
│   │   └── SECURITY_TESTING.md   # Sicherheitstests
│   └── ui/                       # UI-Dokumentation
│       ├── DESIGN_IMPLEMENTATION_PLAN.md # Design-Implementierungsplan
│       ├── DESIGN_STRATEGY.md    # Design-Strategie
│       ├── README.md             # UI-Übersicht
│       ├── accessibility/        # Barrierefreiheit
│       ├── api/                  # UI-API-Dokumentation
│       ├── components/           # Komponenten-Dokumentation
│       ├── dashboard/            # Dashboard-Dokumentation
│       ├── deployment/           # UI-Deployment
│       ├── routing/              # Routing-Dokumentation
│       ├── stores/               # Store-Dokumentation
│       ├── styling/              # Styling-Dokumentation
│       └── types/                # TypeScript-Typen
├── scripts/                      # Hilfsskripte (leer)
├── src/                          # Quellcode
│   ├── .gitkeep                  # Git-Platzhalter
│   ├── ai-service/               # Python AI-Service (28 Dateien)
│   │   ├── protos/               # gRPC Protokolldefinitionen
│   │   ├── src/                  # Python-Quellcode (17 Dateien)
│   │   │   ├── __init__.py       # Package-Initialisierung
│   │   │   ├── config.py         # Service-Konfiguration
│   │   │   ├── grpc_service.py   # gRPC-Server-Implementierung
│   │   │   ├── main.py           # Haupteinstiegspunkt
│   │   │   ├── anonymization/    # Anonymisierungslogik [AIU]
│   │   │   ├── metrics/          # Metriken-Sammlung [ATV]
│   │   │   ├── providers/        # AI-Provider-Kette [PK]
│   │   │   ├── swiss/            # Schweizerdeutsch-Erkennung [SDH]
│   │   │   └── whisper/          # Audio-Transkription [WMM]
│   │   ├── tests/                # Python-Tests (6 Dateien)
│   │   ├── medeasy_pb2.py        # Generierte gRPC-Stubs
│   │   ├── medeasy_pb2_grpc.py   # Generierte gRPC-Service-Stubs
│   │   ├── requirements.txt      # Python-Abhängigkeiten
│   │   ├── pyproject.toml        # Python-Projekt-Konfiguration
│   │   ├── .env.example          # Beispiel-Umgebungsvariablen
│   │   └── venv/                 # Virtuelle Umgebung (lokal)
│   ├── backend/                  # .NET Backend (26 Dateien)
│   │   ├── MedEasy.API/          # HTTP-API für Desktop-Frontend (10 Dateien)
│   │   ├── MedEasy.Application/  # Anwendungslogik (2 Dateien)
│   │   ├── MedEasy.Domain/       # Domain-Modelle (6 Dateien)
│   │   ├── MedEasy.FinalSecurityTests/ # Sicherheitstests (2 Dateien)
│   │   ├── MedEasy.Infrastructure/ # Infrastruktur (5 Dateien)
│   │   └── MedEasy.sln           # Visual Studio Solution
│   ├── frontend/                 # Tauri + Svelte Frontend (92 Dateien)
│   │   ├── src/                  # Svelte-Quellcode (50 Dateien)
│   │   │   ├── app.d.ts          # TypeScript-Definitionen
│   │   │   ├── app.html          # HTML-Template
│   │   │   ├── demo.spec.ts      # Demo-Test
│   │   │   ├── lib/              # Bibliotheken (46 Dateien)
│   │   │   └── routes/           # SvelteKit-Routen (1 Datei)
│   │   ├── src-tauri/            # Tauri Desktop-Backend (27 Dateien)
│   │   │   ├── scripts/          # Build-Skripte (1 Datei)
│   │   │   ├── .env.development.example # Entwicklungs-Umgebungsvariablen
│   │   │   ├── .env.production.example  # Produktions-Umgebungsvariablen
│   │   │   └── tauri.conf.json   # Tauri-Konfiguration
│   │   ├── static/               # Statische Dateien (2 Dateien)
│   │   ├── .svelte-kit/          # SvelteKit-Build-Cache
│   │   ├── dist/                 # Build-Ausgabe
│   │   ├── node_modules/         # NPM-Abhängigkeiten
│   │   ├── .gitignore            # Git-Ignore-Datei
│   │   ├── .npmrc                # NPM-Konfiguration
│   │   ├── .prettierignore       # Prettier-Ignore
│   │   ├── .prettierrc           # Prettier-Konfiguration
│   │   ├── eslint.config.js      # ESLint-Konfiguration
│   │   ├── package.json          # NPM-Konfiguration
│   │   ├── package-lock.json     # NPM-Lock-Datei
│   │   ├── svelte.config.js      # Svelte-Konfiguration
│   │   ├── tsconfig.json         # TypeScript-Konfiguration
│   │   ├── vite.config.js        # Vite-Konfiguration
│   │   ├── vite.config.ts        # Vite-TypeScript-Konfiguration
│   │   └── vitest-setup-client.ts # Vitest-Setup
│   └── frontend-backup/          # Frontend-Backup (11 Dateien)
├── test-data/                    # Testdaten
│   ├── anonymization/            # Anonymisierungstests
│   ├── audio/                    # Audiodateien für Tests
│   │   ├── samples_ch/           # Schweizerdeutsche Beispiele
│   │   └── samples_de/           # Hochdeutsche Beispiele
│   └── mock-patients.json        # Mock-Patientendaten
├── LICENSE                       # Lizenzinformationen
├── README.md                     # Projektübersicht
└── setup-branch-protection.ps1   # GitHub Branch-Schutz-Konfiguration
```

## Wichtige Komponenten

### Backend (.NET 8)

Die Backend-Komponente folgt der Clean Architecture mit klarer Trennung der Schichten:

- **MedEasy.Domain**: Enthält die Kernlogik ohne externe Abhängigkeiten
- **MedEasy.Application**: Implementiert CQRS mit MediatR
- **MedEasy.Infrastructure**: Enthält SQLCipher, gRPC und externe Services
- **MedEasy.API**: Stellt REST-APIs mit JWT-Authentifizierung bereit

### Frontend (Svelte Desktop App)

Das Frontend ist eine Svelte-basierte Desktop-Anwendung, die über HTTP mit dem .NET Backend kommuniziert:

- **Svelte-Komponenten**: UI-Elemente und Logik
- **HTTP-Client**: REST API-Kommunikation mit .NET Backend
- **Keine direkte Datenbankverbindung**: Alle Daten über .NET Backend API
- **JWT-Authentifizierung**: Sichere API-Kommunikation

### AI-Service (Python)

Der AI-Service ist in Python implementiert und kommuniziert über gRPC:

- **gRPC-Schnittstelle**: Definiert in medeasy.proto
- **Provider-Kette**: Unterstützung für mehrere KI-Provider mit Fallback
- **Whisper-Integration**: Spracherkennung mit Schweizerdeutsch-Unterstützung

### Sicherheitsfeatures (.NET Backend)

Die Sicherheitsimplementierung erfolgt ausschließlich im .NET Backend:

- **SQLCipher**: AES-256-Verschlüsselung für die Datenbank (Entity Framework)
- **Feldverschlüsselung**: AES-256 für sensible Felder (.NET Cryptography)
- **Anonymisierung**: Automatische Erkennung und Maskierung von PII [AIU]
- **Audit-Trail**: Vollständige Protokollierung aller Operationen [ATV]
- **JWT-Authentifizierung**: Sichere API-Zugriffskontrolle [ZTS]
- **Key-Rotation**: Automatische Schlüsselrotation alle 90 Tage [SP]

## Dokumentationsstruktur

Die Dokumentation ist nach dem DOCS-INDEX.md-Prinzip organisiert:

- **API**: REST-Endpunkte und gRPC-Schnittstellen
- **Architektur**: Clean Architecture, Abhängigkeiten und Diagramme
- **Compliance**: Sicherheits- und Datenschutzanforderungen
- **Datenbank**: Schema, Beziehungen und Implementierungsdetails
- **Frontend**: UI-Komponenten und Datenbankintegration
- **Projekt**: Checklisten und Projektstruktur
- **Testing**: Sicherheitstests und Testabdeckung

## Entwicklungsworkflow

Der Entwicklungsworkflow umfasst:

1. Dokumentation lesen (DOCS-INDEX.md)
2. Feature implementieren gemäß Clean Architecture
3. Tests schreiben (100% Abdeckung für Sicherheitsfeatures)
4. Dokumentation aktualisieren
5. Code-Review mit Fokus auf Sicherheit
6. Merge in den Hauptzweig

## Sicherheitsrichtlinien

Alle Implementierungen müssen den MedEasy-Projektregeln entsprechen:

- [SP] SQLCipher Pflicht
- [AIU] Anonymisierung ist unveränderlich
- [ATV] Audit-Trail vollständig
- [EIV] Entitäten immer verschlüsselt
- [ZTS] Zero Tolerance Security
- [DSC] Datenschutz Schweiz

## 🚨 MIGRATION ZU .NET BACKEND

### Migrationspfad [CAS]

1. **Phase 9.2**: .NET Backend implementieren und testen
2. **Phase 9.3**: Frontend auf HTTP-Client umstellen (Mock-Daten entfernen)
3. **Phase 9.4**: Rust/Tauri-Code vollständig entfernen
4. **Produktions-Readiness**: Nur .NET Backend + Svelte Frontend

### Neue Architektur nach Migration

```
Frontend (Svelte) ←→ HTTP/REST ←→ .NET Backend ←→ SQLCipher DB
                                      ↕
                                  gRPC/Python AI
```

**Keine direkte Datenbankverbindung vom Frontend!** [CAS]

Stand: 21.07.2025
