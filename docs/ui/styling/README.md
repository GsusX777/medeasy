# MedEasy Styling & Design System

„Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

Diese Dokumentation beschreibt das Design-System und die Styling-Richtlinien für MedEasy.

## Design-System

### Farbpalette

#### Primärfarben (entsprechend DESIGN_STRATEGY.md)
- **Primary Blue**: #3B82F6 (Hauptaktionen, UI-Elemente)
- **Success Green**: #10B981 (Erfolg, Bereit-Status)
- **Warning Orange**: #F59E0B (Warnungen, Aufmerksamkeit)
- **Danger Red**: #EF4444 (Fehler, Stop, kritische Aktionen)
- **Neutral Gray**: #6B7280 (Inaktiv, Text, Hintergründe)

#### Sekundärfarben
- **Background Primary**: #FFFFFF (Haupthintergrund)
- **Background Secondary**: #F9FAFB (Sekundärer Hintergrund)
- **Border Color**: #E5E7EB (Rahmenfarbe)
- **Text Dark**: #374151 (Überschriften)
- **Text Light**: #9CA3AF (Sekundärer Text)

#### Sicherheitsfarben [CT]
- **Lokal-Grün**: #10B981 (🔒 Lokale Verarbeitung)
- **Cloud-Blau**: #3B82F6 (☁️ Cloud-Verarbeitung)
- **Verschlüsselt-Gold**: #F59E0B (Verschlüsselungsstatus)

### Typografie

#### Schriftarten
- **Primär**: Inter (für UI-Elemente)
- **Medizinisch**: Source Sans Pro (für medizinische Inhalte)
- **Monospace**: JetBrains Mono (für Code und IDs)

#### Schriftgrößen
- **Überschrift 1**: 2.25rem (36px)
- **Überschrift 2**: 1.875rem (30px)
- **Überschrift 3**: 1.5rem (24px)
- **Body**: 1rem (16px)
- **Small**: 0.875rem (14px)
- **Caption**: 0.75rem (12px)

### Spacing

#### Abstände (Tailwind-basiert)
- **xs**: 0.25rem (4px)
- **sm**: 0.5rem (8px)
- **md**: 1rem (16px)
- **lg**: 1.5rem (24px)
- **xl**: 2rem (32px)
- **2xl**: 3rem (48px)

### Komponenten-Styling

#### Buttons
- **Primär**: Medizinisches Blau mit weißem Text
- **Sekundär**: Grauer Rahmen mit dunklem Text
- **Gefahr**: Fehlerrot für kritische Aktionen
- **Sicherheit**: Sicherheitsgrün für sichere Aktionen

#### Formulare
- **Input-Felder**: Grauer Rahmen, Fokus in Primärfarbe
- **Labels**: Dunkelgrau, fett
- **Validierung**: Grün für gültig, Rot für Fehler
- **Pflichtfelder**: Roter Stern (*)

#### Karten
- **Standard**: Weißer Hintergrund, subtiler Schatten
- **Sicherheit**: Grüner Rahmen für sichere Inhalte
- **Warnung**: Gelber Rahmen für Warnungen
- **Fehler**: Roter Rahmen für Fehler

## Responsive Design

### Breakpoints
- **Mobile**: < 640px
- **Tablet**: 640px - 1024px
- **Desktop**: > 1024px

### Layout-Prinzipien
- Mobile-First Approach
- Flexible Grid-System
- Touch-freundliche Buttons (min. 44px)
- Lesbare Schriftgrößen auf allen Geräten

## Accessibility

### Kontrast
- **Normal Text**: Mindestens 4.5:1 Kontrastverhältnis
- **Große Texte**: Mindestens 3:1 Kontrastverhältnis
- **Sicherheitskritische Elemente**: Mindestens 7:1 [PSF]

### Fokus-Indikatoren
- Deutlich sichtbare Fokus-Rahmen
- Keyboard-Navigation für alle interaktiven Elemente
- Skip-Links für Hauptinhalte

### Screen Reader
- Semantische HTML-Struktur
- ARIA-Labels für komplexe Komponenten
- Alt-Texte für alle Bilder
- Deutsche Sprache mit Schweizer Begriffen [SF]

## Sicherheits-Styling [CT]

### Sicherheitsindikatoren
- **🔒 Lokal**: Grünes Icon mit "Lokal" Text
- **☁️ Cloud**: Blaues Icon mit "Cloud" Text
- **Verschlüsselt**: Goldenes Schloss-Icon
- **Audit aktiv**: Grünes Protokoll-Icon

### Warnungen [SDH]
- **Schweizerdeutsch**: Gelber Hintergrund mit Warnsymbol
- **Niedrige Konfidenz**: Orange Hintergrund
- **Sicherheitswarnung**: Roter Hintergrund

### CSS-Variablen (entsprechend DESIGN_STRATEGY.md)

```css
:root {
  /* Primärfarben */
  --primary: #3B82F6;      /* Blau - Hauptaktionen */
  --success: #10B981;      /* Grün - Erfolg/Bereit */
  --warning: #F59E0B;      /* Orange - Warnung */
  --danger: #EF4444;       /* Rot - Fehler/Stop */
  --neutral: #6B7280;      /* Grau - Inaktiv */
  
  /* Hintergründe */
  --bg-primary: #FFFFFF;   /* Haupthintergrund */
  --bg-secondary: #F9FAFB; /* Sekundärer Hintergrund */
  --border: #E5E7EB;       /* Rahmenfarbe */
  
  /* Text */
  --text-primary: #374151; /* Haupt-Text */
  --text-secondary: #9CA3AF; /* Sekundärer Text */
}
```

## Theme-System

### Light Theme (Standard)
- Heller Hintergrund für medizinische Klarheit
- Hoher Kontrast für Lesbarkeit
- Beruhigende Farben für Patientenumgebung

### Dark Theme (Geplant)
- Dunkler Hintergrund für Augenentlastung
- Angepasste Kontraste
- Erhaltung der Sicherheitsfarben

## Styling-Richtlinien

### Entwicklungsstandards
- TailwindCSS für Utility-First CSS
- Konsistente Spacing-Skala
- Wiederverwendbare Komponenten-Klassen
- CSS Custom Properties für Themes

### Sicherheitsrichtlinien
- Sicherheitsstatus immer sichtbar [CT]
- Kritische Aktionen deutlich markiert [PSF]
- Keine irreführenden visuellen Hinweise [ZTS]
- Barrierefreie Farbkodierung

## Testing

Siehe [../testing/README.md](../testing/README.md) für Visual Regression Tests.
