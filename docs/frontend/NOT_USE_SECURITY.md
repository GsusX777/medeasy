<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Frontend Sicherheitsdokumentation

Diese Dokumentation beschreibt die implementierten Sicherheitsmaßnahmen im MedEasy Frontend und dient als Referenz für Entwickler, um die strengen Sicherheits- und Compliance-Anforderungen einzuhalten.

## Grundprinzipien [PSF] [PbD] [RA]

### Patient Safety First [PSF]
Alle Implementierungen im Frontend priorisieren die Patientensicherheit und den Datenschutz. Die Benutzeroberfläche ist so gestaltet, dass sie Fehler minimiert und kritische Informationen klar darstellt.

### Privacy by Design [PbD]
Datenschutz ist von Grund auf in die Architektur integriert:
- Anonymisierung ist unveränderlich implementiert
- Verschlüsselung ist obligatorisch
- Datenminimierung wird durchgesetzt

### Regulatory Awareness [RA]
Die Implementierung berücksichtigt:
- nDSG (Schweizer Datenschutzgesetz)
- DSGVO/GDPR (wo anwendbar)
- Medizinprodukteverordnung (MDR)

## Kritische Sicherheitsfeatures

### 1. Unveränderliche Anonymisierung [AIU]

Die Anonymisierungsfunktion ist unveränderlich und kann unter keinen Umständen deaktiviert werden.

**Implementierung:**
- `AnonymizationNotice.svelte`: Informiert Benutzer über die automatische Anonymisierung
- `TranscriptViewer.svelte`: Zeigt nur anonymisierte Transkripte an
- `session.ts`: Erzwingt Anonymisierung bei allen Datenoperationen

**Technische Details:**
- Anonymisierung erfolgt serverseitig vor der Anzeige im Frontend
- Frontend enthält keine Funktionen zum Deaktivieren der Anonymisierung
- Review-Queue für unsichere Erkennungen implementiert in `TranscriptViewer.svelte`

### 2. SQLCipher Verschlüsselung [SP]

Alle Patientendaten werden mit SQLCipher (AES-256) verschlüsselt.

**Implementierung:**
- Tauri-Backend verwendet SQLCipher für die lokale Datenspeicherung
- Verschlüsselungsschlüssel werden sicher verwaltet
- Keine unverschlüsselten temporären Dateien

**Technische Details:**
- Konfiguration in `Cargo.toml` mit SQLCipher-Dependency
- Implementierung in Tauri-Commands für Datenbankzugriff
- Keine Möglichkeit, unverschlüsselte Speicherung zu aktivieren

### 3. Vollständiger Audit-Trail [ATV]

Jede Benutzeraktion wird protokolliert, um Compliance und Nachvollziehbarkeit zu gewährleisten.

**Implementierung:**
- `session.ts`: `logAuditEvent`-Funktion für Frontend-Ereignisse
- Tauri-Commands protokollieren alle Backend-Operationen
- Jede Komponente mit Patientendaten erzeugt Audit-Einträge

**Technische Details:**
- Audit-Logs enthalten Zeitstempel, Benutzer-ID, Aktionstyp und Kontext
- Logs werden verschlüsselt gespeichert
- Keine Möglichkeit, Audit-Logging zu deaktivieren

### 4. Cloud-Transparenz [CT]

Die Benutzeroberfläche zeigt immer transparent an, ob Daten lokal oder in der Cloud verarbeitet werden.

**Implementierung:**
- `ProcessingLocationIndicator.svelte`: Visuelle Anzeige der Verarbeitungslokation
- `SecuritySettings.svelte`: Einwilligungsverwaltung für Cloud-Verarbeitung
- Tauri-Command `get_processing_location` zur Bestimmung der Verarbeitungslokation

**Technische Details:**
- Farbkodierung: Grün für lokale Verarbeitung, Blau für Cloud-Verarbeitung
- Icons: 🔒 für lokal, ☁️ für Cloud
- Opt-in pro Feature, nicht global

## Schweizer Anforderungen

### 1. Schweizerdeutsch-Handling [SDH]

Die Anwendung erkennt Schweizerdeutsch und informiert den Benutzer über mögliche Einschränkungen.

**Implementierung:**
- `SwissGermanAlert.svelte`: Warnung bei erkanntem Schweizerdeutsch
- Simulation der Erkennung in `SessionRecorder.svelte`

**Technische Details:**
- Beta-Badge für experimentelle Funktionalität
- Warnung kann temporär ausgeblendet werden
- Reduzierte Genauigkeit wird transparent kommuniziert

