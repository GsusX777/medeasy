# MedEasy UI Design-Implementierungsplan

„Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

Dieser Plan zeigt, wie die Design-Strategie mit den vorhandenen Komponenten umgesetzt wird.

## 🎯 Übersicht

**Ziel:** Integration der vorhandenen 13 UI-Komponenten in die geplante Design-Strategie aus `DESIGN_STRATEGY.md`

**Status:** Planungsphase - Mapping zwischen Design und vorhandenen Komponenten

---

## 📋 Komponenten-Mapping

### ✅ Bereits vorhanden (15 Komponenten)

| Design-Element | Vorhandene Komponente | Status | Anpassung nötig |
|----------------|----------------------|--------|-----------------|
| **Header-Bereich** | | | |
| Logo & Session-Info | AppLayout.svelte | ✅ Vorhanden | Layout angepasst |
| Session-Status | SessionRecorder.svelte | ✅ Vorhanden | Status-Indikatoren sollen nicht erweitert werden |
| Notfall-Funktionen | SecuritySettings.svelte | ✅ Vorhanden | Notfall-Buttons hinzugefügt |
| **Sidebar** | | | |
| Session-Management | Sidebar.svelte | ✅ Implementiert | ✅ Abgeschlossen |
| Audio-Control | Sidebar.svelte | ✅ Implementiert | ✅ Aufnahme-Button integriert |
| Performance Monitor | PerformanceMonitor.svelte | ✅ Implementiert | ✅ Abgeschlossen |
| **Hauptbereich** | | | |
| Transkript-Anzeige | TranscriptViewer.svelte | ✅ Vorhanden | Design-Spezifikation umsetzen |
| Content-Tabs | - | ❌ Fehlt | Tab-System implementieren |
| Split-View | - | ❌ Fehlt | Neue Komponente nötig |
| **Sicherheit** | | | |
| Anonymisierung | AnonymizationNotice.svelte | ✅ Vorhanden | Status-Anzeige erweitern |
| Anonymisierungs-Review | AnonymizationReview.svelte | ✅ Vorhanden | Review-UI anpassen |
| Cloud-Indikator | ProcessingLocationIndicator.svelte | ✅ Vorhanden | Design anpassen |
| Schweizerdeutsch-Warnung | SwissGermanAlert.svelte | ✅ Vorhanden | Integration in Header |
| Audit-Trail | AuditTrailViewer.svelte | ✅ Vorhanden | Panel-Integration |
| **Gemeinsame Elemente** | | | |
| Bestätigungsdialoge | ConfirmDialog.svelte | ✅ Vorhanden | Einwilligungs-Dialog erweitern |
| Sicherheits-Badges | SecurityBadge.svelte | ✅ Vorhanden | Design-System anpassen |
| Ladeanzeigen | Spinner.svelte | ✅ Vorhanden | Styling anpassen |

---

## 🏗️ Implementierungsplan

### **Phase 1: Layout-Struktur (Woche 1-2)**

#### 1.1 AppLayout.svelte erweitern [TSF][ZTS]
- [x] Header-Bereich mit Logo implementieren
- [x] Sub-Header-Bereich mit Session und Patienten-Info implementieren
- [x] Sidebar-Struktur (280px) mit Navigation erstellen
- [x] Hauptbereich für Content-Tabs vorbereiten (3 Tabs: Transkript, Patienten, Konsultationen)
- [x] Analyse-Panel (collapsible, höhenverstellbar 300px) hinzufügen
- [x] Responsive Grid-Layout implementieren

**Dateien:**
- `src/lib/components/AppLayout.svelte` (✅ erweitert)
- `src/lib/components/Header.svelte` (neu)
- `src/lib/components/Sidebar.svelte` (✅ vollständig implementiert)
- `src/lib/components/PerformanceMonitor.svelte` (✅ in Sidebar integriert)

#### 1.2 SessionRecorder.svelte anpassen [PK][CT]
- [x] Audio-Control in Sidebar integrieren
- [x] Session-Status-Indikatoren erweitern (🔴⏸️✅)
- [x] Aufnahme-Button (100px) mit Tastenkürzel (Space)
- [x] Audio-Quality-Meter hinzufügen
- [x] Performance-Monitor-Integration vorbereiten

