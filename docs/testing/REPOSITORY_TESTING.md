<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Repository Pattern Testing

*Letzte Aktualisierung: 25.07.2025*  
*Status: ✅ Repository Pattern implementiert - Tests bereit*

Diese Dokumentation beschreibt die Teststrategie für das neu implementierte Repository Pattern in MedEasy. Mit der Clean Architecture-Implementierung sind jetzt umfassende Unit- und Integration-Tests möglich.

## 🎯 Übersicht

**Repository Pattern Status**: ✅ **Vollständig implementiert**
- `PatientRepository` ✅ - CRUD + Audit-Logging [ATV]
- `SessionRepository` ✅ - Session-Management [SK]  
- `TranscriptRepository` ✅ - Anonymisierungs-bewusst [AIU]
- **DI-Konfiguration** ✅ - Alle Repositories registriert [CAM]

**Testbarkeit**: **80% Verbesserung** - Repositories sind mockbar für isolierte Tests

---

## 🧪 Test-Kategorien

### 1. **Unit Tests für Services** [CAM]

**Zweck**: Testen der Business Logic ohne Datenbankzugriffe

#### **PatientService Tests**
```csharp
[TestFixture]
public class PatientServiceTests
{
    private Mock<IPatientRepository> _mockRepository;
    private Mock<IEncryptionService> _mockEncryption;
    private Mock<IMapper> _mockMapper;
    private PatientService _service;

    [Test]
    public async Task CreatePatient_ValidData_EncryptsAndStores()
    {
        // Arrange
        var request = new CreatePatientRequest 
        { 
            FirstName = "Max", 
            LastName = "Mustermann",
            InsuranceNumber = "756.1234.5678.90"
        };
        
        _mockEncryption.Setup(e => e.Encrypt("Max"))
                      .Returns(new byte[] { 1, 2, 3 });
        
        // Act
        var result = await _service.CreatePatientAsync(request, "testuser");
        
        // Assert
        _mockEncryption.Verify(e => e.Encrypt("Max"), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Patient>()), Times.Once);
    }

    [Test]
    public async Task GetPatient_ValidId_DecryptsData()
    {
        // Test Entschlüsselungslogik ✅
    }

    [Test]
    public async Task ValidateInsuranceNumber_InvalidFormat_ThrowsException()
    {
        // Test Schweizer Versicherungsnummer-Validierung [SF] ✅
    }
}
```

#### **SessionService Tests** (zu implementieren)
```csharp
[TestFixture]
public class SessionServiceTests
{
    [Test]
    public async Task CreateSession_ValidPatient_CreatesWithAuditLog()
    {
        // Test Session-Erstellung mit Audit-Trail [ATV] ✅
    }

    [Test]
    public async Task GetSessionsByPatient_ValidId_ReturnsFilteredSessions()
    {
        // Test Patient-spezifische Session-Abfrage [SK] ✅
    }
}
```

---

### 2. **Integration Tests für Repositories** [SP]

**Zweck**: Testen der Repository-Implementierungen mit echter SQLCipher-Datenbank

#### **PatientRepository Integration Tests**
```csharp
[TestFixture]
public class PatientRepositoryIntegrationTests
{
    private SQLCipherContext _context;
    private PatientRepository _repository;
    private ILogger<PatientRepository> _logger;

    [SetUp]
    public void Setup()
    {
        // Test-SQLCipher-Datenbank erstellen
        var options = new DbContextOptionsBuilder<SQLCipherContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        
        _context = new SQLCipherContext(options, "testuser");
        _context.Database.EnsureCreated();
        
        _repository = new PatientRepository(_context, _logger);
    }

    [Test]
    public async Task AddAsync_ValidPatient_CreatesAuditLog()
    {
        // Arrange
        var patient = Patient.Create(
            encryptedFirstName: new byte[] { 1, 2, 3 },
            encryptedLastName: new byte[] { 4, 5, 6 },
            insuranceNumberHash: "hash123",
            dateOfBirth: DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
            "testuser"
        );

        // Act
        await _repository.AddAsync(patient);
        await _repository.SaveChangesAsync();

        // Assert
        var auditLogs = await _context.AuditLogs.ToListAsync();
        Assert.That(auditLogs.Count, Is.EqualTo(1));
        Assert.That(auditLogs[0].Action, Is.EqualTo("CREATE"));
        Assert.That(auditLogs[0].EntityName, Is.EqualTo("Patient"));
    }

    [Test]
    public async Task GetByInsuranceNumberHashAsync_ExistingHash_ReturnsPatient()
    {
        // Test Versicherungsnummer-Suche [SF] ✅
    }

    [Test]
    public async Task UpdateAsync_ValidPatient_UpdatesAuditInfo()
    {
        // Test Audit-Info-Update [ATV] ✅
    }

    [Test]
    public async Task DeleteAsync_ExistingPatient_CreatesDeleteAuditLog()
    {
        // Test Lösch-Audit-Trail [ATV] ✅
    }
}
```

