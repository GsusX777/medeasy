<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Datenbankimplementierung

*Letzte Aktualisierung: 12.07.2025*

## Übersicht [SP][AIU][ATV]

Diese Dokumentation beschreibt die Implementierung der sicheren Datenbankschicht für MedEasy mit SQLCipher in der Tauri-Backend-Anwendung. Die Implementierung folgt den MedEasy-Projektregeln für Sicherheit, Datenschutz und Clean Architecture.

## Architektur [CAS][DD]

Die Datenbankimplementierung folgt dem Clean Architecture-Prinzip mit klarer Trennung der Verantwortlichkeiten:

```
src-tauri/
├── database/             # Datenbankzugriff und -modelle
│   ├── connection.rs     # Verbindungsverwaltung mit SQLCipher
│   ├── models.rs         # Konsolidierte Datenmodelle (Patient, Session, Transcript, AnonymizationReviewItem, AuditLog)
│   ├── schema.rs         # Datenbankschema
│   ├── encryption.rs     # Feldverschlüsselung
│   ├── audit.rs          # AuditLogger für Sicherheits-Logging [ATV]
│   └── migrations.rs     # Schema-Migrationen
├── repositories/         # Geschäftslogik für Datenzugriff
│   ├── patient_repository.rs
│   ├── session_repository.rs
│   ├── transcript_repository.rs
│   ├── anonymization_review_repository.rs
│   └── audit_repository.rs     # Audit-Logging mit Erzwingungsfunktion
├── security/             # Sicherheitskomponenten [SP][ATV]
│   ├── key_manager.rs    # Schlüsselverwaltung und -rotation
│   └── key_manager_backup.rs  # Backup-Strategien für Schlüssel
└── commands.rs           # Tauri-Befehle für Frontend-Zugriff
```

## API-Referenz [D=C]

### DatabaseManager

Der `DatabaseManager` ist verantwortlich für die Verwaltung von Datenbankverbindungen und stellt sicher, dass alle Sicherheitsrichtlinien eingehalten werden.

#### Konstruktoren

```rust
// Standardkonstruktor für normale Anwendungsfälle
pub fn new() -> Result<Self, DatabaseError>

// Konstruktor mit explizitem Produktions-Flag für Tests und spezielle Umgebungen
pub fn new_with_production_flag(is_production: bool) -> Result<Self, DatabaseError>
```

#### Verbindungsverwaltung

```rust
// Liefert eine Verbindung aus dem Pool
pub fn get_connection(&self) -> Result<PooledConnection<SqliteConnectionManager>, DatabaseError>

// Führt eine Funktion mit einer Verbindung aus
pub fn with_connection<F, T>(&self, f: F) -> Result<T, DatabaseError>
where
    F: FnOnce(&Connection) -> Result<T, Error>
```

#### Migrationen und Schema

```rust
// Führt Datenbankmigrationen aus und erstellt alle notwendigen Tabellen
pub fn run_migrations(&self) -> Result<(), DatabaseError>
```

#### Hilfsmethoden

```rust
// Prüft, ob die Datenbank verschlüsselt ist
pub fn is_encrypted(&self) -> bool
```

## Sicherheitsmerkmale [SP][AIU][ATV]

### SQLCipher-Integration [SP]

Die Datenbank verwendet SQLCipher mit AES-256-Verschlüsselung und folgenden Sicherheitseinstellungen:

```rust
// Sichere SQLCipher-Konfiguration
conn.pragma_update(None, "cipher_page_size", 4096)?;
conn.pragma_update(None, "kdf_iter", 256000)?;
conn.pragma_update(None, "cipher_memory_security", "ON")?;
conn.pragma_update(None, "cipher_default_kdf_algorithm", "PBKDF2_HMAC_SHA512")?;
conn.pragma_update(None, "cipher_default_plaintext_header_size", 32)?;
conn.pragma_update(None, "cipher_hmac_algorithm", "HMAC_SHA512")?;
```

### Erzwungene Verschlüsselung [SP]

Die Verschlüsselung ist in Produktionsumgebungen erzwungen und kann nicht deaktiviert werden. Alle Konstruktoren prüfen diese Bedingung:

```rust
// Erzwinge Verschlüsselung in Produktion
let use_encryption = if !is_production {
    env::var("USE_ENCRYPTION")
        .unwrap_or_else(|_| "false".to_string())
        .parse::<bool>()
        .unwrap_or(false)
} else {
    true
};

if is_production && !use_encryption {
    return Err(DatabaseError::EnvError(
        "Encryption must be enabled in production".to_string()
    ));
}
```

