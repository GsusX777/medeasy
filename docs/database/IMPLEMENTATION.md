# MedEasy Datenbank-Implementierung (.NET Backend)

„Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

## Übersicht

Die MedEasy-Datenbank wird als .NET 8 Backend mit Entity Framework Core und SQLCipher implementiert. Diese Implementierung folgt Clean Architecture-Prinzipien und bietet vollständige Verschlüsselung, Anonymisierung und Audit-Trail-Funktionalität.

## Architektur-Übersicht

### Clean Architecture Struktur [CAS]

```
MedEasy.Domain/
├── Entities/           # Domain-Entitäten (Patient, Session, etc.)
├── ValueObjects/       # Werteobjekte (InsuranceNumber, etc.)
├── Enums/             # Domain-Enumerationen
└── Interfaces/        # Repository-Interfaces

MedEasy.Application/
├── Commands/          # CQRS Commands
├── Queries/           # CQRS Queries
├── Handlers/          # MediatR Handlers
├── DTOs/              # Data Transfer Objects
└── Services/          # Application Services

MedEasy.Infrastructure/
├── Data/              # Entity Framework Context
├── Repositories/      # Repository-Implementierungen
├── Encryption/        # Verschlüsselungsservices
├── Audit/             # Audit-Trail-Implementierung
└── Migrations/        # EF Core Migrations

MedEasy.API/
├── Controllers/       # REST API Controllers
├── Middleware/        # Security Middleware
└── Configuration/     # Startup-Konfiguration
```

## Datenbank-Konfiguration

### SQLCipher Integration [SP]

```csharp
// MedEasy.Infrastructure/Data/MedEasyDbContext.cs
public class MedEasyDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = GetSQLCipherConnectionString();
        optionsBuilder.UseSqlite(connectionString);
    }

    private string GetSQLCipherConnectionString()
    {
        var dbPath = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), 
            "MedEasy", "medeasy.db");
            
        var key = Environment.GetEnvironmentVariable("MEDEASY_DB_KEY") 
            ?? throw new InvalidOperationException("Database key not found");
            
        return $"Data Source={dbPath};Password={key};";
    }
}
```

### Entity Framework Konfiguration

```csharp
// MedEasy.Infrastructure/Data/Configurations/PatientConfiguration.cs
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients");
        
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        
        // Verschlüsselte Felder [EIV]
        builder.Property(p => p.EncryptedFirstName)
            .HasColumnName("encrypted_first_name")
            .HasColumnType("BLOB");
            
        builder.Property(p => p.EncryptedLastName)
            .HasColumnName("encrypted_last_name")
            .HasColumnType("BLOB");
            
        // Anonymisierte Felder [AIU]
        builder.Property(p => p.AnonymizedFirstName)
            .HasColumnName("anonymized_first_name")
            .HasMaxLength(100);
    }
}
```

## Domain-Entitäten

### Patient Entity [EIV]

```csharp
// MedEasy.Domain/Entities/Patient.cs
public class Patient : AuditableEntity
{
    public Guid Id { get; set; }
    
    // Verschlüsselte Originaldaten [SP]
    public byte[] EncryptedFirstName { get; set; }
    public byte[] EncryptedLastName { get; set; }
    public byte[] EncryptedDateOfBirth { get; set; }
    public byte[] EncryptedInsuranceNumber { get; set; }
    
    // Anonymisierte Daten für UI [AIU]
    public string AnonymizedFirstName { get; set; }
    public string AnonymizedLastName { get; set; }
    public string AnonymizedDateOfBirth { get; set; }
    public string InsuranceNumberHash { get; set; }
    
    // Anonymisierungs-Metadaten
    public bool IsAnonymized { get; set; } = true;
    public DateTime? AnonymizedAt { get; set; }
    public string AnonymizedBy { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
```

### Session Entity [EIV]

```csharp
// MedEasy.Domain/Entities/Session.cs
public class Session : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    
    public DateTime SessionDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    
    public SessionStatus Status { get; set; }
    
    // Verschlüsselte Daten [SP]
    public byte[] EncryptedNotes { get; set; }
    public byte[] EncryptedAudioReference { get; set; }
    
    // Navigation Properties
    public virtual Patient Patient { get; set; }
    public virtual ICollection<Transcript> Transcripts { get; set; } = new List<Transcript>();
}
```

