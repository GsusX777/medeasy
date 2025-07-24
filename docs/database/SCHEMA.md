<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Datenbankschema

*Letzte Aktualisierung: 23.07.2025*

## Frontend-Integration Status [AKTUELL]

**Mock-API Phase:**
- ✅ Dashboard verwendet temporäre Mock-Daten
- ✅ TypeScript-DTOs erweitert um fehlende Session-Felder
- ✅ Feldnamen korrigiert: camelCase → snake_case
- 🔄 Nächster Schritt: .NET Backend Integration

**Bekannte Abweichungen Mock vs. Schema:**
- Mock verwendet `string` IDs statt `Guid`
- Mock-Daten nicht verschlüsselt (nur für Tests)
- Schweizer Datumsformat (DD.MM.YYYY) korrekt implementiert [SF]

## Übersicht [SP][EIV]

Das MedEasy-Datenbankschema verwendet SQLCipher mit AES-256-Verschlüsselung für alle Patientendaten. Die Datenbank ist so konzipiert, dass sie den strengen Schweizer Datenschutzanforderungen (nDSG) und medizinischen Sicherheitsstandards entspricht.

## Verschlüsselungsstrategie [SP][NUS]

### Datenbankverschlüsselung
- **Algorithmus**: AES-256 (SQLCipher)
- **Schlüsselableitung**: PBKDF2 mit 256.000 Iterationen
- **Schlüsselquelle**: Umgebungsvariable `MEDEASY_DB_KEY`
- **SQLCipher-Pragmas**:
  ```sql
  PRAGMA cipher_page_size = 4096;
  PRAGMA kdf_iter = 256000;
  PRAGMA cipher_memory_security = ON;
  PRAGMA cipher_default_kdf_algorithm = PBKDF2_HMAC_SHA512;
  PRAGMA cipher_default_plaintext_header_size = 32;
  PRAGMA cipher_hmac_algorithm = HMAC_SHA512;
  ```

### Feldverschlüsselung
- **Algorithmus**: AES-256 mit zufälligem IV
- **Schlüsselquelle**: Umgebungsvariable `MEDEASY_FIELD_ENCRYPTION_KEY` (Base64-kodiert)
- **Implementierung**: .NET `System.Security.Cryptography.Aes`
- **Format**: `[16-byte IV][verschlüsselter Text]`

## Entitäten

### Patient [EIV][PbD]

Speichert Patienteninformationen mit verschlüsselten persönlichen Daten.

| Spalte | Typ | Beschreibung | Verschlüsselt |
|--------|-----|-------------|--------------|
| Id | Guid | Primärschlüssel | Nein |
| EncryptedFirstName | byte[] | Verschlüsselte Vorname des Patienten [EIV] | Ja |
| EncryptedLastName | byte[] | Verschlüsselte Nachname des Patienten [EIV] | Ja |
| EncryptedDateOfBirth | byte[] | Verschlüsselte Geburtsdatum [EIV] | Ja |
| EncryptedInsuranceNumber | byte[] | Verschlüsselte Versicherungsnummer [EIV] | Ja |
| AnonymizedFirstName | string | Anonymisierte Vorname für UI [AIU] | Nein |
| AnonymizedLastName | string | Anonymisierte Nachname für UI [AIU] | Nein |
| AnonymizedDateOfBirth | string | Anonymisierte Geburtsdatum für UI [AIU] | Nein |
| InsuranceNumberHash | string | Hash der Versicherungsnummer (nicht die Originalnummer) | Nein |
| DateOfBirth | DateOnly | Geburtsdatum (für Altersberechnung) | Nein |
| Created | DateTime | Erstellungszeitpunkt | Nein |
| CreatedBy | string | Benutzer, der den Eintrag erstellt hat | Nein |
| LastModified | DateTime | Zeitpunkt der letzten Änderung | Nein |
| LastModifiedBy | string | Benutzer, der die letzte Änderung vorgenommen hat | Nein |

### Session [SK][EIV][SF]

Repräsentiert eine Konsultation oder einen Arztbesuch.

