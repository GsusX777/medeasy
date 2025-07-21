# MedEasy UI-Komponenten

„Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

Diese Dokumentation beschreibt alle UI-Komponenten der MedEasy-Anwendung.

## Komponentenstatistiken

- **Gesamt**: 15 Komponenten
- **Hauptkomponenten**: 5 (AppLayout, Header, SubHeader, Sidebar, SessionRecorder, TranscriptViewer)
- **Sicherheitskomponenten**: 8
- **Gemeinsame Komponenten**: 4 (ConfirmDialog, SecurityBadge, Spinner, PerformanceMonitor)

## Komponentenübersicht

### Hauptkomponenten

#### AppLayout.svelte
- **Zweck**: Haupt-Layout-Komponente der Anwendung
- **Features**: Header, SubHeader, Integration der Sidebar-Komponente
- **Sicherheit**: Zeigt Sicherheitsstatus und Verschlüsselungsindikator [CT]

#### Sidebar.svelte
**Zweck:** Hauptnavigation mit Aufnahme-Controls und integrierter Performance-Anzeige  
**Features:**
- Drei Hauptmenüpunkte: Dashboard, Patienten, Einstellungen
- Großer roter Aufnahmebutton (●) mit rechteckigen Pause (❚❚) und Stop (■) Controls
- Mikrofon dB-Level-Anzeige während Aufnahme
- Integrierte Performance-Anzeige (CPU, RAM, optional GPU) - einklappbar
- Footer mit Versionsnummer
- Anonymisierung immer sichtbar [AIU]
- Bessere Button-Erkennbarkeit mit Farbkodierung (Rot=Aufnahme/Stop, Orange=Pause)
**Sicherheit:** Aufnahme-Status transparent, Performance-Überwachung [ZTS]

#### Header.svelte
- **Zweck**: Hauptheader mit essentiellen Informationen (55px Höhe)
- **Features**: Logo (160x48px), Schweizer Datum/Zeit-Format [SF]
- **Sicherheit**: Notfall-Killswitch [DK], Benutzerinformationen

#### SubHeader.svelte
- **Zweck**: Sekundärer Header mit Kontext-Informationen (35px Höhe)
- **Features**: Aktueller Patient [MFD], Session-Status-Anzeige [TSF], Sprach-Status [SDH]
- **Layout**: Positioniert neben Sidebar (240px margin-left), weiße Schrift für Status

#### SessionRecorder.svelte
- **Zweck**: Aufnahme und Verwaltung von Patientensitzungen
- **Features**: Audio-Aufnahme, Whisper-Integration, Provider-Kette [PK]
- **Sicherheit**: Automatische Anonymisierung [AIU], Cloud-Transparenz [CT]

#### TranscriptViewer.svelte
- **Zweck**: Anzeige und Bearbeitung von Transkripten
- **Features**: Syntax-Highlighting, Bearbeitungsmodus, Export
- **Sicherheit**: Anonymisierte Darstellung [AIU], Audit-Logging [ATV]

### Sicherheitskomponenten

#### AnonymizationNotice.svelte
- **Zweck**: Hinweis auf aktive Anonymisierung
- **Features**: Unveränderlicher Status-Indikator
- **Sicherheit**: Kann nicht deaktiviert werden [AIU]

#### AnonymizationReview.svelte
- **Zweck**: Review-Prozess für Anonymisierung mit niedriger Konfidenz
- **Features**: Manuelle Überprüfung, Korrektur-Interface
- **Sicherheit**: Review-Queue [ARQ], Audit-Trail [ATV]

#### SecuritySettings.svelte
- **Zweck**: Sicherheitseinstellungen der Anwendung
- **Features**: Verschlüsselungsoptionen, Audit-Konfiguration
- **Sicherheit**: Kritische Einstellungen unveränderlich [ZTS]

