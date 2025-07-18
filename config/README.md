<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Konfiguration [ZTS][NEA]

Dieses Verzeichnis enthält umgebungsspezifische Konfigurationen für MedEasy. Alle sensiblen Informationen wie API-Schlüssel, Verbindungszeichenfolgen und Geheimnisse werden über Umgebungsvariablen oder sichere Secret-Management-Systeme verwaltet, **niemals im Code oder in Konfigurationsdateien**. [NEA]

## Struktur

```
config/
│
├── development/              # Entwicklungsumgebung
│   ├── appsettings.json      # .NET-Konfiguration
│   ├── logging.json          # Logging-Konfiguration
│   └── ai-services.json      # KI-Dienste-Konfiguration
│
├── testing/                  # Testumgebung
│   ├── appsettings.json      # .NET-Konfiguration
│   ├── logging.json          # Logging-Konfiguration
│   └── ai-services.json      # KI-Dienste-Konfiguration
│
├── production/               # Produktionsumgebung
│   ├── appsettings.json      # .NET-Konfiguration
│   ├── logging.json          # Logging-Konfiguration
│   └── ai-services.json      # KI-Dienste-Konfiguration
│
└── templates/                # Konfigurationsvorlagen
    ├── appsettings.template.json      # Vorlage für .NET-Konfiguration
    ├── logging.template.json          # Vorlage für Logging-Konfiguration
    ├── ai-services.template.json      # Vorlage für KI-Dienste-Konfiguration
    └── .env.template                  # Vorlage für Umgebungsvariablen
```

## Umgebungsvariablen [NEA]

Sensible Informationen werden über Umgebungsvariablen bereitgestellt. Eine `.env`-Datei kann für die lokale Entwicklung verwendet werden, sollte aber **niemals** in das Repository eingecheckt werden.

Beispiel für eine `.env`-Datei:

```
# Datenbank
DB_CONNECTION_STRING=Host=localhost;Database=medeasy;Username=dev;Password=******
DB_ENCRYPTION_KEY=******

# API-Schlüssel
OPENAI_API_KEY=******
AZURE_API_KEY=******

# JWT
JWT_SECRET=******
JWT_ISSUER=medeasy-dev
JWT_AUDIENCE=medeasy-client
JWT_EXPIRY_HOURS=8

# Logging
LOG_LEVEL=Debug
```

## Konfigurationsdateien

### appsettings.json

Enthält allgemeine Anwendungseinstellungen:

```json
{
  "Application": {
    "Name": "MedEasy",
    "Version": "1.0.0",
    "Environment": "Development"
  },
  "Cors": {
    "AllowedOrigins": ["https://localhost:5173"]
  },
  "RateLimiting": {
    "EnableRateLimiting": true,
    "PermitLimit": 100,
    "Window": "00:01:00"
  }
}
```

### logging.json

Konfiguriert das Logging-Verhalten:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "./logs/medeasy-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### ai-services.json

Konfiguriert die KI-Dienste:

```json
{
  "Whisper": {
    "DefaultModel": "medium",
    "AvailableModels": ["tiny", "base", "small", "medium"],
    "Language": "de",
    "UseGPU": true,
    "BatchSize": 16
  },
  "Anonymization": {
    "ConfidenceThreshold": 0.8,
    "ReviewQueueThreshold": 0.8,
    "EnabledByDefault": true
  },
  "ProviderChain": [
    {
      "Name": "OpenAI",
      "Priority": 1,
      "Timeout": 30
    },
    {
      "Name": "Claude",
      "Priority": 2,
      "Timeout": 30
    },
    {
      "Name": "Gemini",
      "Priority": 3,
      "Timeout": 30
    },
    {
      "Name": "Local",
      "Priority": 4,
      "Timeout": 60
    }
  ]
}
```

## Umgebungsspezifische Konfiguration

- **Development**: Ausführliche Logs, Entwicklungsfunktionen aktiviert
- **Testing**: Minimale Logs, In-Memory-Datenbank für Tests
- **Production**: Optimierte Einstellungen, strenge Sicherheitsrichtlinien

## Secret Management [ZTS][NEA]

In Produktionsumgebungen werden Secrets über sichere Secret-Management-Systeme verwaltet:

- **Entwicklung**: .env-Datei (nicht eingecheckt)
- **Test**: Azure Key Vault / HashiCorp Vault
- **Produktion**: Azure Key Vault / HashiCorp Vault

## Konfigurationsvalidierung

Alle Konfigurationen werden beim Anwendungsstart validiert, um sicherzustellen, dass alle erforderlichen Einstellungen vorhanden sind.

[DM][D=C] Diese Dokumentation muss bei Änderungen der Konfigurationsstruktur aktualisiert werden.
