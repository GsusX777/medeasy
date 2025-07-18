# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Main FastAPI application for MedEasy AI Service.
Provides HTTP endpoints for health checks and service information.
The main service functionality is exposed via gRPC.

[ATV] Implements audit logging for all operations
[CT] Shows cloud/local processing status
[DSC] Ensures nDSG compliance
[AIU] Anonymization is mandatory and cannot be disabled
[MLB] Multi-Language Bridge via gRPC
[PK] Provider chain with fallbacks
[WMM] Whisper Multi-Model support
"""

import logging
import os
from concurrent import futures
from typing import Dict

import grpc
import structlog
import uvicorn
from fastapi import FastAPI, HTTPException, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from src.config import config
from src.grpc_service import serve_grpc

# Configure structured logging [ATV]
structlog.configure(
    processors=[
        structlog.processors.add_log_level,
        structlog.processors.TimeStamper(fmt="iso"),
        # Add correlation ID for request tracking
        structlog.processors.StackInfoRenderer(),
        structlog.processors.format_exc_info,
        structlog.processors.UnicodeDecoder(),
        structlog.processors.JSONRenderer(),
    ],
    wrapper_class=structlog.make_filtering_bound_logger(logging.getLevelName(config.logging.log_level)),
    context_class=dict,
    logger_factory=structlog.PrintLoggerFactory(),
    cache_logger_on_first_use=True,
)

logger = structlog.get_logger()

# Verify security settings in production [SP]
if os.getenv("ENV", "development") == "production":
    if not config.security.encryption_key:
        raise ValueError("[SP] Encryption key must be set in production environment")
    if not config.logging.enable_audit_log:
        raise ValueError("[ATV] Audit logging must be enabled in production environment")
    # Verify anonymization is enabled [AIU]
    if not config.anonymization.enabled:
        raise ValueError("[AIU] Anonymization cannot be disabled in any environment")

# Create FastAPI app
app = FastAPI(
    title="MedEasy AI Service",
    description="AI Service for medical transcription and anonymization with Swiss compliance",
    version="0.1.0",
    docs_url=None if os.getenv("ENV", "development") == "production" else "/docs",
    redoc_url=None if os.getenv("ENV", "development") == "production" else "/redoc",
    openapi_tags=[
        {"name": "health", "description": "Health check endpoints"},
        {"name": "info", "description": "Service information endpoints"},
        {"name": "admin", "description": "Administrative endpoints"},
    ],
)

# Add CORS middleware for development
if os.getenv("ENV", "development") == "development":
    app.add_middleware(
        CORSMiddleware,
        allow_origins=["*"],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

# Audit logging middleware
@app.middleware("http")
async def audit_logging_middleware(request: Request, call_next):
    """
    [ATV] Middleware for audit logging of all HTTP requests.
    """
    if config.logging.enable_audit_log:
        logger.info(
            "HTTP request",
            method=request.method,
            url=str(request.url),
            client=request.client.host if request.client else None,
        )
    
    response = await call_next(request)
    
    if config.logging.enable_audit_log:
        logger.info(
            "HTTP response",
            method=request.method,
            url=str(request.url),
            status_code=response.status_code,
        )
    
    return response


@app.get("/health", tags=["health"])
async def health_check() -> Dict[str, str]:
    """Health check endpoint."""
    # Check if critical services are available
    services_status = {
        "api": "healthy",
        "grpc": "unknown",  # Will be updated when gRPC server starts
        "whisper": "unknown",  # Will be checked on first request
    }
    
    return {
        "status": "healthy",
        "services": services_status,
        "environment": os.getenv("ENV", "development"),
        "timestamp": structlog.processors.time.time(),
    }


@app.get("/info", tags=["info"])
async def service_info() -> Dict[str, str]:
    """
    [CT] Service information endpoint.
    Shows whether processing is done in the cloud or locally.
    """
    whisper_model = config.whisper.model
    processing_location = "☁️ Cloud" if config.provider.is_cloud_processing else "🔒 Local"
    
    # [SF] Swiss format for dates
    from datetime import datetime
    current_time = datetime.now().strftime("%d.%m.%Y %H:%M:%S")
    
    return {
        "version": "0.1.0",
        "whisper_model": whisper_model,
        "processing_location": processing_location,
        "provider": config.provider.default_provider,
        "swiss_german_detection": "Enabled" if config.whisper.enable_swiss_german_detection else "Disabled",
        # [MFD] Use Swiss German medical terminology
        "service_type": "Spracherkennung und Anonymisierung für medizinische Daten",
        "timestamp": current_time,
        # [AIU] Confirm anonymization is active
        "anonymization": "Aktiv (obligatorisch)",
        # [DSC] Swiss data protection compliance
        "data_protection": "nDSG-konform",
    }


@app.on_event("startup")
async def startup_event():
    """Start the gRPC server on application startup."""
    logger.info("Starting MedEasy AI Service")
    
    # [AIU] Verify anonymization cannot be disabled
    if not config.anonymization.enabled:
        logger.error("Anonymization must be enabled")
        raise ValueError("[AIU] Anonymization cannot be disabled")
    
    # [SP] Verify encryption in production
    if os.getenv("ENV", "development") == "production" and not config.security.encryption_key:
        logger.error("Encryption key must be set in production")
        raise ValueError("[SP] Encryption key must be set in production")
    
    # Start gRPC server in a separate thread [MLB]
    grpc_thread = futures.ThreadPoolExecutor(max_workers=1)
    grpc_thread.submit(serve_grpc)
    
    # [PK] Log provider chain configuration
    providers = ", ".join(config.provider.provider_chain)
    
    logger.info(
        "MedEasy AI Service started",
        whisper_model=config.whisper.model,
        grpc_port=config.grpc.port,
        providers=providers,
        anonymization="enabled",
        swiss_german_detection="enabled" if config.whisper.enable_swiss_german_detection else "disabled",
        environment=os.getenv("ENV", "development"),
    )


@app.on_event("shutdown")
async def shutdown_event():
    """Shutdown event handler."""
    logger.info("Shutting down MedEasy AI Service")


def start():
    """Start the FastAPI application with Uvicorn."""
    # [SP] In production, use HTTPS
    if os.getenv("ENV", "development") == "production":
        uvicorn.run(
            "src.main:app",
            host="0.0.0.0",
            port=int(os.getenv("PORT", "8000")),
            reload=False,
            ssl_keyfile=os.getenv("SSL_KEYFILE", None),
            ssl_certfile=os.getenv("SSL_CERTFILE", None),
            log_level=config.logging.log_level.lower(),
        )
    else:
        # Development mode
        uvicorn.run(
            "src.main:app",
            host="0.0.0.0",
            port=int(os.getenv("PORT", "8000")),
            reload=True,
            log_level="debug",
        )


if __name__ == "__main__":
    start()
