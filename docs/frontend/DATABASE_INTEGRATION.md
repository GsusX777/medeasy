<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Frontend-Datenbankintegration [SP][AIU][ATV][ZTS][FSD]

*Letzte Aktualisierung: 09.07.2025*

## Übersicht

Diese Dokumentation beschreibt die Integration der SQLCipher-Datenbank und der Schlüsselverwaltung in das Svelte-Frontend über Tauri-Befehle. Die Integration stellt sicher, dass alle Sicherheitsanforderungen des MedEasy-Projekts eingehalten werden, insbesondere die Verschlüsselung, Anonymisierung, Audit-Trail-Funktionen und sichere Schlüsselverwaltung mit Notfallwiederherstellungsoptionen.

## Architektur [CAS]

Die Frontend-Datenbankintegration folgt der Clean Architecture:

1. **Präsentationsschicht (Svelte-Komponenten)**
   - Benutzeroberfläche für Datenbankoperationen
   - Anzeige verschlüsselter/anonymisierter Daten
   - Sicherheitseinstellungen und Audit-Trail-Anzeige

2. **Anwendungsschicht (Svelte-Stores)**
   - Zustandsverwaltung für Datenbankoperationen
   - Validierung und Geschäftslogik
   - Fehlerbehandlung und Benutzerrückmeldung

3. **API-Schicht (Tauri-API-Client)**
   - Typsichere Wrapper für Tauri-Befehle
   - Fehlerbehandlung und Logging
   - Konsistente Schnittstelle zur Backend-Logik

4. **Backend-Schicht (Tauri-Befehle)**
   - Implementierung der Datenbankoperationen
   - Verschlüsselung und Entschlüsselung
   - Anonymisierung und Audit-Trail

## Komponenten

### 1. API-Client (`database.ts`)

Der API-Client kapselt alle Tauri-Befehle für Datenbankoperationen und stellt eine typsichere Schnittstelle für das Frontend bereit:

- `patientApi`: Operationen für Patienten (erstellen, abrufen, auflisten)
- `sessionApi`: Operationen für Sitzungen (erstellen, abrufen, auflisten)
- `transcriptApi`: Operationen für Transkripte (erstellen, abrufen, Review)
- `auditApi`: Operationen für Audit-Logs (abrufen, filtern)
- `initializeDatabase`: Datenbankinitialisierung

### 2. Svelte-Stores (`database.ts`)

Die Svelte-Stores verwalten den Zustand der Datenbankoperationen und stellen reaktive Daten für die Komponenten bereit:

- Patienten-, Sitzungs- und Transkript-Stores
- Lade- und Fehlerzustände
- Ausgewählte Elemente und abgeleitete Zustände
- Funktionen für CRUD-Operationen mit Validierung

### 3. Komponenten

#### Anonymisierungsüberprüfung (`AnonymizationReview.svelte`) [AIU][ARQ]

Diese Komponente ermöglicht die Überprüfung von Transkripten mit niedriger Anonymisierungskonfidenz:

- Anzeige von Transkripten, die eine Überprüfung benötigen
- Genehmigung, Ablehnung oder Whitelist-Markierung
- Notizen zur Überprüfung
- Konfidenzanzeige und Warnungen

#### Audit-Trail-Anzeige (`AuditTrailViewer.svelte`) [ATV]

Diese Komponente zeigt den Audit-Trail der Datenbankoperationen an:

- Filterung nach Entität, Aktion, Benutzer und Zeitraum
- Statistiken zu Audit-Logs
- Hervorhebung sensibler Datenzugriffe
- Paginierung und detaillierte Anzeige

#### Datenbanksicherheitseinstellungen (`DatabaseSecuritySettings.svelte`) [SP][AIU][ATV]

Diese Komponente ermöglicht die Verwaltung der Datenbanksicherheitseinstellungen:

- Anzeige des Verschlüsselungsstatus
- Aktivierung/Deaktivierung der Verschlüsselung (nur in Entwicklung)
- Grundlegende Schlüsseleinstellungen
- Audit-Trail-Einstellungen

#### Schlüsselverwaltung (`KeyManagement.svelte`) [ZTS][SP][FSD]

Diese Komponente bietet eine umfassende Benutzeroberfläche für die Schlüsselverwaltung:

