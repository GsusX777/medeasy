<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Frontend Testing [KP100][TR][TSF][AIU][CT]

*Letzte Aktualisierung: 23.07.2025*

## Übersicht

Diese Dokumentation beschreibt die Teststrategie für das MedEasy Frontend (Svelte + Tauri Desktop-Anwendung). Die Tests fokussieren sich ausschließlich auf Frontend-Komponenten, UI-Logik, HTTP-API-Integration und Frontend-Sicherheitsfeatures.

**WICHTIG**: Backend-Tests (SQLCipher, .NET Entity Framework, Verschlüsselung) sind separat implementiert und nicht Teil dieser Dokumentation.

## Test-Architektur [TSF][CAS]

### Frontend-Test-Bereiche

```
FRONTEND TESTS (diese Dokumentation):
├── 1. Unit Tests (Vitest)
│   ├── Svelte-Komponenten
│   ├── TypeScript-Stores
│   └── Utility-Funktionen
├── 2. Integration Tests
│   ├── HTTP-API-Aufrufe
│   ├── Store-Komponenten-Integration
│   └── Tauri-Commands (falls vorhanden)
├── 3. UI-Sicherheitstests [AIU][CT]
│   ├── Anonymisierungs-Anzeige
│   ├── Cloud-Indikatoren
│   └── Sicherheits-Badges
└── 4. Accessibility Tests [PSF]
    ├── WCAG 2.1 AA Compliance
    └── Keyboard-Navigation
```

## Test-Setup

### 1. Vitest Konfiguration

Die Vitest-Konfiguration ist bereits in `package.json` vorhanden:

```json
{
  "scripts": {
    "test": "vitest",
    "test:ui": "vitest --ui",
    "test:run": "vitest run"
  }
}
```

### 2. Erforderliche Test-Dependencies

```bash
# Installiere Test-Dependencies
npm install --save-dev @testing-library/svelte @testing-library/jest-dom
npm install --save-dev @vitest/ui jsdom
```

### 3. Vitest Setup (vitest.config.ts)

```typescript
import { defineConfig } from 'vitest/config';
import { sveltekit } from '@sveltejs/kit/vite';

export default defineConfig({
  plugins: [sveltekit()],
  test: {
    include: ['src/**/*.{test,spec}.{js,ts}'],
    environment: 'jsdom',
    setupFiles: ['./vitest-setup-client.ts']
  }
});
```

## 1. Unit Tests

### Svelte-Komponenten Tests

#### Beispiel: AnonymizationNotice.test.ts

```typescript
import { render, screen } from '@testing-library/svelte';
import { describe, it, expect } from 'vitest';
import AnonymizationNotice from '$lib/components/AnonymizationNotice.svelte';

describe('AnonymizationNotice [AIU]', () => {
  it('zeigt Anonymisierungs-Warnung wenn aktiviert', () => {
    render(AnonymizationNotice, {
      props: { isEnabled: true }
    });
    
    expect(screen.getByText(/Anonymisierung aktiv/i)).toBeInTheDocument();
    expect(screen.getByTestId('anonymization-icon')).toBeInTheDocument();
  });

  it('zeigt Fehler-Status wenn Anonymisierung deaktiviert', () => {
    render(AnonymizationNotice, {
      props: { isEnabled: false }
    });
    
    // Anonymisierung kann NIEMALS deaktiviert werden [AIU]
    expect(screen.getByText(/Kritischer Fehler/i)).toBeInTheDocument();
    expect(screen.getByRole('alert')).toBeInTheDocument();
  });
});
```

#### Beispiel: ProcessingLocationIndicator.test.ts

