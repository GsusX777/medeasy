<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

# MedEasy AI Service Architecture

**Version:** 1.0.0  
**Last Updated:** 08.07.2025  
**Status:** Active

## Overview

The MedEasy AI Service provides secure, compliant AI capabilities for the MedEasy application, including transcription, analysis, and Swiss German dialect detection. It follows the Clean Architecture principles and adheres to Swiss data protection requirements.

## Service Components

### gRPC Service

The gRPC service is the main entry point for the AI Service, providing a secure, high-performance interface for the MedEasy backend to communicate with the AI components.

#### Service Methods

| Method | Description | Project Rules |
|--------|-------------|--------------|
| `Transcribe` | Transcribes audio to text with mandatory anonymization | [AIU], [SP], [ATV], [SDH] |
| `AnalyzeText` | Analyzes medical text using AI provider chain | [PK], [CT], [NDW] |
| `ReviewAnonymization` | Reviews and approves/rejects anonymization decisions | [ARQ], [ATV] |
| `HealthCheck` | Checks service health and status | [ATV], [SF], [DSC] |
| `DetectSwissGerman` | Detects Swiss German dialect in text | [SDH], [MFD], [ATV] |
| `GetServiceMetrics` | Gets service metrics and audit trail statistics | [ATV], [DSC], [SF], [PK] |

### Core Components

#### Whisper Transcriber

Handles audio transcription using OpenAI's Whisper model with support for multiple model sizes and local/offline processing.

- **Features:**
  - Swiss German dialect detection
  - Multi-model support (tiny, base, small, medium)
  - Local model fallback for offline operation
  - CUDA acceleration when available

#### PII Detector and Anonymizer

Provides mandatory anonymization for all patient data, with no option to disable.

- **Features:**
  - Entity detection and anonymization
  - Review queue for low-confidence detections
  - Audit logging for all anonymization decisions
  - Support for Swiss medical terminology

#### Swiss German Dialect Detector

Specialized component for detecting and handling Swiss German dialect in medical contexts.

- **Features:**
  - Dialect detection with confidence scoring
  - Swiss medical terminology extraction
  - Standard German equivalents for dialect terms
  - Beta warning for Swiss German processing

#### AI Provider Chain

Implements a provider chain pattern for AI services with automatic fallbacks.

- **Providers:**
  1. OpenAI (primary)
  2. Claude/Anthropic (fallback)
  3. Google (fallback)
  4. Local models (offline fallback)

- **Features:**
  - Automatic fallback on provider failure
  - Cloud transparency indicators
  - Metrics collection for provider performance
  - Medical disclaimer for AI-generated content

#### Metrics Collector

Collects and provides metrics for the AI service with full audit trail.

- **Metrics Types:**
  - Provider metrics (usage, success rate, latency)
  - Anonymization metrics (entities, reviews, confidence)
  - Swiss German metrics (detection rate, confidence)
  - Audit trail metrics (events by type)

## Security and Compliance

### Data Protection

- **Encryption:** AES-256 encryption for sensitive data
- **Anonymization:** Mandatory PII detection and anonymization
- **Audit Trail:** Complete logging of all operations
- **Cloud Transparency:** Clear indication of cloud vs. local processing

### Swiss Compliance

- **nDSG Compliance:** Adheres to Swiss data protection law
- **Date Format:** DD.MM.YYYY HH:MM:SS
- **Medical Terminology:** Support for Swiss German medical terms
- **Dialect Handling:** Detection and warning for Swiss German

## Dependencies

| Component | Version | Purpose | Project Rules |
|-----------|---------|---------|--------------|
| gRPC | 1.54.0+ | Service communication | [MLB] |
| Whisper | 20230314+ | Audio transcription | [WMM] |
| Pydantic | 2.0.0+ | Configuration validation | [FSD] |
| Cryptography | 41.0.0+ | Data encryption | [SP] |
| Structlog | 23.1.0+ | Structured logging | [ATV] |
| Psutil | 5.9.0+ | System metrics | [PB] |

## Configuration

The service uses a hierarchical configuration system with environment variable overrides:

- **Security:** Encryption keys, SSL certificates
- **Whisper:** Model selection, CUDA support
- **Anonymization:** Confidence thresholds, review queue
- **Provider Chain:** API keys, fallback order
- **Swiss German:** Detection settings, medical terminology
- **Metrics:** Collection settings, retention policy
- **gRPC:** Server settings, performance tuning
- **Logging:** Log levels, audit trail

## Deployment

The AI service is deployed as a standalone gRPC server that can be run:

1. As a local service
2. As a systemd service
3. Directly from Python (development only)

## Whisper Model Integration [WMM]

The AI service provides comprehensive Whisper model support with benchmarking capabilities:

### Supported Models (faster-whisper)
| Model | Size | VRAM | Performance | Accuracy | Use Case |
|-------|------|------|-------------|----------|----------|
| **base** | 142MB | 1GB | ⚡⚡ | ⭐⭐⭐⭐⭐ | Standard medical use |
| **small** | 466MB | 2GB | ⚡ | ⭐⭐⭐⭐⭐⭐⭐ | High accuracy |
| **medium** | 1500MB | 5GB | ⚡ | ⭐⭐⭐⭐⭐⭐⭐⭐⭐ | Professional use |
| **large-v3** | 2900MB | 10GB | ⚡ | ⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐ | **Medical terminology** [MFD] |

### Benchmarking Features [PB]
- **Performance Testing:** Real-time transcription speed measurement
- **Hardware Analysis:** CUDA/CPU utilization monitoring
- **Quality Metrics:** Word Error Rate (WER) calculation
- **Swiss German Detection:** Dialect recognition and warnings [SDH]
- **Medical Terminology:** Specialized vocabulary support [MFD]

### New API Endpoints [WMM]
- `POST /api/v1/ai/benchmark-models` - Multi-model performance testing
- `GET /api/v1/ai/available-models` - Model availability and recommendations
- `GET /api/v1/ai/hardware-info` - System capability analysis
- `POST /api/v1/ai/transcribe` - Enhanced with model selection

## Integration

The AI service integrates with the MedEasy backend via gRPC, providing a secure, high-performance interface for AI capabilities.

```
MedEasy.API <-> MedEasy.AI.Client <-> gRPC <-> AI Service
```

## Metrics and Monitoring

The service provides comprehensive metrics and monitoring through:

1. Health check endpoint for service status
2. Metrics endpoint for detailed statistics
3. Structured logging with audit trail
4. Component-level health status

## Project Rule Tags

- **[AIU]:** Anonymization is UNVERÄNDERLICH (mandatory)
- **[SP]:** SQLCipher Pflicht (encryption requirement)
- **[ATV]:** Audit-Trail Vollständig (complete audit trail)
- **[DK]:** Diagnose-Killswitch (diagnosis disclaimer)
- **[CAS]:** Clean Architecture Struktur
- **[MLB]:** Multi-Language Bridge
- **[TSF]:** Technologie-Stack Fest
- **[DSC]:** Datenschutz Schweiz
- **[SDH]:** Schweizerdeutsch-Handling
- **[SF]:** Schweizer Formate
- **[MFD]:** Medizinische Fachbegriffe DE-CH
- **[PK]:** Provider-Kette
- **[WMM]:** Whisper Multi-Model
- **[CT]:** Cloud-Transparenz
- **[EIV]:** Entitäten Immer Verschlüsselt
- **[KP100]:** Kritische Pfade 100% (test coverage)
- **[NDW]:** NIE Diagnose ohne Warnung
