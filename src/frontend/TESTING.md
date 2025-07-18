<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Frontend Testing-Dokumentation

Diese Dokumentation beschreibt die Teststrategie und -anforderungen für die MedEasy Frontend-Anwendung unter Berücksichtigung der strengen Sicherheits- und Compliance-Anforderungen.

## Testprinzipien [KP100] [TR]

### Kritische Pfade 100% [KP100]
Sicherheitskritische Funktionen erfordern 100% Testabdeckung:
- Anonymisierung: 100% Coverage PFLICHT
- Verschlüsselung: 100% Coverage PFLICHT
- Audit: 100% Coverage PFLICHT
- Andere Komponenten: >80% angestrebt

### Testability Requirement [TR]
Alle Komponenten müssen testbar gestaltet werden:
- Klare Trennung von UI und Logik
- Dependency Injection für externe Services
- Mocking-Möglichkeiten für Tauri-Commands

## Testarten

### 1. Unit Tests

Unit Tests prüfen einzelne Komponenten und Funktionen isoliert.

**Frameworks und Tools:**
- Vitest für JavaScript/TypeScript Tests
- Testing Library für Svelte-Komponenten

**Beispiel für Komponententest:**

```typescript
// AnonymizationNotice.test.ts
import { render, screen } from '@testing-library/svelte';
import AnonymizationNotice from './AnonymizationNotice.svelte';

describe('AnonymizationNotice', () => {
  test('zeigt Anonymisierungshinweis an [AIU]', () => {
    render(AnonymizationNotice);
    
    // Prüft, ob der Hinweis angezeigt wird
    expect(screen.getByText(/Anonymisierung aktiv/i)).toBeInTheDocument();
    
    // Prüft, ob keine Deaktivierungsoption vorhanden ist
    expect(screen.queryByText(/Anonymisierung deaktivieren/i)).not.toBeInTheDocument();
  });
});
```

### 2. Integration Tests

Integration Tests prüfen das Zusammenspiel mehrerer Komponenten.

**Frameworks und Tools:**
- Vitest mit Testing Library
- Mock Service Worker für API-Mocks

**Beispiel für Integrationstest:**

```typescript
// SessionFlow.test.ts
import { render, screen, fireEvent } from '@testing-library/svelte';
import { session } from '$lib/stores/session';
import SessionRecorder from './SessionRecorder.svelte';
import TranscriptViewer from './TranscriptViewer.svelte';

describe('Session Recording Flow', () => {
  test('Aufnahme und Transkription mit Anonymisierung [AIU][ATV]', async () => {
    // Mock für Tauri-Commands
    vi.mock('@tauri-apps/api/tauri', () => ({
      invoke: vi.fn().mockImplementation((cmd, args) => {
        if (cmd === 'get_processing_location') return Promise.resolve('local');
        // Weitere Mocks...
      })
    }));
    
    // Komponenten rendern
    const { component } = render(SessionRecorder);
    
    // Aufnahme starten
    await fireEvent.click(screen.getByText('Aufnahme starten'));
    
    // Prüfen, ob Audit-Event geloggt wurde [ATV]
    expect(session.logAuditEvent).toHaveBeenCalledWith('recording_started', expect.any(Object));
    
    // Aufnahme beenden und Transkript prüfen
    await fireEvent.click(screen.getByText('Aufnahme beenden'));
    
    // TranscriptViewer rendern mit simuliertem Transkript
    render(TranscriptViewer, { props: { transcript: mockTranscript } });
    
    // Prüfen, ob Anonymisierung angewendet wurde [AIU]
    expect(screen.getByText(/\[ANONYMISIERT\]/i)).toBeInTheDocument();
  });
});
```

### 3. End-to-End Tests

E2E-Tests prüfen die gesamte Anwendung in einer realistischen Umgebung.

**Frameworks und Tools:**
- Playwright für browserbasierte Tests
- Tauri Testing für Desktop-App-Tests

**Beispiel für E2E-Test:**

```typescript
// recording.spec.ts
import { test, expect } from '@playwright/test';

test('Vollständiger Aufnahme-Workflow mit Sicherheitsprüfungen [AIU][ATV][CT]', async ({ page }) => {
  // Anwendung öffnen
  await page.goto('http://localhost:1420/');
  
  // Prüfen, ob ProcessingLocationIndicator sichtbar ist [CT]
  await expect(page.locator('[data-testid="processing-location"]')).toBeVisible();
  
  // Prüfen, ob Anonymisierungshinweis angezeigt wird [AIU]
  await expect(page.locator('[data-testid="anonymization-notice"]')).toBeVisible();
  
  // Aufnahme starten
  await page.click('[data-testid="start-recording"]');
  
  // Warten und Aufnahme beenden
  await page.waitForTimeout(2000);
  await page.click('[data-testid="stop-recording"]');
  
  // Prüfen, ob Transkript angezeigt wird
  await expect(page.locator('[data-testid="transcript-viewer"]')).toBeVisible();
  
  // Prüfen, ob Audit-Trail-Einträge erzeugt wurden [ATV]
  // (über spezielle Test-API oder UI-Indikator)
  await expect(page.locator('[data-testid="audit-indicator"]')).toHaveText(/2 Einträge/);
});
```