```typescript
import { render, screen } from '@testing-library/svelte';
import { describe, it, expect } from 'vitest';
import ProcessingLocationIndicator from '$lib/components/ProcessingLocationIndicator.svelte';

describe('ProcessingLocationIndicator [CT]', () => {
  it('zeigt Lokal-Icon für lokale Verarbeitung', () => {
    render(ProcessingLocationIndicator, {
      props: { isCloud: false }
    });
    
    expect(screen.getByText('🔒')).toBeInTheDocument();
    expect(screen.getByText(/Lokal/i)).toBeInTheDocument();
  });

  it('zeigt Cloud-Icon mit Warnung für Cloud-Verarbeitung', () => {
    render(ProcessingLocationIndicator, {
      props: { isCloud: true }
    });
    
    expect(screen.getByText('☁️')).toBeInTheDocument();
    expect(screen.getByText(/Cloud/i)).toBeInTheDocument();
    expect(screen.getByRole('alert')).toBeInTheDocument();
  });
});
```

### TypeScript-Store Tests

#### Beispiel: database.store.test.ts

```typescript
import { describe, it, expect, beforeEach } from 'vitest';
import { get } from 'svelte/store';
import { patients, sessions, loadPatients } from '$lib/stores/database';

describe('Database Store', () => {
  beforeEach(() => {
    // Reset stores vor jedem Test
    patients.set([]);
    sessions.set([]);
  });

  it('lädt Patienten korrekt', async () => {
    const mockPatients = [
      {
        id: '1',
        first_name: 'Hans',
        last_name: 'Müller',
        date_of_birth: '01.01.1980',
        insurance_number: '756.1234.5678.90'
      }
    ];

    // Mock HTTP-Response
    global.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => mockPatients
    });

    await loadPatients();
    
    expect(get(patients)).toEqual(mockPatients);
  });

  it('behandelt API-Fehler korrekt', async () => {
    global.fetch = vi.fn().mockRejectedValue(new Error('API Error'));

    await expect(loadPatients()).rejects.toThrow('API Error');
    expect(get(patients)).toEqual([]);
  });
});
```

## 2. Integration Tests

### HTTP-API-Integration Tests

#### Beispiel: api-client.test.ts

```typescript
import { describe, it, expect, vi } from 'vitest';
import { patientApi } from '$lib/api/database';

describe('Patient API Client', () => {
  it('erstellt Patient mit korrekten Headers', async () => {
    const mockPatient = {
      first_name: 'Hans',
      last_name: 'Müller',
      date_of_birth: '01.01.1980',
      insurance_number: '756.1234.5678.90'
    };

    global.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ id: '123', ...mockPatient })
    });

    const result = await patientApi.create(mockPatient);

    expect(fetch).toHaveBeenCalledWith(
      'http://localhost:5000/api/v1/patients',
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': expect.stringMatching(/Bearer .+/)
        },
        body: JSON.stringify(mockPatient)
      }
    );

    expect(result.id).toBe('123');
  });

  it('verwendet Schweizer Datumsformat [SF]', async () => {
    const patient = {
      date_of_birth: '01.01.1980' // DD.MM.YYYY Format
    };

    global.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => patient
    });

    await patientApi.create(patient);

    const callBody = JSON.parse(
      (fetch as any).mock.calls[0][1].body
    );
    
    expect(callBody.date_of_birth).toMatch(/^\d{2}\.\d{2}\.\d{4}$/);
  });
});
```

## 3. UI-Sicherheitstests [AIU][CT][ZTS]

### Anonymisierungs-Tests

```typescript
import { render, screen } from '@testing-library/svelte';
import { describe, it, expect } from 'vitest';
import TranscriptViewer from '$lib/components/TranscriptViewer.svelte';

describe('Transcript Security [AIU]', () => {
  it('zeigt nur anonymisierte Texte an', () => {
    const mockTranscript = {
      id: '1',
      anonymized_text: 'Patient klagt über [SYMPTOM] seit [ZEITRAUM]',
      needs_review: false
    };

    render(TranscriptViewer, {
      props: { transcript: mockTranscript }
    });

    // Prüfe, dass anonymisierte Platzhalter angezeigt werden
    expect(screen.getByText(/\[SYMPTOM\]/)).toBeInTheDocument();
    expect(screen.getByText(/\[ZEITRAUM\]/)).toBeInTheDocument();
    
    // Prüfe, dass KEINE echten PII-Daten angezeigt werden
    expect(screen.queryByText(/Kopfschmerzen/)).not.toBeInTheDocument();
    expect(screen.queryByText(/3 Tage/)).not.toBeInTheDocument();
  });

  it('zeigt Review-Warnung bei niedriger Konfidenz', () => {
    const mockTranscript = {
      id: '1',
      anonymized_text: 'Text mit [UNSICHER] Anonymisierung',
      anonymization_confidence: 0.6,
      needs_review: true
    };

    render(TranscriptViewer, {
      props: { transcript: mockTranscript }
    });

    expect(screen.getByText(/Review erforderlich/i)).toBeInTheDocument();
    expect(screen.getByRole('alert')).toBeInTheDocument();
  });
});
```