- Statusübersicht aller Schlüssel mit Rotationsinformationen
- Schlüsselrotation mit konfigurierbaren Zeitplänen
- Änderung des Master-Passworts mit Sicherheitsvalidierung
- Notfall-Wiederherstellungsoptionen mit Recovery-Daten
- Shamir's Secret Sharing für Master-Schlüssel (M-von-N-Schema)
- Vollständiger Audit-Trail für alle Schlüsseloperationen

## Sicherheitsfunktionen

### 1. Verschlüsselung [SP][EIV]

Die Verschlüsselung wird auf zwei Ebenen implementiert:

1. **Datenbankverschlüsselung (SQLCipher)**
   - In Produktion immer aktiviert
   - In Entwicklung optional, aber empfohlen
   - Status und Schlüsselverwaltung in den Sicherheitseinstellungen

2. **Feldverschlüsselung (AES-256-GCM)**
   - Immer aktiviert für sensible Daten
   - Separate Schlüssel für Feldverschlüsselung
   - Transparente Ver- und Entschlüsselung im Backend

### 2. Schlüsselverwaltung [ZTS][SP][FSD]

Die Schlüsselverwaltung implementiert eine mehrschichtige Sicherheitsarchitektur:

1. **Master-Schlüssel**
   - Abgeleitet aus Benutzerpasswort mit Argon2id (Memory-Hard-Funktion)
   - Hohe Iterationszahl und Speicheranforderungen gegen Brute-Force
   - Salz und Nonce werden sicher gespeichert

2. **Schlüsselhierarchie**
   - Datenbankschlüssel (SQLCipher)
   - Feldverschlüsselungsschlüssel (AES-256-GCM)
   - Backup-Schlüssel (für zukünftige Implementierung)
   - Alle Schlüssel werden mit dem Master-Schlüssel verschlüsselt

3. **Schlüsselrotation**
   - Regelmäßige automatische Rotation
   - Manuelle Rotation bei Sicherheitsbedenken
   - Vollständige Audit-Trail-Protokollierung aller Rotationen

4. **Notfallwiederherstellung**
   - Recovery-Daten für Notfallzugriff
   - Shamir's Secret Sharing (M-von-N-Schema)
   - Sichere Verteilung der Shares an vertrauenswürdige Parteien

5. **Sicherheitsmaßnahmen**
   - Keine Speicherung des Master-Passworts
   - Zero-Knowledge-Design
   - Sichere Speicherung der verschlüsselten Schlüssel
   - Vollständiger Audit-Trail aller Operationen [ATV]

### 3. Anonymisierung [AIU]

Die Anonymisierung ist eine Kernfunktion und kann nicht deaktiviert werden:

- Erzwungene Anonymisierung bei der Transkripterstellung
- Client-seitige Validierung der Anonymisierung
- Review-Prozess für niedrige Konfidenzwerte
- Whitelist-Funktionalität für medizinische Begriffe

### 3. Audit-Trail [ATV]

Der Audit-Trail protokolliert alle Datenbankoperationen:

- In Produktion immer aktiviert
- In Entwicklung optional, aber empfohlen
- Detaillierte Protokollierung von Aktionen, Entitäten und Benutzern
- Markierung sensibler Datenzugriffe

## Datenfluss

1. **Benutzeraktion in Svelte-Komponente**
   ```
   Benutzer → Svelte-Komponente → Svelte-Store → API-Client → Tauri-Befehl
   ```

2. **Datenbankoperation im Backend**
   ```
   Tauri-Befehl → Repository → SQLCipher → Verschlüsselte Daten
   ```

3. **Rückgabe der Ergebnisse**
   ```
   Verschlüsselte Daten → Repository (Entschlüsselung) → Tauri-Befehl → API-Client → Svelte-Store → Svelte-Komponente → Benutzer
   ```

4. **Audit-Protokollierung**
   ```
   Datenbankoperation → Audit-Repository → Audit-Log-Eintrag
   ```

## Fehlerbehandlung

Die Frontend-Integration implementiert eine umfassende Fehlerbehandlung:

1. **API-Client**
   - Fehlerprotokollierung in der Konsole
   - Typisierte Fehlerrückgabe
   - Konsistente Fehlerstruktur