### 4. Sicherheitstests

Spezielle Tests für sicherheitskritische Funktionen.

**Frameworks und Tools:**
- Spezielle Test-Suites für Sicherheitsfunktionen
- Penetrationstests und Sicherheitsaudits

**Beispiel für Sicherheitstest:**

```typescript
// security.test.ts
import { render, screen, fireEvent } from '@testing-library/svelte';
import { session } from '$lib/stores/session';
import SecuritySettings from './SecuritySettings.svelte';

describe('Sicherheitseinstellungen', () => {
  test('Anonymisierung kann nicht deaktiviert werden [AIU]', async () => {
    render(SecuritySettings);
    
    // Alle Checkboxen in den Sicherheitseinstellungen finden
    const checkboxes = screen.getAllByRole('checkbox');
    
    // Prüfen, ob keine Checkbox zum Deaktivieren der Anonymisierung existiert
    for (const checkbox of checkboxes) {
      expect(checkbox).not.toHaveAttribute('data-setting', 'disable-anonymization');
    }
    
    // Alternativ: Prüfen, ob die Anonymisierungseinstellung als "locked" markiert ist
    const anonymizationSetting = screen.getByTestId('anonymization-setting');
    expect(anonymizationSetting).toHaveAttribute('data-locked', 'true');
  });
  
  test('Cloud-Verarbeitung erfordert explizite Einwilligung [CT]', async () => {
    render(SecuritySettings);
    
    // Cloud-Processing-Checkbox finden und aktivieren
    const cloudCheckbox = screen.getByTestId('cloud-processing');
    expect(cloudCheckbox).not.toBeChecked();
    
    // Checkbox aktivieren
    await fireEvent.click(cloudCheckbox);
    
    // Prüfen, ob Einwilligungsdialog angezeigt wird
    expect(screen.getByText(/Einwilligung zur Cloud-Verarbeitung/i)).toBeInTheDocument();
    
    // Einwilligung bestätigen
    await fireEvent.click(screen.getByText('Einverstanden'));
    
    // Prüfen, ob Einstellung gespeichert wurde
    expect(cloudCheckbox).toBeChecked();
    expect(session.getCloudConsent()).toBe(true);
  });
});
```

## Medizinische Test-Szenarien [MTS]

Spezielle Testszenarien für medizinische Anwendungsfälle:

### 1. Extreme Vitalwerte

```typescript
test('Erkennung extremer Vitalwerte [MV]', async () => {
  // Transkript mit extremen Vitalwerten simulieren
  const extremeValuesTranscript = "Patient hat einen Blutdruck von 220/110 und eine Herzfrequenz von 180.";
  
  render(TranscriptViewer, { props: { transcript: extremeValuesTranscript } });
  
  // Prüfen, ob Warnhinweise für extreme Werte angezeigt werden
  expect(screen.getByTestId('vital-warning')).toBeVisible();
});
```

### 2. Medikamenteninteraktionen

```typescript
test('Warnung bei Medikamenteninteraktionen [MV]', async () => {
  // Transkript mit interagierenden Medikamenten simulieren
  const medicationTranscript = "Patient nimmt Marcoumar und ASS ein.";
  
  render(TranscriptViewer, { props: { transcript: medicationTranscript } });
  
  // Prüfen, ob Interaktionswarnung angezeigt wird
  expect(screen.getByTestId('medication-interaction')).toBeVisible();
});
```

### 3. Notfallsituationen

```typescript
test('Hervorhebung von Notfallsituationen [PSF]', async () => {
  // Notfall-Transkript simulieren
  const emergencyTranscript = "Patient zeigt Anzeichen eines akuten Myokardinfarkts.";
  
  render(TranscriptViewer, { props: { transcript: emergencyTranscript } });
  
  // Prüfen, ob Notfallhinweis angezeigt wird
  expect(screen.getByTestId('emergency-alert')).toBeVisible();
  expect(screen.getByTestId('emergency-alert')).toHaveClass('high-priority');
});
```

## Test-Daten [TD]

Für Tests dürfen ausschließlich synthetische Daten verwendet werden:

### Synthetische Patientendaten

