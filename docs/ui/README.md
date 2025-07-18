# MedEasy UI-Dokumentation

„Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

Diese Dokumentation beschreibt die Benutzeroberfläche von MedEasy, die mit Svelte 4 und Tauri 1.5 entwickelt wurde.

## Übersicht

Die MedEasy UI folgt den Schweizer Datenschutzbestimmungen [DSC] und implementiert Privacy by Design [PbD]. Alle sicherheitskritischen Funktionen sind unveränderlich aktiviert [AIU][ATV].

**Wichtige Dokumente:**
- [DESIGN_STRATEGY.md](./DESIGN_STRATEGY.md) - Vollständige UI-Spezifikation und Funktionsbeschreibung
- [DESIGN_IMPLEMENTATION_PLAN.md](./DESIGN_IMPLEMENTATION_PLAN.md) - 6-Phasen-Plan zur Umsetzung

## Technologie-Stack [TSF]

- **Frontend Framework**: Svelte 4 mit SvelteKit
- **Desktop Framework**: Tauri 1.5
- **Styling**: TailwindCSS
- **State Management**: Svelte Stores
- **API Communication**: Tauri Commands
- **Build Tool**: Vite

## Architektur

```
src/
├── lib/
│   ├── components/          # UI-Komponenten
│   │   ├── common/         # Wiederverwendbare Komponenten
│   │   └── [feature]/      # Feature-spezifische Komponenten
│   ├── stores/             # Zustandsverwaltung
│   ├── types/              # TypeScript-Typen
│   ├── api/                # API-Schnittstellen
│   └── utils/              # Hilfsfunktionen
├── routes/                 # SvelteKit-Routen
└── app.html               # HTML-Template
```

## Sicherheitsfeatures [ZTS]

- **Unveränderliche Anonymisierung** [AIU]: Kann nicht deaktiviert werden
- **Vollständiger Audit-Trail** [ATV]: Alle Aktionen werden protokolliert
- **Cloud-Transparenz** [CT]: Klare Anzeige von 🔒 Lokal vs ☁️ Cloud
- **Schweizerdeutsch-Warnung** [SDH]: Automatische Erkennung mit Genauigkeitshinweis

## Dokumentationsstruktur

- [Komponenten](./components/README.md) - Übersicht aller UI-Komponenten
- [Stores](./stores/README.md) - Zustandsverwaltung und Datenfluss
- [Routing](./routing/README.md) - Navigation und Seitenstruktur
- [Styling](./styling/README.md) - Design-System und Themes
- [Testing](./testing/README.md) - UI-Tests und Testrichtlinien
- [Accessibility](./accessibility/README.md) - Barrierefreiheit
- [Deployment](./deployment/README.md) - Build und Deployment

## Entwicklungsrichtlinien

1. **Medizinische Terminologie** [MDL]: Verwende deutsche medizinische Fachbegriffe
2. **Schweizer Formate** [SF]: DD.MM.YYYY, CHF, XXX.XXXX.XXXX.XX
3. **Sicherheit First** [PSF]: Patientensicherheit hat oberste Priorität
4. **Clean Code**: Komponenten sollen klein, testbar und wiederverwendbar sein

## Compliance

- **nDSG-Konformität** [DSC]: Schweizer Datenschutzgesetz
- **GDPR/DSGVO**: Europäische Datenschutz-Grundverordnung
- **MDR**: Medical Device Regulation (EU)

## Nächste Schritte

Siehe [ROADMAP.md](../frontend/ROADMAP.md) für geplante UI-Verbesserungen.
