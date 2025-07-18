# MedEasy Projektstruktur

„Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

Diese Dokumentation bietet einen umfassenden Überblick über die Projektstruktur von MedEasy, einschließlich aller Verzeichnisse und Dateien. Standardbibliotheken wie `node_modules` und andere externe Abhängigkeiten werden nur angedeutet.

## Gesamtstruktur

```
medeasy/
├── .vscode/                      # VS Code Konfiguration
│   └── settings.json
├── .windsurfrules                # Windsurf AI Regeln für das Projekt
├── config/                       # Konfigurationsdateien
│   └── README.md
├── data/                         # Datenspeicher (verschlüsselt)
├── docs/                         # Projektdokumentation
│   ├── api/                      # API-Dokumentation
│   │   └── API_REFERENCE.md
│   ├── architecture/             # Architektur-Dokumentation
│   │   ├── AI_SERVICE.md
│   │   ├── DEPENDENCIES.md
│   │   ├── Dependency_Graph.svg
│   │   └── README.md
│   ├── compliance/               # Compliance und Sicherheit
│   │   └── SECURITY.md
│   ├── database/                 # Datenbankdokumentation
│   │   ├── IMPLEMENTATION.md
│   │   ├── RELATIONSHIPS.mermaid
│   │   └── SCHEMA.md
│   ├── frontend/                 # Frontend-Dokumentation
│   │   └── DATABASE_INTEGRATION.md
│   ├── project/                  # Projektmanagement
│   │   ├── PROJECT_STRUCTURE.md
│   │   ├── PROJECT_STRUCTURE_COMPLETE.md
│   │   └── checkliste.md
│   ├── security/                 # Sicherheitsdokumentation
│   ├── testing/                  # Testdokumentation
│   │   └── SECURITY_TESTING.md
│   ├── DOCS-INDEX.md             # Zentrale Dokumentationsreferenz
│   └── FEATURE_STATUS.md         # Status der Funktionen
├── scripts/                      # Hilfsskripte
├── src/                          # Quellcode
│   ├── ai-service/               # Python AI-Service
│   │   ├── protos/               # gRPC Protokolldefinitionen
│   │   │   └── medeasy.proto
│   │   ├── pyproject.toml        # Python-Projektdefinition
│   │   ├── requirements.txt      # Python-Abhängigkeiten
│   │   └── src/                  # Python-Quellcode
│   │       ├── config/           # Konfiguration
│   │       ├── models/           # KI-Modelle
│   │       ├── services/         # gRPC-Services
│   │       └── utils/            # Hilfsfunktionen
│   ├── backend/                  # .NET Backend
│   │   ├── MedEasy.API/          # REST API
│   │   │   ├── Controllers/      # API-Controller
│   │   │   ├── Middleware/       # API-Middleware
│   │   │   └── Program.cs        # Einstiegspunkt
│   │   ├── MedEasy.Application/  # Anwendungslogik
│   │   │   ├── Commands/         # CQRS-Befehle
│   │   │   ├── Queries/          # CQRS-Abfragen
│   │   │   └── Services/         # Anwendungsdienste
│   │   ├── MedEasy.Domain/       # Domänenlogik
│   │   │   ├── Entities/         # Domänenentitäten
│   │   │   ├── Exceptions/       # Domänenspezifische Ausnahmen
│   │   │   └── ValueObjects/     # Wertobjekte
│   │   ├── MedEasy.Infrastructure/ # Infrastruktur
│   │   │   ├── Data/             # Datenzugriff
│   │   │   ├── Migrations/       # Datenbankmigrationen
│   │   │   └── Services/         # Externe Dienste
│   │   └── MedEasy.sln           # Visual Studio Solution
│   └── frontend/                 # Tauri + Svelte Frontend
│       ├── src/                  # Svelte-Quellcode
│       │   ├── components/       # UI-Komponenten
│       │   ├── lib/              # Bibliotheken
│       │   ├── routes/           # SvelteKit-Routen
│       │   └── stores/           # Svelte-Stores
│       ├── src-tauri/            # Tauri-Quellcode
│       │   ├── src/              # Rust-Quellcode
│       │   │   ├── commands/     # Tauri-Befehle
│       │   │   ├── database/     # Datenbankzugriff
│       │   │   │   ├── connection.rs  # SQLCipher-Verbindung
│       │   │   │   ├── encryption.rs  # Feldverschlüsselung
│       │   │   │   ├── migrations.rs  # Datenbankmigrationen
│       │   │   │   └── repositories/  # Datenzugriffsschicht
│       │   │   │       ├── audit_repository.rs
│       │   │   │       ├── patient_repository.rs
│       │   │   │       ├── session_repository.rs
│       │   │   │       └── transcript_repository.rs
│       │   │   ├── models/       # Datenmodelle
│       │   │   └── tests/        # Sicherheitstests
│       │   │       ├── audit_tests.rs
│       │   │       ├── database_tests.rs
│       │   │       ├── encryption_tests.rs
│       │   │       └── repository_tests.rs
│       │   ├── build.rs          # Tauri-Build-Skript
│       │   ├── Cargo.toml        # Rust-Abhängigkeiten
│       │   └── tauri.conf.json   # Tauri-Konfiguration
│       ├── static/               # Statische Dateien
│       │   └── favicon.svg
│       ├── .env                  # Umgebungsvariablen
│       ├── .env.example          # Beispiel-Umgebungsvariablen
│       ├── package.json          # NPM-Konfiguration
│       ├── svelte.config.js      # Svelte-Konfiguration
│       ├── tailwind.config.cjs   # Tailwind-Konfiguration
│       ├── tsconfig.json         # TypeScript-Konfiguration
│       └── vite.config.ts        # Vite-Konfiguration
├── test-data/                    # Testdaten
│   ├── anonymization/            # Anonymisierungstests
│   ├── audio/                    # Audiodateien für Tests
│   │   ├── samples_ch/           # Schweizerdeutsche Beispiele
│   │   └── samples_de/           # Hochdeutsche Beispiele
│   └── mock-patients.json        # Mock-Patientendaten
├── tools/                        # Entwicklungswerkzeuge
│   └── scripts/                  # Hilfsskripte
│       └── check-tools.ps1       # Überprüfung der Entwicklungsumgebung
├── Dockerfile.test               # Docker für Sicherheitstests
├── LICENSE                       # Lizenzinformationen
├── README.md                     # Projektübersicht
├── run_security_tests.ps1        # Skript für Sicherheitstests
└── setup-branch-protection.ps1   # GitHub Branch-Schutz-Konfiguration
```

