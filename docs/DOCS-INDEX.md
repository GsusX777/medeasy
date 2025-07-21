<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Dokumentationsindex [DL][DSU]

*Erstellt am: 09.07.2025*

Dieser Index bietet einen Überblick über alle Dokumentationsdateien im MedEasy-Projekt, kategorisiert nach Themenbereich und mit kurzen Beschreibungen des Inhalts. Die Dokumentation folgt den MedEasy-Projektregeln für Dokumentationspflicht [D=C] und wird bei jeder Code-Änderung aktualisiert [DSU].

## Projektübersicht

| Dokument | Beschreibung | Projektregeln |
|----------|-------------|--------------|
| [README.md](../README.md) | Hauptdokumentation mit Projektübersicht, Technologie-Stack, Projektstruktur und Entwicklungsrichtlinien | [PSF], [PbD], [RA], [MDL], [TSF], [CAS] |
| [docs/FEATURE_STATUS.md](./FEATURE_STATUS.md) | Aktueller Implementierungsstatus aller MedEasy-Features mit Versionsinformationen | [DSU], [AIU], [SDH], [CT], [PK], [NDW] |

## Architektur

| Dokument | Beschreibung | Projektregeln |
|----------|-------------|--------------|
| [docs/architecture/README.md](./architecture/README.md) | Detaillierte Beschreibung der Clean Architecture, Schichten und Abhängigkeiten | [CAM], [CAS], [CQA], [DD] |
| [docs/architecture/AI_SERVICE.md](./architecture/AI_SERVICE.md) | Dokumentation des AI-Service mit Provider-Kette und Whisper-Integration | [PK], [WMM], [CT], [MLB] |
| [docs/architecture/Dependency_Graph.svg](./architecture/Dependency_Graph.svg) | Visualisierung der Clean Architecture und Komponentenabhängigkeiten | [CAM], [DD], [CAS] |

## Datenbank

| Dokument | Beschreibung | Projektregeln |
|----------|-------------|--------------|
| [docs/database/SCHEMA.md](./database/SCHEMA.md) | Detailliertes Datenbankschema mit Tabellendefinitionen und Verschlüsselungsstrategie | [SP], [EIV], [NUS] |
| [docs/database/IMPLEMENTATION.md](./database/IMPLEMENTATION.md) | Implementierung der SQLCipher-Datenbank in .NET Entity Framework mit Repositories und Sicherheitsmerkmalen | [SP], [AIU], [ATV], [CAS], [DD] |
| [docs/database/RELATIONSHIPS.mermaid](./database/RELATIONSHIPS.mermaid) | Mermaid-Diagramm der Datenbankbeziehungen und Entitätsrelationen | [EIV], [SK] |

## API

| Dokument | Beschreibung | Projektregeln |
|----------|-------------|--------------|
| [docs/api/API_REFERENCE.md](./api/API_REFERENCE.md) | Vollständige API-Referenz mit Endpunkten, Parametern, Antworten und Sicherheitsmerkmalen | [ZTS], [PbD], [ATV], [SP], [AIU] |

## Frontend

| Dokument | Beschreibung | Projektregeln |
|----------|-------------|--------------|
| [docs/frontend/DATABASE_INTEGRATION.md](./frontend/DATABASE_INTEGRATION.md) | Integration der SQLCipher-Datenbank und Schlüsselverwaltung im Svelte-Frontend | [SP], [AIU], [ATV], [ZTS], [FSD], [CAS] |
| [src/frontend/COMPONENTS.md](../src/frontend/COMPONENTS.md) | Dokumentation der Frontend-Komponenten und ihrer Funktionen | [TSF], [CT], [ARQ] |
| [src/frontend/README.md](../src/frontend/README.md) | Übersicht über das Frontend mit Installationsanleitung und Entwicklungsrichtlinien | [TSF], [FBN] |
| [src/frontend/SECURITY.md](../src/frontend/SECURITY.md) | Sicherheitsrichtlinien für die Frontend-Entwicklung | [ZTS], [AIU], [CT], [NEA] |
| [src/frontend/TESTING.md](../src/frontend/TESTING.md) | Testrichtlinien für das Frontend mit Fokus auf Sicherheitstests | [KP100], [TD], [PB] |
| [src/frontend/ROADMAP.md](../src/frontend/ROADMAP.md) | Entwicklungsfahrplan für das Frontend | [DSU] |
| [src/frontend/REMAINING_ISSUES.md](../src/frontend/REMAINING_ISSUES.md) | Bekannte Probleme und geplante Verbesserungen | [DSU] |

