# MedEasy Accessibility (Barrierefreiheit)

„Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

Diese Dokumentation beschreibt die Barrierefreiheits-Richtlinien für MedEasy.

## Accessibility-Übersicht

MedEasy folgt den WCAG 2.1 AA-Richtlinien und berücksichtigt die besonderen Anforderungen medizinischer Software.

## WCAG 2.1 Compliance

### Level A (Erfüllt)
- **Tastaturzugänglichkeit**: Alle Funktionen per Tastatur bedienbar
- **Fokus-Indikatoren**: Deutlich sichtbare Fokus-Rahmen
- **Alt-Texte**: Alle Bilder haben beschreibende Alt-Texte
- **Überschriften-Hierarchie**: Logische H1-H6 Struktur

### Level AA (Erfüllt)
- **Kontrast**: Mindestens 4.5:1 für normalen Text
- **Textgröße**: Skalierbar bis 200% ohne Funktionsverlust
- **Farbkodierung**: Informationen nicht nur über Farbe vermittelt
- **Fokus-Sichtbarkeit**: Fokus-Indikatoren bei 2:1 Kontrast

### Level AAA (Angestrebt)
- **Hoher Kontrast**: 7:1 für sicherheitskritische Elemente [PSF]
- **Erweiterte Navigation**: Breadcrumbs und Sitemaps
- **Fehlerprävention**: Bestätigungen für kritische Aktionen
- **Hilfe-Kontext**: Kontextuelle Hilfe verfügbar

## Medizinische Accessibility

### Patientensicherheit [PSF]
- **Hoher Kontrast**: 7:1 für medizinische Daten
- **Große Schrift**: Mindestens 16px für medizinische Inhalte
- **Klare Sprache**: Einfache, verständliche Begriffe
- **Fehlerprävention**: Bestätigungen für kritische medizinische Aktionen

### Schweizer Besonderheiten [SF]
- **Deutsche Sprache**: Alle Inhalte in deutscher Sprache
- **Schweizer Begriffe**: "Spital" statt "Krankenhaus"
- **Datumsformat**: DD.MM.YYYY für Schweizer Nutzer
- **Währung**: CHF-Anzeige korrekt formatiert

## Screen Reader Support

### Semantisches HTML
- **Landmarks**: `<main>`, `<nav>`, `<aside>`, `<section>`
- **Headings**: Logische H1-H6 Hierarchie
- **Lists**: `<ul>`, `<ol>` für strukturierte Inhalte
- **Tables**: `<th>`, `<caption>` für Datentabellen

### ARIA-Attribute
- **aria-label**: Beschreibende Labels für Buttons
- **aria-describedby**: Zusätzliche Beschreibungen
- **aria-expanded**: Status von Dropdown-Menüs
- **aria-live**: Dynamische Inhaltsänderungen

### Sicherheitsinformationen [CT]
- **Verarbeitungsort**: "Lokale Verarbeitung aktiv" / "Cloud-Verarbeitung aktiv"
- **Verschlüsselung**: "Daten verschlüsselt" Status
- **Anonymisierung**: "Anonymisierung aktiv" Bestätigung
- **Audit-Status**: "Audit-Protokollierung aktiv"

## Keyboard Navigation

### Tab-Reihenfolge
- **Logische Reihenfolge**: Von oben nach unten, links nach rechts
- **Skip-Links**: Sprung zum Hauptinhalt
- **Fokus-Falle**: Modale Dialoge halten Fokus
- **Escape-Taste**: Schließt Dialoge und Menüs

### Tastaturkürzel
- **Ctrl+S**: Speichern (wo anwendbar)
- **Ctrl+Z**: Rückgängig
- **Ctrl+F**: Suchen
- **F1**: Hilfe aufrufen

### Sicherheitskürzel [ZTS]
- **Ctrl+Shift+K**: Killswitch aktivieren [DK]
- **Ctrl+Shift+A**: Audit-Trail anzeigen [ATV]
- **Ctrl+Shift+S**: Sicherheitsstatus anzeigen [CT]

## Visuelle Accessibility

### Farbkontrast
- **Normal Text**: 4.5:1 Mindestkontrast
- **Große Texte**: 3:1 Mindestkontrast
- **Sicherheitskritisch**: 7:1 Kontrast [PSF]
- **Grafiken**: 3:1 für informative Grafiken