**Dateien:**
- `src/lib/components/SessionRecorder.svelte` (erweitert)
- `src/lib/components/AudioControl.svelte` (neu)

### **Phase 2: Hauptbereich-Seiten & Navigation (Woche 2-3)**

#### 2.1 Content-Tabs implementieren [TSF][ZTS]
- [x] Tab-Navigation für 3 Hauptseiten erstellen
- [x] Tab 1: Transkriptionsansicht (bestehend)
- [x] Tab 2: Patientenlistenansicht (neu)
- [x] Tab 3: Konsultationsübersicht (neu)
- [ ] Tab-State-Management mit Svelte Stores global einrichten

**Dateien:**
- `src/lib/components/ContentTabs.svelte` (neu)
- `src/lib/components/PatientListView.svelte` (neu)
- `src/lib/components/ConsultationListView.svelte` (neu)

#### 2.2 Patientenlistenansicht implementieren [EIV][AIU][SF]
- [x] Patiententabelle mit Datenbankfeldern:
  - [x] AnonymizedFirstName, AnonymizedLastName [AIU]
  - [x] AnonymizedDateOfBirth (DD.MM.YYYY Format) [SF]
  - [x] InsuranceNumberHash (maskiert anzeigen)
  - [x] Anzahl Sessions pro Patient
  - [x] Created/LastModified Zeitstempel
- [x] Patient neu anlegen Button mit Modal-Dialog
- [x] Patient importieren Button (ausgegraut/disabled)
- [x] Suchfunktion und Filteroptionen
- [x] Sortierung nach Name, Geburtsdatum, Erstellungsdatum
- [x] Paginierung für große Patientenlisten

**Dateien:**
- `src/lib/components/PatientListView.svelte` (neu)
- `src/lib/components/PatientCreateModal.svelte` (neu)
- `src/lib/components/PatientImportButton.svelte` (neu, disabled)

#### 2.3 Konsultationsübersicht implementieren [EIV][SF]
- [x] Konsultationstabelle mit Session-Datenbankfeldern:
  - [x] Patient (AnonymizedFirstName + AnonymizedLastName) [AIU]
  - [x] SessionDate (DD.MM.YYYY Format) [SF]
  - [x] StartTime - EndTime (HH:MM Format)
  - [x] Status (Scheduled, InProgress, Completed, Cancelled)
  - [x] EncryptedNotes (Vorschau, verschlüsselt) [EIV]
  - [x] Created/LastModified Zeitstempel
- [x] Status-Filter (Alle, Geplant, Laufend, Abgeschlossen, Abgebrochen)
- [x] Datumsbereich-Filter
- [x] Sortierung nach Datum, Patient, Status
- [x] Konsultation bearbeiten/öffnen Funktionalität
- [x] Neue Konsultation anlegen Button

**Dateien:**
- `src/lib/components/ConsultationListView.svelte` (neu)
- `src/lib/components/ConsultationCreateModal.svelte` (neu)
- `src/lib/components/SessionStatusBadge.svelte` (neu)

### **Phase 3: UX-Optimierung & Subheader (Woche 3-4)**

#### 3.1 Subheader mit Patienteninfo implementieren [MDL][PSF]
- [x] Subheader-Komponente erstellen
- [x] Gewählter Patient anzeigen (anonymisiert)
- [x] Aktuelle Session-Info (Datum, Status, Dauer)
- [x] Aufnahme-Controls (🔴 REC, ⏸️ Pause, ⏹️ Stop)
- [x] Session-Timer mit Live-Update
- [x] Patient-Session-Zuordnung implementieren

**Dateien:**
- `src/lib/components/SubHeader.svelte` (neu)
- `src/lib/components/RecordingControls.svelte` (neu)
- `src/lib/components/SessionTimer.svelte` (neu)
#### 4.1 TranscriptViewer Split-View implementieren [MDL][AIU]
- [x] Split-View Layout: Transkript (links) + Analyse (rechts)
- [x] Live-Transkript-Bereich:
  - [x] Speaker-Kennzeichnung
  - [x] Editierbare Einträge
  - [x] Timestamp-Anzeige
  - [x] Scroll-to-latest-Funktionalität