## Verschlüsselungsservice

### Field Encryption Service [SP][EIV]

```csharp
// MedEasy.Infrastructure/Encryption/FieldEncryptionService.cs
public class FieldEncryptionService : IFieldEncryptionService
{
    private readonly byte[] _encryptionKey;
    
    public FieldEncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:FieldKey"] 
            ?? throw new InvalidOperationException("Field encryption key not configured");
        _encryptionKey = Convert.FromBase64String(keyBase64);
        
        if (_encryptionKey.Length != 32)
            throw new ArgumentException("Encryption key must be exactly 32 bytes");
    }
    
    public byte[] Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return null;
            
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        
        // IV am Anfang speichern
        msEncrypt.Write(aes.IV, 0, aes.IV.Length);
        
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plaintext);
        }
        
        return msEncrypt.ToArray();
    }
    
    public string Decrypt(byte[] ciphertext)
    {
        if (ciphertext == null || ciphertext.Length == 0)
            return null;
            
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        
        // IV aus dem Anfang lesen
        var iv = new byte[16];
        Array.Copy(ciphertext, 0, iv, 0, 16);
        aes.IV = iv;
        
        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(ciphertext, 16, ciphertext.Length - 16);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        
        return srDecrypt.ReadToEnd();
    }
}
```

## Anonymisierungsservice

### Anonymization Service [AIU]

```csharp
// MedEasy.Infrastructure/Services/AnonymizationService.cs
public class AnonymizationService : IAnonymizationService
{
    private readonly ILogger<AnonymizationService> _logger;
    private readonly IAuditService _auditService;
    
    public AnonymizationService(ILogger<AnonymizationService> logger, IAuditService auditService)
    {
        _logger = logger;
        _auditService = auditService;
    }
    
    public async Task<AnonymizationResult> AnonymizePatientAsync(Patient patient, string userId)
    {
        var result = new AnonymizationResult();
        
        try
        {
            // Unveränderliche Anonymisierung [AIU]
            patient.AnonymizedFirstName = AnonymizeFirstName(patient.EncryptedFirstName);
            patient.AnonymizedLastName = AnonymizeLastName(patient.EncryptedLastName);
            patient.AnonymizedDateOfBirth = AnonymizeDateOfBirth(patient.EncryptedDateOfBirth);
            patient.InsuranceNumberHash = HashInsuranceNumber(patient.EncryptedInsuranceNumber);
            
            patient.IsAnonymized = true;
            patient.AnonymizedAt = DateTime.UtcNow;
            patient.AnonymizedBy = userId;
            
            // Audit-Log [ATV]
            await _auditService.LogAsync(new AuditEntry
            {
                EntityType = "Patient",
                EntityId = patient.Id.ToString(),
                Action = "Anonymize",
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Details = "Patient data anonymized"
            });
            
            result.Success = true;
            _logger.LogInformation("Patient {PatientId} anonymized successfully", patient.Id);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Failed to anonymize patient {PatientId}", patient.Id);
        }
        
        return result;
    }
    
    private string AnonymizeFirstName(byte[] encryptedFirstName)
    {
        // Implementierung der Anonymisierungslogik
        // Diese Funktion kann NIEMALS deaktiviert werden [AIU]
        var decrypted = _fieldEncryption.Decrypt(encryptedFirstName);
        return decrypted?.Length > 0 ? $"{decrypted[0]}***" : "***";
    }
}
```

## Repository-Implementierung

### Patient Repository [CAS]

