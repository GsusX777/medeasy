<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Naming Conventions

*Letzte Aktualisierung: 25.07.2025*  
*Status: ✅ Aktuell implementiert*

Dieses Dokument definiert die **tatsächlich implementierten** Naming-Konventionen für MedEasy. Alle hier dokumentierten Standards sind im Code umgesetzt.

## 🎯 Übersicht der Konventionen

| Layer | Klassen/Types | Properties/Fields | JSON API | Besonderheiten |
|-------|---------------|-------------------|----------|----------------|
| **Database** | `PascalCase` | `PascalCase` | - | `Encrypted`/`Anonymized` Präfixe |
| **.NET Backend** | `PascalCase` | `PascalCase` | `camelCase` | DTOs, Services, Controllers |
| **TypeScript Frontend** | `PascalCase` | `camelCase` | `camelCase` | Interfaces, Enums |
| **Python AI** | `PascalCase` | `snake_case` | - | Classes, functions |

## 1. Database (SQLCipher) [SP][EIV]

```sql
-- Tabellen: PascalCase, Singular
CREATE TABLE Patient (
    Id TEXT PRIMARY KEY,
    EncryptedFirstName BLOB,        -- [EIV] Verschlüsselte Felder
    EncryptedLastName BLOB,         -- [EIV] 
    InsuranceNumberHash TEXT,       -- [SF] Hash-Suffix
    DateOfBirth TEXT,               -- [SF] Schweizer Format
    LastModified TEXT,              -- [ATV] Audit-Felder
    LastModifiedBy TEXT             -- [ATV]
);

CREATE TABLE Session (
    Id TEXT PRIMARY KEY,
    PatientId TEXT,                 -- Foreign Keys
    SessionDate TEXT,               -- [SF] DD.MM.YYYY
    StartTime TEXT,                 -- [SF] HH:MM
    Status TEXT,                    -- Enums als String
    EncryptedNotes BLOB             -- [EIV]
);
```

## 2. .NET Backend [CAM][ZTS]

### Entities (Domain Layer)
```csharp
public class Patient : IHasAuditInfo
{
    public Guid Id { get; private set; }
    public byte[] EncryptedFirstName { get; private set; }  // [EIV] Getrennte Namen
    public byte[] EncryptedLastName { get; private set; }   // [EIV]
    public string InsuranceNumberHash { get; private set; } // [SF]
    public DateOnly DateOfBirth { get; private set; }      // [SF]
    public DateTime LastModified { get; private set; }     // [ATV]
    public string LastModifiedBy { get; private set; }     // [ATV]
}
```

### DTOs (Application Layer)
```csharp
// Response DTOs
public class PatientDto
{
    public string Id { get; set; }                    // Guid → string für JSON
    public string FirstName { get; set; }             // Entschlüsselt
    public string LastName { get; set; }              // Entschlüsselt
    public DateTime DateOfBirth { get; set; }         // ISO-8601 für API
    public string DateOfBirthFormatted { get; set; }  // DD.MM.YYYY für UI [SF]
    public string InsuranceNumberMasked { get; set; } // XXX.XXXX.XXXX.## [AIU]
}

// Request DTOs
public class CreatePatientRequest
{
    [Required]
    public string FirstName { get; set; }             // Validierung
    
    [Required]
    public string LastName { get; set; }
    
    [Required]
    [RegularExpression(@"^\d{3}\.\d{4}\.\d{4}\.\d{2}$")]
    public string InsuranceNumber { get; set; }       // [SF] Schweizer Format
}
```

### Services & Controllers
```csharp
// Services: Klassenname + Service
public class PatientService : IPatientService
{
    public async Task<PatientDto> CreatePatientAsync(CreatePatientRequest request, string currentUser)
    public async Task<PatientDto?> GetPatientByIdAsync(Guid patientId)
}

// Controllers: Plural + Controller
[Route("api/v1/[controller]")]
public class PatientsController : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPatient(Guid id)
    
    [HttpPost]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientRequest request)
}
```

## 3. JSON API [ZTS]

**Konfiguration**: `JsonNamingPolicy.CamelCase` in `Program.cs`

```json
// Request (camelCase)
{
  "firstName": "Max",
  "lastName": "Mustermann",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "insuranceNumber": "756.1234.5678.90"
}

// Response (camelCase)
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "firstName": "Max",
  "lastName": "Mustermann",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "dateOfBirthFormatted": "15.01.1990",
  "insuranceNumberMasked": "756.1234.XXXX.XX",
  "lastModified": "2025-07-25T09:24:02Z"
}
```

## 4. TypeScript Frontend [TSF]

```typescript
// Interfaces: PascalCase
export interface PatientDto {
  id: string;                         // camelCase Properties
  firstName: string;                  // ✅ Konsistent mit API
  lastName: string;
  dateOfBirth: string;                // ISO-8601 von API
  dateOfBirthFormatted?: string;      // DD.MM.YYYY für UI [SF]
  insuranceNumberMasked: string;      // Maskiert [AIU]
  lastModified: string;
}

// Enums: PascalCase
export enum SessionStatus {
  Scheduled = 'Scheduled',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

// Svelte Stores: camelCase
export const patients = writable<PatientDto[]>([]);
export const selectedPatientId = writable<string | null>(null);
```

## 5. Schweizer Besonderheiten [SF]

### Versicherungsnummer
```
Format: XXX.XXXX.XXXX.XX
Regex: ^\d{3}\.\d{4}\.\d{4}\.\d{2}$
Beispiel: 756.1234.5678.90
```

### Datumsformate
```
Database: "15.01.1990" (DD.MM.YYYY)
API: "1990-01-15T00:00:00Z" (ISO-8601)
UI: "15.01.1990" (DD.MM.YYYY)
```

### Sprache
```
Code/Comments: Englisch
UI-Texte: Deutsch (Hochdeutsch)
Medizinische Begriffe: Deutsch [MDL]
```

## 6. Sicherheitspräfixe [EIV][AIU]

```
Encrypted{FieldName}     - AES-256-GCM verschlüsselt
Anonymized{FieldName}    - PII entfernt/ersetzt
{FieldName}Hash          - SHA-256 Hash
{FieldName}Masked        - Für UI-Anzeige maskiert
```

## 7. API-Endpoint-Patterns [WMM][CAM]

### REST-Endpoints
```
/api/v{version}/{resource}           - Standard CRUD
/api/v{version}/{resource}/{id}      - Spezifische Ressource
/api/v{version}/{service}/{action}   - Service-Aktionen
```

### Whisper-spezifische Endpoints [WMM]
```
POST /api/v1/ai/transcribe          - Audio-Transkription
POST /api/v1/ai/benchmark-models    - Model-Performance-Test
GET  /api/v1/ai/available-models    - Verfügbare Modelle
GET  /api/v1/ai/hardware-info       - Hardware-Analyse
```

### JSON-Serialisierung [SF]
```csharp
// .NET Properties: PascalCase
public string ModelName { get; set; }
public int ProcessingTimeMs { get; set; }

// JSON Output: camelCase (via JsonNamingPolicy)
{
  "modelName": "base",
  "processingTimeMs": 1250
}
```

## 8. Audit-Trail [ATV]

Alle Entities haben:
```csharp
public DateTime Created { get; private set; }
public string CreatedBy { get; private set; }
public DateTime LastModified { get; private set; }
public string LastModifiedBy { get; private set; }
```

---

**Compliance**: Alle Konventionen entsprechen den MedEasy-Projektregeln [PSF][ZTS][SF][CAM][EIV][SP][AIU][ATV] und unterstützen Clean Architecture sowie Schweizer Datenschutzanforderungen.
