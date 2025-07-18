// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MedEasy.Infrastructure.Database;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MedEasy.Domain.Entities;
using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Linq;
using System.Collections.Generic;
using MedEasy.API.HealthChecks;
using MedEasy.API.Middleware;

// [PSF][PbD][RA][SC][DSC] MedEasy API Startup
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Verwende Enums als Strings für bessere Lesbarkeit
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Ignoriere Null-Werte in der JSON-Ausgabe
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger/OpenAPI Konfiguration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "MedEasy API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
});

// CORS-Konfiguration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder => builder
            .WithOrigins("https://localhost:5173") // Tauri + Svelte Frontend [TSF]
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// SQLCipher Konfiguration für AES-256 Verschlüsselung [SP]
builder.Services.Configure<SQLCipherOptions>(options =>
{
    // Schlüssel aus Konfiguration oder Umgebungsvariable laden [NEA]
    var encryptionKeyBase = builder.Configuration["Database:EncryptionKey"] 
        ?? Environment.GetEnvironmentVariable("MEDEASY_DB_KEY");
    
    if (string.IsNullOrEmpty(encryptionKeyBase))
    {
        throw new InvalidOperationException(
            "Database encryption key is not configured. Set 'Database:EncryptionKey' in configuration or 'MEDEASY_DB_KEY' environment variable [SP][ZTS]");
    }
    
    // Sicheren Schlüssel aus Basis-Schlüssel ableiten
    var salt = builder.Configuration["Database:Salt"] ?? "MedEasy-SQLCipher-Salt";
    options.EncryptionKey = SQLCipherOptions.DeriveKeyFromPassword(encryptionKeyBase, salt);
});

// SQLCipherContext mit AES-256 Verschlüsselung registrieren [SP]
builder.Services.AddDbContext<SQLCipherContext>((serviceProvider, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "Database connection string is not configured. Set 'ConnectionStrings:DefaultConnection' in configuration [SP]");
    }
    
    // SQLite mit SQLCipher konfigurieren
    options.UseSqlite(connectionString);
    
    // SQLCipher-Konfiguration anwenden
    options.UseSQLCipher(config =>
    {
        config.KeyDerivationIterations = 256000; // Hohe Anzahl für bessere Sicherheit
        config.CipherMode = "aes-256-cbc"; // AES-256 als Standard [SP]
    });
    
    // Aktuellen Benutzer für Audit-Trail injizieren
    var httpContextAccessor = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
    var currentUser = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
    
    // SQLCipherContext mit aktuellem Benutzer initialisieren
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
});

// HttpContextAccessor für Benutzer-Identifikation
builder.Services.AddHttpContextAccessor();

// Health Checks [MPR]
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SQLCipherContext>("database", tags: new[] { "ready" })
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "live" });

// JWT Authentication [ZTS]
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
        };
    });

// Rate Limiting [ZTS]
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = Microsoft.AspNetCore.RateLimiting.PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(context =>
    {
        return Microsoft.AspNetCore.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new Microsoft.AspNetCore.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });
    
    // Spezielle Rate Limits für sensible Endpunkte
    options.AddPolicy("sensitive", context =>
    {
        return Microsoft.AspNetCore.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new Microsoft.AspNetCore.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[] { "application/json", "text/plain" };
});

// Weitere DI-Registrierungen hier...

var app = builder.Build();

// Zentrale Fehlerbehandlung [ECP][NSF]
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Produktion: Strikte Sicherheitseinstellungen [ZTS]
    app.UseHsts();
}

// Response Compression
app.UseResponseCompression();

// Sicherheits-Header [ZTS]
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    context.Response.Headers.Add("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowSpecificOrigins");

// Rate Limiting [ZTS]
app.UseRateLimiter();

// Authentifizierung und Autorisierung [ZTS]
app.UseAuthentication();
app.UseAuthorization();

// Audit-Logging Middleware [ATV]
app.UseMiddleware<AuditLoggingMiddleware>();

// Health Checks [MPR]
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Minimal API Endpoints [SC][RA]

// Error-Endpunkt [ECP][NSF]
app.Map("/error", (HttpContext context) => {
    var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
    var exception = exceptionHandler?.Error;
    
    // Sichere Fehlerbehandlung ohne Preisgabe sensibler Informationen [ZTS]
    return Results.Problem(
        title: "Ein interner Serverfehler ist aufgetreten.",
        statusCode: 500,
        instance: context.Request.Path,
        detail: null // Keine Details in der Produktion [ZTS]
    );
});

// Status-Endpunkt (öffentlich)
app.MapGet("/api/v1/status", () => Results.Ok(new { Status = "Operational", Timestamp = DateTime.UtcNow }))
    .WithName("GetStatus")
    .WithOpenApi()
    .Produces<dynamic>(StatusCodes.Status200OK);

// Patienten-Endpunkte [PbD][EIV][ZTS]
var patientGroup = app.MapGroup("/api/v1/patients")
    .RequireAuthorization()
    .WithTags("Patients")
    .RequireRateLimiting("sensitive");

// GET /api/v1/patients - Liste aller Patienten (nur IDs und Versicherungsnummer-Hash) [PbD]
patientGroup.MapGet("/", async (SQLCipherContext db) =>
{
    var patients = await db.Patients
        .Select(p => new { p.Id, p.InsuranceNumberHash })
        .ToListAsync();
        
    return Results.Ok(patients);
})
.WithName("GetPatients")
.WithOpenApi()
.Produces<IEnumerable<dynamic>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);

