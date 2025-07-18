<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy SQLCipher Datenbank-Integration

Diese Komponente implementiert eine sichere Datenbankschicht für MedEasy mit SQLCipher-Verschlüsselung und strikter Einhaltung der Schweizer Datenschutzanforderungen.

## Sicherheitsmerkmale [SP] [AIU] [ATV]

- **SQLCipher Pflicht**: Alle Patientendaten werden mit AES-256 verschlüsselt
- **Unveränderliche Anonymisierung**: Kann niemals deaktiviert oder umgangen werden
- **Vollständiger Audit-Trail**: Jede Datenbankoperation wird protokolliert
- **Feldverschlüsselung**: Sensible Felder werden zusätzlich einzeln verschlüsselt
- **Schweizer Formate**: Unterstützung für CH-spezifische Datenformate (Versicherungsnummern, Datumsformate)

## Komponenten

### Datenbankschicht (`src/database/`)

- **connection.rs**: Verwaltet SQLCipher-Verbindungen mit Verschlüsselungs-Enforcement
- **models.rs**: Definiert Datenmodelle mit verschlüsselten Feldern
- **schema.rs**: SQL-Schema mit Indizes und Fremdschlüsseln
- **encryption.rs**: AES-256-GCM Feldverschlüsselung mit Nonce-Handling
- **migrations.rs**: Versionierte Datenbankmigrationen

### Repositories (`src/repositories/`)

- **patient_repository.rs**: CRUD für Patienten mit Verschlüsselung und Audit
- **session_repository.rs**: CRUD für Sitzungen mit Verschlüsselung und Audit
- **transcript_repository.rs**: CRUD für Transkripte mit Anonymisierung
- **audit_repository.rs**: Zugriff auf Audit-Logs

### Tauri-Integration

- **commands.rs**: Tauri-Befehle für Frontend-Zugriff
- **main.rs**: Initialisierung und Konfiguration

## Umgebungsvariablen

Die Anwendung erwartet Umgebungsvariablen in `.env.development` oder `.env.production`:

```
DATABASE_URL=sqlite://./medeasy.db
USE_ENCRYPTION=true
MEDEASY_DB_KEY=<Verschlüsselungsschlüssel>
MEDEASY_FIELD_ENCRYPTION_KEY=<Base64-kodierter Schlüssel>
RUST_LOG=info
```

Beispieldateien sind unter `.env.development.example` und `.env.production.example` verfügbar.

## Datenbank-Hilfsskript

Das PowerShell-Skript `scripts/db-helper.ps1` bietet Funktionen für:

- Datenbankerstellung: `.\db-helper.ps1 create development`
- Backup: `.\db-helper.ps1 backup production`
- Reset: `.\db-helper.ps1 reset development`
- Schlüsselgenerierung: `.\db-helper.ps1 genkey`

## Entwicklung vs. Produktion

- **Entwicklung**: Verschlüsselung optional, feste Testschlüssel erlaubt
- **Produktion**: Verschlüsselung erzwungen, sichere Schlüssel erforderlich

## Compliance-Hinweise [RW]

Diese Implementierung berücksichtigt:
- Schweizer nDSG (strengere Anforderungen als GDPR)
- Medizinische Datenschutzbestimmungen
- Audit-Anforderungen für medizinische Software

## Sicherheitshinweise [ZTS]

- Verwenden Sie NIEMALS die Entwicklungsschlüssel in Produktion
- Generieren Sie sichere Schlüssel mit `.\db-helper.ps1 genkey`
- Speichern Sie Produktionsschlüssel sicher außerhalb der Versionskontrolle
- Die Anonymisierung kann NICHT deaktiviert werden [AIU]
