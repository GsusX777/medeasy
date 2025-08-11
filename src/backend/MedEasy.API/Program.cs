// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using MedEasy.Application.Services;
using MedEasy.Application.Interfaces;
using MedEasy.Domain.Interfaces;
using MedEasy.Infrastructure.Services;
using MedEasy.Infrastructure.Repositories;
using MedEasy.Infrastructure.Configuration;
using MedEasy.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure request size limits for audio file uploads [WMM][PSF]
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB for large audio files
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB for large audio files
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
    options.ValueLengthLimit = int.MaxValue;
    options.ValueCountLimit = int.MaxValue;
    options.KeyLengthLimit = int.MaxValue;
});

// Register SystemPerformanceService [PSF][ZTS]
builder.Services.AddScoped<SystemPerformanceService>();

// Configure gRPC settings [MLB]
builder.Services.Configure<GrpcSettings>(builder.Configuration.GetSection(GrpcSettings.SectionName));

// Register WhisperService for gRPC communication [WMM][MLB]
builder.Services.AddScoped<IWhisperService, WhisperService>();

// Configure AES Encryption for text data [SP][AIU]
builder.Configuration["Encryption:AES_KEY"] = Environment.GetEnvironmentVariable("MEDEASY_AES_KEY") 
    ?? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("dev-aes-key-32-chars-for-testing")); // 32 chars = 256 bits

// Configure SQLCipher Database [SP][EIV]
builder.Services.Configure<SQLCipherOptions>(options =>
{
    // Use environment variable for production, fallback for development [SP]
    options.EncryptionKey = Environment.GetEnvironmentVariable("MEDEASY_DB_KEY") ?? "dev-key-not-for-production";
});

// Register SQLCipher Connection Factory [SP]
builder.Services.AddSingleton<ISqliteConnectionFactory, SQLCipherConnectionFactory>();

// Add Entity Framework with SQLCipher [SP][ATV]
builder.Services.AddDbContext<SQLCipherContext>(options =>
{
    var connectionString = "Data Source=medeasy.db";
    options.UseSqlite(connectionString, sqliteOptions =>
    {
        sqliteOptions.MigrationsAssembly("MedEasy.Infrastructure");
    });
});

// Register BenchmarkResultRepository for persistent benchmark storage [ATV][PSF]
builder.Services.AddScoped<IBenchmarkResultRepository, BenchmarkResultRepository>();

// Register AudioRecordRepository for encrypted audio storage [SP][EIV][ATV]
builder.Services.AddScoped<IAudioRecordRepository, AudioRecordRepository>();

// Register SessionRepository for consultation session management [SK][ATV]
builder.Services.AddScoped<ISessionRepository, SessionRepository>();

// Register TranscriptRepository for encrypted transcript storage [EIV][AIU][ATV]
builder.Services.AddScoped<ITranscriptRepository, TranscriptRepository>();

// Register EncryptionService for secure data handling [SP][AIU]
builder.Services.AddScoped<IEncryptionService, EncryptionService>();

// TODO: LiveTranscriptionService will be implemented from scratch
// builder.Services.AddScoped<ILiveTranscriptionService, LiveTranscriptionService>();

// Authentication services temporarily removed for Health-Monitor testing [ZTS]
// Will be re-enabled when SystemController Authorization is restored

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database [SP][ATV]
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SQLCipherContext>();
    try
    {
        // Ensure database is created and migrated [SP]
        context.Database.EnsureCreated();
        app.Logger.LogInformation("Database initialized successfully [SP]");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to initialize database [SP]");
        // Don't throw - allow app to start but log the error
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");
app.UseRouting();

// Enable multipart form data processing [WMM][PSF]
app.Use(async (context, next) =>
{
    if (context.Request.HasFormContentType)
    {
        // Enable form parsing for multipart requests
        await context.Request.ReadFormAsync();
    }
    await next();
});

// Authentication middleware temporarily disabled [ZTS]
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

app.Run();