### 2. Schweizer Formate [SF]

Die Anwendung verwendet durchgängig Schweizer Formate.

**Implementierung:**
- Datumsformat: DD.MM.YYYY
- Versicherungsnummernformat: XXX.XXXX.XXXX.XX
- Schweizer medizinische Fachbegriffe

**Technische Details:**
- Formatierungsfunktionen in `AppLayout.svelte`
- Lokalisierung in `app.html`

### 3. Datenschutz Schweiz [DSC]

Die Implementierung berücksichtigt die strengen Anforderungen des Schweizer Datenschutzgesetzes (nDSG).

**Implementierung:**
- Explizite Einwilligung für Cloud-Verarbeitung
- Transparente Datenverarbeitung
- Datensparsamkeit

**Technische Details:**
- Einwilligungsverwaltung in `SecuritySettings.svelte`
- Transparenzanzeigen in allen relevanten Komponenten

## Sicherheits-Checkliste für Entwickler

Bei der Entwicklung neuer Frontend-Komponenten müssen folgende Punkte beachtet werden:

### Allgemeine Sicherheit

- [ ] **Keine hardcodierten Credentials** [ZTS]
- [ ] **Keine unverschlüsselten Patientendaten** [NUS]
- [ ] **Keine Umgehung von Sicherheitsfeatures** [NSB]
- [ ] **Keine stillen Fehler** [NSF]

### Patientendaten

- [ ] **Anonymisierung unveränderlich** [AIU]
- [ ] **Verschlüsselung mit SQLCipher** [SP]
- [ ] **Audit-Trail für alle Operationen** [ATV]
- [ ] **Keine echten Patientendaten in Tests oder Beispielen** [NRPD]

### Benutzeroberfläche

- [ ] **Cloud-Transparenz implementiert** [CT]
- [ ] **Schweizer Formate verwendet** [SF]
- [ ] **Schweizerdeutsch-Erkennung (falls relevant)** [SDH]
- [ ] **Medizinische Validierung von Eingaben** [MV]

### Fehlerbehandlung

- [ ] **Fehlerkontext wird bewahrt** [ECP]
- [ ] **Keine stillen Fehler** [NSF]
- [ ] **Benutzerfreundliche Fehlermeldungen**

## Verbotene Praktiken

Die folgenden Praktiken sind im MedEasy Frontend **strikt verboten**:

- **Keine echten Patientendaten** [NRPD]: Niemals echte Patientendaten in Code, Kommentaren oder Tests verwenden
- **Keine Sicherheitsumgehungen** [NSB]: Keine Backdoors oder "Admin-Overrides" für Sicherheitsfunktionen
- **Keine unverschlüsselte Speicherung** [NUS]: Patientendaten dürfen niemals unverschlüsselt gespeichert werden
- **Keine stillen Fehler** [NSF]: Fehler müssen klar und deutlich kommuniziert werden
- **Keine experimentelle Medizin** [NEM]: Nur bewährte, validierte medizinische Algorithmen verwenden

## Sicherheits-Review-Prozess

Jede Änderung am Frontend muss einen Sicherheits-Review durchlaufen:

1. **Selbst-Review**: Entwickler prüft Code anhand der Sicherheits-Checkliste
2. **Security Review First** [SRF]: Sicherheits-Review vor funktionalem Review
3. **Automatisierte Tests**: Sicherheitstests müssen bestanden werden
4. **Peer-Review**: Zweiter Entwickler überprüft Sicherheitsaspekte
5. **Dokumentation**: Sicherheitsrelevante Änderungen müssen dokumentiert werden

## Sicherheits-Testabdeckung

Sicherheitskritische Funktionen erfordern 100% Testabdeckung [KP100]:

- Anonymisierung: 100% Coverage
- Verschlüsselung: 100% Coverage
- Audit: 100% Coverage

## Incident Response

Bei Erkennung eines Sicherheitsproblems:

1. Sofort das Sicherheitsteam informieren
2. Problem dokumentieren
3. Keine weiteren Änderungen am betroffenen Code vornehmen
4. Auf Anweisungen des Sicherheitsteams warten

## Ressourcen

- [MedEasy Globale Sicherheitsrichtlinien](../docs/compliance/SECURITY.md)
- [Schweizer Datenschutzgesetz (nDSG)](https://www.fedlex.admin.ch/eli/fga/2020/1998/de)
- [OWASP Frontend Security Best Practices](https://owasp.org/www-project-top-ten/)