// GET /api/v1/patients/{id} - Details eines Patienten (mit Audit-Log) [ATV]
patientGroup.MapGet("/{id:guid}", async (Guid id, SQLCipherContext db) =>
{
    var patient = await db.Patients.FindAsync(id);
    if (patient == null)
        return Results.NotFound();
        
    // Audit-Log für Zugriff erstellen [ATV]
    db.AuditLogs.Add(new AuditLog
    {
        Id = Guid.NewGuid(),
        EntityName = "Patient",
        EntityId = id.ToString(),
        Action = "READ",
        Changes = "Patient details accessed",
        ContainsSensitiveData = true,
        Timestamp = DateTime.UtcNow,
        UserId = "API User" // In echter Implementierung: aus dem Token extrahieren
    });
    await db.SaveChangesAsync();
    
    return Results.Ok(patient);
})
.WithName("GetPatientById")
.WithOpenApi()
.Produces<Patient>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status401Unauthorized);

// Sessions-Endpunkte [SK][EIV][ZTS]
var sessionGroup = app.MapGroup("/api/v1/sessions")
    .RequireAuthorization()
    .WithTags("Sessions")
    .RequireRateLimiting("sensitive");

// GET /api/v1/sessions - Liste aller Sessions (nur IDs und Datum) [PbD]
sessionGroup.MapGet("/", async (SQLCipherContext db) =>
{
    var sessions = await db.Sessions
        .Select(s => new { s.Id, s.SessionDate, s.Status, s.PatientId })
        .ToListAsync();
        
    return Results.Ok(sessions);
})
.WithName("GetSessions")
.WithOpenApi()
.Produces<IEnumerable<dynamic>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);

// GET /api/v1/sessions/{id} - Details einer Session [SK]
sessionGroup.MapGet("/{id:guid}", async (Guid id, SQLCipherContext db) =>
{
    var session = await db.Sessions.FindAsync(id);
    if (session == null)
        return Results.NotFound();
        
    // Audit-Log für Zugriff erstellen [ATV]
    db.AuditLogs.Add(new AuditLog
    {
        Id = Guid.NewGuid(),
        EntityName = "Session",
        EntityId = id.ToString(),
        Action = "READ",
        Changes = "Session details accessed",
        ContainsSensitiveData = true,
        Timestamp = DateTime.UtcNow,
        UserId = "API User" // In echter Implementierung: aus dem Token extrahieren
    });
    await db.SaveChangesAsync();
    
    return Results.Ok(session);
})
.WithName("GetSessionById")
.WithOpenApi()
.Produces<Session>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status401Unauthorized);

// Transkript-Endpunkte [AIU][EIV][ZTS]
var transcriptGroup = app.MapGroup("/api/v1/transcripts")
    .RequireAuthorization()
    .WithTags("Transcripts")
    .RequireRateLimiting("sensitive");

// GET /api/v1/transcripts/{id} - Details eines Transkripts [AIU]
transcriptGroup.MapGet("/{id:guid}", async (Guid id, SQLCipherContext db) =>
{
    var transcript = await db.Transcripts.FindAsync(id);
    if (transcript == null)
        return Results.NotFound();
        
    // Audit-Log für Zugriff erstellen [ATV]
    db.AuditLogs.Add(new AuditLog
    {
        Id = Guid.NewGuid(),
        EntityName = "Transcript",
        EntityId = id.ToString(),
        Action = "READ",
        Changes = "Transcript accessed",
        ContainsSensitiveData = true,
        Timestamp = DateTime.UtcNow,
        UserId = "API User" // In echter Implementierung: aus dem Token extrahieren
    });
    await db.SaveChangesAsync();
    
    return Results.Ok(transcript);
})
.WithName("GetTranscriptById")
.WithOpenApi()
.Produces<Transcript>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status401Unauthorized);

// Anonymisierungs-Review-Queue Endpunkte [ARQ][ZTS]
var reviewGroup = app.MapGroup("/api/v1/anonymization-reviews")
    .RequireAuthorization()
    .WithTags("AnonymizationReviews")
    .RequireRateLimiting("sensitive");

// GET /api/v1/anonymization-reviews - Liste aller Review-Items
reviewGroup.MapGet("/", async (SQLCipherContext db) =>
{
    var reviews = await db.Set<AnonymizationReviewItem>()
        .Where(r => r.Status == ReviewStatus.Pending)
        .Select(r => new { r.Id, r.TranscriptId, r.AnonymizationConfidence, r.Created })
        .ToListAsync();
        
    return Results.Ok(reviews);
})
.WithName("GetAnonymizationReviews")
.WithOpenApi()
.Produces<IEnumerable<dynamic>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);

app.MapControllers();

app.Run();

// Audit-Logging Middleware [ATV]
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Logge jeden API-Zugriff (ohne sensible Daten) [ATV]
        var username = context.User?.Identity?.Name ?? "Anonymous";
        var endpoint = context.Request.Path;
        var method = context.Request.Method;
        
        _logger.LogInformation(
            "API Access: {Method} {Endpoint} by {Username} at {Timestamp}",
            method, endpoint, username, DateTime.UtcNow);

        await _next(context);
    }
}