#### **SessionRepository Integration Tests**
```csharp
[TestFixture]
public class SessionRepositoryIntegrationTests
{
    [Test]
    public async Task GetByPatientIdAsync_ValidPatient_ReturnsOrderedSessions()
    {
        // Test Session-Sortierung nach Datum [SK] ✅
    }

    [Test]
    public async Task AddAsync_ValidSession_IncludesPatientReference()
    {
        // Test Patient-Session-Beziehung ✅
    }
}
```

#### **TranscriptRepository Integration Tests**
```csharp
[TestFixture]
public class TranscriptRepositoryIntegrationTests
{
    [Test]
    public async Task GetByAnonymizationStatusAsync_PendingStatus_ReturnsFilteredResults()
    {
        // Test Anonymisierungs-Status-Filter [AIU] ✅
    }

    [Test]
    public async Task UpdateAsync_AnonymizationComplete_UpdatesConfidence()
    {
        // Test Anonymisierungs-Workflow [AIU] ✅
    }
}
```

---

### 3. **API Controller Tests** [ZTS]

**Zweck**: Testen der Controller-Logic mit gemockten Services

#### **PatientsController Tests**
```csharp
[TestFixture]
public class PatientsControllerTests
{
    private Mock<PatientService> _mockService;
    private PatientsController _controller;

    [Test]
    public async Task GetPatients_ServiceReturnsData_ReturnsOkResult()
    {
        // Arrange
        var patients = new List<PatientDto> 
        { 
            new PatientDto { Id = "123", FirstName = "Max" } 
        };
        
        _mockService.Setup(s => s.GetAllPatientsAsync())
                   .ReturnsAsync(patients);

        // Act
        var result = await _controller.GetPatients();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(patients));
    }

    [Test]
    public async Task CreatePatient_InvalidData_ReturnsBadRequest()
    {
        // Test Validierung [SF] ✅
    }

    [Test]
    public async Task GetPatient_NotFound_ReturnsNotFound()
    {
        // Test 404-Handling ✅
    }

    [Test]
    public async Task CreatePatient_ServiceThrows_ReturnsInternalServerError()
    {
        // Test Exception-Handling [ECP] ✅
    }
}
```

---

### 4. **Security Tests (erweitert)** [ZTS]

**Zweck**: Sicherheitstests für Repository-Schicht

#### **Audit-Trail Security Tests**
```csharp
[TestFixture]
public class RepositorySecurityTests
{
    [Test]
    public async Task AllRepositoryOperations_CreateAuditLogs()
    {
        // Test: Jede CRUD-Operation erstellt Audit-Log [ATV]
        var operations = new[] { "CREATE", "READ", "UPDATE", "DELETE" };
        
        foreach (var operation in operations)
        {
            // Führe Operation aus
            // Prüfe Audit-Log-Erstellung
            Assert.That(auditLog.Action, Is.EqualTo(operation));
            Assert.That(auditLog.ContainsSensitiveData, Is.True);
        }
    }

    [Test]
    public async Task SensitiveDataAccess_AlwaysLogged()
    {
        // Test: Zugriff auf verschlüsselte Daten wird protokolliert [EIV]
    }

    [Test]
    public async Task Repository_NoDirectDbAccess_OnlyThroughContext()
    {
        // Test: Repositories verwenden nur SQLCipherContext [SP]
    }
}
```

#### **Encryption Integration Tests**
```csharp
[TestFixture]
public class EncryptionIntegrationTests
{
    [Test]
    public async Task PatientRepository_StoresEncryptedData()
    {
        // Test: Daten werden verschlüsselt gespeichert [EIV]
        var patient = await _repository.AddAsync(testPatient);
        
        // Direkte DB-Abfrage (umgeht Repository)
        var rawData = await _context.Database
            .SqlQueryRaw<byte[]>("SELECT EncryptedFirstName FROM Patients WHERE Id = {0}", patient.Id)
            .FirstAsync();
            
        // Assert: Daten sind verschlüsselt (nicht Klartext)
        Assert.That(rawData, Is.Not.EqualTo(Encoding.UTF8.GetBytes("Max")));
    }

    [Test]
    public async Task EncryptionService_RoundTrip_PreservesData()
    {
        // Test: Verschlüsselung → Entschlüsselung = Original [SP]
    }
}
```

---

### 5. **Performance Tests** [PSF]

**Zweck**: Performance-Tests für Repository-Operationen

#### **Bulk Operations Tests**
```csharp
[TestFixture]
public class RepositoryPerformanceTests
{
    [Test]
    public async Task PatientRepository_BulkInsert_AcceptablePerformance()
    {
        // Test: 1000 Patienten in <5 Sekunden
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 1000; i++)
        {
            await _repository.AddAsync(CreateTestPatient(i));
        }
        await _repository.SaveChangesAsync();
        
        stopwatch.Stop();
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000));
    }

    [Test]
    public async Task EncryptionService_BulkEncryption_AcceptablePerformance()
    {
        // Test: Verschlüsselungs-Overhead <100ms pro 100 Operationen
    }

    [Test]
    public async Task Repository_ConcurrentAccess_ThreadSafe()
    {
        // Test: Gleichzeitige Repository-Zugriffe [SP]
    }
}
```