```csharp
// MedEasy.Infrastructure/Repositories/PatientRepository.cs
public class PatientRepository : IPatientRepository
{
    private readonly MedEasyDbContext _context;
    private readonly IFieldEncryptionService _encryption;
    private readonly IAnonymizationService _anonymization;
    private readonly IAuditService _audit;
    
    public PatientRepository(
        MedEasyDbContext context,
        IFieldEncryptionService encryption,
        IAnonymizationService anonymization,
        IAuditService audit)
    {
        _context = context;
        _encryption = encryption;
        _anonymization = anonymization;
        _audit = audit;
    }
    
    public async Task<Patient> CreateAsync(CreatePatientDto dto, string userId)
    {
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            
            // Verschlüsselung der Originaldaten [SP]
            EncryptedFirstName = _encryption.Encrypt(dto.FirstName),
            EncryptedLastName = _encryption.Encrypt(dto.LastName),
            EncryptedDateOfBirth = _encryption.Encrypt(dto.DateOfBirth.ToString("yyyy-MM-dd")),
            EncryptedInsuranceNumber = _encryption.Encrypt(dto.InsuranceNumber),
            
            Created = DateTime.UtcNow,
            CreatedBy = userId,
            LastModified = DateTime.UtcNow,
            LastModifiedBy = userId
        };
        
        // Automatische Anonymisierung [AIU]
        await _anonymization.AnonymizePatientAsync(patient, userId);
        
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        
        // Audit-Log [ATV]
        await _audit.LogAsync(new AuditEntry
        {
            EntityType = "Patient",
            EntityId = patient.Id.ToString(),
            Action = "Create",
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            Details = "New patient created and anonymized"
        });
        
        return patient;
    }
    
    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        // Nur anonymisierte Daten zurückgeben [AIU]
        return await _context.Patients
            .Select(p => new Patient
            {
                Id = p.Id,
                AnonymizedFirstName = p.AnonymizedFirstName,
                AnonymizedLastName = p.AnonymizedLastName,
                AnonymizedDateOfBirth = p.AnonymizedDateOfBirth,
                InsuranceNumberHash = p.InsuranceNumberHash,
                IsAnonymized = p.IsAnonymized,
                Created = p.Created,
                LastModified = p.LastModified
            })
            .ToListAsync();
    }
}
```

## API Controller

### Patients Controller

```csharp
// MedEasy.API/Controllers/PatientsController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PatientsController> _logger;
    
    public PatientsController(IMediator mediator, ILogger<PatientsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients()
    {
        try
        {
            var query = new GetPatientsQuery();
            var patients = await _mediator.Send(query);
            return Ok(patients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patients");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<PatientDto>> CreatePatient([FromBody] CreatePatientDto dto)
    {
        try
        {
            var command = new CreatePatientCommand(dto, User.Identity.Name);
            var patient = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

## CQRS Implementation

### Commands und Queries

```csharp
// MedEasy.Application/Commands/CreatePatientCommand.cs
public record CreatePatientCommand(CreatePatientDto Patient, string UserId) : IRequest<PatientDto>;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, PatientDto>
{
    private readonly IPatientRepository _repository;
    private readonly IMapper _mapper;
    
    public CreatePatientCommandHandler(IPatientRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    
    public async Task<PatientDto> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _repository.CreateAsync(request.Patient, request.UserId);
        return _mapper.Map<PatientDto>(patient);
    }
}
```

## Audit-Trail Implementation

### Audit Service [ATV]

```csharp
// MedEasy.Infrastructure/Audit/AuditService.cs
public class AuditService : IAuditService
{
    private readonly MedEasyDbContext _context;
    private readonly ILogger<AuditService> _logger;
    
    public async Task LogAsync(AuditEntry entry)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                Action = entry.Action,
                UserId = entry.UserId,
                Timestamp = entry.Timestamp,
                Details = entry.Details,
                IpAddress = entry.IpAddress,
                UserAgent = entry.UserAgent
            };
            
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Audit entry logged: {Action} on {EntityType} {EntityId} by {UserId}", 
                entry.Action, entry.EntityType, entry.EntityId, entry.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit entry");
            // Audit-Fehler dürfen nicht die Hauptoperation blockieren
        }
    }
}
```

## Konfiguration und Startup

### Program.cs

```csharp
// MedEasy.API/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddDbContext<MedEasyDbContext>();
builder.Services.AddScoped<IFieldEncryptionService, FieldEncryptionService>();
builder.Services.AddScoped<IAnonymizationService, AnonymizationService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePatientCommand).Assembly));

// AutoMapper
builder.Services.AddAutoMapper(typeof(PatientProfile));

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

var app = builder.Build();

// Middleware
app.UseAuthentication();
app.UseAuthorization();

// Audit Middleware
app.UseMiddleware<AuditMiddleware>();