Alle Datenbankverbindungen in Produktionsumgebungen werden strikt mit dem AES-256-Schlüssel initialisiert, der als Base64-kodierter 32-Byte-Schlüssel aus der Umgebungsvariable `MEDEASY_DB_KEY` gelesen wird. Eine fehlerhafte oder fehlende Konfiguration führt zu einem sofortigen Fehler.

### Feldverschlüsselung [EIV]

Sensible Daten werden zusätzlich auf Feldebene mit AES-256-GCM verschlüsselt:

```rust
pub fn encrypt(&self, plaintext: &str) -> Result<Vec<u8>, EncryptionError> {
    // Generiere zufälligen Nonce für jede Verschlüsselung
    let mut nonce = [0u8; 12];
    getrandom::getrandom(&mut nonce).map_err(|e| EncryptionError::RandomError(e.to_string()))?;
    let nonce = Nonce::from_slice(&nonce);
    
    // Verschlüssele den Text
    let ciphertext = self.cipher.encrypt(nonce, plaintext.as_bytes().as_ref())
        .map_err(|e| EncryptionError::EncryptionError(e.to_string()))?;
    
    // Kombiniere Nonce und Ciphertext für Speicherung
    let mut result = Vec::with_capacity(nonce.len() + ciphertext.len());
    result.extend_from_slice(nonce.as_slice());
    result.extend_from_slice(&ciphertext);
    
    Ok(result)
}
```

### Unveränderliche Anonymisierung [AIU]

Die Anonymisierung ist obligatorisch und kann nicht deaktiviert werden:

```rust
// Enforce anonymization [AIU]
if anonymized_text.trim().is_empty() {
    return Err(TranscriptRepositoryError::AnonymizationRequired(
        "Anonymized text is required and cannot be empty".to_string()
    ));
}
```

### Vollständiger Audit-Trail [ATV]

Jede Datenbankoperation wird protokolliert. Die Audit-Log-Tabelle wird bei der Migration automatisch erstellt:

```rust
// Erstelle Audit-Log-Tabelle [ATV]
conn.execute(
    "CREATE TABLE IF NOT EXISTS audit_logs (
        id TEXT PRIMARY KEY,
        user_id TEXT,
        action TEXT NOT NULL,
        entity_type TEXT NOT NULL,
        entity_id TEXT NOT NULL,
        details TEXT,
        is_sensitive INTEGER NOT NULL DEFAULT 0,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    )",
    [],
).map_err(|e| DatabaseError::SqlError(e))?
```

```rust
// Create audit log entry [ATV]
let audit_log = AuditLog::new(
    "patients",
    &patient_id,
    "INSERT",
    Some("Created new patient record"),
    true, // Contains sensitive data
    user_id,
);

conn.execute(
    "INSERT INTO audit_logs (
        id, entity_name, entity_id, action, changes,
        contains_sensitive_data, timestamp, user_id
    ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8)",
    params![
        audit_log.id,
        audit_log.entity_name,
        audit_log.entity_id,
        audit_log.action,
        audit_log.changes,
        audit_log.contains_sensitive_data as i32,
        audit_log.timestamp,
        audit_log.user_id
    ],
)?;
```

## Fehlerbehandlung [ZTS][ECP]

Die Datenbankimplementierung verwendet eine typisierte Fehlerbehandlung, um alle möglichen Fehlersituationen korrekt zu behandeln:

```rust
#[derive(Debug, thiserror::Error)]
pub enum DatabaseError {
    #[error("SQL error: {0}")]
    SqlError(#[from] rusqlite::Error),
    
    #[error("Connection pool error: {0}")]
    PoolError(#[from] r2d2::Error),
    
    #[error("Environment error: {0}")]
    EnvError(String),
    
    #[error("Migration error: {0}")]
    MigrationError(String),
    
    #[error("Encryption error: {0}")]
    EncryptionError(#[from] EncryptionError),
}
```

Beachten Sie, dass der vorher verwendete `ConfigurationError`-Typ entfernt und durch den spezifischeren `EnvError` ersetzt wurde, um Umgebungskonfigurationsprobleme genauer zu identifizieren. Dies verbessert insbesondere die Fehlerbehandlung für Verschlüsselungserzwingung in Produktion und Schlüsselvalidierung.

## Testbarkeit [TR][ECP]

Die Implementierung wurde für umfassende Testbarkeit optimiert:

### Isolierte Tests mit TestGuard

