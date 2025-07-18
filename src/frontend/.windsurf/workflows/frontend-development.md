<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

---
description: MedEasy Frontend Development Workflow
---

# MedEasy Frontend Development Workflow

Dieser Workflow beschreibt den standardisierten Prozess für die Entwicklung neuer Frontend-Komponenten im MedEasy-Projekt unter Einhaltung aller Sicherheits- und Compliance-Anforderungen.

## 1. Vorbereitung

### 1.1 Dokumentation lesen [DL]
```bash
# Relevante Dokumentation prüfen
cat docs/architecture/README.md
cat docs/api/API_REFERENCE.md
cat src/frontend/README.md
```

### 1.2 Feature-Branch erstellen [FBN]
```bash
# Neuen Feature-Branch erstellen
git checkout -b feature/med-XXX-beschreibung
```

### 1.3 Abhängigkeiten aktualisieren
```bash
# Im Frontend-Verzeichnis
cd src/frontend
npm install
```

## 2. Komponenten-Entwicklung

### 2.1 Neue Komponente erstellen
```bash
# Komponente erstellen
mkdir -p src/frontend/src/lib/components/MeineKomponente
touch src/frontend/src/lib/components/MeineKomponente/MeineKomponente.svelte
```

### 2.2 Sicherheitsrichtlinien implementieren [AIU] [SP] [ATV]

Jede Komponente, die mit Patientendaten arbeitet, muss:

1. Anonymisierung verwenden (nicht deaktivierbar) [AIU]
2. Verschlüsselte Speicherung nutzen [SP]
3. Audit-Trail-Einträge erzeugen [ATV]
4. Cloud-Verarbeitung transparent anzeigen [CT]

**Beispiel-Template:**

```svelte
<script lang="ts">
  import { session } from '$lib/stores/session';
  import ProcessingLocationIndicator from '$lib/components/ProcessingLocationIndicator.svelte';
  import AnonymizationNotice from '$lib/components/AnonymizationNotice.svelte';
  
  // Audit-Trail Eintrag [ATV]
  $: {
    if ($session.active) {
      session.logAuditEvent('component_viewed', 'MeineKomponente');
    }
  }
</script>

<!-- Komponenten-Inhalt -->
<div class="meine-komponente">
  <!-- Cloud-Transparenz [CT] -->
  <ProcessingLocationIndicator />
  
  <!-- Patientendaten-Bereich mit Anonymisierungshinweis [AIU] -->
  {#if $session.hasPatientData}
    <AnonymizationNotice />
  {/if}
  
  <!-- Hauptinhalt -->
  <div class="content">
    <!-- Hier kommt der eigentliche Inhalt -->
  </div>
</div>

<style>
  /* Styling hier */
</style>
```

### 2.3 Schweizer Anforderungen beachten [SF] [MFD] [DSC]

- Datumsformat: DD.MM.YYYY [SF]
- Schweizer Fachbegriffe verwenden [MFD]
- Schweizerdeutsch-Erkennung einbinden [SDH]

## 3. Integration und Tests

### 3.1 Komponente in Route einbinden
```bash
# Route-Datei bearbeiten
code src/frontend/src/routes/meine-route/+page.svelte
```

### 3.2 Tests schreiben [KP100]
```bash
# Test-Datei erstellen
touch src/frontend/src/lib/components/MeineKomponente/MeineKomponente.test.ts
```

Sicherheitskritische Funktionen benötigen 100% Test-Coverage:
- Anonymisierung [AIU]
- Verschlüsselung [SP]
- Audit-Trail [ATV]

### 3.3 Lokales Testen
```bash
# Entwicklungsserver starten
cd src/frontend
npm run tauri dev
```

## 4. Dokumentation und Review

### 4.1 Dokumentation aktualisieren [DSU]
```bash
# Dokumentation aktualisieren
code docs/FEATURE_STATUS.md
```

### 4.2 Code-Review anfordern
```bash
# Änderungen committen
git add .
git commit -m "feat(frontend): Neue Komponente hinzugefügt [AIU][SP][ATV]"
git push origin feature/med-XXX-beschreibung
```

### 4.3 Pull Request erstellen
- Titel: `[Frontend] Neue Komponente hinzugefügt`
- Beschreibung:
  - Implementierte Sicherheitsfeatures auflisten [AIU][SP][ATV]
  - Schweizer Anforderungen dokumentieren [SF][MFD][DSC]
  - Screenshots der Komponente beifügen

## 5. Sicherheits-Checkliste vor Merge

- [ ] Anonymisierung ist nicht deaktivierbar [AIU]
- [ ] Patientendaten werden verschlüsselt [SP]
- [ ] Audit-Trail für alle Aktionen vorhanden [ATV]
- [ ] Cloud-Verarbeitung wird transparent angezeigt [CT]
- [ ] Schweizer Formate werden verwendet [SF]
- [ ] Schweizerdeutsch-Erkennung implementiert (falls relevant) [SDH]
- [ ] Tests für sicherheitskritische Funktionen vorhanden [KP100]
- [ ] Dokumentation aktualisiert [DSU]