## Wichtige Komponenten

### Backend (.NET 8)

Die Backend-Komponente folgt der Clean Architecture mit klarer Trennung der Schichten:

- **MedEasy.Domain**: Enthält die Kernlogik ohne externe Abhängigkeiten
- **MedEasy.Application**: Implementiert CQRS mit MediatR
- **MedEasy.Infrastructure**: Enthält SQLCipher, gRPC und externe Services
- **MedEasy.API**: Stellt REST-APIs mit JWT-Authentifizierung bereit

### Frontend (Tauri + Svelte)

Das Frontend kombiniert Tauri für die Desktop-Anwendung mit Svelte für die Benutzeroberfläche:

- **Svelte-Komponenten**: UI-Elemente und Logik
- **Tauri-Rust-Code**: Native Funktionalität und Datenbankzugriff
- **SQLCipher-Integration**: Verschlüsselte Datenspeicherung

### AI-Service (Python)

Der AI-Service ist in Python implementiert und kommuniziert über gRPC:

- **gRPC-Schnittstelle**: Definiert in medeasy.proto
- **Provider-Kette**: Unterstützung für mehrere KI-Provider mit Fallback
- **Whisper-Integration**: Spracherkennung mit Schweizerdeutsch-Unterstützung

### Sicherheitsfeatures

Die Sicherheitsimplementierung umfasst:

- **SQLCipher**: AES-256-Verschlüsselung für die Datenbank
- **Feldverschlüsselung**: AES-256-GCM für sensible Felder
- **Anonymisierung**: Automatische Erkennung und Maskierung von PII
- **Audit-Trail**: Vollständige Protokollierung aller Operationen

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

Stand: 12.07.2025
