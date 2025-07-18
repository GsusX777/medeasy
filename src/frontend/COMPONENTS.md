<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Frontend Komponenten

Diese Dokumentation beschreibt die wichtigsten Komponenten der MedEasy Frontend-Anwendung und wie sie die Sicherheits- und Compliance-Anforderungen implementieren.

## Kern-Komponenten

### ProcessingLocationIndicator [CT]

**Zweck**: Zeigt transparent an, ob Daten lokal oder in der Cloud verarbeitet werden.

**Sicherheitsrelevanz**: 
- Implementiert Cloud-Transparenz [CT]
- Erfüllt nDSG-Anforderungen zur Transparenz der Datenverarbeitung [DSC]

**Verwendung**:
```svelte
<ProcessingLocationIndicator />
```

**Eigenschaften**:
- Zeigt 🔒 für lokale Verarbeitung
- Zeigt ☁️ für Cloud-Verarbeitung
- Farbkodierung (grün für lokal, blau für Cloud)

---

### AnonymizationNotice [AIU]

**Zweck**: Informiert über die automatische Anonymisierung von Patientendaten.

**Sicherheitsrelevanz**:
- Implementiert unveränderliche Anonymisierung [AIU]
- Erfüllt nDSG-Anforderungen zum Datenschutz [DSC]

**Verwendung**:
```svelte
<AnonymizationNotice />
```

**Eigenschaften**:
- Nicht deaktivierbar
- Expandierbare Details zur Anonymisierungsfunktion
- Rechtliche Hinweise zur Datenverarbeitung

---

### SwissGermanAlert [SDH]

**Zweck**: Warnt bei Erkennung von Schweizerdeutsch über mögliche Einschränkungen der Genauigkeit.

**Sicherheitsrelevanz**:
- Implementiert Schweizerdeutsch-Handling [SDH]
- Erhöht Transparenz bei der Spracherkennung

**Verwendung**:
```svelte
<SwissGermanAlert 
  visible={schweizerDeutschErkannt} 
  onDismiss={() => schweizerDeutschErkannt = false} 
/>
```

**Eigenschaften**:
- Beta-Badge für experimentelle Funktionalität
- Schließen-Button für temporäre Ausblendung
- Visuelle Warnung mit Erklärung

---

### KeyManagement.svelte [SP] [ZTS] [ATV]

**Zweck**: Verwaltet die Verschlüsselungsschlüssel für SQLCipher und Feldverschlüsselung mit Schlüsselrotation.

**Sicherheitsrelevanz**:
- Implementiert SQLCipher-Schlüsselverwaltung [SP]
- Erzwingt Zero Tolerance Security [ZTS]
- Vollständiger Audit-Trail für Schlüsseloperationen [ATV]
- Sichere Schlüsselrotation ohne Datenverlust
- Backup und Wiederherstellung von Schlüsseln

**Verwendung**:
```svelte
<KeyManagement 
  onKeyRotated={(result) => handleKeyRotation(result)}
  onBackupCreated={(path) => handleBackupCreated(path)}
/>
```

**Eigenschaften**:
- Automatische Schlüsselrotation nach konfigurierbaren Intervallen
- Sichere Schlüsselgenerierung mit kryptographisch sicheren Zufallszahlen
- Backup-Funktionalität für Notfallwiederherstellung
- Status-Anzeige für aktive Schlüssel
- Audit-Logging aller Schlüsseloperationen
- Produktions-/Entwicklungsmodus-Erkennung

---

### SessionRecorder [ATV] [AIU]

**Zweck**: Steuert die Aufnahme von Arzt-Patienten-Gesprächen mit automatischer Anonymisierung.

**Sicherheitsrelevanz**:
- Implementiert Audit-Trail für alle Aufnahmen [ATV]
- Erzwingt Anonymisierung [AIU]
- Erkennt Schweizerdeutsch [SDH]

**Verwendung**:
```svelte
<SessionRecorder 
  onRecordingComplete={(data) => handleRecordingComplete(data)} 
/>
```

**Eigenschaften**:
- Start/Stopp-Kontrolle
- Automatische Erkennung von Schweizerdeutsch
- Audit-Logging aller Aktionen
- Anzeige des Verarbeitungsstatus

---

### TranscriptViewer [AIU] [ARQ] [MFD]

**Zweck**: Zeigt anonymisierte Transkripte mit Möglichkeit zur Überprüfung unsicherer Anonymisierungen.

**Sicherheitsrelevanz**:
- Zeigt nur anonymisierte Daten [AIU]
- Implementiert Review-Queue für unsichere Erkennungen [ARQ]
- Verwendet Schweizer medizinische Fachbegriffe [MFD]

**Verwendung**:
```svelte
<TranscriptViewer transcript={anonymizedTranscript} />
```

**Eigenschaften**:
- Hervorhebung anonymisierter Bereiche
- Review-Funktion für unsichere Erkennungen
- Korrekte Darstellung medizinischer Fachbegriffe

---

### SecuritySettings [SP] [CT] [AIU] [ATV]

**Zweck**: Ermöglicht Konfiguration von Sicherheitseinstellungen und Cloud-Einwilligung.

