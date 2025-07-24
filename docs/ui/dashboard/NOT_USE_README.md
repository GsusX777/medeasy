<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# Dashboard-Dokumentation [SF][SK][DSU]

*Erstellt am: 09.07.2025*

## Übersicht

Das MedEasy Dashboard wurde vereinfacht, um eine klare Tagesübersicht und effiziente Konsultationsverwaltung zu bieten. Es zeigt nur die wichtigsten Informationen für den täglichen Praxisablauf.

## Dashboard-Komponenten

### 1. Tagesübersicht (Vollbreite) [SF]
- **Zweck**: Zeigt alle Konsultationen für den aktuellen Tag
- **Datenquelle**: Session-Tabelle mit `sessionDate = heute`
- **Format**: Schweizer Datumsformat DD.MM.YYYY
- **Status-Anzeige**: 
  - 📅 Geplant (Scheduled)
  - 🔴 Laufend (InProgress) 
  - ✅ Abgeschlossen (Completed)
  - ❌ Abgebrochen (Cancelled)

### 2. Offene Konsultationen [SK]
- **Zweck**: Alle geplanten und laufenden Konsultationen
- **Filter**: `status = 'Scheduled' OR 'InProgress'`
- **Sortierung**: Nach Datum aufsteigend (nächste zuerst)
- **Anzeige**: Patient, Datum, Uhrzeit, Status

### 3. Letzte Konsultationen [SK]
- **Zweck**: Die 5 zuletzt abgeschlossenen Konsultationen
- **Filter**: `status = 'Completed'`
- **Sortierung**: Nach Datum absteigend (neueste zuerst)
- **Limit**: Maximal 5 Einträge

## Datenintegration [EIV][SP]

### Session-Tabelle Mapping
```typescript
interface Session {
  id: string;
  patientId: string;        // Verknüpfung zu Patient
  sessionDate: string;      // DD.MM.YYYY Format [SF]
  startTime: string;        // HH:MM Format
  endTime?: string;         // HH:MM Format (optional)
  status: SessionStatus;    // Scheduled | InProgress | Completed | Cancelled
  encryptedNotes?: string;  // Verschlüsselte Notizen [EIV]
  encryptedAudioReference?: string; // Verschlüsselte Audio-Referenz [EIV]
}
```

### Sicherheitsfeatures [ZTS][AIU][ATV]
- **Datenverschlüsselung**: Alle Patientendaten verschlüsselt [EIV]
- **Anonymisierung**: Patientennamen werden automatisch anonymisiert [AIU]
- **Audit-Trail**: Alle Dashboard-Zugriffe werden geloggt [ATV]
- **Fehlerbehandlung**: Graceful Degradation bei Datenbankfehlern [FSD]

## UI-Design [PSF][SF]

### Farbkodierung
- **Heute**: Blau (#3b82f6) - Hervorhebung der aktuellen Tagesübersicht
- **Offen**: Orange (#f59e0b) - Warnung für ausstehende Termine
- **Abgeschlossen**: Grün (#10b981) - Erfolgreiche Konsultationen

### Responsive Design
- **Desktop**: 3-Spalten-Grid (Heute vollbreit, Offen/Letzte nebeneinander)
- **Mobile**: 1-Spalten-Layout mit vertikaler Anordnung
- **Accessibility**: WCAG 2.1 AA konform [PSF]

### Schweizer Lokalisierung [SF][MFD]
- **Datumsformat**: DD.MM.YYYY (Schweizer Standard)
- **Wochentage**: Deutsche Bezeichnungen
- **Medizinische Begriffe**: Deutsche Fachterminologie
- **Status-Texte**: Deutsche Übersetzungen

## Fehlerbehandlung [FSD][ZTS]

### Loading States
- **Initial Load**: Spinner mit "Lade Dashboard-Daten..."
- **Empty States**: Informative Meldungen bei fehlenden Daten
- **Error States**: Retry-Button mit Fehlermeldung

### Fallback-Verhalten
- **Datenbankfehler**: Fehlermeldung mit Retry-Option
- **Fehlende Patienten**: "Unbekannter Patient" als Fallback
- **Netzwerkfehler**: Offline-Indikator (geplant)

## Performance [PUL]

### Optimierungen
- **Lazy Loading**: Dashboard lädt nur bei Bedarf
- **Caching**: Patient-Daten werden im Store gecacht
- **Batching**: Sessions und Patienten parallel laden
- **Filtering**: Client-seitige Filterung für bessere Performance

### Monitoring
- **Load Time**: <2 Sekunden für komplettes Dashboard
- **Update Frequency**: Automatische Aktualisierung alle 30 Sekunden (geplant)
- **Memory Usage**: Minimaler Speicherverbrauch durch effiziente Filterung

## Compliance [DSC][RA]

### Datenschutz (nDSG/GDPR)
- **Datenminimierung**: Nur notwendige Daten werden angezeigt
- **Zweckbindung**: Dashboard dient nur der Terminübersicht
- **Transparenz**: Benutzer sehen nur ihre eigenen Daten
- **Löschung**: Abgeschlossene Sessions werden nach Retention-Period entfernt

### Medizinische Compliance (MDR)
- **Audit-Trail**: Vollständige Nachverfolgung aller Dashboard-Aktionen [ATV]
- **Datenintegrität**: Konsistente Darstellung der Session-Daten
- **Verfügbarkeit**: 99.9% Uptime für kritische Praxisfunktionen [PSF]

## Entwicklung

### Technische Details
- **Framework**: Svelte 4 mit TypeScript [TSF]
- **Styling**: CSS Grid + Flexbox für responsive Layout
- **State Management**: Svelte Stores für reaktive Updates
- **API Integration**: Tauri Commands für Datenbankzugriff

### Testing [TR]
- **Unit Tests**: Alle Hilfsfunktionen (getPatientName, getStatusIcon, etc.)
- **Integration Tests**: Dashboard-Datenladung und -anzeige
- **E2E Tests**: Vollständige Benutzerinteraktionen
- **Security Tests**: Datenschutz und Verschlüsselung [ZTS]

## Zukünftige Erweiterungen

### Geplante Features (v1.1)
- **Kalender-Widget**: Monatliche Übersicht mit Terminen
- **Drag & Drop**: Termine verschieben per Drag & Drop
- **Benachrichtigungen**: Push-Notifications für anstehende Termine
- **Export**: PDF-Export der Tagesübersicht
- **Statistiken**: Konsultations-Metriken und Trends

### Performance-Verbesserungen
- **Real-time Updates**: WebSocket-Verbindung für Live-Updates
- **Offline-Modus**: Cached Dashboard für Offline-Nutzung
- **Progressive Loading**: Stufenweise Datenladung für bessere UX

## Projektregeln-Compliance

- **[SF]**: Schweizer Datumsformat und deutsche Lokalisierung
- **[SK]**: Session-basierte Konsultationsverwaltung
- **[DSU]**: Dokumentation bei jeder Änderung aktualisiert
- **[EIV]**: Alle Patientendaten verschlüsselt
- **[AIU]**: Unveränderliche Anonymisierung aktiv
- **[ATV]**: Vollständiger Audit-Trail
- **[PSF]**: Patient Safety First - Fehlertolerantes Design
- **[FSD]**: Fail-Safe Defaults bei Systemfehlern
- **[ZTS]**: Zero Tolerance Security - Keine Sicherheitslücken
- **[TSF]**: Technologie-Stack fest definiert (Svelte 4)
