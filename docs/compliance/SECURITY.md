<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Sicherheitskonzept

*Letzte Aktualisierung: 08.07.2025*

## Übersicht [ZTS][PbD]

Das MedEasy Sicherheitskonzept wurde entwickelt, um die strengen Anforderungen des Schweizer Gesundheitswesens zu erfüllen und sensible Patientendaten optimal zu schützen. Es folgt dem Prinzip "Privacy by Design" und implementiert mehrere Sicherheitsschichten.

## Datenverschlüsselung [SP][ES][NUS]

### Datenbank-Verschlüsselung

- **Technologie**: SQLCipher mit AES-256-CBC
- **Schlüsselableitung**: PBKDF2 mit 256.000 Iterationen
- **Schlüsselverwaltung**: 
  - Produktionsumgebung: Secure Key Vault
  - Entwicklung: Umgebungsvariable `MEDEASY_DB_KEY`
  - Konfiguration: `Database:EncryptionKey` (nur für Entwicklung)
- **Salt**: Konfigurierbar über `Database:Salt`

### Feldverschlüsselung [EIV]

Sensible Daten werden zusätzlich auf Feldebene verschlüsselt:

| Entität | Verschlüsselte Felder |
|---------|----------------------|
| Patient | Name, Adresse, Kontaktdaten |
| Session | Notizen, Diagnosen, Behandlungspläne |
| Transcript | Originaltext, Anonymisierter Text |

### Datei-Verschlüsselung

- **Audio-Dateien**: AES-256-GCM mit individuellem Schlüssel pro Datei
- **Export-Dateien**: Passwortgeschützte ZIP-Archive mit AES-256

## Authentifizierung und Autorisierung [ZTS]

### Desktop-Authentifizierung

- **Session-Management**: JWT für lokale Session-Verwaltung
- **Gültigkeit**: 8 Stunden Arbeitszeit (konfigurierbar)
- **Auto-Lock**: Nach 30 Minuten Inaktivität
- **Biometrische Integration**: Windows Hello/Touch ID Support (geplant)
- **Validierung**: Lokale Signaturprüfung ohne externe Services

### Funktionsbasierte Zugriffssteuerung

- **Einzelbenutzer-Modus**: Ein Arzt pro Anwendungsinstanz
- **Funktions-Toggles**: Aktivierung/Deaktivierung von Features
- **Sicherheitsstufen**: Verschiedene Sicherheitsmodi (Standard/Hoch)
- **Audit**: Alle Konfigurationsänderungen werden protokolliert [ATV]

## Audit-Logging [ATV]

### Datenbankoperationen

Alle Datenbankoperationen werden protokolliert:

- **INSERT**: Neue Einträge
- **UPDATE**: Geänderte Felder mit alten und neuen Werten
- **DELETE**: Gelöschte Einträge (logisches Löschen)
- **READ**: Lesezugriffe auf sensible Daten

### API-Zugriffe

Alle API-Zugriffe werden protokolliert:

- **Zeitpunkt**: Genaue Zeit des Zugriffs
- **Benutzer**: Authentifizierter Benutzer
- **Endpunkt**: Aufgerufener API-Endpunkt
- **IP-Adresse**: Quell-IP-Adresse
- **Status**: HTTP-Statuscode

### Sicherheitsereignisse

Sicherheitsrelevante Ereignisse werden separat protokolliert:

- **Anmeldeversuche**: Erfolgreiche und fehlgeschlagene Anmeldungen
- **Berechtigungsänderungen**: Änderungen an Benutzerrollen
- **Konfigurationsänderungen**: Änderungen an Sicherheitseinstellungen
- **Schlüsselverwendung**: Verwendung von Verschlüsselungsschlüsseln

## Desktop-Anwendungssicherheit [ZTS]

### Lokale HTTP-Server Sicherheit

- **Binding**: 127.0.0.1 only (keine externe Erreichbarkeit)
- **Port**: Dynamisch zugewiesen durch .NET (nicht fest)
- **CORS**: Restriktiv auf Tauri WebView beschränkt
- **Request-Validierung**: Alle API-Anfragen werden validiert

### Tauri Security Model

- **CSP**: Tauri-spezifische Content Security Policy
- **IPC-Sicherheit**: Sichere Kommunikation zwischen Frontend und Backend
- **Allowlist**: Nur explizit erlaubte Tauri-Commands verfügbar
- **File System**: Beschränkter Zugriff auf definierte Verzeichnisse

### Process-Sicherheit

- **Isolation**: Backend (.NET) und Frontend (Tauri) laufen getrennt
- **Memory Protection**: Sensible Daten werden nach Verwendung gelöscht
- **Single-User**: Nur ein Benutzer pro Anwendungsinstanz
- **Session-Timeout**: Automatische Abmeldung nach Inaktivität

## Anonymisierung [AIU][AR][NAU]

### Automatische Erkennung von PII