**Sicherheitsrelevanz**:
- Konfiguration der SQLCipher-Verschlüsselung [SP]
- Verwaltung der Cloud-Einwilligung [CT]
- Audit-Trail-Einstellungen [ATV]
- Keine Deaktivierung der Anonymisierung möglich [AIU]

**Verwendung**:
```svelte
<SecuritySettings />
```

**Eigenschaften**:
- Opt-in für Cloud-Verarbeitung (nicht global, pro Feature)
- Verschlüsselungseinstellungen
- Audit-Trail-Konfiguration
- Keine Option zur Deaktivierung der Anonymisierung

---

### AppLayout [CT] [SF]

**Zweck**: Hauptlayout der Anwendung mit Navigation, Benutzerinfo und Sicherheitsanzeigen.

**Sicherheitsrelevanz**:
- Zeigt Cloud-Transparenz-Indikator [CT]
- Verwendet Schweizer Formate [SF]

**Verwendung**:
```svelte
<AppLayout>
  <!-- Seiteninhalt hier -->
</AppLayout>
```

**Eigenschaften**:
- Konsistentes Layout mit Sicherheitshinweisen
- Navigationselemente
- Benutzerinformationen
- Schweizer Formatierung (Datum, etc.)

## Stores

### session.ts [SK] [ATV]

**Zweck**: Zentraler Store für Session-Management und Anwendungszustand.

**Sicherheitsrelevanz**:
- Implementiert Session-Konzept [SK]
- Erzeugt Audit-Trail-Einträge [ATV]
- Verwaltet Cloud-Verarbeitungs-Status [CT]

**Verwendung**:
```typescript
import { session } from '$lib/stores/session';

// Session starten
session.startSession();

// Audit-Event loggen
session.logAuditEvent('action_type', 'details');

// Auf Session-Status reagieren
$: if ($session.active) {
  // Aktion bei aktiver Session
}
```

**Eigenschaften**:
- Session-Lebenszyklus-Management
- App-Zustandsverwaltung
- Integration mit Tauri-Commands
- Audit-Logging-Funktionalität

## Tauri Commands

### get_app_info [ATV]

**Zweck**: Liefert Anwendungsinformationen mit Audit-Logging.

**Sicherheitsrelevanz**:
- Erzeugt Audit-Trail-Einträge [ATV]

**Verwendung** (TypeScript):
```typescript
import { invoke } from '@tauri-apps/api/tauri';

const appInfo = await invoke('get_app_info');
```

### get_processing_location [CT]

**Zweck**: Bestimmt, ob Daten lokal oder in der Cloud verarbeitet werden.

**Sicherheitsrelevanz**:
- Implementiert Cloud-Transparenz [CT]
- Respektiert Benutzereinwilligung für Cloud-Verarbeitung

**Verwendung** (TypeScript):
```typescript
import { invoke } from '@tauri-apps/api/tauri';

const location = await invoke('get_processing_location', { 
  feature: 'transcription' 
});
// Gibt 'local' oder 'cloud' zurück
```

## Verwendung der Komponenten

Alle Komponenten wurden entwickelt, um nahtlos zusammenzuarbeiten und die strengen Sicherheits- und Compliance-Anforderungen von MedEasy zu erfüllen. Bei der Implementierung neuer Features sollten diese Komponenten wiederverwendet werden, um Konsistenz und Compliance zu gewährleisten.

### Beispiel: Neue Seite mit Patientendaten

```svelte
<script lang="ts">
  import { session } from '$lib/stores/session';
  import AppLayout from '$lib/components/AppLayout.svelte';
  import ProcessingLocationIndicator from '$lib/components/ProcessingLocationIndicator.svelte';
  import AnonymizationNotice from '$lib/components/AnonymizationNotice.svelte';
  
  // Audit-Trail [ATV]
  $: {
    if ($session.active) {
      session.logAuditEvent('page_viewed', 'patient_data');
    }
  }
</script>

<AppLayout>
  <div class="patient-data-page">
    <h1>Patientendaten</h1>
    
    <!-- Cloud-Transparenz [CT] -->
    <ProcessingLocationIndicator />
    
    <!-- Anonymisierungshinweis [AIU] -->
    <AnonymizationNotice />
    
    <!-- Patientendaten hier (immer anonymisiert) -->
  </div>
</AppLayout>
```

## Compliance-Checkliste

Bei der Entwicklung neuer Komponenten oder der Änderung bestehender Komponenten müssen folgende Punkte beachtet werden:

- [ ] **Anonymisierung [AIU]**: Ist die Anonymisierung unveränderlich implementiert?
- [ ] **Verschlüsselung [SP]**: Werden Patientendaten mit SQLCipher verschlüsselt?
- [ ] **Audit-Trail [ATV]**: Werden alle relevanten Aktionen protokolliert?
- [ ] **Cloud-Transparenz [CT]**: Wird die Verarbeitungslokation klar angezeigt?
- [ ] **Schweizer Formate [SF]**: Werden Schweizer Datumsformate etc. verwendet?
- [ ] **Schweizerdeutsch [SDH]**: Ist die Erkennung von Schweizerdeutsch implementiert (falls relevant)?
- [ ] **Medizinische Fachbegriffe [MFD]**: Werden die korrekten Schweizer Fachbegriffe verwendet?