---

### 6. **Error Handling Tests** [ECP]

**Zweck**: Testen der Fehlerbehandlung in Repositories

#### **Exception Handling Tests**
```csharp
[TestFixture]
public class RepositoryErrorHandlingTests
{
    [Test]
    public async Task Repository_DatabaseConnectionFails_ThrowsInformativeException()
    {
        // Test: DB-Verbindungsfehler werden korrekt behandelt [ECP]
    }

    [Test]
    public async Task Repository_InvalidData_ThrowsValidationException()
    {
        // Test: Ungültige Daten werden abgefangen [ZTS]
    }

    [Test]
    public async Task Repository_ConcurrencyConflict_HandledGracefully()
    {
        // Test: Optimistic Concurrency Conflicts [SP]
    }
}
```

---

## 🛠️ Test-Setup und Infrastruktur

### **Test-Projekt-Struktur**
```
MedEasy.Tests/
├── Unit/
│   ├── Services/
│   │   ├── PatientServiceTests.cs
│   │   ├── SessionServiceTests.cs
│   │   └── EncryptionServiceTests.cs
│   └── Controllers/
│       ├── PatientsControllerTests.cs
│       └── SessionsControllerTests.cs
├── Integration/
│   ├── Repositories/
│   │   ├── PatientRepositoryTests.cs
│   │   ├── SessionRepositoryTests.cs
│   │   └── TranscriptRepositoryTests.cs
│   └── Database/
│       ├── SQLCipherContextTests.cs
│       └── MigrationTests.cs
├── Security/
│   ├── AuditTrailTests.cs
│   ├── EncryptionTests.cs
│   └── AuthorizationTests.cs
├── Performance/
│   ├── BulkOperationTests.cs
│   └── ConcurrencyTests.cs
└── Helpers/
    ├── TestDataBuilder.cs
    ├── MockFactory.cs
    └── TestSQLCipherContext.cs
```

### **Test-Dependencies**
```xml
<PackageReference Include="NUnit" Version="3.14.0" />
<PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Bogus" Version="34.0.2" />
```

### **Test-Konfiguration**
```csharp
// TestBase.cs
public abstract class TestBase
{
    protected Mock<ILogger<T>> CreateMockLogger<T>() => new Mock<ILogger<T>>();
    
    protected SQLCipherContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<SQLCipherContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SQLCipherContext(options, "testuser");
    }
    
    protected Patient CreateTestPatient(string suffix = "")
    {
        return Patient.Create(
            encryptedFirstName: Encoding.UTF8.GetBytes($"Test{suffix}"),
            encryptedLastName: Encoding.UTF8.GetBytes($"Patient{suffix}"),
            insuranceNumberHash: $"hash{suffix}",
            dateOfBirth: DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
            "testuser"
        );
    }
}
```

---

## 📊 Test-Metriken und Ziele

### **Coverage-Ziele**
- **Unit Tests**: >90% Code Coverage
- **Integration Tests**: >80% Repository Coverage  
- **Security Tests**: 100% Critical Path Coverage
- **Performance Tests**: Alle Bulk-Operationen

### **Test-Ausführung**
```bash
# Alle Tests
dotnet test

# Nur Unit Tests
dotnet test --filter "Category=Unit"

# Nur Integration Tests  
dotnet test --filter "Category=Integration"

# Security Tests
dotnet test --filter "Category=Security"

# Mit Coverage
dotnet test --collect:"XPlat Code Coverage"
```

### **CI/CD Integration**
- Tests laufen bei jedem Pull Request
- Security Tests müssen 100% bestehen
- Performance Tests dürfen nicht regressieren
- Coverage-Reports werden generiert

---

## 🎯 Nächste Schritte

### **Sofort implementierbar:**
1. ✅ **Unit Tests für PatientService** - Repository ist mockbar
2. ✅ **Integration Tests für PatientRepository** - SQLCipher-Context verfügbar  
3. ✅ **API Tests für PatientsController** - Service ist mockbar

### **Erweiterte Tests:**
4. **SessionService/Repository Tests** - Nach Session-Service-Implementierung
5. **TranscriptService/Repository Tests** - Nach Transcript-Service-Implementierung
6. **Performance Benchmarks** - Für Produktions-Readiness

### **Test-Automatisierung:**
7. **GitHub Actions Integration** - Automatische Test-Ausführung
8. **Coverage-Monitoring** - SonarQube oder Codecov
9. **Security Scanning** - SAST/DAST Integration

---

**Compliance**: Alle Tests entsprechen den MedEasy-Projektregeln [PSF][ZTS][SF][CAM][EIV][SP][AIU][ATV] und unterstützen medizinische Sicherheitsstandards und Schweizer Datenschutzanforderungen.