- **Technologie**: Named Entity Recognition (NER)
- **Erkannte Entitäten**:
  - Namen (Personen, Organisationen)
  - Adressen und Orte
  - Datumsangaben (außer medizinisch relevante)
  - Kontaktdaten (Telefon, E-Mail)
  - Versicherungsnummern
  - Eindeutige Identifikatoren

### Anonymisierungsprozess

1. **Erkennung**: PII wird automatisch erkannt
2. **Klassifizierung**: Erkannte Entitäten werden kategorisiert
3. **Maskierung**: PII wird durch Platzhalter ersetzt
4. **Konfidenzberechnung**: Berechnung der Anonymisierungskonfidenz
5. **Review**: Bei niedriger Konfidenz (<80%) wird eine manuelle Überprüfung angefordert

### Review-Queue [ARQ]

- **Automatische Sortierung**: Nach Konfidenz und Sensitivität
- **Batch-Review**: Effiziente Überprüfung mehrerer Items
- **Whitelist**: Medizinische Fachbegriffe können auf die Whitelist gesetzt werden

## Notfallplan und Wiederherstellung [FSD]

### Backup-Strategie

- **Vollbackup**: Täglich
- **Inkrementelles Backup**: Stündlich
- **Verschlüsselung**: Alle Backups werden verschlüsselt
- **Aufbewahrung**: 30 Tage (konfigurierbar)

### Notfallwiederherstellung

- **RPO (Recovery Point Objective)**: <1 Stunde
- **RTO (Recovery Time Objective)**: <4 Stunden
- **Testwiederherstellung**: Monatlich

### Business Continuity

- **Offline-Modus**: Grundfunktionen bleiben bei Netzwerkausfall verfügbar
- **Lokale Zwischenspeicherung**: Daten werden lokal zwischengespeichert und später synchronisiert
- **Manuelle Eingabe**: Fallback auf manuelle Eingabe bei Systemausfall

## Compliance [RA][DSC]

Dieses Sicherheitskonzept wurde entwickelt, um folgende Vorschriften zu erfüllen:

- **Schweizer nDSG**: Datenschutzgesetz
- **DSGVO/GDPR**: Für EU-Kompatibilität
- **MDR**: Medizinprodukteverordnung für medizinische Software
- **GoodPriv@cy**: Schweizer Datenschutzstandard
- **ISO 27001**: Informationssicherheitsmanagement
- **ISO 27799**: Gesundheitsinformatik - Sicherheitsmanagement

## Frontend-Sicherheit [AIU][SP][ATV][CT]

### Tauri Desktop-Anwendung

- **Verschlüsselte Datenspeicherung**: SQLCipher (AES-256) für lokale Daten [SP]
- **Berechtigungsmodell**: Minimale Berechtigungen in `tauri.conf.json` [ZTS]
  - Eingeschränkter Dateisystemzugriff (`scope: ["$APP/*"]`)
  - Keine Shell-Ausführung (`"shell": { "all": false, "open": true }`)
- **Content Security Policy**: Strikte CSP in `tauri.conf.json` [ZTS]
  - `"csp": "default-src 'self'; img-src 'self' asset: https://asset.localhost; style-src 'self' 'unsafe-inline'"`
- **Audit-Logging**: Protokollierung aller Benutzeraktionen [ATV]
  - Jede Aktion wird mit Zeitstempel, Benutzer und Kontext protokolliert
  - Audit-Logs werden verschlüsselt gespeichert

### Svelte Frontend

- **Unveränderliche Anonymisierung**: Keine Möglichkeit zur Deaktivierung [AIU]
  - Anonymisierung ist fest im Code verankert
  - Keine Konfigurationsoptionen zum Deaktivieren
- **Cloud-Transparenz**: Klare Anzeige der Verarbeitungslokation [CT]
  - Visuelle Indikatoren für lokale vs. Cloud-Verarbeitung
  - Explizite Einwilligung für Cloud-Verarbeitung erforderlich
- **Schweizerdeutsch-Handling**: Transparente Kommunikation bei Dialekterkennung [SDH]
  - Warnhinweise bei erkanntem Schweizerdeutsch
  - Reduzierte Genauigkeit wird transparent kommuniziert
- **TypeScript**: Typsichere Entwicklung zur Vermeidung von Laufzeitfehlern [TR]

### Datenschutz im Frontend

- **Keine Patientendaten im Klartext**: Patientendaten werden nie unverschlüsselt angezeigt [NUS]
- **Keine lokalen Caches**: Sensible Daten werden nicht im Browser-Cache gespeichert [NUS]
- **Automatisches Session-Timeout**: Automatische Abmeldung nach Inaktivität [ZTS]
- **Sichere Fehlerbehandlung**: Keine sensiblen Informationen in Fehlermeldungen [NSF]

## Sicherheitsüberprüfungen [SRF]

- **Statische Code-Analyse**: Bei jedem Commit
- **Dependency-Scanning**: Wöchentlich
- **Penetrationstests**: Vierteljährlich
- **Sicherheitsaudits**: Jährlich
- **Compliance-Checks**: Bei jeder größeren Änderung
- **Frontend-Tests**: 100% Coverage für sicherheitskritische Funktionen [KP100]