app.MapControllers();
app.Run();
```

## Sicherheitsfeatures

### Umgebungsvariablen [ZTS]

```bash
# Erforderliche Umgebungsvariablen
MEDEASY_DB_KEY=base64_encoded_32_byte_key
MEDEASY_FIELD_ENCRYPTION_KEY=base64_encoded_32_byte_key
MEDEASY_JWT_KEY=your_jwt_signing_key
```

### Schlüsselverwaltung [SP]

```csharp
// MedEasy.Infrastructure/Security/KeyManager.cs
public class KeyManager : IKeyManager
{
    public void ValidateKeys()
    {
        ValidateKey("MEDEASY_DB_KEY", "Database encryption key");
        ValidateKey("MEDEASY_FIELD_ENCRYPTION_KEY", "Field encryption key");
    }
    
    private void ValidateKey(string envVar, string description)
    {
        var keyBase64 = Environment.GetEnvironmentVariable(envVar);
        if (string.IsNullOrEmpty(keyBase64))
            throw new InvalidOperationException($"{description} not configured");
            
        var key = Convert.FromBase64String(keyBase64);
        if (key.Length != 32)
            throw new ArgumentException($"{description} must be exactly 32 bytes");
    }
}
```

## Testing

### Unit Tests

```csharp
// MedEasy.Tests/Repositories/PatientRepositoryTests.cs
public class PatientRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ShouldEncryptAndAnonymizeData()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MedEasyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        using var context = new MedEasyDbContext(options);
        var encryption = new Mock<IFieldEncryptionService>();
        var anonymization = new Mock<IAnonymizationService>();
        var audit = new Mock<IAuditService>();
        
        var repository = new PatientRepository(context, encryption.Object, anonymization.Object, audit.Object);
        
        var dto = new CreatePatientDto
        {
            FirstName = "Max",
            LastName = "Mustermann",
            DateOfBirth = new DateTime(1990, 1, 1),
            InsuranceNumber = "123.4567.8901.23"
        };
        
        // Act
        var result = await repository.CreateAsync(dto, "test-user");
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsAnonymized);
        encryption.Verify(x => x.Encrypt("Max"), Times.Once);
        anonymization.Verify(x => x.AnonymizePatientAsync(It.IsAny<Patient>(), "test-user"), Times.Once);
        audit.Verify(x => x.LogAsync(It.IsAny<AuditEntry>()), Times.Once);
    }
}
```

## Migration und Deployment

### Entity Framework Migrations

```bash
# Migration erstellen
dotnet ef migrations add InitialCreate --project MedEasy.Infrastructure --startup-project MedEasy.API

# Datenbank aktualisieren
dotnet ef database update --project MedEasy.Infrastructure --startup-project MedEasy.API
```

### Deployment-Konfiguration

```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=%LOCALAPPDATA%\\MedEasy\\medeasy.db;Password={DB_KEY};"
  },
  "Encryption": {
    "FieldKey": "{FIELD_ENCRYPTION_KEY}"
  },
  "Jwt": {
    "Key": "{JWT_KEY}",
    "Issuer": "MedEasy",
    "Audience": "MedEasy-Client"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Compliance und Sicherheit

### Schweizer Compliance [SF][DSC]

- **nDSG-Konformität**: Vollständige Verschlüsselung und Anonymisierung
- **Datenschutz**: Lokale Speicherung, keine Cloud-Übertragung ohne Einwilligung
- **Audit-Trail**: Vollständige Nachverfolgbarkeit aller Operationen [ATV]
- **Unveränderliche Anonymisierung**: Kann niemals deaktiviert werden [AIU]

### Sicherheitsmaßnahmen [ZTS]

- **Verschlüsselung**: SQLCipher + AES-256 Feldverschlüsselung [SP]
- **Authentifizierung**: JWT-basierte API-Authentifizierung
- **Autorisierung**: Rollenbasierte Zugriffskontrolle
- **Audit-Trail**: Vollständige Protokollierung [ATV]
- **Schlüsselverwaltung**: Sichere Umgebungsvariablen [ZTS]

## Nächste Schritte

1. **Frontend-Integration**: HTTP-Client für API-Aufrufe implementieren
2. **JWT-Authentication**: Frontend-Authentifizierung einrichten
3. **Mock-Daten Cleanup**: Übergang von Mock-API zu echter API
4. **Testing**: Umfassende Test-Suite implementieren
5. **Deployment**: Produktions-Deployment vorbereiten

---

**Projektregeln angewendet**: [CAS][SP][AIU][ATV][EIV][ZTS][SF][DSC][D=C][DSU]