### Farbblindheit
- **Deuteranopie**: Rot-Grün-Schwäche berücksichtigt
- **Protanopie**: Rot-Schwäche berücksichtigt
- **Tritanopie**: Blau-Gelb-Schwäche berücksichtigt
- **Zusätzliche Indikatoren**: Icons zusätzlich zu Farben

### Zoom und Skalierung
- **200% Zoom**: Alle Funktionen verfügbar
- **400% Zoom**: Text lesbar (WCAG AAA)
- **Responsive**: Layout passt sich an
- **Horizontales Scrollen**: Vermieden bei 320px Breite

## Motor-Accessibility

### Touch-Targets
- **Mindestgröße**: 44x44px für Touch-Ziele
- **Abstand**: 8px zwischen Touch-Zielen
- **Große Buttons**: Für kritische Aktionen
- **Drag & Drop**: Alternative Eingabemethoden

### Maus-Alternativen
- **Tastatur-Navigation**: Vollständig ohne Maus bedienbar
- **Voice Control**: Kompatibel mit Sprachsteuerung
- **Switch Navigation**: Unterstützung für Switch-Geräte
- **Eye Tracking**: Vorbereitung für Eye-Tracking-Geräte

## Kognitive Accessibility

### Einfache Sprache
- **Medizinische Begriffe**: Deutsche Fachbegriffe erklärt [MDL]
- **Kurze Sätze**: Maximal 20 Wörter pro Satz
- **Aktive Sprache**: Aktiv statt Passiv
- **Konsistente Begriffe**: Gleiche Begriffe für gleiche Konzepte

### Fehlerbehandlung
- **Klare Fehlermeldungen**: Verständliche Beschreibungen
- **Lösungsvorschläge**: Wie Fehler behoben werden können
- **Fehlerprävention**: Validierung vor Übermittlung
- **Bestätigungen**: Für kritische Aktionen [PSF]

### Navigation
- **Breadcrumbs**: Orientierung in der Anwendung
- **Suchfunktion**: Inhalte schnell finden
- **Sitemap**: Übersicht aller Bereiche
- **Hilfe-System**: Kontextuelle Hilfe verfügbar

## Testing

### Automated Testing
- **axe-core**: Automatische Accessibility-Prüfung
- **Lighthouse**: Accessibility-Score > 95
- **WAVE**: Web Accessibility Evaluation
- **Pa11y**: Command-line Accessibility Testing

### Manual Testing
- **Screen Reader**: NVDA, JAWS, VoiceOver
- **Keyboard Only**: Komplette Navigation ohne Maus
- **High Contrast**: Windows High Contrast Mode
- **Zoom**: 200% und 400% Zoom-Tests

### User Testing
- **Benutzer mit Behinderungen**: Echte Nutzer testen
- **Medizinisches Personal**: Ärzte und Pflegekräfte
- **Verschiedene Altersgruppen**: Jung bis alt
- **Verschiedene Technologien**: Verschiedene Hilfsmittel

## Compliance

### Rechtliche Anforderungen
- **WCAG 2.1 AA**: Internationale Standards
- **EN 301 549**: Europäische Norm
- **Behindertengleichstellungsgesetz**: Schweizer Recht
- **Barrierefreiheitsstärkungsgesetz**: EU-Richtlinie

### Medizinische Standards
- **MDR**: Medical Device Regulation
- **IEC 62304**: Medizinische Software
- **ISO 14155**: Klinische Prüfungen
- **ISO 27001**: Informationssicherheit

## Dokumentation

### Accessibility Statement
- **Compliance-Level**: WCAG 2.1 AA
- **Bekannte Probleme**: Dokumentierte Einschränkungen
- **Kontakt**: Feedback-Möglichkeiten
- **Updates**: Regelmäßige Aktualisierungen

### Benutzerhandbuch
- **Hilfsmittel-Unterstützung**: Kompatible Technologien
- **Tastaturkürzel**: Vollständige Liste
- **Anpassungen**: Verfügbare Einstellungen
- **Problemlösung**: Häufige Probleme und Lösungen

## Nächste Schritte

- Implementierung von Voice Control Support
- Erweiterte Eye-Tracking-Unterstützung
- Verbesserung der kognitiven Accessibility
- Regelmäßige Benutzertests mit Menschen mit Behinderungen
