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
- [ ] Hauptbereich für Content-Tabs vorbereiten
- [ ] Analyse-Panel (collapsible, höhenverstellbar 300px) hinzufügen
- [ ] Responsive Grid-Layout implementieren

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

### **Phase 2: Transkript & Sicherheit (Woche 2-3)**

#### 2.1 TranscriptViewer.svelte erweitern [AIU][ATV]
- [ ] Live-Transkript-Header mit Anonymisierungs-Status
- [ ] Transkript-Einträge mit Speaker-Kennzeichnung
- [ ] Editierbare Einträge implementieren
- [ ] Timestamp-Anzeige hinzufügen
- [ ] Scroll-to-latest-Funktionalität

**Dateien:**
- `src/lib/components/TranscriptViewer.svelte` (erweitern)
- `src/lib/components/TranscriptEntry.svelte` (neu)

#### 2.2 Anonymisierung integrieren [AIU][ARQ]
- [ ] AnonymizationNotice.svelte in Header integrieren (nicht benötigt)
- [ ] AnonymizationReview.svelte als Modal/Panel
- [ ] Anonymisierungs-Status in TranscriptViewer
- [ ] Review-Button bei unsicheren Erkennungen
- [ ] Confidence-Level-Anzeige

**Dateien:**
- `src/lib/components/AnonymizationNotice.svelte` (erweitern)
- `src/lib/components/AnonymizationReview.svelte` (erweitern)

#### 2.3 Sicherheits-Indikatoren [CT][ZTS]
- [ ] ProcessingLocationIndicator.svelte in Header (nicht benötigt)
- [ ] SecurityBadge.svelte für Status-Anzeigen (nicht benötigt)
- [ ] SwissGermanAlert.svelte als Notification (nicht benötigt)
- [ ] Datenschutz-Indikatoren (🔒☁️🔐⚠️) (nicht benötigt)

**Dateien:**
- `src/lib/components/ProcessingLocationIndicator.svelte` (erweitern)
- `src/lib/components/SecurityBadge.svelte` (erweitern)
- `src/lib/components/SwissGermanAlert.svelte` (erweitern)

### **Phase 3: Erweiterte Features (Woche 3-4)**

#### 3.1 Content-Tabs implementieren [TSF]
- [ ] Tab-System für Transkript/Zusammenfassung
- [ ] Tab-Navigation mit Icons (📝📊)
- [ ] Content-Switching-Logik
- [ ] Tab-State-Management

**Dateien:**
- `src/lib/components/ContentTabs.svelte` (neu)
- `src/lib/components/SummaryView.svelte` (neu)

#### 3.2 Analyse-Panel erstellen [PK]
- [ ] Collapsible Panel (300px Höhe)
- [ ] Tabs: Symptome, Diagnose, Export
- [ ] AI-Provider-Anzeige (OpenAI/Claude/Gemini/Lokal)
- [ ] Processing-Status mit Spinner
- [ ] Symptom-Liste mit ICD-Codes

**Dateien:**
- `src/lib/components/AnalysisPanel.svelte` (neu)
- `src/lib/components/SymptomsTab.svelte` (neu)
- `src/lib/components/DiagnosisTab.svelte` (neu)
- `src/lib/components/ExportTab.svelte` (neu)

#### 3.3 Einwilligungs-Management [PbD]
- [ ] ConfirmDialog.svelte für Einwilligung erweitern
- [ ] Checkboxes für Aufnahme/Cloud-Verarbeitung
- [ ] Session-Start-Workflow implementieren
- [ ] Consent-State-Management

**Dateien:**
- `src/lib/components/ConfirmDialog.svelte` (erweitern)
- `src/lib/components/ConsentDialog.svelte` (neu)

### **Phase 4: Export & Split-View (Woche 4-5)**

#### 4.1 Export-Funktionen [SF]
- [ ] Export-Tab mit Optionen (E-PAT, PDF, FHIR)
- [ ] Export-History-Anzeige
- [ ] Download-Funktionalität
- [ ] Split-View-Trigger

**Dateien:**
- `src/lib/components/ExportTab.svelte` (erweitern)
- `src/lib/components/ExportHistory.svelte` (neu)

#### 4.2 Split-View implementieren [TSF]
- [ ] Split-View-Container (50%/50%)
- [ ] Read-only Transkript links
- [ ] Editierbares Dokument rechts
- [ ] Toolbar mit Aktionen
- [ ] Resize-Funktionalität

**Dateien:**
- `src/lib/components/SplitView.svelte` (neu)
- `src/lib/components/DocumentEditor.svelte` (neu)

### **Phase 5: Einstellungen & Performance (Woche 5-6)**

#### 5.1 Einstellungen erweitern [ZTS][SP]
- [ ] SecuritySettings.svelte für Audio/Performance
- [ ] DatabaseSecuritySettings.svelte Integration
- [ ] KeyManagement.svelte für Verschlüsselung
- [ ] Whisper-Model-Auswahl
- [ ] GPU-Acceleration-Toggle

**Dateien:**
- `src/lib/components/SecuritySettings.svelte` (erweitern)
- `src/lib/components/DatabaseSecuritySettings.svelte` (erweitern)
- `src/lib/components/KeyManagement.svelte` (erweitern)
- `src/lib/components/AudioSettings.svelte` (neu)

#### 5.2 Performance-Monitor [TSF]
- [x] CPU/RAM/Latency-Anzeige
- [x] Collapsible in Sidebar
- [ ] Warning/Critical-Thresholds
- [ ] Real-time Updates

**Dateien:**
- `src/lib/components/PerformanceMonitor.svelte` (neu)

#### 5.3 Audit-Integration [ATV]
- [ ] AuditTrailViewer.svelte in Panel
- [ ] Audit-Logging für UI-Aktionen
- [ ] Session-Audit-Anzeige

**Dateien:**
- `src/lib/components/AuditTrailViewer.svelte` (erweitern)

### **Phase 6: Polish & Testing (Woche 6-7)**

#### 6.1 Design-System finalisieren [PSF]
- [ ] Farbschema implementieren
- [ ] Responsive Breakpoints
- [ ] Animationen & Transitions
- [ ] Accessibility-Features

#### 6.2 Keyboard-Shortcuts [PSF]
- [ ] Globale Shortcuts (Space, Ctrl+S, F11)
- [ ] Shortcut-Anzeige in UI
- [ ] Accessibility-Navigation

#### 6.3 Error-Handling [ZTS]
- [ ] Error-Boundaries
- [ ] User-friendly Error-Messages
- [ ] Fallback-UI-States

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
