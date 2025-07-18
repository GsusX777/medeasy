<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy AI Service

**Version:** 1.0.0  
**Letzte Aktualisierung:** 08.07.2025

## Übersicht [CAM][DSC]

Der MedEasy AI Service bietet sichere, datenschutzkonforme KI-Funktionen für die MedEasy-Anwendung, einschließlich Transkription, Textanalyse, Schweizerdeutsch-Erkennung und umfassender Metriken. Der Service ist als gRPC-Server implementiert und folgt den Clean Architecture-Prinzipien.

## Funktionen

### Kernfunktionen

- **Transkription** [AIU][SDH]: Audio-zu-Text mit Whisper, obligatorischer Anonymisierung und Schweizerdeutsch-Erkennung
- **Textanalyse** [PK][NDW]: Medizinische Textanalyse mit Provider-Kette und automatischen Fallbacks
- **Anonymisierung** [AIU][ARQ]: PII-Erkennung und Maskierung mit Review-Queue für unsichere Erkennungen
- **Schweizerdeutsch-Erkennung** [SDH][MFD]: Dialekterkennung mit Konfidenzwerten und medizinischer Terminologie
- **Gesundheitsüberwachung** [ATV]: Service-Status und Komponenten-Gesundheit mit System-Ressourcen
- **Metriken** [ATV][DSC]: Umfassende Dienstmetriken und Statistiken mit Audit-Trail

### Sicherheitsmerkmale

- **Verschlüsselung** [SP]: AES-256 für sensible Daten
- **Anonymisierung** [AIU]: Obligatorische PII-Erkennung und Maskierung
- **Audit-Trail** [ATV]: Vollständige Protokollierung aller Operationen
- **Cloud-Transparenz** [CT]: Klare Anzeige von Cloud- vs. lokaler Verarbeitung
- **Schweizer Datenschutz** [DSC]: Einhaltung des nDSG

## Installation

### Voraussetzungen

- Python 3.11+
- pip
- venv (empfohlen)
- CUDA-fähige GPU (optional, für schnellere Transkription)

### Einrichtung

1. Repository klonen:
   ```bash
   git clone https://github.com/GsusX777/medeasy.git
   cd medeasy/src/ai-service
   ```

2. Virtuelle Umgebung erstellen und aktivieren:
   ```bash
   python -m venv venv
   # Windows
   venv\Scripts\activate
   # Linux/macOS
   source venv/bin/activate
   ```

3. Abhängigkeiten installieren:
   ```bash
   pip install -r requirements.txt
   ```

4. Spacy-Modell herunterladen (falls nicht automatisch installiert):
   ```bash
   python -m spacy download de_core_news_lg
   ```

5. Umgebungsvariablen konfigurieren:
   ```bash
   # .env-Datei erstellen
   cp .env.example .env
   # .env-Datei bearbeiten und API-Schlüssel hinzufügen
   ```

## Konfiguration [DSC][AIU]

Die Konfiguration erfolgt über Umgebungsvariablen und die `.env`-Datei:

### Allgemeine Konfiguration

- `ENV`: Umgebung (`development` oder `production`)
- `ENCRYPTION_KEY`: Schlüssel für AES-256-Verschlüsselung (in Produktion erforderlich)
- `GRPC_SERVER_HOST`: Host für gRPC-Server (Standard: `0.0.0.0`)
- `GRPC_SERVER_PORT`: Port für gRPC-Server (Standard: `50051`)
- `GRPC_MAX_WORKERS`: Maximale Anzahl von gRPC-Workern (Standard: `4`)

### Whisper-Konfiguration

- `WHISPER_MODEL`: Whisper-Modellgröße (`tiny`, `base`, `small`, `medium`)
- `WHISPER_LOCAL_MODELS_PATH`: Pfad zu lokalen Whisper-Modellen
- `ENABLE_CUDA`: CUDA für Whisper aktivieren (`true`/`false`)

### Provider-Konfiguration

- `DEFAULT_PROVIDER`: Standard-AI-Provider (`openai`, `anthropic`, `google`, `local`)
- `FALLBACK_PROVIDERS`: Fallback-Provider (kommagetrennt)
- `OPENAI_API_KEY`: OpenAI API-Schlüssel
- `ANTHROPIC_API_KEY`: Anthropic API-Schlüssel
- `GOOGLE_API_KEY`: Google API-Schlüssel

### Schweizerdeutsch-Konfiguration

- `SWISS_GERMAN_ENABLED`: Schweizerdeutsch-Erkennung aktivieren (`true`/`false`)
- `SWISS_GERMAN_MIN_CONFIDENCE`: Minimale Konfidenz für Schweizerdeutsch-Erkennung
- `SWISS_GERMAN_MIN_MATCHES`: Minimale Anzahl von Dialektmarkern
- `SWISS_GERMAN_BETA_WARNING`: Beta-Warnung für Schweizerdeutsch anzeigen (`true`/`false`)
- `SWISS_GERMAN_MEDICAL_TERMS_ENABLED`: Schweizer medizinische Terminologie extrahieren (`true`/`false`)

