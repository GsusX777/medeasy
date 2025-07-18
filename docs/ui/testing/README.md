# MedEasy UI-Testing

„Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

Diese Dokumentation beschreibt die Teststrategien und -richtlinien für die MedEasy UI.

## Testing-Übersicht

### Test-Pyramide
1. **Unit Tests**: Einzelne Komponenten und Funktionen
2. **Integration Tests**: Komponenten-Interaktionen
3. **E2E Tests**: Vollständige Benutzerflows
4. **Security Tests**: Sicherheitskritische Funktionen [KP100]

## Test-Frameworks

### Vitest
- **Zweck**: Unit und Integration Tests
- **Features**: Fast, TypeScript-Support, Svelte-Integration
- **Konfiguration**: `vitest.config.ts`

### Playwright
- **Zweck**: End-to-End Tests
- **Features**: Cross-Browser, Mobile Testing
- **Konfiguration**: `playwright.config.ts`

### Testing Library
- **Zweck**: Component Testing
- **Features**: User-centric Testing, Accessibility
- **Integration**: `@testing-library/svelte`

## Test-Kategorien

### Component Tests
- **Rendering**: Komponenten rendern korrekt
- **Props**: Props werden korrekt verarbeitet
- **Events**: Event-Handler funktionieren
- **Accessibility**: Screen Reader Kompatibilität

### Security Tests [KP100]
- **Anonymisierung**: Patientendaten werden anonymisiert [AIU]
- **Audit-Trail**: Alle Aktionen werden protokolliert [ATV]
- **Verschlüsselung**: Daten werden verschlüsselt übertragen [SP]
- **Cloud-Transparenz**: Verarbeitungsort wird angezeigt [CT]

### Integration Tests
- **Store-Integration**: Komponenten arbeiten mit Stores
- **API-Integration**: Tauri Commands funktionieren
- **Navigation**: Routing funktioniert korrekt
- **Form-Validation**: Formulare validieren korrekt

### E2E Tests
- **Benutzerflows**: Komplette Arbeitsabläufe
- **Cross-Browser**: Chrome, Firefox, Safari
- **Mobile**: Responsive Design
- **Performance**: Ladezeiten und Reaktionszeiten

## Sicherheitstests [KP100]

### Unveränderliche Anonymisierung [AIU]
```typescript
test('Anonymisierung kann nicht deaktiviert werden', async () => {
  // Test dass Anonymisierung immer aktiv ist
  // Keine UI-Option zum Deaktivieren vorhanden
});
```

### Audit-Trail [ATV]
```typescript
test('Alle kritischen Aktionen werden auditiert', async () => {
  // Test dass Audit-Logs erstellt werden
  // Vollständige Protokollierung
});
```

### Cloud-Transparenz [CT]
```typescript
test('Verarbeitungsort wird angezeigt', async () => {
  // Test dass 🔒/☁️ Indikatoren sichtbar sind
  // Korrekte Anzeige des Verarbeitungsorts
});
```

### Schweizerdeutsch-Handling [SDH]
```typescript
test('Schweizerdeutsch-Warnung wird angezeigt', async () => {
  // Test dass Warnung bei Schweizerdeutsch erscheint
  // Beta-Hinweis wird angezeigt
});
```

## Test-Daten

### Synthetische Daten
- **Keine echten Patientendaten** [NRPD]
- **Realistische Testdaten** für UI-Tests
- **Schweizer Formate** [SF]: DD.MM.YYYY, XXX.XXXX.XXXX.XX

### Test-Fixtures
- **Komponenten-Props**: Vordefinierte Props für Tests
- **API-Responses**: Mock-Responses für Tauri Commands
- **Store-States**: Vordefinierte Store-Zustände

## Accessibility Testing

### Automated Tests
- **axe-core**: Automatische Accessibility-Prüfung
- **Lighthouse**: Performance und Accessibility
- **WAVE**: Web Accessibility Evaluation

### Manual Tests
- **Keyboard Navigation**: Alle Funktionen per Tastatur
- **Screen Reader**: NVDA, JAWS, VoiceOver
- **High Contrast**: Sichtbarkeit bei hohem Kontrast
- **Zoom**: Funktionalität bei 200% Zoom

## Visual Regression Testing

### Chromatic (Geplant)
- **Storybook Integration**: Komponenten-Screenshots
- **Cross-Browser**: Visuelle Konsistenz
- **Responsive**: Screenshots für alle Breakpoints

### Percy (Alternative)
- **Automated Screenshots**: Bei jedem Build
- **Diff-Detection**: Visuelle Änderungen erkennen
- **Review Process**: Manuelle Freigabe von Änderungen

## Performance Testing

### Lighthouse
- **Performance Score**: > 90
- **Accessibility Score**: > 95
- **Best Practices**: > 90
- **SEO**: > 90

### Web Vitals
- **LCP**: < 2.5s (Largest Contentful Paint)
- **FID**: < 100ms (First Input Delay)
- **CLS**: < 0.1 (Cumulative Layout Shift)

## Test-Richtlinien

### Entwicklungsstandards
- **Test-Driven Development**: Tests vor Implementierung
- **100% Coverage**: Für sicherheitskritische Komponenten [KP100]
- **Descriptive Names**: Aussagekräftige Testnamen
- **Arrange-Act-Assert**: Klare Teststruktur

### Sicherheitsrichtlinien
- **Keine echten Daten**: Nur synthetische Testdaten [NRPD]
- **Security First**: Sicherheitstests haben Priorität [PSF]
- **Fail-Safe**: Tests müssen sicher fehlschlagen [FSD]
- **Audit Tests**: Alle Sicherheitsfeatures testen [ATV]

## CI/CD Integration

### GitHub Actions
- **Automated Testing**: Bei jedem Push
- **Security Scans**: Sicherheitstests bei jedem Build
- **Performance Monitoring**: Lighthouse CI
- **Accessibility Checks**: axe-core Integration

### Test Reports
- **Coverage Reports**: Testabdeckung visualisieren
- **Security Reports**: Sicherheitstests dokumentieren
- **Performance Reports**: Web Vitals tracking
- **Accessibility Reports**: Barrierefreiheit überwachen

## Nächste Schritte

- Implementierung der E2E-Tests mit Playwright
- Einrichtung von Visual Regression Testing
- Erweiterte Sicherheitstests für alle Komponenten
- Performance-Monitoring in Produktion
