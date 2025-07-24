<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy Desktop-Konfiguration [ZTS][NEA][TSF]

Diese Dokumentation beschreibt die Konfiguration der MedEasy Desktop-Anwendung. Als Desktop-App verwendet MedEasy eine vereinfachte Konfigurationsstruktur mit lokalen Dateien und Umgebungsvariablen. Alle sensiblen Informationen werden über Umgebungsvariablen verwaltet, **niemals im Code oder in Konfigurationsdateien**. [NEA]

## Desktop-Konfigurationsstruktur [TSF]

```
MedEasy Desktop-Konfiguration:
├── Backend (.NET):
│   └── src/backend/MedEasy.API/appsettings.json
├── AI-Service (Python):
│   ├── src/ai-service/.env
│   └── src/ai-service/.env.example
├── Frontend (Svelte):
│   └── src/frontend/src/app.html
└── Desktop (Tauri):
    └── src/frontend/src-tauri/tauri.conf.json
```

## 1. Backend-Konfiguration (.NET) [SP][ZTS]

### Datei: `src/backend/MedEasy.API/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Database": {
    "ConnectionString": "Data Source=medeasy.db",
    "EncryptionKey": "${MEDEASY_DB_KEY}"
  },
  "JWT": {
    "SecretKey": "${JWT_SECRET}",
    "Issuer": "MedEasy",
    "Audience": "MedEasy-Desktop",
    "ExpirationHours": 8
  }
}
```

### Umgebungsvariablen für Backend:
```bash
# SQLCipher Datenbankschlüssel [SP]
MEDEASY_DB_KEY=base64-encoded-32-byte-key

# JWT Secret für Session-Management
JWT_SECRET=your-jwt-secret-key

# Entwicklungsmodus (optional)
ASPNETCORE_ENVIRONMENT=Development
```

## 2. AI-Service Konfiguration (Python) [PK][SDH][AIU]

### Datei: `src/ai-service/.env`

```bash
# Umgebung
ENV=development

# Verschlüsselung [SP]
ENCRYPTION_KEY=base64-encoded-32-byte-key

# gRPC Server
GRPC_SERVER_HOST=127.0.0.1
GRPC_SERVER_PORT=50051
GRPC_MAX_WORKERS=4

# Whisper Konfiguration [WMM]
WHISPER_MODEL=base
WHISPER_LOCAL_MODELS_PATH=./models
ENABLE_CUDA=false

# Provider-Kette [PK]
DEFAULT_PROVIDER=openai
FALLBACK_PROVIDERS=anthropic,google,local
OPENAI_API_KEY=your-openai-key
ANTHROPIC_API_KEY=your-anthropic-key
GOOGLE_API_KEY=your-google-key

# Schweizerdeutsch [SDH]
SWISS_GERMAN_ENABLED=true
SWISS_GERMAN_MIN_CONFIDENCE=0.7
SWISS_GERMAN_MIN_MATCHES=3
SWISS_GERMAN_BETA_WARNING=true
SWISS_GERMAN_MEDICAL_TERMS_ENABLED=true

# Anonymisierung [AIU]
ANONYMIZATION_ENABLED=true
ANONYMIZATION_CONFIDENCE_THRESHOLD=0.8

# Metriken [ATV]
METRICS_ENABLED=true
METRICS_RETENTION_DAYS=30
METRICS_ANONYMIZE_IP=true

# Logging [ATV]
LOG_LEVEL=INFO
ENABLE_AUDIT_LOG=true
```

### Beispieldatei: `src/ai-service/.env.example`
Enthält alle Konfigurationsoptionen mit Beispielwerten (ohne echte Secrets).

## 3. Desktop-Konfiguration (Tauri) [TSF][ZTS]

### Datei: `src/frontend/src-tauri/tauri.conf.json`

```json
{
  "package": {
    "productName": "MedEasy",
    "version": "0.1.0"
  },
  "tauri": {
    "allowlist": {
      "all": false,
      "shell": {
        "all": false,
        "open": true
      },
      "dialog": {
        "all": true
      },
      "fs": {
        "all": true,
        "scope": ["$APP/*"]
      }
    },
    "security": {
      "csp": "default-src 'self'; connect-src 'self' http://127.0.0.1:*"
    },
    "windows": [
      {
        "title": "MedEasy",
        "width": 1024,
        "height": 768,
        "minWidth": 800,
        "minHeight": 600,
        "resizable": true,
        "fullscreen": false
      }
    ]
  }
}
```

## 4. Frontend-Konfiguration (Svelte) [TSF]

### Datei: `src/frontend/src/app.html`

```html
<!DOCTYPE html>
<html lang="de-CH">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>MedEasy</title>
    %sveltekit.head%
</head>
<body data-sveltekit-preload-data="hover">
    <div style="display: contents">%sveltekit.body%</div>