- [x] Analyse-Bereich (rechts):
  - [x] Confidence-Review-Panel
  - [x] Live-Analyse-Panel
  - [x] Export-Panel

**Dateien:**
- `src/lib/components/TranscriptViewer.svelte` (erweitern)
- `src/lib/components/TranscriptSplitView.svelte` (neu)
- `src/lib/components/TranscriptEntry.svelte` (neu)

#### 4.2 Confidence-Review integrieren [AIU][ARQ]
- [x] Kombinierte Confidence-Review:
  - [x] Anonymisierung (Namen, Adressen, Tel.)
  - [x] Medizinische Begriffe (Symptome, Diagnosen)
  - [x] Unklare Wörter (Akustik-Probleme)
- [x] Visuelles Confidence-System:
  - [x] 🟢 Hoch (>80%): Auto-akzeptiert
  - [x] ⚠️ Mittel (50-80%): Gelb markiert
  - [x] 🔴 Niedrig (<50%): Review erforderlich
- [x] Batch-Review-Funktionen

**Dateien:**
- `src/lib/components/ConfidenceReview.svelte` (neu)
- `src/lib/components/AnonymizationReview.svelte` (erweitern)
- `src/lib/components/ConfidenceIndicator.svelte` (neu)

#### 4.3 Live-Analyse-Panel [PK][MDL]
- [x] Tabs: Symptome, Diagnose, Export
- [x] AI-Provider-Anzeige (OpenAI/Claude/Gemini/Lokal)
- [x] Processing-Status mit Spinner
- [x] Live-Symptom-Erkennung:
  - [x] Symptom-Liste mit ICD-Codes (G43.9, K30, R50.9, J06.9)
  - [x] Confidence-Level pro Symptom (70-100%)
  - [x] Keyword-basierte Erkennung
- [x] Diagnose-Vorschläge:
  - [x] ICD-10-Codes mit Wahrscheinlichkeit
  - [x] Symptom-basierte Vorschläge
  - [x] Confidence-Bewertung
- [x] Export-Funktionen:
  - [x] PDF-Export mit Schweizer Formatierung [SF]
  - [x] Word, Text, JSON Export
  - [x] Anonymisierung immer aktiv [AIU]

**Dateien:**
- `src/lib/components/LiveAnalysisPanel.svelte` (neu)
- `src/lib/components/SymptomsTab.svelte` (neu)
- `src/lib/components/DiagnosisTab.svelte` (neu)
- `src/lib/components/ExportTab.svelte` (neu)

### **Phase 5: Erweiterte Features (Woche 5-6)**

#### 5.1 Einwilligungs-Management [PbD]
- [ ] ConfirmDialog.svelte für Einwilligung erweitern
- [ ] Checkboxes für Aufnahme/Cloud-Verarbeitung
- [ ] Session-Start-Workflow implementieren
- [ ] Consent-State-Management

**Dateien:**
- `src/lib/components/ConfirmDialog.svelte` (erweitern)
- `src/lib/components/ConsentDialog.svelte` (neu)

#### 5.2 Patientenliste UX-Optimierung [MDL][PSF]
- [ ] "Session starten" Button pro Patient
- [ ] "Letzte Session anzeigen" Link
- [ ] Session-Counter und letztes Datum anzeigen
- [ ] Schnellaktionen: Neue Session, Session fortsetzen
- [ ] Patient-Session-Zuordnung in Subheader

**Dateien:**
- `src/lib/components/PatientListView.svelte` (erweitern)
- `src/lib/components/PatientSessionActions.svelte` (neu)
- `src/lib/components/QuickActions.svelte` (neu)

#### 5.3 Session-Detail-Ansicht [MDL][ATV]
- [ ] Vollständige Transkription anzeigen/bearbeiten
- [ ] Session-Notizen hinzufügen
- [ ] Status ändern (Geplant → Laufend → Abgeschlossen)
- [ ] Export-Funktionen [SF]
- [ ] Session-Historie und Audit-Trail