### Cloud-Transparenz Tests

```typescript
describe('Cloud Processing Transparency [CT]', () => {
  it('warnt vor Cloud-Verarbeitung', () => {
    render(SessionRecorder, {
      props: { 
        cloudProcessing: true,
        userConsent: false 
      }
    });

    expect(screen.getByText(/Cloud-Verarbeitung aktiviert/i)).toBeInTheDocument();
    expect(screen.getByText(/☁️/)).toBeInTheDocument();
    expect(screen.getByRole('alert')).toBeInTheDocument();
  });

  it('zeigt lokale Verarbeitung als sicher an', () => {
    render(SessionRecorder, {
      props: { 
        cloudProcessing: false 
      }
    });

    expect(screen.getByText(/🔒/)).toBeInTheDocument();
    expect(screen.getByText(/Lokal/i)).toBeInTheDocument();
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });
});
```

## 4. Accessibility Tests [PSF]

### WCAG 2.1 AA Compliance

```typescript
import { render } from '@testing-library/svelte';
import { axe, toHaveNoViolations } from 'jest-axe';
import AppLayout from '$lib/components/AppLayout.svelte';

expect.extend(toHaveNoViolations);

describe('Accessibility [PSF]', () => {
  it('erfüllt WCAG 2.1 AA Standards', async () => {
    const { container } = render(AppLayout);
    const results = await axe(container);
    
    expect(results).toHaveNoViolations();
  });

  it('unterstützt Keyboard-Navigation', async () => {
    render(SessionRecorder);
    
    const startButton = screen.getByRole('button', { name: /aufnahme starten/i });
    
    // Tab-Navigation testen
    startButton.focus();
    expect(startButton).toHaveFocus();
    
    // Enter-Taste testen
    await user.keyboard('{Enter}');
    expect(screen.getByText(/Aufnahme läuft/i)).toBeInTheDocument();
  });
});
```

## 5. Test-Ausführung

### Lokale Entwicklung

```bash
# Alle Tests ausführen
npm run test

# Tests mit UI
npm run test:ui

# Tests im Watch-Mode
npm run test -- --watch

# Spezifische Test-Datei
npm run test -- AnonymizationNotice.test.ts
```

### CI/CD Pipeline

```bash
# Einmalige Testausführung für CI
npm run test:run

# Mit Coverage-Report
npm run test:run -- --coverage
```

## 6. Test-Richtlinien [TR][KP100]

### Sicherheitstests sind Pflicht

- **Anonymisierung**: Jede Komponente die PII anzeigt MUSS getestet werden [AIU]
- **Cloud-Transparenz**: Alle Cloud-Indikatoren MÜSSEN getestet werden [CT]
- **Sicherheits-Badges**: Verschlüsselungs-/Sicherheitsstatus MUSS getestet werden [ZTS]

### Schweizer Compliance

- **Datumsformat**: DD.MM.YYYY Format MUSS in Tests validiert werden [SF]
- **Versicherungsnummer**: XXX.XXXX.XXXX.XX Format MUSS getestet werden [SF]
- **Deutsche Sprache**: UI-Texte MÜSSEN auf Deutsch getestet werden [MDL]

### Test-Coverage Ziele

- **Sicherheitskomponenten**: 100% Coverage [KP100]
- **Kritische UI-Komponenten**: 90% Coverage [TR]
- **Store-Logik**: 85% Coverage [TR]
- **Utility-Funktionen**: 80% Coverage [TR]

