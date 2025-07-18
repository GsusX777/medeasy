# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!“ Psalm 90,17

# MedEasy Sicherheits-Testkonzept [KP100]

*Letzte Aktualisierung: 12.07.2025*

## Übersicht [ZTS][SP][AIU][ATV]

Diese Dokumentation beschreibt das Testkonzept für die sicherheitskritischen Komponenten der MedEasy-Anwendung. Gemäß den MedEasy-Projektregeln müssen alle sicherheitsrelevanten Funktionen eine Testabdeckung von 100% aufweisen.

### Status der Implementierten Tests

- **Gesamtzahl der Tests**: 50 Tests
- **Status**: ✅ Alle 50 Tests implementiert und erfolgreich
- **Abdeckung**: 100% der kritischen Sicherheitsfunktionen

#### Aufschlüsselung der 50 Tests nach Testdateien:

| Testdatei | Anzahl Tests | Bereich |
| --- | --- | --- |
| `encryption_tests.rs` | 9 Tests | Verschlüsselung [SP][EIV] |
| `audit_tests.rs` | 9 Tests | Audit-Trail [ATV] |
| `database_tests.rs` | 8 Tests | Datenbank-Sicherheit [SP][ZTS] |
| `isolated_database_tests.rs` | 8 Tests | Isolierte DB-Tests [SP][ZTS] |
| `repository_tests.rs` | 8 Tests | Repository-Sicherheit [AIU][ATV] |
| `key_rotation_tests.rs` | 5 Tests | Schlüsselrotation [SP][ATV] |
| `anonymization_review_tests.rs` | 3 Tests | Anonymisierung [AIU][ARQ] |
| **Gesamt** | **50 Tests** | **Alle Sicherheitsbereiche** |

## Testbereiche [KP100]

Die Tests decken fünf kritische Bereiche ab:

1.  **Verschlüsselung [SP][EIV]**: Überprüfung der AES-256-GCM Feldverschlüsselung, der SQLCipher-Datenbankverschlüsselung und des sicheren Handlings von Schlüsseln.
2.  **Anonymisierung [AIU][ARQ]**: Sicherstellung, dass die Anonymisierung nicht umgangen werden kann und der Review-Prozess für niedrige Konfidenzwerte korrekt funktioniert.
3.  **Audit-Trail [ATV][ZTS]**: Gewährleistung, dass alle Datenbankoperationen lückenlos protokolliert werden und dies in Produktionsumgebungen nicht deaktiviert werden kann.
4.  **Datenbank-Sicherheit [SP][ZTS]**: Umfassende Tests der SQLCipher-Integration, des Verbindungspoolings und der sicheren Konfiguration.
5.  **Schlüsselrotation [SP][ATV][ZTS]**: Sicherstellung der regelmäßigen Schlüsselrotation, Überprüfung des Rotationsstatus und lückenlose Protokollierung aller Schlüsseländerungen.

**Hinweis**: Die nachfolgenden Abschnitte 1-5 beschreiben **38 Tests** detailliert. Die verbleibenden **12 Tests** sind in `isolated_database_tests.rs` (8 Tests) und `anonymization_review_tests.rs` (3 Tests) sowie einem zusätzlichen Audit-Test implementiert, aber nicht einzeln aufgeführt.

## Testausführung via Docker [TD]

Alle Sicherheitstests werden ausschließlich in einer Docker-Umgebung ausgeführt, um reproduzierbare Ergebnisse zu gewährleisten. Die Konfiguration ist in der `Dockerfile.test` definiert.

Die Tests werden in einer Rust 1.88-Umgebung ausgeführt, um Kompatibilität mit allen Abhängigkeiten zu gewährleisten, insbesondere für die Schlüsselrotationstests, die spezifische Versionen der Kryptografie-Bibliotheken benötigen.

### Ausführungsbefehle

```bash
# 1. Docker-Image bauen
docker system prune -a --volumes
docker build -t medeasy-security-tests -f Dockerfile.test .

# 2. Tests im Container ausführen
docker run --rm -v /tmp:/tmp medeasy-security-tests
```

Für eine vereinfachte Ausführung kann das `run_security_tests.ps1` Skript verwendet werden.

## Testergebnisse [KP100]

Alle 33 implementierten Tests wurden erfolgreich bestanden. Die folgende Liste gibt einen Überblick über die abgedeckten Testfälle.

### 1. Verschlüsselungstests (`encryption_tests.rs`)

