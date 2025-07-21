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
│       ├── ❌ ZU LÖSCHEN: src-tauri/            # ❌ ZU LÖSCHEN: Rust/Tauri Backend-Code
│       │   ├── ❌ ZU LÖSCHEN: src/              # ❌ ZU LÖSCHEN: Gesamter Rust-Quellcode
│       │   │   ├── ❌ ZU LÖSCHEN: commands/     # ❌ ZU LÖSCHEN: Tauri-Befehle
│       │   │   ├── ❌ ZU LÖSCHEN: database/     # ❌ ZU LÖSCHEN: Rust-Datenbankzugriff
│       │   │   │   ├── ❌ ZU LÖSCHEN: connection.rs  # ❌ ZU LÖSCHEN: SQLCipher-Verbindung
│       │   │   │   ├── ❌ ZU LÖSCHEN: encryption.rs  # ❌ ZU LÖSCHEN: Feldverschlüsselung
│       │   │   │   ├── ❌ ZU LÖSCHEN: migrations.rs  # ❌ ZU LÖSCHEN: Datenbankmigrationen
│       │   │   │   └── ❌ ZU LÖSCHEN: repositories/  # ❌ ZU LÖSCHEN: Datenzugriffsschicht
│       │   │   │       ├── ❌ ZU LÖSCHEN: audit_repository.rs      # ❌ ZU LÖSCHEN
│       │   │   │       ├── ❌ ZU LÖSCHEN: patient_repository.rs    # ❌ ZU LÖSCHEN
│       │   │   │       ├── ❌ ZU LÖSCHEN: session_repository.rs    # ❌ ZU LÖSCHEN
│       │   │   │       └── ❌ ZU LÖSCHEN: transcript_repository.rs # ❌ ZU LÖSCHEN
│       │   │   ├── ❌ ZU LÖSCHEN: models/       # ❌ ZU LÖSCHEN: Datenmodelle
│       │   │   └── ❌ ZU LÖSCHEN: tests/        # ❌ ZU LÖSCHEN: Rust-Sicherheitstests
│       │   │       ├── ❌ ZU LÖSCHEN: audit_tests.rs        # ❌ ZU LÖSCHEN
│       │   │       ├── ❌ ZU LÖSCHEN: database_tests.rs     # ❌ ZU LÖSCHEN
│       │   │       ├── ❌ ZU LÖSCHEN: encryption_tests.rs   # ❌ ZU LÖSCHEN
│       │   │       └── ❌ ZU LÖSCHEN: repository_tests.rs   # ❌ ZU LÖSCHEN
│       │   ├── ❌ ZU LÖSCHEN: build.rs          # ❌ ZU LÖSCHEN: Tauri-Build-Skript
│       │   ├── ❌ ZU LÖSCHEN: Cargo.toml        # ❌ ZU LÖSCHEN: Rust-Abhängigkeiten
│       │   └── ❌ ZU LÖSCHEN: tauri.conf.json   # ❌ ZU LÖSCHEN: Tauri-Konfiguration
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
├── ❌ ZU LÖSCHEN: Dockerfile.test               # ❌ ZU LÖSCHEN: Docker für Rust-Sicherheitstests
├── LICENSE                       # Lizenzinformationen
├── README.md                     # Projektübersicht
├── ❌ ZU LÖSCHEN: run_security_tests.ps1        # ❌ ZU LÖSCHEN: Skript für Rust-Sicherheitstests
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

### Zu löschende Ordner und Dateien nach .NET Integration

**WICHTIG**: Diese Elemente müssen nach erfolgreicher .NET Backend Integration gelöscht werden:

#### Frontend (Rust/Tauri Code)
```
src/frontend/src-tauri/           # Gesamter Rust/Tauri Backend-Code
├── src/                          # Alle Rust-Quelldateien
├── build.rs                      # Tauri-Build-Skript
├── Cargo.toml                    # Rust-Abhängigkeiten
└── tauri.conf.json              # Tauri-Konfiguration
```

#### Root-Level Dateien
```
Dockerfile.test                   # Docker für Rust-Sicherheitstests
run_security_tests.ps1           # PowerShell-Skript für Rust-Tests
```

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
