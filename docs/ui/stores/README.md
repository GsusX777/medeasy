# MedEasy Stores (Zustandsverwaltung)

„Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

Diese Dokumentation beschreibt die Svelte Stores für die Zustandsverwaltung in MedEasy.

## Store-Übersicht

### auth.ts
- **Zweck**: Authentifizierung und Benutzerverwaltung
- **Features**: Login/Logout, Session-Management
- **Sicherheit**: JWT-Token, sichere Session-Speicherung [ZTS]

### database.ts
- **Zweck**: Datenbankoperationen und -zustand
- **Features**: CRUD-Operationen, Verbindungsstatus
- **Sicherheit**: SQLCipher-Integration [SP], Audit-Logging [ATV]

### notifications.ts
- **Zweck**: Benachrichtigungssystem
- **Features**: Toast-Nachrichten, Fehlermeldungen
- **Sicherheit**: Keine sensiblen Daten in Benachrichtigungen [PbD]

### session.ts
- **Zweck**: Sitzungsverwaltung für Patiententermine
- **Features**: Aktive Sitzung, Aufnahmestatus
- **Sicherheit**: Automatische Anonymisierung [AIU], Audit-Trail [ATV]

## Store-Architektur

### Reactive State Management
- Alle Stores verwenden Svelte's reaktive Stores
- Typsichere Interfaces für alle Store-Daten
- Automatische UI-Updates bei Zustandsänderungen

### Sicherheitsintegration
- Kritische Sicherheitsfeatures sind unveränderlich [ZTS]
- Alle Datenänderungen werden auditiert [ATV]
- Patientendaten werden automatisch anonymisiert [AIU]

### Error Handling
- Zentrale Fehlerbehandlung über notification store
- Sichere Fehlermeldungen ohne sensible Daten
- Automatische Wiederherstellung bei Verbindungsfehlern

## Datenfluss

```
UI Component → Store Action → HTTP Request → .NET Backend → Database
     ↑                                                           ↓
UI Update ← Store Update ← HTTP Response ← .NET Response ← Database Response
```

## Store-Richtlinien

### Entwicklungsstandards
- TypeScript für alle Store-Definitionen
- Immutable State Updates
- Reactive Subscriptions
- Error Boundaries

### Sicherheitsrichtlinien
- Keine Klartextpasswörter in Stores [ZTS]
- Automatische Anonymisierung von Patientendaten [AIU]
- Audit-Logging für alle kritischen Operationen [ATV]
- Sichere Token-Speicherung [ZTS]

## Testing

Siehe [../testing/README.md](../testing/README.md) für Store-Testing-Strategien.
