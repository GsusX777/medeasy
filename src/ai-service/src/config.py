# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Configuration module for MedEasy AI Service.
Loads environment variables and provides configuration for all components.

[DSC] Ensures nDSG compliance with proper configuration
[AIU] Enforces that anonymization cannot be disabled
[CT] Implements Cloud Transparency
"""

import os
from enum import Enum
from pathlib import Path
from typing import List, Optional, Literal

from dotenv import load_dotenv
from pydantic import BaseModel, Field, validator

# Load environment variables from .env file
load_dotenv()


class WhisperModel(str, Enum):
    """Available Whisper model sizes for medical transcription [WMM]."""
    BASE = "base"
    SMALL = "small"
    MEDIUM = "medium"
    LARGE_V3 = "large-v3"  # Best accuracy for medical terminology


class ProviderType(str, Enum):
    """Available AI providers."""
    OPENAI = "openai"
    ANTHROPIC = "anthropic"
    GOOGLE = "google"
    LOCAL = "local"


class SecurityConfig(BaseModel):
    """Security configuration."""
    encryption_key: str = Field(
        default_factory=lambda: os.getenv("ENCRYPTION_KEY", ""),
        description="Encryption key for AES-256 encryption"
    )
    
    @validator("encryption_key")
    def validate_encryption_key(cls, v: str) -> str:
        """Validate that encryption key is set in production."""
        if not v and os.getenv("ENV", "development") == "production":
            raise ValueError("[SP] Encryption key must be set in production")
        return v


class WhisperConfig(BaseModel):
    """Whisper configuration."""
    model: WhisperModel = Field(
        default_factory=lambda: WhisperModel(os.getenv("WHISPER_MODEL", "small")),
        description="Whisper model to use"
    )
    local_models_path: Path = Field(
        default_factory=lambda: Path(os.getenv("WHISPER_LOCAL_MODELS_PATH", "./models")),
        description="Path to local Whisper models"
    )
    enable_cuda: bool = Field(
        default_factory=lambda: os.getenv("ENABLE_CUDA", "true").lower() == "true",
        description="Whether to use CUDA for Whisper"
    )
    enable_swiss_german_detection: bool = Field(
        default_factory=lambda: os.getenv("ENABLE_SWISS_GERMAN_DETECTION", "true").lower() == "true",
        description="Whether to enable Swiss German detection"
    )
    swiss_german_beta_warning: bool = Field(
        default_factory=lambda: os.getenv("SWISS_GERMAN_BETA_WARNING", "true").lower() == "true",
        description="Whether to show beta warning for Swiss German"
    )


class AnonymizationConfig(BaseModel):
    """Anonymization configuration."""
    confidence_threshold: float = Field(
        default_factory=lambda: float(os.getenv("ANONYMIZATION_CONFIDENCE_THRESHOLD", "0.8")),
        description="Confidence threshold for automatic anonymization"
    )
    review_queue_size: int = Field(
        default_factory=lambda: int(os.getenv("ANONYMIZATION_REVIEW_QUEUE_SIZE", "100")),
        description="Maximum size of the review queue"
    )
    
    # [AIU] Anonymization is MANDATORY and cannot be disabled
    enabled: Literal[True] = Field(
        default=True,
        description="Whether anonymization is enabled (always true, cannot be disabled)"
    )
    
    @validator("enabled")
    def validate_enabled(cls, v: bool) -> bool:
        """Validate that anonymization is always enabled."""
        if not v:
            raise ValueError("[AIU] Anonymization cannot be disabled")
        return True


class ProviderConfig(BaseModel):
    """Provider configuration."""
    default_provider: ProviderType = Field(
        default_factory=lambda: ProviderType(os.getenv("DEFAULT_PROVIDER", "openai")),
        description="Default AI provider"
    )
    fallback_providers: List[ProviderType] = Field(
        default_factory=lambda: [
            ProviderType(p.strip()) for p in os.getenv("FALLBACK_PROVIDERS", "anthropic,google,local").split(",")
            if p.strip()
        ],
        description="Fallback AI providers"
    )
    openai_api_key: Optional[str] = Field(
        default_factory=lambda: os.getenv("OPENAI_API_KEY", ""),
        description="OpenAI API key"
    )
    anthropic_api_key: Optional[str] = Field(
        default_factory=lambda: os.getenv("ANTHROPIC_API_KEY", ""),
        description="Anthropic API key"
    )
    google_api_key: Optional[str] = Field(
        default_factory=lambda: os.getenv("GOOGLE_API_KEY", ""),
        description="Google API key"
    )
    
    # [CT] Cloud transparency
    is_cloud_processing: bool = Field(
        default_factory=lambda: True,
        description="Whether processing is done in the cloud"
    )
    
    # [PK] Provider chain for fallback
    @property
    def provider_chain(self) -> List[str]:
        """Get the complete provider chain including default and fallbacks."""
        chain = [self.default_provider.value]
        chain.extend([p.value for p in self.fallback_providers])
        return chain
    
    @validator("is_cloud_processing")
    def validate_cloud_processing(cls, v: bool, values: dict) -> bool:
        """Determine if processing is done in the cloud based on provider."""
        if values.get("default_provider") == ProviderType.LOCAL:
            return False
        return True


class GrpcConfig(BaseModel):
    """gRPC server configuration."""
    host: str = Field(
        default_factory=lambda: os.getenv("GRPC_SERVER_HOST", "0.0.0.0"),
        description="gRPC server host"
    )
    port: int = Field(
        default_factory=lambda: int(os.getenv("GRPC_SERVER_PORT", "50051")),
        description="gRPC server port"
    )
    max_workers: int = Field(
        default_factory=lambda: int(os.getenv("GRPC_MAX_WORKERS", "4")),
        description="Maximum number of gRPC server workers"
    )


class SwissGermanConfig(BaseModel):
    """Swiss German dialect detection configuration.
    
    [SDH] Swiss German dialect handling
    [MFD] Swiss medical terminology
    """
    enabled: bool = Field(
        default_factory=lambda: os.getenv("SWISS_GERMAN_ENABLED", "true").lower() == "true",
        description="Whether Swiss German dialect detection is enabled"
    )
    min_confidence: float = Field(
        default_factory=lambda: float(os.getenv("SWISS_GERMAN_MIN_CONFIDENCE", "0.7")),
        description="Minimum confidence score for Swiss German detection"
    )
    min_matches: int = Field(
        default_factory=lambda: int(os.getenv("SWISS_GERMAN_MIN_MATCHES", "2")),
        description="Minimum number of dialect markers for Swiss German detection"
    )
    beta_warning: bool = Field(
        default_factory=lambda: os.getenv("SWISS_GERMAN_BETA_WARNING", "true").lower() == "true",
        description="Whether to show beta warning for Swiss German"
    )
    medical_terms_enabled: bool = Field(
        default_factory=lambda: os.getenv("SWISS_GERMAN_MEDICAL_TERMS_ENABLED", "true").lower() == "true",
        description="Whether to extract Swiss medical terminology"
    )


class MetricsConfig(BaseModel):
    """Metrics collection configuration.
    
    [ATV] Audit trail for all operations
    [DSC] Swiss data protection compliance
    """
    enabled: bool = Field(
        default_factory=lambda: os.getenv("METRICS_ENABLED", "true").lower() == "true",
        description="Whether metrics collection is enabled"
    )
    retention_days: int = Field(
        default_factory=lambda: int(os.getenv("METRICS_RETENTION_DAYS", "90")),
        description="Number of days to retain metrics data"
    )
    anonymize_ip_addresses: bool = Field(
        default_factory=lambda: os.getenv("METRICS_ANONYMIZE_IP", "true").lower() == "true",
        description="Whether to anonymize IP addresses in metrics"
    )
    
    @validator("enabled")
    def validate_metrics_enabled(cls, v: bool) -> bool:
        """Validate that metrics collection is always enabled in production."""
        if not v and os.getenv("ENV", "development") == "production":
            raise ValueError("[ATV] Metrics collection must be enabled in production")
        return v


class LoggingConfig(BaseModel):
    """Logging configuration.
    
    [ATV] Audit trail for all operations
    """
    log_level: str = Field(
        default_factory=lambda: os.getenv("LOG_LEVEL", "INFO"),
        description="Log level"
    )
    enable_audit_log: bool = Field(
        default_factory=lambda: os.getenv("ENABLE_AUDIT_LOG", "true").lower() == "true",
        description="Whether to enable audit logging"
    )
    
    @validator("enable_audit_log")
    def validate_audit_log(cls, v: bool) -> bool:
        """Validate that audit logging is always enabled in production."""
        if not v and os.getenv("ENV", "development") == "production":
            raise ValueError("[ATV] Audit logging must be enabled in production")
        return v


class AppConfig(BaseModel):
    """Main application configuration.
    
    [CAM] Clean Architecture Mandatory
    [DSC] Swiss data protection compliance
    [AIU] Anonymization is mandatory
    [ATV] Audit trail for all operations
    [SDH] Swiss German dialect handling
    [PK] Provider chain with fallbacks
    """
    security: SecurityConfig = Field(default_factory=SecurityConfig)
    whisper: WhisperConfig = Field(default_factory=WhisperConfig)
    anonymization: AnonymizationConfig = Field(default_factory=AnonymizationConfig)
    provider: ProviderConfig = Field(default_factory=ProviderConfig)
    swiss_german: SwissGermanConfig = Field(default_factory=SwissGermanConfig)
    metrics: MetricsConfig = Field(default_factory=MetricsConfig)
    grpc: GrpcConfig = Field(default_factory=GrpcConfig)
    logging: LoggingConfig = Field(default_factory=LoggingConfig)


# Create global config instance
config = AppConfig()