```rust
pub struct TestGuard {
    old_vars: HashMap<String, Option<String>>,
}

impl TestGuard {
    pub fn new() -> Self {
        // Speichere alle relevanten Umgebungsvariablen
        let mut guard = TestGuard {
            old_vars: HashMap::new(),
        };
        
        // Sichere wichtige Variablen
        for var in ["DATABASE_URL", "MEDEASY_DB_KEY", "USE_ENCRYPTION", "ENFORCE_AUDIT", "MEDEASY_FIELD_ENCRYPTION_KEY"] {
            guard.old_vars.insert(var.to_string(), env::var(var).ok());
            env::remove_var(var);
        }
        
        guard
    }
}

impl Drop for TestGuard {
    fn drop(&mut self) {
        // Stelle alle gespeicherten Umgebungsvariablen wieder her
        for (var, value) in &self.old_vars {
            if let Some(val) = value {
                env::set_var(var, val);
            } else {
                env::remove_var(var);
            }
        }
    }
}
```

### UUID-basierte Testdatenbanken

Jeder Test verwendet eine eindeutige, temporäre Datenbankdatei:

```rust
// Eindeutige Datenbank für diesen Test
let db_path = format!("test_db_{}.db", Uuid::new_v4());
env::set_var("DATABASE_URL", &db_path);
```

Dies verhindert Interferenzen zwischen Tests und stellt sicher, dass jeder Test mit einer sauberen Datenbank beginnt.

### Base64-kodierte Testschlüssel

Für Tests werden spezielle 32-Byte-AES-Schlüssel verwendet:

```rust
// Standard-Testschlüssel (32 Bytes für AES-256)
env::set_var("MEDEASY_DB_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
env::set_var("MEDEASY_FIELD_ENCRYPTION_KEY", "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
```

Diese Schlüssel sind Base64-kodiert und entsprechen einem Byte-Array mit Werten von 1-32.

## Datenmodelle [MDL]

Die Datenmodelle folgen dem Domain-Driven Design mit medizinischer Fachsprache:

### Patient

```rust
pub struct Patient {
    pub id: String,
    pub encrypted_first_name: Vec<u8>,
    pub encrypted_last_name: Vec<u8>,
    pub date_of_birth: String,
    pub insurance_number_hash: String,
    pub encrypted_notes: Option<Vec<u8>>,
    pub created: String,
    pub created_by: String,
    pub last_modified: String,
    pub last_modified_by: String,
}
```

### Session

```rust
pub struct Session {
    pub id: String,
    pub patient_id: String,
    pub status: SessionStatus,
    pub encrypted_notes: Option<Vec<u8>>,
    pub encrypted_audio_file_path: Option<Vec<u8>>,
    pub created: String,
    pub created_by: String,
    pub last_modified: String,
    pub last_modified_by: String,
}
```

### Transcript

```rust
pub struct Transcript {
    pub id: String,
    pub session_id: String,
    pub encrypted_original_text: Vec<u8>,
    pub encrypted_anonymized_text: Vec<u8>,
    pub is_anonymized: bool,
    pub anonymization_confidence: Option<f64>,
    pub needs_review: bool,
    pub created: String,
    pub created_by: String,
    pub last_modified: String,
    pub last_modified_by: String,
}
```

### AuditLog

```rust
pub struct AuditLog {
    pub id: String,
    pub entity_name: String,
    pub entity_id: String,
    pub action: String,
    pub changes: Option<String>,
    pub contains_sensitive_data: bool,
    pub timestamp: String,
    pub user_id: String,
}
```

## Repositories [CQA]

Die Repositories implementieren das Command Query Responsibility Segregation (CQRS) Pattern:

### PatientRepository

```rust
pub struct PatientRepository {
    db: DatabaseManager,
    encryption: FieldEncryption,
}

impl PatientRepository {
    pub fn new(db: DatabaseManager) -> Result<Self, PatientRepositoryError> { ... }
    pub fn create_patient(&self, first_name: &str, last_name: &str, date_of_birth: &str, 
                         insurance_number: &str, notes: Option<&str>, user_id: &str) 
                         -> Result<Patient, PatientRepositoryError> { ... }
    pub fn get_patient_by_id(&self, id: &str, user_id: &str) 
                            -> Result<Patient, PatientRepositoryError> { ... }
    pub fn get_all_patients(&self, user_id: &str) 
                           -> Result<Vec<Patient>, PatientRepositoryError> { ... }
    // Weitere Methoden...
}
```

### AuditRepository [ATV][ZTS]