#### DatabaseSecuritySettings.svelte
- **Zweck**: Datenbankspezifische Sicherheitseinstellungen
- **Features**: SQLCipher-Konfiguration, Schlüsselverwaltung
- **Sicherheit**: Verschlüsselung erzwungen [SP], Audit-Trail [ATV]

#### KeyManagement.svelte
- **Zweck**: Verwaltung von Verschlüsselungsschlüsseln
- **Features**: Schlüsselrotation, Backup-Verwaltung
- **Sicherheit**: Sichere Schlüsselspeicherung [ZTS]

#### ProcessingLocationIndicator.svelte
- **Zweck**: Anzeige des Verarbeitungsorts (Lokal/Cloud)
- **Features**: 🔒 Lokal / ☁️ Cloud Indikator
- **Sicherheit**: Transparenz über Datenverarbeitung [CT]

#### SwissGermanAlert.svelte
- **Zweck**: Warnung bei Schweizerdeutsch-Erkennung
- **Features**: Automatische Spracherkennung, Genauigkeitshinweis
- **Sicherheit**: Schweizerdeutsch-Handling [SDH]

#### AuditTrailViewer.svelte
- **Zweck**: Anzeige des Audit-Trails
- **Features**: Filterung, Export, Detailansicht
- **Sicherheit**: Vollständige Protokollierung [ATV]

#### PerformanceMonitor.svelte
- **Zweck**: System-Performance-Überwachung für Stabilität [PSF]
- **Features**: CPU/RAM/GPU-Auslastung, Farbkodierung nach Status, Warnungen bei hoher Last
- **Sicherheit**: Performance-Monitoring für Systemstabilität [ZTS], GPU-Beschleunigung optional

### Gemeinsame Komponenten (Common)

#### ConfirmDialog.svelte
- **Zweck**: Bestätigungsdialog für kritische Aktionen
- **Features**: Anpassbare Nachrichten, Buttons
- **Sicherheit**: Schutz vor versehentlichen Aktionen

#### SecurityBadge.svelte
- **Zweck**: Sicherheitsstatus-Anzeige
- **Features**: Farbkodierte Sicherheitsstufen
- **Sicherheit**: Visuelle Sicherheitsindikatoren [CT]

#### Spinner.svelte
- **Zweck**: Ladeanzeigen für asynchrone Operationen
- **Features**: Verschiedene Größen und Stile
- **Sicherheit**: Zeigt Verarbeitungsstatus an [CT]

## Routen-Komponenten

#### +page.svelte (Hauptseite)
- **Zweck**: Hauptseite der Anwendung
- **Features**: Dashboard, Schnellzugriff
- **Sicherheit**: Sicherheitsstatus-Übersicht [CT]

## Komponentenrichtlinien

### Entwicklungsstandards
- Alle Komponenten folgen Clean Architecture [CAS]
- Sicherheitsfeatures sind unveränderlich [AIU][ATV][SP]
- Schweizer Compliance in allen UI-Elementen [SF][MDL]
- Neue Sidebar mit integriertem Performance-Monitor [ZTS][PSF]
- Aufnahme-Steuerung mit unveränderlicher Anonymisierung [AIU]

### Sicherheitsrichtlinien
- Patientendaten werden automatisch anonymisiert [AIU]
- Alle Aktionen werden auditiert [ATV]
- Cloud-Verarbeitung wird transparent angezeigt [CT]
- Kritische Sicherheitsfeatures können nicht deaktiviert werden [ZTS]

### Accessibility
- Alle Komponenten sind keyboard-navigierbar
- Screen-Reader-Unterstützung
- Hoher Kontrast für medizinische Anwendungen
- Deutsche Sprache mit Schweizer Besonderheiten [SF]

## Testing

Siehe [../testing/README.md](../testing/README.md) für Testrichtlinien und -strategien.

## Nächste Schritte

- Implementierung weiterer medizinischer Komponenten
- Verbesserung der Accessibility
- Mobile Responsive Design
- Erweiterte Anonymisierungsoptionen