### Metriken-Konfiguration

- `METRICS_ENABLED`: Metriken-Sammlung aktivieren (`true`/`false`)
- `METRICS_RETENTION_DAYS`: Anzahl der Tage für die Aufbewahrung von Metriken
- `METRICS_ANONYMIZE_IP`: IP-Adressen in Metriken anonymisieren (`true`/`false`)

### Logging-Konfiguration

- `LOG_LEVEL`: Log-Level (`DEBUG`, `INFO`, `WARNING`, `ERROR`)
- `ENABLE_AUDIT_LOG`: Audit-Logging aktivieren (`true`/`false`)

## Ausführung

### Server starten

```bash
python src/main.py
```

### Mit Docker

```bash
# Docker-Image bauen
docker build -t medeasy-ai-service .

# Container starten
docker run -p 50051:50051 --env-file .env medeasy-ai-service
```

## Tests [KP100]

### Unit-Tests ausführen

```bash
pytest
```

### Tests mit Coverage-Bericht

```bash
pytest --cov=src tests/
```

### Spezifische Tests ausführen

```bash
# Schweizerdeutsch-Tests
pytest tests/test_swiss_german_detector.py

# Metriken-Tests
pytest tests/test_metrics_collector.py

# gRPC-Service-Tests
pytest tests/test_grpc_service_health.py
pytest tests/test_grpc_service_swiss_german.py
pytest tests/test_grpc_service_metrics.py
```

## Projektstruktur [CAS]

```
src/ai-service/
├── protos/                  # gRPC Protobuf-Definitionen
│   └── medeasy.proto        # Service- und Nachrichtendefinitionen
├── src/
│   ├── metrics/             # Metriken-Komponenten
│   │   ├── __init__.py
│   │   └── collector.py     # Metriken-Sammler
│   ├── swiss/               # Schweizerdeutsch-Komponenten
│   │   ├── __init__.py
│   │   └── dialect_detector.py  # Schweizerdeutsch-Erkennung
│   ├── config.py            # Konfiguration
│   ├── grpc_service.py      # gRPC-Service-Implementierung
│   └── main.py              # Haupteinstiegspunkt
├── tests/                   # Unit-Tests
│   ├── test_anonymization.py
│   ├── test_swiss_german_detector.py
│   ├── test_metrics_collector.py
│   ├── test_grpc_service_health.py
│   ├── test_grpc_service_swiss_german.py
│   └── test_grpc_service_metrics.py
├── .env.example             # Beispiel-Umgebungsvariablen
├── Dockerfile               # Docker-Build-Konfiguration
├── pyproject.toml           # Python-Projekt-Konfiguration
└── requirements.txt         # Abhängigkeiten
```

## gRPC-Dienste [MLB]

Der Service bietet folgende gRPC-Methoden:

- `Transcribe`: Transkribiert Audio zu Text mit obligatorischer Anonymisierung
- `AnalyzeText`: Analysiert medizinischen Text mit KI-Provider-Kette
- `ReviewAnonymization`: Überprüft und genehmigt/lehnt Anonymisierungsentscheidungen ab
- `HealthCheck`: Überprüft den Dienststatus und die Gesundheit der Komponenten
- `DetectSwissGerman`: Erkennt Schweizerdeutsch-Dialekt in Texten
- `GetServiceMetrics`: Liefert Dienstmetriken und Audit-Trail-Statistiken

Detaillierte API-Dokumentation finden Sie in der [API-Referenz](../../docs/api/API_REFERENCE.md).

## Compliance [RA][DSC]

Dieser Service entspricht den folgenden Vorschriften:
- Schweizer nDSG (Datenschutzgesetz)
- DSGVO/GDPR (für EU-Kompatibilität)
- Medizinprodukteverordnung (MDR) für medizinische Software

## Projektregeln-Tags

- **[AIU]:** Anonymisierung ist UNVERÄNDERLICH (mandatory)
- **[SP]:** SQLCipher Pflicht (encryption requirement)
- **[ATV]:** Audit-Trail Vollständig (complete audit trail)
- **[CAS]:** Clean Architecture Struktur
- **[MLB]:** Multi-Language Bridge
- **[DSC]:** Datenschutz Schweiz
- **[SDH]:** Schweizerdeutsch-Handling
- **[SF]:** Schweizer Formate
- **[MFD]:** Medizinische Fachbegriffe DE-CH
- **[PK]:** Provider-Kette
- **[CT]:** Cloud-Transparenz
- **[KP100]:** Kritische Pfade 100% (test coverage)
- **[NDW]:** NIE Diagnose ohne Warnung
- **[RA]:** Regulatory Awareness