2. **Svelte-Stores**
   - Fehlerstatusverwaltung
   - Benutzerfreundliche Fehlermeldungen
   - Wiederherstellung des Zustands bei Fehlern

3. **Komponenten**
   - Anzeige von Fehlermeldungen
   - Wiederholungsoptionen
   - Ladezustände und Feedback

## Validierung und Sicherheitskontrollen

Die Frontend-Integration implementiert mehrere Sicherheitskontrollen:

1. **Datenvalidierung**
   - Schweizer Versicherungsnummerformat (XXX.XXXX.XXXX.XX) [SF]
   - Schweizer Datumsformat (DD.MM.YYYY) [SF]
   - Anonymisierungsprüfung vor dem Speichern [AIU]

2. **Berechtigungsprüfungen**
   - Admin-Funktionen nur für Administratoren
   - Sensible Operationen mit Bestätigungsdialogen
   - Audit-Trail-Zugriffskontrolle

3. **Umgebungsspezifische Kontrollen**
   - Produktionsumgebung mit strengeren Sicherheitseinstellungen
   - Entwicklungsumgebung mit optionalen Sicherheitsfunktionen
   - Klare Kennzeichnung der Umgebung

## Implementierungsdetails

### Tauri-Befehle

Die Tauri-Befehle sind in `commands.rs` implementiert und werden über die `invoke`-Funktion aufgerufen:

```typescript
// Beispiel für einen Tauri-Befehlsaufruf
const result = await invoke('create_patient', {
  patient: patientData,
  userId: getCurrentUserId()
});
```

### Svelte-Stores

Die Svelte-Stores verwenden die reaktiven Funktionen von Svelte, um den Zustand zu verwalten:

```typescript
// Beispiel für einen Svelte-Store
export const patients = writable<PatientDto[]>([]);
export const selectedPatientId = writable<string | null>(null);
export const selectedPatient = derived(
  [patients, selectedPatientId],
  ([$patients, $selectedPatientId]) => {
    if (!$selectedPatientId) return null;
    return $patients.find(p => p.id === $selectedPatientId) || null;
  }
);
```

### Komponenten

Die Svelte-Komponenten verwenden die Stores und API-Clients, um Daten anzuzeigen und zu manipulieren:

```svelte
<!-- Beispiel für eine Svelte-Komponente -->
<script>
  import { patients, loadPatients } from '$lib/stores/database';
  
  onMount(async () => {
    await loadPatients();
  });
</script>

<div>
  {#each $patients as patient}
    <div class="patient-card">
      <h3>{patient.first_name} {patient.last_name}</h3>
      <p>Geburtsdatum: {patient.date_of_birth}</p>
    </div>
  {/each}
</div>
```

## Sicherheitsempfehlungen

1. **Schlüsselverwaltung**
   - Regelmäßige Sicherung der Verschlüsselungsschlüssel
   - Sichere Aufbewahrung der Schlüssel außerhalb der Anwendung
   - Schlüsselrotation in regelmäßigen Abständen

2. **Benutzerauthentifizierung**
   - Implementierung einer starken Authentifizierung
   - Rollenbasierte Zugriffskontrolle
   - Automatische Abmeldung bei Inaktivität

3. **Audit-Trail-Überwachung**
   - Regelmäßige Überprüfung der Audit-Logs
   - Automatische Benachrichtigungen bei verdächtigen Aktivitäten
   - Langfristige Aufbewahrung der Audit-Logs

## Compliance [RW][PL]

Die Frontend-Integration erfüllt die folgenden Compliance-Anforderungen:

- **Schweizer nDSG**: Verschlüsselung und Anonymisierung personenbezogener Daten
- **DSGVO/GDPR**: Datenschutz durch Technikgestaltung (Privacy by Design)
- **Medizinische Datenschutzbestimmungen**: Audit-Trail und Zugriffsprotokollierung

## Verbotene Praktiken [NSB][NUS][NRPD]

Die folgenden Praktiken sind in der Frontend-Integration verboten:

- **Keine Umgehung der Verschlüsselung** [NSB]
- **Keine unverschlüsselte Speicherung sensibler Daten** [NUS]
- **Keine Deaktivierung der Anonymisierung** [AIU]
- **Keine Deaktivierung des Audit-Trails in Produktion** [ATV]
- **Keine echten Patientendaten in der Entwicklung** [NRPD]
