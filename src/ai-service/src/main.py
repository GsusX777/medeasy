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

import asyncio
import logging
import os
import time
from concurrent import futures
from datetime import datetime
from typing import Dict

import grpc
import structlog
import uvicorn
from fastapi import FastAPI, HTTPException, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from src.config import config
# serve_grpc is defined locally in this file

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


@app.get("/performance", tags=["admin"])
async def get_live_performance():
    """
    Live performance monitoring for Python AI Service process.
    [PSF] Critical for system reliability monitoring
    [WMM] Shows real-time Whisper processing impact
    """
    import psutil
    import os
    
    try:
        # Get all MedEasy Python processes [PSF][WMM]
        current_process = psutil.Process(os.getpid())
        all_medeasy_processes = []
        
        # Find all Python processes related to MedEasy
        for proc in psutil.process_iter(['pid', 'name', 'cmdline']):
            try:
                if (proc.info['name'] and 'python' in proc.info['name'].lower() and 
                    proc.info['cmdline'] and any('medeasy' in str(cmd).lower() for cmd in proc.info['cmdline'])):
                    all_medeasy_processes.append(psutil.Process(proc.info['pid']))
            except (psutil.NoSuchProcess, psutil.AccessDenied, psutil.ZombieProcess):
                pass
        
        # If no MedEasy processes found, use current process
        if not all_medeasy_processes:
            all_medeasy_processes = [current_process]
        
        # Calculate combined metrics for all MedEasy Python processes
        total_cpu_percent = sum(proc.cpu_percent(interval=0.02) for proc in all_medeasy_processes)
        cpu_cores = psutil.cpu_count(logical=False)
        normalized_cpu = total_cpu_percent / cpu_cores if cpu_cores > 0 else total_cpu_percent
        
        # Combined memory usage in MB
        total_memory_bytes = sum(proc.memory_info().rss for proc in all_medeasy_processes)
        memory_mb = total_memory_bytes / (1024 * 1024)  # Convert bytes to MB
        
        # Process count for debugging
        process_count = len(all_medeasy_processes)
        
        # System memory info
        system_memory = psutil.virtual_memory()
        total_memory_mb = system_memory.total / (1024 * 1024)
        
        # GPU info (if available)
        gpu_usage = 0
        gpu_memory_mb = 0
        try:
            import GPUtil
            gpus = GPUtil.getGPUs()
            if gpus:
                gpu = gpus[0]
                gpu_usage = gpu.load * 100
                gpu_memory_mb = gpu.memoryUsed
        except ImportError:
            pass
        
        return {
            "timestamp": time.time(),
            "process_id": os.getpid(),
            "cpu_usage_percent": round(normalized_cpu, 2),
            "memory_usage_mb": round(memory_mb, 2),
            "total_memory_mb": round(total_memory_mb, 2),
            "memory_percent": round((memory_mb / total_memory_mb) * 100, 2),
            "gpu_usage_percent": round(gpu_usage, 2),
            "gpu_memory_mb": round(gpu_memory_mb, 2),
            "cpu_cores": cpu_cores,
            "status": "active",
            "service": "python_ai_service",
            "medeasy_process_count": process_count,
            "measured_processes": [proc.pid for proc in all_medeasy_processes]
        }
    except Exception as e:
        logger.error("Failed to get performance metrics", error=str(e))
        return {
            "error": "Failed to retrieve performance metrics",
            "timestamp": time.time(),
            "status": "error"
        }

@app.get("/health", tags=["health"])
async def health_check() -> Dict[str, str]:
    """Health check endpoint."""
    # Check if critical services are available
    services_status = {
        "api": "healthy",
        "grpc": "unknown",  # Will be updated when gRPC server starts
        "whisper": "unknown",  # Will be checked on first request
    }
    
    # Convert services dict to string for FastAPI validation
    services_str = f"api:{services_status['api']}, grpc:{services_status['grpc']}, whisper:{services_status['whisper']}"
    
    return {
        "status": "healthy",
        "services": services_str,
        "environment": os.getenv("ENV", "development"),
        "timestamp": datetime.utcnow().isoformat() + "Z",
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


async def serve_grpc():
    """Start the async gRPC server [MLB]."""
    try:
        # Import gRPC modules (minimal schema)
        import medeasy_minimal_pb2_grpc as medeasy_pb2_grpc
        import medeasy_minimal_pb2 as medeasy_pb2
        from src.services.grpc_service_minimal import MedEasyServiceImpl
        
        # Create async gRPC server for WhisperService
        server = grpc.aio.server(futures.ThreadPoolExecutor(max_workers=config.grpc.max_workers))
        
        # Add MedEasy service
        medeasy_pb2_grpc.add_MedEasyServiceServicer_to_server(
            MedEasyServiceImpl(), server
        )
        
        # Listen on configured port
        listen_addr = f'[::]:{config.grpc.port}'
        server.add_insecure_port(listen_addr)
        
        # Start async server
        await server.start()
        logger.info(
            "gRPC server started",
            port=config.grpc.port,
            max_workers=config.grpc.max_workers
        )
        
        # Keep server running
        await server.wait_for_termination()
        
    except ImportError as e:
        logger.error(
            f"🚨 IMPORT ERROR in gRPC server: {str(e)}",
            error_type=type(e).__name__,
            error_details=str(e)
        )
        logger.warning(
            "gRPC service registration skipped - protobuf modules not generated yet",
            message="Run 'python -m grpc_tools.protoc -I./protos --python_out=. --grpc_python_out=. ./protos/medeasy.proto'"
        )
    except Exception as e:
        logger.error(f"🚨 FAILED to start gRPC server: {str(e)}", error=str(e), error_type=type(e).__name__)


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
    
    # Start async gRPC server as a background task [MLB]
    asyncio.create_task(serve_grpc())
    
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