```rust
pub struct AuditRepository {
    db: DatabaseManager,
}

impl AuditRepository {
    // Konstruktor mit Erzwingungsprüfung
    pub fn new(db: DatabaseManager) -> Result<Self, AuditRepositoryError> { ... }
    
    // Prüft, ob Audit-Logging erzwungen wird (basierend auf MEDEASY_ENFORCE_AUDIT)
    pub fn is_enforced(&self) -> bool { ... }
    
    // Erstellt einen Audit-Log-Eintrag
    pub fn create_audit_log(&self, entity_name: &str, entity_id: &str, 
                           action: &str, changes: Option<&str>, 
                           contains_sensitive_data: bool, user_id: &str) 
                           -> Result<AuditLog, AuditRepositoryError> { ... }
    
    // Sucht Audit-Logs nach verschiedenen Kriterien
    pub fn get_audit_logs_by_entity(&self, entity_name: &str, entity_id: &str) 
                                   -> Result<Vec<AuditLog>, AuditRepositoryError> { ... }
                                   
    pub fn get_audit_logs_by_user(&self, user_id: &str) 
                                -> Result<Vec<AuditLog>, AuditRepositoryError> { ... }
                                
    pub fn get_audit_logs_by_timerange(&self, start_time: &str, end_time: &str) 
                                     -> Result<Vec<AuditLog>, AuditRepositoryError> { ... }
    
    // Weitere Methoden...
}
```

### AnonymizationReviewRepository [AIU][ARQ][ATV]

```rust
pub struct AnonymizationReviewRepository {
    db: DatabaseManager,
    audit_repository: AuditRepository,
}

impl AnonymizationReviewRepository {
    pub fn new(db: DatabaseManager, audit_repository: AuditRepository) -> Result<Self, AnonymizationReviewRepositoryError> { ... }
    
    pub fn create_review_item(&self, transcript_id: &str, detected_pii: Option<&str>,
                             anonymization_confidence: f64, review_reason: &str, user_id: &str)
                             -> Result<AnonymizationReviewItem, AnonymizationReviewRepositoryError> { ... }
                             
    pub fn get_review_item_by_id(&self, id: &str, user_id: &str)
                                -> Result<AnonymizationReviewItem, AnonymizationReviewRepositoryError> { ... }
                                
    pub fn get_review_items_by_status(&self, status: ReviewStatus, user_id: &str)
                                     -> Result<Vec<AnonymizationReviewItem>, AnonymizationReviewRepositoryError> { ... }
                                     
    pub fn update_review_status(&self, id: &str, status: ReviewStatus, notes: Option<&str>, user_id: &str)
                               -> Result<AnonymizationReviewItem, AnonymizationReviewRepositoryError> { ... }
    
    // Weitere Methoden...
}
```

## Umgebungskonfiguration [IC]

Die Anwendung unterstützt verschiedene Umgebungen mit unterschiedlichen Sicherheitsanforderungen:

### Entwicklung

```
# Datenbankeinstellungen
DATABASE_URL=sqlite://./medeasy.development.db
USE_ENCRYPTION=false

# Audit-Einstellungen
MEDEASY_ENFORCE_AUDIT=false  # [ATV] In Entwicklung optional

# Entwicklungsschlüssel (NUR für Entwicklung!)
MEDEASY_DB_KEY=dev_key_please_change_in_production_1234567890
MEDEASY_FIELD_ENCRYPTION_KEY=ZGV2X2tleV9wbGVhc2VfY2hhbmdlX2luX3Byb2R1Y3Rpb25fMTIzNDU2Nzg5MA==
```

### Produktion

```
# Datenbankeinstellungen
DATABASE_URL=sqlite://./medeasy.production.db
USE_ENCRYPTION=true  # [SP] In Produktion MUSS Verschlüsselung aktiviert sein

# Audit-Einstellungen
MEDEASY_ENFORCE_AUDIT=true  # [ATV][ZTS] In Produktion MUSS Audit-Logging erzwungen werden

# Produktionsschlüssel (MÜSSEN sicher generiert werden!)
MEDEASY_DB_KEY=<Generierter 32-Byte Schlüssel für SQLCipher>
MEDEASY_FIELD_ENCRYPTION_KEY=<Generierter Base64-kodierter 32-Byte Schlüssel für Feldverschlüsselung>
```

## Datenbank-Hilfsskript [DU]

Das PowerShell-Skript `scripts/db-helper.ps1` bietet Funktionen für:

- Datenbankerstellung: `.\db-helper.ps1 create development`
- Backup: `.\db-helper.ps1 backup production`
- Reset: `.\db-helper.ps1 reset development`
- Schlüsselgenerierung: `.\db-helper.ps1 genkey`