```typescript
// test-data.ts
export const mockPatients = [
  {
    id: "P12345",
    encryptedName: new Uint8Array([...]), // Verschlüsselte Daten simulieren
    insuranceNumberHash: "a1b2c3d4e5f6g7h8i9j0", // Hash, kein Klartext
    dateOfBirth: "01.05.1975" // Für Altersberechnung
  },
  // Weitere synthetische Patienten...
];
```

### Beispiel-Audios

```typescript
// audio-mocks.ts
export const mockAudioFiles = [
  {
    id: "audio1",
    path: "/test-assets/de_consultation_mock_1.mp3",
    language: "de",
    duration: 120 // Sekunden
  },
  {
    id: "audio2",
    path: "/test-assets/ch_de_consultation_mock_1.mp3",
    language: "ch_de", // Schweizerdeutsch
    duration: 90
  },
  // Weitere Beispiel-Audios...
];
```

## Performance-Tests [PB]

Tests zur Überprüfung der Performance-Anforderungen:

```typescript
test('Transkription unter 3 Sekunden Latenz [PB]', async () => {
  // Zeitmessung starten
  const startTime = performance.now();
  
  // Transkription durchführen
  await session.transcribeAudio(mockAudioFiles[0].path);
  
  // Zeitmessung beenden
  const endTime = performance.now();
  const duration = endTime - startTime;
  
  // Prüfen, ob unter 3 Sekunden
  expect(duration).toBeLessThan(3000);
});

test('Anonymisierung unter 100ms [PB]', async () => {
  const testText = "Patient Max Mustermann berichtet über Kopfschmerzen.";
  
  // Zeitmessung starten
  const startTime = performance.now();
  
  // Anonymisierung durchführen
  const anonymizedText = await session.anonymizeText(testText);
  
  // Zeitmessung beenden
  const endTime = performance.now();
  const duration = endTime - startTime;
  
  // Prüfen, ob unter 100ms
  expect(duration).toBeLessThan(100);
  
  // Prüfen, ob korrekt anonymisiert
  expect(anonymizedText).toContain("[ANONYMISIERT]");
  expect(anonymizedText).not.toContain("Max Mustermann");
});
```

## Test-Workflow

### 1. Lokale Tests ausführen

```bash
# Im Frontend-Verzeichnis
cd src/frontend

# Unit- und Integrationstests ausführen
npm run test

# Testabdeckung prüfen
npm run test:coverage

# E2E-Tests ausführen
npm run test:e2e
```

### 2. Sicherheitstests ausführen

```bash
# Sicherheitstests ausführen
npm run test:security

# Sicherheitsaudit durchführen
npm run security:audit
```

### 3. Performance-Tests ausführen

```bash
# Performance-Tests ausführen
npm run test:performance
```

## Kontinuierliche Integration

Die Tests werden automatisch in der CI/CD-Pipeline ausgeführt:

1. **Sicherheitstests** werden zuerst ausgeführt [STP]
2. **Unit- und Integrationstests** folgen
3. **E2E-Tests** werden als letztes ausgeführt
4. **Coverage-Report** wird generiert und geprüft

## Testabdeckung-Anforderungen

- **Sicherheitskritische Funktionen**: 100% Coverage
  - Anonymisierung [AIU]
  - Verschlüsselung [SP]
  - Audit-Trail [ATV]
  - Cloud-Transparenz [CT]
  
- **Allgemeine Funktionen**: >80% Coverage
  - UI-Komponenten
  - Stores
  - Utility-Funktionen

## Best Practices für Tests

1. **Keine echten Patientendaten** [NRPD]
   - Immer synthetische Testdaten verwenden
   - Keine sensiblen Daten in Tests oder Kommentaren

2. **Sicherheitsfeatures testen** [ZTS]
   - Explizit testen, dass Sicherheitsfeatures nicht umgangen werden können
   - Negativtests für verbotene Aktionen

3. **Fehlerszenarien testen** [NSF]
   - Testen, dass Fehler korrekt behandelt werden
   - Prüfen, ob Fehlerkontext bewahrt wird [ECP]

4. **Schweizer Anforderungen testen** [SF] [MFD] [SDH]
   - Korrekte Formatierung von Schweizer Formaten
   - Erkennung von Schweizerdeutsch
   - Verwendung korrekter medizinischer Fachbegriffe

## Checkliste vor Pull Request

- [ ] Unit-Tests für neue Funktionen geschrieben
- [ ] Integrationstests für Komponenteninteraktionen geschrieben
- [ ] Sicherheitstests für kritische Funktionen geschrieben
- [ ] 100% Testabdeckung für sicherheitskritische Funktionen erreicht
- [ ] Performance-Tests bestanden
- [ ] Keine echten Patientendaten in Tests verwendet
- [ ] Testdokumentation aktualisiert
