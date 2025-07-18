# MedEasy Routing

„Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

Diese Dokumentation beschreibt die Navigation und Seitenstruktur von MedEasy.

## Routing-Übersicht

MedEasy verwendet SvelteKit's dateibasiertes Routing-System.

### Aktuelle Routen

#### / (Hauptseite)
- **Datei**: `src/routes/+page.svelte`
- **Zweck**: Dashboard und Hauptnavigation
- **Features**: Sitzungsübersicht, Schnellzugriff, Sicherheitsstatus
- **Sicherheit**: Sicherheitsstatus-Anzeige [CT]

### Geplante Routen

#### /sessions
- **Zweck**: Sitzungsverwaltung
- **Features**: Neue Sitzung, Sitzungshistorie
- **Sicherheit**: Anonymisierte Patientendaten [AIU]

#### /transcripts
- **Zweck**: Transkript-Verwaltung
- **Features**: Transkript-Liste, Bearbeitung, Export
- **Sicherheit**: Audit-Trail [ATV], Anonymisierung [AIU]

#### /settings
- **Zweck**: Anwendungseinstellungen
- **Features**: Sicherheitseinstellungen, Präferenzen
- **Sicherheit**: Unveränderliche Sicherheitsfeatures [ZTS]

#### /audit
- **Zweck**: Audit-Trail-Ansicht
- **Features**: Protokoll-Anzeige, Filterung
- **Sicherheit**: Vollständige Audit-Protokollierung [ATV]

#### /security
- **Zweck**: Sicherheitsverwaltung
- **Features**: Schlüsselverwaltung, Verschlüsselungseinstellungen
- **Sicherheit**: Erweiterte Sicherheitsoptionen [SP][ZTS]

## Navigation

### Layout-Struktur
- Hauptnavigation in `AppLayout.svelte`
- Responsive Design für verschiedene Bildschirmgrößen
- Breadcrumb-Navigation für tiefere Hierarchien

### Sicherheitsnavigation
- Sicherheitsstatus immer sichtbar [CT]
- Schnellzugriff auf kritische Sicherheitsfunktionen
- Notfall-Buttons für Killswitch [DK]

## Route Guards

### Authentifizierung
- Alle Routen erfordern gültige Authentifizierung
- Automatische Umleitung zur Login-Seite
- Session-Timeout-Handling

### Sicherheitsprüfungen
- Verschlüsselungsstatus-Prüfung [SP]
- Audit-Trail-Aktivierung [ATV]
- Anonymisierung-Status [AIU]

## Routing-Richtlinien

### Entwicklungsstandards
- Dateibasiertes Routing mit SvelteKit
- TypeScript für alle Route-Komponenten
- Lazy Loading für bessere Performance
- SEO-optimierte Meta-Tags

### Sicherheitsrichtlinien
- Keine sensiblen Daten in URLs [PbD]
- Sichere Parameter-Validierung
- CSRF-Schutz für alle Formulare
- Audit-Logging für Navigation [ATV]

## Testing

Siehe [../testing/README.md](../testing/README.md) für Routing-Tests.