## Tauri-Integration

Die Datenbankfunktionalität wird über Tauri-Befehle für das Frontend verfügbar gemacht:

```rust
#[tauri::command]
pub async fn create_patient(
    patient_repo: State<'_, PatientRepoState>,
    patient: CreatePatientDto,
    user_id: String,
) -> CommandResult<PatientDto> {
    // Implementierung...
}
```

## Schlüsselrotation [SP][ATV][ZTS]

Die Schlüsselrotation ist ein kritischer Sicherheitsaspekt der MedEasy-Datenbankimplementierung. Sie gewährleistet, dass Verschlüsselungsschlüssel regelmäßig erneuert werden, um die Sicherheit der gespeicherten Daten zu maximieren.

### KeyManager-Architektur

Der `KeyManager` verwaltet verschiedene Schlüsseltypen:

```rust
pub enum KeyType {
    Database,        // SQLCipher-Datenbankschlüssel
    FieldPatient,    // Patientendaten-Verschlüsselung
    FieldSession,    // Sitzungsdaten-Verschlüsselung
    FieldTranscript, // Transkript-Verschlüsselung
    Backup,          // Backup-Verschlüsselung
}
```

### Rotationsstatus

Jeder Schlüssel hat einen Rotationsstatus:

- **UpToDate**: Schlüssel ist aktuell
- **DueSoon**: Rotation wird in den nächsten 7 Tagen empfohlen
- **Overdue**: Rotation ist überfällig (>30 Tage)

### Automatische Rotation

```rust
// Beispiel: Schlüsselrotation mit Audit-Logging
let key_manager = KeyManager::new(audit_logger)?;
key_manager.initialize("master_password")?;

// Rotation eines spezifischen Schlüssels
key_manager.rotate_key(KeyType::FieldPatient, "admin_user")?;

// Rotation aller Schlüssel
for key_type in [KeyType::Database, KeyType::FieldPatient, /* ... */] {
    key_manager.rotate_key(key_type, "admin_user")?;
}
```

### Sicherheitsfeatures

1. **Deadlock-Vermeidung**: Mutex-Locking mit begrenztem Scope
2. **Audit-Trail**: Jede Rotation wird protokolliert [ATV]
3. **Versionierung**: Schlüssel haben Versionsnummern für Nachverfolgung
4. **Sichere Speicherung**: Alle Schlüssel werden verschlüsselt gespeichert [SP]

### Integration mit Datenbank

Die Schlüsselrotation ist nahtlos in die Datenbankoperationen integriert:

- **Feldverschlüsselung**: Neue Daten werden mit aktuellen Schlüsseln verschlüsselt
- **Backward-Kompatibilität**: Alte Daten können mit vorherigen Schlüsselversionen entschlüsselt werden
- **Migration**: Schrittweise Re-Verschlüsselung mit neuen Schlüsseln

### Testabdeckung

Die Schlüsselrotation hat 100% Testabdeckung mit 5 spezialisierten Tests:

- `test_key_manager_initialization`: Initialisierung
- `test_key_rotation`: Einzelne Schlüsselrotation
- `test_rotation_status`: Status-Überprüfung
- `test_key_rotation_audit`: Audit-Logging
- `test_rotate_all_key_types`: Multi-Key-Rotation

## Compliance-Hinweise [RW][PL]

Diese Implementierung erfüllt folgende Anforderungen:

- **Schweizer nDSG**: Verschlüsselung aller personenbezogenen Daten
- **Medizinische Datenschutzbestimmungen**: Audit-Trail und Zugriffsprotokollierung
- **DSGVO/GDPR**: Datenschutz durch Technikgestaltung (Privacy by Design)

## Verbotene Praktiken [NSB][NUS][NRPD]

Die folgenden Praktiken sind in dieser Implementierung verboten:

- **Keine echten Patientendaten in Tests oder Entwicklung** [NRPD]
- **Keine Umgehung der Verschlüsselung** [NSB]
- **Keine unverschlüsselte Speicherung sensibler Daten** [NUS]
- **Keine Deaktivierung der Anonymisierung** [AIU]
- **Keine Deaktivierung des Audit-Trails** [ATV]

## Testabdeckung [KP100]

Kritische Sicherheitsfunktionen erfordern 100% Testabdeckung:

- Verschlüsselung: 100% Coverage PFLICHT
- Anonymisierung: 100% Coverage PFLICHT
- Audit: 100% Coverage PFLICHT