</body>
</html>
```

## Desktop-spezifische Konfigurationsaspekte [TSF][ZTS]

### Lokale Datenpfade
```bash
# Windows
%APPDATA%\MedEasy\
├── medeasy.db          # SQLCipher Datenbank
├── audio\              # Audio-Dateien
└── logs\               # Log-Dateien

# macOS
~/Library/Application Support/MedEasy/

# Linux
~/.local/share/MedEasy/
```

### Sicherheitskonfiguration [ZTS][SP]

#### Tauri Security Model
- **Allowlist**: Nur explizit erlaubte APIs verfügbar
- **CSP**: Content Security Policy für WebView
- **File System**: Beschränkter Zugriff auf App-Verzeichnis
- **Network**: Nur localhost-Verbindungen erlaubt

#### Datenbankschlüssel [SP]
```bash
# Schlüsselgenerierung für SQLCipher
openssl rand -base64 32

# Beispiel (NICHT in Produktion verwenden):
MEDEASY_DB_KEY=AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=
```

## Umgebungsspezifische Einstellungen

### Entwicklung
```bash
# Backend
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://127.0.0.1:5000

# AI-Service
ENV=development
LOG_LEVEL=DEBUG
WHISPER_MODEL=tiny  # Schneller für Entwicklung
```

### Produktion
```bash
# Backend
ASPNETCORE_ENVIRONMENT=Production

# AI-Service
ENV=production
LOG_LEVEL=INFO
WHISPER_MODEL=base  # Bessere Qualität
ENABLE_CUDA=true    # Falls GPU verfügbar
```

## Secret Management [ZTS][NEA]

### Entwicklung
- **Lokale .env-Dateien** (nicht in Git eingecheckt)
- **Umgebungsvariablen** in IDE/Terminal
- **Entwicklungsschlüssel** (nicht für Produktion)

### Produktion
- **Betriebssystem-Keystore** (Windows Credential Manager, macOS Keychain)
- **Umgebungsvariablen** beim App-Start
- **Sichere Schlüsselgenerierung** mit kryptographisch sicheren Zufallszahlen

## Konfigurationsvalidierung [ZTS]

Alle Konfigurationen werden beim Anwendungsstart validiert:

### Backend (.NET)
```csharp
// Validierung in Program.cs
if (string.IsNullOrEmpty(dbKey))
    throw new InvalidOperationException("MEDEASY_DB_KEY ist erforderlich");
```

### AI-Service (Python)
```python
# Validierung in config.py
if not os.getenv('ENCRYPTION_KEY'):
    raise ValueError("ENCRYPTION_KEY ist erforderlich")
```

## Schweizer Compliance [DSC][SF]

### Datenschutz-Konfiguration
- **Lokale Datenverarbeitung**: Standardmäßig aktiviert
- **Cloud-Transparenz**: Explizite Kennzeichnung [CT]
- **Anonymisierung**: Immer aktiviert [AIU]
- **Audit-Trail**: Vollständige Protokollierung [ATV]

### Schweizer Formate [SF]
```json
{
  "locale": "de-CH",
  "dateFormat": "DD.MM.YYYY",
  "timeFormat": "HH:mm",
  "currency": "CHF",
  "insuranceNumberFormat": "XXX.XXXX.XXXX.XX"
}
```

## Troubleshooting

### Häufige Konfigurationsprobleme

1. **Datenbankschlüssel fehlt**
   ```
   Fehler: MEDEASY_DB_KEY ist nicht gesetzt
   Lösung: Umgebungsvariable setzen oder .env-Datei erstellen
   ```

2. **AI-Service nicht erreichbar**
   ```
   Fehler: gRPC-Verbindung fehlgeschlagen
   Lösung: AI-Service starten und Port 50051 prüfen
   ```

3. **Tauri-Allowlist-Fehler**
   ```
   Fehler: API nicht erlaubt
   Lösung: tauri.conf.json Allowlist erweitern
   ```

## Projektregeln-Tags

- **[ZTS]:** Zero Tolerance Security
- **[NEA]:** No Environment Assumptions
- **[TSF]:** Technologie-Stack Fest
- **[SP]:** SQLCipher Pflicht
- **[PK]:** Provider-Kette
- **[SDH]:** Schweizerdeutsch-Handling
- **[AIU]:** Anonymisierung ist UNVERÄNDERLICH
- **[ATV]:** Audit-Trail Vollständig
- **[CT]:** Cloud-Transparenz
- **[DSC]:** Datenschutz Schweiz
- **[SF]:** Schweizer Formate
- **[WMM]:** Whisper Multi-Model

---

[D=C][DSU] Diese Dokumentation muss bei Änderungen der Konfigurationsstruktur aktualisiert werden.
