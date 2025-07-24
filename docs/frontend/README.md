<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Frontend [TSF]  

**Frontend-Komponente des MedEasy Systems, basierend auf Tauri + SvelteKit**

![Status: In Entwicklung](https://img.shields.io/badge/Status-In%20Entwicklung-blue)  
![Technologie: Svelte + Tauri](https://img.shields.io/badge/Technologie-Svelte%20%2B%20Tauri-orange)

> [!WICHTIG]  
> Dieses Dokument folgt den [D=C] und [DSU] Dokumentationsrichtlinien des MedEasy-Projekts. Alle Änderungen müssen sofort dokumentiert werden.

## Architektur [CAS]

Die Frontend-Komponente ist Teil der MedEasy Clean Architecture und implementiert die Präsentationsschicht mit folgenden Technologien:

- **Tauri 1.5**: Native Desktop-Integration
- **Svelte 4**: Reaktive UI-Komponenten
- **TypeScript**: Typsicherheit gemäß [ZTS] Zero Tolerance Security

## Entwicklung

### Lokaler Entwicklungs-Server

```bash
# Entwicklungsserver starten
npm run dev

# Server mit geöffnetem Browser starten
npm run dev -- --open
```

### Projektstruktur

- **src/lib/**: Hauptkomponenten und wiederverwendbare Module
  - **components/**: UI-Komponenten mit [ZTS], [CT] und [AIU] Sicherheitsmerkmalen
  - **stores/**: Svelte-Stores für State Management
  - **types/**: TypeScript-Definitionen für strikte Typsicherheit

- **src/routes/**: Anwendungsseiten und Routing

## Build-Prozess

### Web-Build (Entwicklung/Test)

```bash
# Web-Produktions-Build erstellen
npm run build

# Build lokal testen
npm run preview
```

### Tauri Desktop-App Build

```bash
# Im Frontend-Verzeichnis
npm run build

# Im Hauptverzeichnis
cd src-tauri
cargo tauri build
```

## Sicherheitsmerkmale [ZTS] [AIU] [CT]

Dieses Frontend implementiert kritische Sicherheitsfeatures gemäß den MedEasy-Projektregeln:

- **[AIU] Anonymisierung ist UNVERÄNDERLICH**: Kann nicht deaktiviert werden
- **[CT] Cloud-Transparenz**: UI zeigt immer den Datenverarbeitungsort (🔒 Lokal/☁️ Cloud)
- **[ZTS] Zero Tolerance Security**: Strikte Typisierung und sichere API-Integration
- **[ATV] Audit-Trail**: Vollständige Protokollierung aller Operationen

Go into the `package.json` and give your package the desired name through the `"name"` option. Also consider adding a `"license"` field and point it to a `LICENSE` file which you can create from a template (one popular option is the [MIT license](https://opensource.org/license/mit/)).

To publish your library to [npm](https://www.npmjs.com):

```bash
npm publish
```