## UI-Dokumentation

| Dokument | Beschreibung | Projektregeln |
|----------|-------------|--------------|
| [docs/ui/README.md](./ui/README.md) | Übersicht über die MedEasy UI mit Svelte 4 und Tauri 1.5, Technologie-Stack und Sicherheitsfeatures | [TSF], [ZTS], [AIU], [ATV], [CT], [SDH] |
| [docs/ui/DESIGN_STRATEGY.md](./ui/DESIGN_STRATEGY.md) | UI-Spezifikation und Funktionsbeschreibung | [TSF], [ZTS], [AIU], [ATV], [CT], [SDH] |
| [docs/ui/DESIGN_IMPLEMENTATION_PLAN.md](./ui/DESIGN_IMPLEMENTATION_PLAN.md) | Implementierungsplan zur Integration der Design-Strategie mit vorhandenen Komponenten | [D=C], [DSU], [TSF], [ZTS], [AIU], [ATV] |
| [docs/ui/components/README.md](./ui/components/README.md) | Vollständige Dokumentation aller UI-Komponenten: Haupt-, Sicherheits- und gemeinsame Komponenten | [TSF], [CT], [ARQ], [AIU], [ATV], [SP], [ZTS] |
| [docs/ui/stores/README.md](./ui/stores/README.md) | Svelte Stores für Zustandsverwaltung: Auth, Database, Notifications, Session | [ZTS], [SP], [ATV], [AIU] |
| [docs/ui/routing/README.md](./ui/routing/README.md) | SvelteKit-basiertes Routing-System mit Sicherheitsprüfungen und Navigation | [CT], [AIU], [ATV], [PbD] |
| [docs/ui/styling/README.md](./ui/styling/README.md) | Design-System mit Farbpalette, Typografie, Responsive Design und Sicherheits-Styling | [CT], [PSF], [SF], [ZTS] |
| [docs/ui/testing/README.md](./ui/testing/README.md) | UI-Testing mit Vitest, Playwright, Security Tests und Accessibility Testing | [KP100], [AIU], [ATV], [SP], [CT], [PSF] |
| [docs/ui/accessibility/README.md](./ui/accessibility/README.md) | WCAG 2.1 AA Compliance, medizinische Accessibility und Schweizer Besonderheiten | [PSF], [SF], [MDL], [CT], [ZTS] |
| [docs/ui/deployment/README.md](./ui/deployment/README.md) | Build- und Deployment-Prozess mit Sicherheitskonfiguration und Compliance | [SP], [ZTS], [ATV], [AIU], [DSC] |
| [docs/ui/dashboard/README.md](./ui/dashboard/README.md) | Vereinfachtes Dashboard mit Tagesübersicht und Konsultationslisten | [SF], [SK], [DSU], [EIV], [AIU], [ATV] |

## Compliance und Sicherheit

| Dokument | Beschreibung | Projektregeln |
|----------|-------------|--------------|
| [docs/compliance/SECURITY.md](./compliance/SECURITY.md) | Umfassendes Sicherheitskonzept mit Verschlüsselung, Authentifizierung, Audit und Notfallwiederherstellung | [ZTS], [PbD], [SP], [ES], [NUS], [EIV] |
| [docs/testing/SECURITY_TESTING.md](./testing/SECURITY_TESTING.md) | Dokumentation der Docker-basierten KP100-Sicherheitstests und Testrichtlinien, inkl. Verschlüsselung, Datenbank, Repository, Audit und Schlüsselrotation | [KP100], [SP], [AIU], [ATV], [EIV], [TD], [ZTS] |

## AI-Service

| Dokument | Beschreibung | Projektregeln |
|----------|-------------|--------------|
| [src/ai-service/README.md](../src/ai-service/README.md) | Dokumentation des Python-basierten AI-Service mit Whisper und Provider-Kette | [PK], [WMM], [CT], [SDH] |

## Konfiguration

| Dokument | Beschreibung | Projektregeln |
|----------|-------------|--------------|
| [config/README.md](../config/README.md) | Konfigurationsrichtlinien und -optionen | [NEA], [DSC] |

## Projekt

| Dokument | Beschreibung | Projektregeln |
|----------|-------------|--------------|
| [docs/project/checkliste.md](./project/checkliste.md) | Entwicklungs-Checkliste mit Fortschrittsübersicht | [DSU], [D=C] |
| [docs/project/PROJECT_STRUCTURE.md](./project/PROJECT_STRUCTURE.md) | Projektstruktur mit Dokumentationspflicht | [DSU], [D=C] |