**Dateien:**
- `src/lib/components/SessionDetailView.svelte` (neu)
- `src/lib/components/SessionNotes.svelte` (neu)
- `src/lib/components/SessionStatusEditor.svelte` (neu)

### **Phase 6: Polish & Testing (Woche 6-7)**

#### 6.1 Design-System finalisieren [PSF]
- [ ] Konsistente Farb- und Typografie-Palette
- [ ] Icon-System standardisieren
- [ ] Component-Library dokumentieren
- [ ] Accessibility-Standards (WCAG 2.1 AA)

**Dateien:**
- `src/lib/styles/design-system.css` (neu)
- `src/lib/components/IconLibrary.svelte` (neu)
- `docs/ui/component-library.md` (neu)

#### 6.2 Keyboard-Shortcuts & Error-Handling [ZTS]
- [ ] Globale Shortcuts (Ctrl+N für neue Session)
- [ ] Tab-Navigation optimieren
- [ ] Accessibility-Shortcuts
- [ ] Shortcut-Hilfe-Dialog
- [ ] Globale Error-Boundary
- [ ] User-friendly Error-Messages
- [ ] Retry-Mechanismen
- [ ] Offline-Handling

**Dateien:**
- `src/lib/utils/keyboard-shortcuts.ts` (neu)
- `src/lib/components/ShortcutHelp.svelte` (neu)
- `src/lib/components/ErrorBoundary.svelte` (neu)
- `src/lib/components/ErrorMessage.svelte` (neu)
- `src/lib/utils/error-handling.ts` (neu)

---

## 🚨 Kritische Anforderungen

### **Unveränderliche Sicherheitsfeatures [AIU][ZTS]**
- [ ] Anonymisierung IMMER aktiv und sichtbar
- [ ] Cloud-Verarbeitung nur mit expliziter Zustimmung
- [ ] Notfall-Funktionen immer erreichbar
- [ ] Session-Status permanent sichtbar
- [ ] Audit-Trail für alle UI-Aktionen

### **Schweizer Compliance [SF][MDL]**
- [ ] Deutsche Sprache (Hochdeutsch)
- [ ] Schweizer Begriffe ("Spital")
- [ ] Datumsformat DD.MM.YYYY
- [ ] CHF-Währungsanzeige

### **Performance-Anforderungen [PSF]**
- [ ] <100ms UI-Response-Zeit
- [ ] Smooth Scrolling im Transkript
- [ ] Effiziente Real-time Updates
- [ ] Memory-optimierte Komponenten

---

## 📊 Fortschritts-Tracking

### **Komponenten-Status**
- ✅ **Vorhanden (13):** Basis-Komponenten implementiert
- 🔄 **In Arbeit (0):** Noch keine Komponenten in Bearbeitung
- ❌ **Fehlend (8):** Neue Komponenten nötig

### **Design-Elemente-Status**
- ✅ **Layout-Struktur:** 30% (AppLayout vorhanden)
- ✅ **Sicherheits-UI:** 80% (Meiste Komponenten vorhanden)
- ❌ **Analyse-Panel:** 0% (Komplett neu)
- ❌ **Split-View:** 0% (Komplett neu)
- ✅ **Einstellungen:** 60% (Basis vorhanden)

---

## 🔄 Nächste Schritte

1. **Sofort:** AppLayout.svelte erweitern für Basis-Struktur
2. **Diese Woche:** SessionRecorder.svelte und TranscriptViewer.svelte anpassen
3. **Nächste Woche:** Analyse-Panel und Content-Tabs implementieren
4. **Danach:** Split-View und Export-Funktionen

---

## 📝 Notizen

- Alle vorhandenen Komponenten können wiederverwendet werden
- Hauptaufwand liegt in Layout-Integration und neuen Komponenten
- Sicherheitsfeatures sind bereits gut abgedeckt
- Performance-Monitor und Split-View sind die größten neuen Features

**Compliance-Hinweis:** Alle Implementierungen müssen [PSF], [ZTS], [AIU], [ATV], [SF] und [MDL] befolgen.