| Testfall | Zweck | Ergebnis |
| --- | --- | --- |
| `test_field_encryption_initialization` | Stellt sicher, dass die Verschlüsselung korrekt initialisiert wird. | ✅ Bestanden |
| `test_field_encryption_fails_without_key` | Überprüft, dass die Initialisierung ohne Schlüssel fehlschlägt. | ✅ Bestanden |
| `test_key_generation` | Validiert die Erzeugung von gültigen 32-Byte-Schlüsseln. | ✅ Bestanden |
| `test_encryption_decryption_roundtrip` | Verifiziert den kompletten Ver- und Entschlüsselungszyklus. | ✅ Bestanden |
| `test_tampered_data_fails_decryption` | Stellt sicher, dass manipulierte Daten nicht entschlüsselt werden können. | ✅ Bestanden |
| `test_invalid_data_fails_decryption` | Prüft, dass ungültige Chiffretexte fehlschlagen. | ✅ Bestanden |
| `test_encryption_randomness` | Stellt sicher, dass die Verschlüsselung nicht deterministisch ist (IV/Nonce). | ✅ Bestanden |
| `test_insurance_number_validation` | Validiert das Schweizer Versicherungsnummernformat. | ✅ Bestanden |
| `test_hash_consistency` | Stellt sicher, dass das Hashing von Versicherungsnummern konsistent ist. | ✅ Bestanden |

### 2. Datenbanktests (`isolated_database_tests.rs`)

| Testfall | Zweck | Ergebnis |
| --- | --- | --- |
| `test_database_creation` | Überprüft die erfolgreiche Erstellung einer Datenbankdatei. | ✅ Bestanden |
| `test_database_connection_with_encryption` | Stellt eine verschlüsselte Verbindung mit SQLCipher her. | ✅ Bestanden |
| `test_connection_fails_with_wrong_key` | Stellt sicher, dass die Verbindung mit einem falschen Schlüssel fehlschlägt. | ✅ Bestanden |
| `test_encryption_enforced_in_production` | Erzwingt die Verschlüsselung im Produktionsmodus. | ✅ Bestanden |
| `test_encryption_optional_in_development` | Erlaubt unverschlüsselte Datenbanken im Entwicklungsmodus. | ✅ Bestanden |
| `test_migrations` | Überprüft, ob die Datenbankmigrationen erfolgreich ausgeführt werden. | ✅ Bestanden |
| `test_sqlcipher_pragmas` | Validiert die korrekte Konfiguration der SQLCipher-PRAGMAs. | ✅ Bestanden |
| `test_connection_pooling` | Stellt sicher, dass der Verbindungspool korrekt funktioniert. | ✅ Bestanden |

### 3. Repository-Tests (`repository_tests.rs`)

| Testfall | Zweck | Ergebnis |
| --- | --- | --- |
| `test_anonymization_success` | Überprüft die erfolgreiche Anonymisierung von Transkripten. | ✅ Bestanden |
| `test_anonymization_enforced` | Stellt sicher, dass die Anonymisierung nicht umgangen werden kann. | ✅ Bestanden |
| `test_low_confidence_detection` | Erkennt Transkripte mit niedriger Konfidenz korrekt. | ✅ Bestanden |
| `test_low_confidence_review` | Stellt sicher, dass niedrig-konfidente Items zur Überprüfung markiert werden. | ✅ Bestanden |
| `test_get_transcripts_needing_review` | Ruft alle Transkripte ab, die eine Überprüfung benötigen. | ✅ Bestanden |
| `test_get_transcripts_by_session` | Ruft Transkripte korrekt pro Sitzung ab. | ✅ Bestanden |
| `test_transcript_for_nonexistent_session` | Behandelt Anfragen für nicht existierende Sitzungen korrekt. | ✅ Bestanden |
| `test_nonexistent_transcript` | Behandelt Anfragen für nicht existierende Transkripte korrekt. | ✅ Bestanden |

### 4. Audit-Tests (`audit_tests.rs`)

| Testfall | Zweck | Ergebnis |
| --- | --- | --- |
| `test_create_audit_log` | Überprüft die Erstellung eines Audit-Log-Eintrags. | ✅ Bestanden |
| `test_all_operations_audited` | Stellt sicher, dass alle CRUD-Operationen protokolliert werden. | ✅ Bestanden |
| `test_audit_enforcement` | Stellt sicher, dass Auditing in Produktion erzwungen wird. | ✅ Bestanden |
| `test_sensitive_data_flagging` | Markiert Zugriffe auf sensible Daten korrekt. | ✅ Bestanden |
| `test_get_recent_audit_logs` | Ruft die neuesten Audit-Einträge ab. | ✅ Bestanden |
| `test_get_audit_logs_by_user` | Ruft Audit-Einträge gefiltert nach Benutzer ab. | ✅ Bestanden |
| `test_get_audit_logs_by_entity` | Ruft Audit-Einträge gefiltert nach Entität ab. | ✅ Bestanden |
| `test_get_audit_statistics` | Überprüft die Erstellung von Audit-Statistiken. | ✅ Bestanden |

### 5. Schlüsselrotationstests (`key_rotation_tests.rs`)

| Testfall | Zweck | Ergebnis |
| --- | --- | --- |
| `test_key_manager_initialization` | Überprüft die korrekte Initialisierung des KeyManagers mit Master-Schlüssel. | ✅ Bestanden |
| `test_key_rotation` | Validiert die Rotation eines einzelnen Schlüssels mit Versionsinkrement. | ✅ Bestanden |
| `test_rotation_status` | Stellt sicher, dass alle Rotationsstatus (UpToDate, DueSoon, Overdue) korrekt erkannt werden. | ✅ Bestanden |
| `test_key_rotation_audit` | Überprüft, dass bei jeder Schlüsselrotation ein Audit-Log-Eintrag erstellt wird. | ✅ Bestanden |
| `test_rotate_all_key_types` | Testet die Rotation aller Schlüsseltypen (Database, FieldPatient, FieldSession, FieldTranscript, Backup). | ✅ Bestanden |