| Spalte | Typ | Beschreibung | Verschlüsselt |
|--------|-----|-------------|--------------|
| Id | Guid | Primärschlüssel | Nein |
| PatientId | Guid | Fremdschlüssel zum Patienten | Nein |
| SessionDate | DateTime | Datum der Session [SF] | Nein |
| StartTime | TimeSpan? | Startzeit der Konsultation (nullable) | Nein |
| EndTime | TimeSpan? | Endzeit der Konsultation (nullable) | Nein |
| Status | SessionStatus | Status der Session (Enum: Scheduled, InProgress, Completed, Cancelled) | Nein |
| EncryptedNotes | byte[] | Verschlüsselte Notizen zur Session [EIV] | Ja |
| EncryptedAudioReference | byte[] | Verschlüsselte Referenz zur Audiodatei [EIV] | Ja |
| Created | DateTime | Erstellungszeitpunkt | Nein |
| CreatedBy | string | Benutzer, der den Eintrag erstellt hat | Nein |
| LastModified | DateTime | Zeitpunkt der letzten Änderung | Nein |
| LastModifiedBy | string | Benutzer, der die letzte Änderung vorgenommen hat | Nein |

**Zukünftige MVP-Erweiterung:**
| InsuranceCaseNumber | TEXT | Fallnummer für die Versicherung [SF][MFD] | Nein |
| IsFollowUp | INTEGER | Flag für Folgebehandlung (0/1) | Nein |

### Transcript [AIU][EIV]

Enthält Transkriptionen von Arzt-Patienten-Gesprächen.

| Spalte | Typ | Beschreibung | Verschlüsselt |
|--------|-----|-------------|------------|
| Id | TEXT | Primärschlüssel | Nein |
| SessionId | TEXT | Fremdschlüssel zur Session | Nein |
| EncryptedOriginalText | TEXT | Verschlüsselter Originaltext des Transkripts | Ja |
| EncryptedAnonymizedText | TEXT | Verschlüsselter anonymisierter Text | Ja |
| IsAnonymized | INTEGER | Flag, ob das Transkript anonymisiert wurde (immer 1) [AIU] | Nein |
| AnonymizationConfidence | REAL | Konfidenz der Anonymisierung (0-1) | Nein |
| NeedsReview | INTEGER | Flag, ob eine manuelle Überprüfung erforderlich ist (0/1) | Nein |
| Created | TEXT | Erstellungszeitpunkt | Nein |
| CreatedBy | TEXT | Benutzer, der den Eintrag erstellt hat | Nein |
| LastModified | TEXT | Zeitpunkt der letzten Änderung | Nein |
| LastModifiedBy | TEXT | Benutzer, der die letzte Änderung vorgenommen hat | Nein |

### AnonymizationReviewItem [ARQ]

Enthält Einträge für die manuelle Überprüfung von Anonymisierungen.

| Spalte | Typ | Beschreibung | Verschlüsselt |
|--------|-----|-------------|------------|
| Id | TEXT | Primärschlüssel | Nein |
| TranscriptId | TEXT | Fremdschlüssel zum Transkript | Nein |
| Status | TEXT | Status des Reviews (Pending, Approved, Rejected) | Nein |
| DetectedPII | TEXT | Erkannte persönlich identifizierbare Informationen (JSON) | Nein |
| AnonymizationConfidence | REAL | Konfidenz der Anonymisierung (0-1) | Nein |
| ReviewReason | TEXT | Grund für die manuelle Überprüfung | Nein |
| ReviewerNotes | TEXT | Notizen des Prüfers | Nein |
| Created | TEXT | Erstellungszeitpunkt | Nein |
| CreatedBy | TEXT | Benutzer, der den Eintrag erstellt hat | Nein |
| LastModified | TEXT | Zeitpunkt der letzten Änderung | Nein |
| LastModifiedBy | TEXT | Benutzer, der die letzte Änderung vorgenommen hat | Nein |

#### ReviewStatus Enum [ARQ]

Das `ReviewStatus`-Enum definiert die möglichen Status-Werte für die manuelle Überprüfung von Anonymisierungen:

| Wert | Beschreibung |
|------|-------------|
| Pending | Die Überprüfung steht noch aus |
| Approved | Die Anonymisierung wurde genehmigt |
| Rejected | Die Anonymisierung wurde abgelehnt und muss verbessert werden |

### AuditLog [ATV]

Protokolliert alle Datenbankoperationen für Audit-Zwecke.

| Spalte | Typ | Beschreibung |
|--------|-----|--------------|
| Id | TEXT | Primärschlüssel |
| EntityName | TEXT | Name der betroffenen Entität |
| EntityId | TEXT | ID der betroffenen Entität |
| Action | TEXT | Art der Aktion (INSERT, UPDATE, DELETE, READ) |
| Changes | TEXT | Beschreibung der Änderungen (JSON) |
| ContainsSensitiveData | INTEGER | Flag, ob sensible Daten betroffen sind (0/1) |
| Timestamp | TEXT | Zeitpunkt der Aktion |
| UserId | TEXT | Benutzer, der die Aktion ausgeführt hat |