## 7. Häufige Test-Patterns

### Mock HTTP-Responses

```typescript
// Standard HTTP-Mock
global.fetch = vi.fn().mockResolvedValue({
  ok: true,
  json: async () => mockData
});

// Fehler-Mock
global.fetch = vi.fn().mockRejectedValue(new Error('Network Error'));
```

### Store-Testing

```typescript
import { get } from 'svelte/store';

// Store-Wert prüfen
expect(get(myStore)).toBe(expectedValue);

// Store-Änderung prüfen
myStore.set(newValue);
expect(get(myStore)).toBe(newValue);
```

### Svelte-Component Props

```typescript
// Props testen
render(MyComponent, {
  props: { 
    title: 'Test Title',
    isActive: true 
  }
});
```

## Visual Regression Testing

### Chromatic (Geplant)
```typescript
// Storybook Integration für Komponenten-Screenshots
// Multi-Platform visuelle Konsistenz (Windows, macOS, Linux)
// Responsive Screenshots für verschiedene Fenstergrößen
```

### Percy (Alternative)
```typescript
// Automated Screenshots bei jedem Build
// Diff-Detection für visuelle Änderungen zwischen Plattformen
// Review Process für manuelle Freigabe
```

## Performance Testing

### Lighthouse Integration
```typescript
// Performance Score: > 90
// Accessibility Score: > 95
// Best Practices: > 90
// SEO: > 90
```

### Web Vitals Monitoring
```typescript
// LCP: < 2.5s (Largest Contentful Paint)
// FID: < 100ms (First Input Delay)
// CLS: < 0.1 (Cumulative Layout Shift)
```

## CI/CD Integration

### GitHub Actions
```yaml
# Automated Testing bei jedem Push
# Security Scans für Sicherheitstests bei jedem Build
# Performance Monitoring mit Lighthouse CI
# Accessibility Checks mit axe-core Integration
```

### Test Reports
- **Coverage Reports**: Testabdeckung visualisieren
- **Security Reports**: Sicherheitstests dokumentieren [KP100]
- **Performance Reports**: Web Vitals tracking
- **Accessibility Reports**: Barrierefreiheit überwachen [PSF]

## Test-Richtlinien

### Entwicklungsstandards
- **Test-Driven Development**: Tests vor Implementierung
- **100% Coverage**: Für sicherheitskritische Komponenten [KP100]
- **Descriptive Names**: Aussagekräftige Testnamen
- **Arrange-Act-Assert**: Klare Teststruktur

### Sicherheitsrichtlinien [KP100]
- **Keine echten Daten**: Nur synthetische Testdaten [NRPD]
- **Security First**: Sicherheitstests haben Priorität [PSF]
- **Fail-Safe**: Tests müssen sicher fehlschlagen [FSD]
- **Audit Tests**: Alle Sicherheitsfeatures testen [ATV]

## Nächste Schritte

1. **Implementierung der Unit Tests** für alle Svelte-Komponenten
2. **HTTP-API-Integration Tests** für .NET Backend
3. **Sicherheitstests** für alle kritischen Features [KP100]
4. **Accessibility Tests** für WCAG 2.1 AA Compliance [PSF]
5. **Store-Tests** für reactive State Management
6. **Mock-Service Integration** für isolierte Tests
7. **Visual Regression Testing** mit Chromatic/Percy
8. **Performance-Monitoring** in Produktion
9. **CI/CD Pipeline** für automatisierte Tests

---

*Diese Dokumentation wird kontinuierlich erweitert, sobald Tests implementiert werden.*

Diese Test-Dokumentation fokussiert sich ausschließlich auf **Frontend-Tests** für die MedEasy Desktop-Anwendung. Backend-Tests (Verschlüsselung, Datenbank, .NET) sind separat implementiert und dokumentiert.

**Priorität**: Sicherheitstests [AIU][CT][ZTS] haben höchste Priorität, da MedEasy medizinische Software ist und Patientensicherheit gewährleisten muss [PSF].