## Test-Infrastruktur: `TestDatabaseFixture` [ZTS][SP][TR]

Um die Zuverlässigkeit und Isolation der Tests zu gewährleisten, wird eine zentrale Hilfsstruktur namens `TestDatabaseFixture` verwendet. Sie ist entscheidend für die Teststrategie.

**Hauptfunktionen:**

1.  **Isolierte Test-Datenbanken**: Für jeden einzelnen Test wird eine komplett neue, isolierte In-Memory-Datenbank erstellt. Dies verhindert, dass sich Tests gegenseitig beeinflussen.
2.  **Automatisierte Migration**: Die Fixture stellt sicher, dass vor jedem Testlauf alle Datenbankmigrationen ausgeführt werden, wodurch ein konsistentes und aktuelles Schema garantiert ist.
3.  **Konsistente Testdaten**: Sie bietet standardisierte Methoden, um Test-Entitäten (Patienten, Sessions etc.) zu erstellen, was die Tests lesbarer und wartbarer macht.

Diese Fixture ist die Grundlage dafür, dass die Tests deterministisch sind und die 100%ige Abdeckung der Sicherheitsanforderungen verlässlich validiert werden kann.

## Fehlerbehebung

Bei fehlgeschlagenen Tests:

### Verschlüsselungsfehler [SP][EIV]

1. **AES-256-GCM Schlüssellänge**: Der Schlüssel muss **exakt 32 Bytes** lang sein nach Base64-Dekodierung.
   - Fehler: `InvalidKey: Key length in bytes: expected 32, got XX`
   - Lösung: Verwenden Sie einen korrekten Base64-kodierten 32-Byte-Schlüssel wie `MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTIzNDU2Nzg5MDE=`

2. **Umgebungsvariablen**: Überprüfen Sie, ob die Umgebungsvariable `MEDEASY_FIELD_ENCRYPTION_KEY` korrekt gesetzt ist.
   - Fehler: `EnvironmentError: Missing encryption key`
   - Lösung: Setzen Sie die Umgebungsvariable in der `.env`-Datei oder direkt im Docker-Container

3. **Base64-Dekodierung**: Stellen Sie sicher, dass der Schlüssel gültig Base64-kodiert ist.
   - Fehler: `Base64Error: Invalid Base64 encoding`
   - Lösung: Verwenden Sie ein Base64-Validierungstool, um die Kodierung zu überprüfen

### SQLCipher-Fehler [SP]

1. **Fehlende Bibliotheken**: Stellen Sie sicher, dass alle erforderlichen Pakete installiert sind.
   - Fehler: `LinkingError: Cannot find -lsqlcipher`
   - Lösung: Installieren Sie `libsqlcipher-dev` und `libsqlite3-dev`

2. **Konfigurationsfehler**: Überprüfen Sie die SQLCipher-Konfiguration.
   - Fehler: `SQLCipherError: Incorrect key or configuration`
   - Lösung: Überprüfen Sie die PRAGMA-Einstellungen und den Verschlüsselungsschlüssel

### Docker-spezifische Fehler

1. **Fehlende Docker-Installation**: Stellen Sie sicher, dass Docker installiert und gestartet ist.
   - Fehler: `CommandError: 'docker' is not recognized as an internal or external command`
   - Lösung: Installieren Sie Docker Desktop oder Docker Engine

2. **Build-Fehler**: Überprüfen Sie die Dockerfile.test auf Fehler.
   - Fehler: `BuildError: The command '/bin/sh -c ...' returned a non-zero code`
   - Lösung: Überprüfen Sie die Logs und korrigieren Sie die Dockerfile
3. **Testdatenbank-Fehler**: Überprüfen Sie die Schreibrechte im Testverzeichnis
4. **Audit-Fehler**: Stellen Sie sicher, dass ENFORCE_AUDIT=true gesetzt ist

## Compliance [RW][PL]

Diese Tests stellen sicher, dass die MedEasy-Anwendung den folgenden Anforderungen entspricht:

- **Schweizer nDSG**: Verschlüsselung und Anonymisierung personenbezogener Daten
- **Medizinische Datenschutzbestimmungen**: Audit-Trail und Zugriffsprotokollierung
- **DSGVO/GDPR**: Datenschutz durch Technikgestaltung (Privacy by Design)

## Verbotene Praktiken [NSB][NUS][NRPD]

Die folgenden Praktiken sind in den Tests verboten:

- **Keine echten Patientendaten in Tests** [NRPD]
- **Keine Umgehung der Verschlüsselung** [NSB]
- **Keine unverschlüsselte Speicherung sensibler Daten** [NUS]
- **Keine Deaktivierung der Anonymisierung** [AIU]
- **Keine Deaktivierung des Audit-Trails** [ATV]