## Indizes

### Patient
- Primärindex: `Id`
- Sekundärindex: `InsuranceNumberHash` (für effiziente Suche)

### Session
- Primärindex: `Id`
- Sekundärindex: `PatientId` (für effiziente Suche nach Sessions eines Patienten)
- Sekundärindex: `SessionDate` (für Datumsfilterung)

### Transcript
- Primärindex: `Id`
- Sekundärindex: `SessionId` (für effiziente Suche nach Transkripten einer Session)
- Sekundärindex: `NeedsReview` (für schnellen Zugriff auf zu überprüfende Einträge)

### AnonymizationReviewItem
- Primärindex: `Id`
- Sekundärindex: `TranscriptId` (für effiziente Suche nach Reviews eines Transkripts)
- Sekundärindex: `Status` (für Filterung nach Status)
- Sekundärindex: `AnonymizationConfidence` (für Filterung nach Konfidenz)

### AuditLog
- Primärindex: `Id`
- Sekundärindex: `EntityName, EntityId` (für effiziente Suche nach Audit-Einträgen einer Entität)
- Sekundärindex: `Timestamp` (für Zeitfilterung)
- Sekundärindex: `UserId` (für Benutzerfilterung)

## Beziehungen

- **Patient** 1:N **Session** (Ein Patient kann mehrere Sessions haben)
- **Session** 1:N **Transcript** (Eine Session kann mehrere Transkripte haben)
- **Transcript** 1:N **AnonymizationReviewItem** (Ein Transkript kann mehrere Review-Items haben)

### KeyRotationLog [SP][ATV]

Protokolliert alle Schlüsselrotationen für Compliance und Sicherheit.

| Spalte | Typ | Beschreibung | Verschlüsselt |
|--------|-----|-------------|------------|
| Id | Guid | Primärschlüssel | Nein |
| KeyType | KeyType | Typ des rotierten Schlüssels (Enum: Database, FieldPatient, FieldSession, FieldTranscript, Backup) | Nein |
| OldKeyVersion | int | Version des alten Schlüssels | Nein |
| NewKeyVersion | int | Version des neuen Schlüssels | Nein |
| RotationReason | string | Grund für die Rotation (Scheduled, Compromised, Manual) | Nein |
| RotationStatus | RotationStatus | Status der Rotation (Enum: InProgress, Completed, Failed, RolledBack) | Nein |
| StartedAt | DateTime | Zeitpunkt des Rotationsbeginns | Nein |
| CompletedAt | DateTime? | Zeitpunkt der Rotationsvollendung (nullable) | Nein |
| RotatedBy | string | Benutzer/System, das die Rotation durchgeführt hat | Nein |
| ErrorMessage | string? | Fehlermeldung bei fehlgeschlagener Rotation (nullable) | Nein |
| AffectedRecords | int | Anzahl der betroffenen Datensätze | Nein |
| Created | DateTime | Erstellungszeitpunkt | Nein |
| CreatedBy | string | System/Benutzer, der den Eintrag erstellt hat | Nein |

**Wichtige Hinweise:**
- Schlüsselrotation erfolgt automatisch alle 90 Tage [SP]
- Manuelle Rotation bei Sicherheitsvorfällen möglich [ZTS]
- Vollständige Audit-Trail für Compliance [ATV]
- Alte Schlüssel werden für Backward-Kompatibilität 1 Jahr aufbewahrt

## Sicherheitsmerkmale [ZTS][SP]

1. **Verschlüsselung auf Spaltenebene**: Sensible Daten werden auf Spaltenebene verschlüsselt
2. **Audit-Trail**: Vollständige Protokollierung aller Änderungen [ATV]
3. **Keine Löschung**: Daten werden nie physisch gelöscht, sondern nur als gelöscht markiert
4. **Automatische Anonymisierung**: PII wird automatisch erkannt und anonymisiert [AIU]
5. **Review-Prozess**: Unsichere Anonymisierungen werden manuell überprüft [ARQ]

## Migrationen

Alle Datenbankmigrationen werden mit EF Core verwaltet und sind versioniert. Die Migrationen enthalten keine sensiblen Daten oder Schlüssel [NEA].
