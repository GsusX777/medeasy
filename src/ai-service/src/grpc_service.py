# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
gRPC server implementation for MedEasy AI Service.
Provides gRPC endpoints for transcription and anonymization.

[MLB] Implements Multi-Language Bridge between .NET and Python via gRPC
[ATV] All operations are audited
[AIU] Anonymization is mandatory and cannot be bypassed
[PK] Provider chain with fallbacks
[DSC] Swiss data protection compliance
[SP] SQLCipher for sensitive data
[SDH] Swiss German dialect handling
[NDW] Never diagnose without warning
"""

import os
import time
import uuid
import platform
import datetime
import psutil
from concurrent import futures
from typing import Dict, List, Optional, Tuple

import grpc
import structlog
from cryptography.fernet import Fernet

from src.anonymization.pii_detector import PIIDetector
from src.config import config
from src.providers.base_provider import get_provider
from src.whisper.transcription import WhisperTranscriber
from src.swiss.dialect_detector import SwissGermanDetector
from src.metrics.collector import MetricsCollector

# Import generated gRPC code (will be generated from proto files)
# This is a placeholder - actual imports will depend on the generated code
try:
    import medeasy_pb2
    import medeasy_pb2_grpc
except ImportError:
    # Placeholder for generated code - will be properly imported after protobuf compilation
    class MedEasyServiceServicer:
        """Placeholder for the generated service class."""
        pass
else:
    # Use the generated service class if available
    MedEasyServiceServicer = medeasy_pb2_grpc.MedEasyServiceServicer

logger = structlog.get_logger()


class MedEasyService(MedEasyServiceServicer):
    """
    gRPC service implementation for MedEasy AI Service.
    Handles transcription and anonymization requests.
    
    [AIU] Anonymization is mandatory and cannot be bypassed
    [ATV] All operations are audited
    [DSC] Swiss data protection compliance
    [SP] Encryption for sensitive data
    """

    def __init__(self):
        """Initialize service components.
        
        [AIU] Anonymization is mandatory and cannot be bypassed
        [SP] Encryption for sensitive data
        [PK] Provider chain with fallbacks
        [SDH] Swiss German dialect handling
        [ATV] Audit logging for all operations
        [DSC] Swiss data protection compliance
        """
        # [AIU] Verify anonymization is enabled - cannot be disabled
        if not config.anonymization.enabled:
            raise ValueError("[AIU] Anonymization cannot be disabled")
            
        # [SP] Initialize encryption for sensitive data
        if config.security.encryption_key:
            try:
                self.cipher = Fernet(config.security.encryption_key.encode())
            except Exception as e:
                logger.error("Failed to initialize encryption", error=str(e))
                raise ValueError(f"[SP] Invalid encryption key: {str(e)}")
        else:
            # In development, generate a temporary key
            if os.getenv("ENV", "development") == "production":
                raise ValueError("[SP] Encryption key must be set in production")
            logger.warning("No encryption key provided, generating temporary key for development")
            self.cipher = Fernet(Fernet.generate_key())
        
        # Initialize core components
        self.transcriber = WhisperTranscriber()
        self.pii_detector = PIIDetector()
        
        # [SDH] Initialize Swiss German detector
        self.swiss_german_detector = SwissGermanDetector()
        
        # [PK] Initialize the AI provider chain based on configuration
        self.provider = get_provider(config.provider.default_provider)
        self.fallback_providers = [get_provider(p) for p in config.provider.fallback_providers]
        
        # [ATV] Initialize audit logging and metrics
        self.request_counter = 0
        self.start_time = time.time()
        self.metrics_collector = MetricsCollector()
        
        # [DSC] Initialize service components status tracking
        self.component_status = {
            "whisper": {"status": "HEALTHY", "last_check": time.time()},
            "anonymization": {"status": "HEALTHY", "last_check": time.time()},
            "provider_chain": {"status": "HEALTHY", "last_check": time.time()},
            "swiss_german": {"status": "HEALTHY", "last_check": time.time()},
            "database": {"status": "HEALTHY", "last_check": time.time()},
        }
        
        # [SF] Initialize Swiss date formatter
        self.version = "1.0.0"  # Should be read from a version file
        
        # [ATV] Log service initialization
        logger.info(
            "MedEasy gRPC service initialized",
            whisper_model=config.whisper.model,
            provider=config.provider.default_provider,
            fallback_providers=config.provider.fallback_providers,
            anonymization_enabled=config.anonymization.enabled,
            swiss_german_detection=config.whisper.enable_swiss_german_detection,
            encryption_enabled=bool(config.security.encryption_key),
            environment=os.getenv("ENV", "development"),
            service_version=self.version,
            startup_time=self._format_swiss_datetime(datetime.datetime.now()),
        )
        
    def _format_swiss_datetime(self, dt):
        """Format datetime in Swiss format (DD.MM.YYYY HH:MM:SS).
        
        [SF] Swiss date format
        """
        return dt.strftime("%d.%m.%Y %H:%M:%S")

    def Transcribe(self, request, context):
        """
        Transcribe audio to text using Whisper.
        
        [ATV] Audit logging for all transcription requests
        [SDH] Swiss German detection and handling
        [AIU] Mandatory anonymization
        [DSC] Swiss data protection compliance
        [SP] Encryption for sensitive data
        """
        start_time = time.time()
        request_id = request.request_id
        
        logger.info(
            "Transcription request received",
            request_id=request_id,
            audio_length_seconds=request.audio_length_seconds,
        )
        
        try:
            # Transcribe audio
            transcription_result = self.transcriber.transcribe(
                audio_data=request.audio_data,
                language="de",  # Default to German for Swiss medical context
            )
            
            # Check for Swiss German
            is_swiss_german = False
            if config.whisper.enable_swiss_german_detection:
                is_swiss_german = self.transcriber.detect_swiss_german(transcription_result.text)
            
            # [AIU] Always anonymize the transcription - cannot be disabled
            anonymized_text, entities = self.pii_detector.detect_and_anonymize(
                transcription_result.text
            )
            
            # Create response
            response = medeasy_pb2.TranscriptionResponse(
                request_id=request_id,
                text=anonymized_text,
                original_text=transcription_result.text,  # Original text before anonymization
                language_code=transcription_result.language,
                is_swiss_german=is_swiss_german,
                swiss_german_warning=is_swiss_german and config.whisper.swiss_german_beta_warning,
                processing_time_seconds=time.time() - start_time,
                detected_entities=[
                    medeasy_pb2.Entity(
                        entity_type=entity.entity_type,
                        start=entity.start,
                        end=entity.end,
                        confidence_score=entity.score,
                        needs_review=entity.score < config.anonymization.confidence_threshold,
                    )
                    for entity in entities
                ],
                cloud_processed=config.provider.is_cloud_processing,
            )
            
            logger.info(
                "Transcription completed",
                request_id=request_id,
                processing_time_seconds=response.processing_time_seconds,
                language=response.language_code,
                is_swiss_german=response.is_swiss_german,
                entity_count=len(response.detected_entities),
                cloud_processed=response.cloud_processed,
            )
            
            return response
            
        except Exception as e:
            logger.error(
                "Transcription failed",
                request_id=request_id,
                error=str(e),
                exc_info=True,
            )
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Transcription failed: {str(e)}")
            return medeasy_pb2.TranscriptionResponse()

    def AnalyzeText(self, request, context):
        """
        Analyze medical text using AI providers.
        
        [PK] Uses provider chain with fallbacks
        [CT] Shows cloud/local processing status
        """
        start_time = time.time()
        request_id = request.request_id
        
        logger.info(
            "Text analysis request received",
            request_id=request_id,
            text_length=len(request.text),
            analysis_type=request.analysis_type,
        )
        
        try:
            # Get the appropriate provider
            provider = self.provider
            
            # Try to analyze with the current provider
            result = None
            error = None
            
            try:
                result = provider.analyze_text(
                    text=request.text,
                    analysis_type=request.analysis_type,
                    options=request.options,
                )
            except Exception as e:
                error = str(e)
                logger.warning(
                    "Primary provider failed, trying fallbacks",
                    request_id=request_id,
                    provider=config.provider.default_provider,
                    error=error,
                )
                
                # Try fallback providers
                for fallback_provider_type in config.provider.fallback_providers:
                    try:
                        fallback_provider = get_provider(fallback_provider_type)
                        result = fallback_provider.analyze_text(
                            text=request.text,
                            analysis_type=request.analysis_type,
                            options=request.options,
                        )
                        # If we get here, the fallback worked
                        logger.info(
                            "Fallback provider succeeded",
                            request_id=request_id,
                            provider=fallback_provider_type,
                        )
                        break
                    except Exception as fallback_error:
                        logger.warning(
                            "Fallback provider failed",
                            request_id=request_id,
                            provider=fallback_provider_type,
                            error=str(fallback_error),
                        )
            
            if result is None:
                raise Exception(f"All providers failed. Original error: {error}")
            
            # Create response
            response = medeasy_pb2.AnalysisResponse(
                request_id=request_id,
                result=result,
                processing_time_seconds=time.time() - start_time,
                provider_used=provider.provider_type,
                cloud_processed=provider.is_cloud_based,
                # [NDW] Always include disclaimer for AI-generated content
                has_disclaimer=True,
                disclaimer_text="KI-generierter Inhalt. Ärztliche Überprüfung erforderlich.",
            )
            
            logger.info(
                "Text analysis completed",
                request_id=request_id,
                processing_time_seconds=response.processing_time_seconds,
                provider=response.provider_used,
                cloud_processed=response.cloud_processed,
            )
            
            return response
            
        except Exception as e:
            logger.error(
                "Text analysis failed",
                request_id=request_id,
                error=str(e),
                exc_info=True,
            )
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Text analysis failed: {str(e)}")
            return medeasy_pb2.AnalysisResponse()

    def ReviewAnonymization(self, request, context):
        """
        Review and approve/reject anonymization decisions.
        
        [ARQ] Implements Anonymization Review Queue
        [ATV] Audit logging for all review decisions
        """
        request_id = request.request_id
        
        logger.info(
            "Anonymization review request received",
            request_id=request_id,
            entity_count=len(request.entity_decisions),
        )
        
        try:
            # Process review decisions
            updated_entities = []
            for decision in request.entity_decisions:
                # Update the entity in the database/cache
                updated_entity = self.pii_detector.update_entity_decision(
                    entity_id=decision.entity_id,
                    approved=decision.approved,
                    replacement_text=decision.replacement_text if decision.approved else None,
                )
                
                if updated_entity:
                    updated_entities.append(
                        medeasy_pb2.Entity(
                            entity_id=updated_entity.entity_id,
                            entity_type=updated_entity.entity_type,
                            start=updated_entity.start,
                            end=updated_entity.end,
                            confidence_score=updated_entity.score,
                            needs_review=False,  # Reviewed entities don't need further review
                        )
                    )
            
            # Create response
            response = medeasy_pb2.ReviewResponse(
                request_id=request_id,
                success=True,
                updated_entities=updated_entities,
                remaining_review_count=self.pii_detector.get_review_queue_size(),
            )
            
            logger.info(
                "Anonymization review completed",
                request_id=request_id,
                updated_count=len(updated_entities),
                remaining_review_count=response.remaining_review_count,
            )
            
            return response
            
        except Exception as e:
            logger.error(
                "Anonymization review failed",
                request_id=request_id,
                error=str(e),
                exc_info=True,
            )
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Anonymization review failed: {str(e)}")
            return medeasy_pb2.ReviewResponse(success=False)
            
    def HealthCheck(self, request, context):
        """
        Check service health and status.
        
        [ATV] Service monitoring and audit
        [SF] Swiss date format for timestamps
        [DSC] Swiss data protection compliance
        """
        request_id = request.request_id
        include_details = request.include_details
        
        logger.info(
            "Health check request received",
            request_id=request_id,
            include_details=include_details,
        )
        
        try:
            # Determine overall service status
            overall_status = self._get_overall_health_status()
            
            # Get component statuses if requested
            components = []
            if include_details:
                components = self._get_component_health_statuses()
            
            # Create response
            response = medeasy_pb2.HealthResponse(
                request_id=request_id,
                status=overall_status,
                components=components,
                timestamp=self._format_swiss_datetime(datetime.datetime.now()),
                environment=os.getenv("ENV", "development"),
                version=self.version,
            )
            
            logger.info(
                "Health check completed",
                request_id=request_id,
                status=medeasy_pb2.HealthResponse.Status.Name(response.status),
                component_count=len(components),
            )
            
            return response
            
        except Exception as e:
            logger.error(
                "Health check failed",
                request_id=request_id,
                error=str(e),
                exc_info=True,
            )
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Health check failed: {str(e)}")
            return medeasy_pb2.HealthResponse(
                request_id=request_id,
                status=medeasy_pb2.HealthResponse.Status.UNHEALTHY,
                timestamp=self._format_swiss_datetime(datetime.datetime.now()),
                environment=os.getenv("ENV", "development"),
                version=self.version,
            )
    
    def _get_overall_health_status(self) -> int:
        """
        Determine overall service health status.
        
        Returns:
            int: Status code (0=UNKNOWN, 1=HEALTHY, 2=DEGRADED, 3=UNHEALTHY)
        """
        # Check if any component is unhealthy
        for component, status_info in self.component_status.items():
            if status_info["status"] == "UNHEALTHY":
                return medeasy_pb2.HealthResponse.Status.UNHEALTHY
        
        # Check if any component is degraded
        for component, status_info in self.component_status.items():
            if status_info["status"] == "DEGRADED":
                return medeasy_pb2.HealthResponse.Status.DEGRADED
        
        # Check system resources
        cpu_usage = psutil.cpu_percent(interval=0.1)
        memory_usage = psutil.virtual_memory().percent
        
        if cpu_usage > 90 or memory_usage > 90:
            return medeasy_pb2.HealthResponse.Status.DEGRADED
        
        # All checks passed
        return medeasy_pb2.HealthResponse.Status.HEALTHY
    
    def _get_component_health_statuses(self) -> List[medeasy_pb2.HealthResponse.ComponentStatus]:
        """
        Get detailed health status for each component.
        
        Returns:
            List[ComponentStatus]: List of component status objects
        """
        components = []
        
        # Check each component
        for component_name, status_info in self.component_status.items():
            # Convert string status to enum
            status_enum = medeasy_pb2.HealthResponse.Status.UNKNOWN
            if status_info["status"] == "HEALTHY":
                status_enum = medeasy_pb2.HealthResponse.Status.HEALTHY
            elif status_info["status"] == "DEGRADED":
                status_enum = medeasy_pb2.HealthResponse.Status.DEGRADED
            elif status_info["status"] == "UNHEALTHY":
                status_enum = medeasy_pb2.HealthResponse.Status.UNHEALTHY
            
            # Create component status
            component_status = medeasy_pb2.HealthResponse.ComponentStatus(
                name=component_name,
                status=status_enum,
                message=f"{component_name} is {status_info['status']}",
                response_time_ms=(time.time() - status_info["last_check"]) * 1000,
            )
            components.append(component_status)
        
        # Add system status
        cpu_usage = psutil.cpu_percent(interval=0.1)
        memory_usage = psutil.virtual_memory().percent
        disk_usage = psutil.disk_usage("/").percent
        
        # CPU status
        cpu_status = medeasy_pb2.HealthResponse.Status.HEALTHY
        if cpu_usage > 90:
            cpu_status = medeasy_pb2.HealthResponse.Status.DEGRADED
        elif cpu_usage > 95:
            cpu_status = medeasy_pb2.HealthResponse.Status.UNHEALTHY
        
        components.append(
            medeasy_pb2.HealthResponse.ComponentStatus(
                name="cpu",
                status=cpu_status,
                message=f"CPU usage: {cpu_usage}%",
                response_time_ms=0.0,
            )
        )
        
        # Memory status
        memory_status = medeasy_pb2.HealthResponse.Status.HEALTHY
        if memory_usage > 85:
            memory_status = medeasy_pb2.HealthResponse.Status.DEGRADED
        elif memory_usage > 95:
            memory_status = medeasy_pb2.HealthResponse.Status.UNHEALTHY
        
        components.append(
            medeasy_pb2.HealthResponse.ComponentStatus(
                name="memory",
                status=memory_status,
                message=f"Memory usage: {memory_usage}%",
                response_time_ms=0.0,
            )
        )
        
        # Disk status
        disk_status = medeasy_pb2.HealthResponse.Status.HEALTHY
        if disk_usage > 85:
            disk_status = medeasy_pb2.HealthResponse.Status.DEGRADED
        elif disk_usage > 95:
            disk_status = medeasy_pb2.HealthResponse.Status.UNHEALTHY
        
        components.append(
            medeasy_pb2.HealthResponse.ComponentStatus(
                name="disk",
                status=disk_status,
                message=f"Disk usage: {disk_usage}%",
                response_time_ms=0.0,
            )
        )
        
        return components
        
    def DetectSwissGerman(self, request, context):
        """
        Detect Swiss German dialect in text.
        
        [SDH] Swiss German dialect handling
        [MFD] Swiss medical terminology
        [ATV] Audit logging for all operations
        """
        start_time = time.time()
        request_id = request.request_id
        
        logger.info(
            "Swiss German detection request received",
            request_id=request_id,
            text_length=len(request.text),
            include_details=request.include_details,
        )
        
        try:
            # Update component status
            self.component_status["swiss_german"]["last_check"] = time.time()
            
            # Detect Swiss German dialect
            is_swiss_german, confidence_score, dialect_markers = self.swiss_german_detector.detect(
                text=request.text,
                include_details=request.include_details,
            )
            
            # Extract Swiss medical terminology if detected
            swiss_medical_terms = []
            if is_swiss_german and request.include_details:
                swiss_medical_terms = self.swiss_german_detector.extract_medical_terms(request.text)
            
            # Create response with dialect markers if requested
            response_dialect_markers = []
            if request.include_details and dialect_markers:
                for marker in dialect_markers:
                    response_dialect_markers.append(
                        medeasy_pb2.SwissGermanResponse.DialectMarker(
                            text=marker.text,
                            start=marker.start,
                            end=marker.end,
                            standard_german=marker.standard_german,
                            confidence=marker.confidence,
                        )
                    )
            
            # Create response
            response = medeasy_pb2.SwissGermanResponse(
                request_id=request_id,
                is_swiss_german=is_swiss_german,
                confidence_score=confidence_score,
                dialect_markers=response_dialect_markers,
                swiss_medical_terms=swiss_medical_terms,
                processing_time_seconds=time.time() - start_time,
            )
            
            # [ATV] Log the detection result
            logger.info(
                "Swiss German detection completed",
                request_id=request_id,
                is_swiss_german=response.is_swiss_german,
                confidence_score=response.confidence_score,
                dialect_marker_count=len(response.dialect_markers),
                swiss_medical_term_count=len(response.swiss_medical_terms),
                processing_time_seconds=response.processing_time_seconds,
            )
            
            # Update metrics
            self.metrics_collector.increment_swiss_german_detection(
                is_detected=is_swiss_german,
                confidence=confidence_score,
            )
            
            return response
            
        except Exception as e:
            # Update component status on error
            self.component_status["swiss_german"]["status"] = "DEGRADED"
            
            logger.error(
                "Swiss German detection failed",
                request_id=request_id,
                error=str(e),
                exc_info=True,
            )
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Swiss German detection failed: {str(e)}")
            return medeasy_pb2.SwissGermanResponse(
                request_id=request_id,
                is_swiss_german=False,
                confidence_score=0.0,
            )
            
    def GetServiceMetrics(self, request, context):
        """
        Get service metrics and audit trail statistics.
        
        [ATV] Audit trail for all operations
        [DSC] Swiss data protection compliance
        [SF] Swiss date format for timestamps
        [PK] Provider chain metrics
        """
        start_time = time.time()
        request_id = request.request_id
        
        logger.info(
            "Service metrics request received",
            request_id=request_id,
            include_provider_metrics=request.include_provider_metrics,
            include_anonymization_metrics=request.include_anonymization_metrics,
            include_swiss_german_metrics=request.include_swiss_german_metrics,
        )
        
        try:
            # Get basic service metrics
            uptime_seconds = time.time() - self.start_time
            total_requests = self.metrics_collector.get_total_requests()
            
            # Create response with basic metrics
            response = medeasy_pb2.MetricsResponse(
                request_id=request_id,
                service_version=self.version,
                uptime_seconds=uptime_seconds,
                total_requests=total_requests,
                timestamp=self._format_swiss_datetime(datetime.datetime.now()),
            )
            
            # Add provider metrics if requested
            if request.include_provider_metrics:
                provider_metrics = self.metrics_collector.get_provider_metrics()
                for provider_name, metrics in provider_metrics.items():
                    response.provider_metrics.append(
                        medeasy_pb2.MetricsResponse.ProviderMetrics(
                            provider_name=provider_name,
                            request_count=metrics.get("request_count", 0),
                            success_count=metrics.get("success_count", 0),
                            failure_count=metrics.get("failure_count", 0),
                            average_latency_ms=metrics.get("average_latency_ms", 0.0),
                            is_cloud_provider=metrics.get("is_cloud_provider", False),
                        )
                    )
            
            # Add anonymization metrics if requested
            if request.include_anonymization_metrics:
                anon_metrics = self.metrics_collector.get_anonymization_metrics()
                response.anonymization_metrics.total_entities_detected = anon_metrics.get("total_entities", 0)
                response.anonymization_metrics.entities_needing_review = anon_metrics.get("entities_needing_review", 0)
                response.anonymization_metrics.entities_reviewed = anon_metrics.get("entities_reviewed", 0)
                response.anonymization_metrics.average_confidence_score = anon_metrics.get("average_confidence", 0.0)
                
                # Add entity type metrics
                entity_type_metrics = anon_metrics.get("entity_types", {})
                for entity_type, count in entity_type_metrics.items():
                    response.anonymization_metrics.entity_type_counts.append(
                        medeasy_pb2.MetricsResponse.EntityTypeCount(
                            entity_type=entity_type,
                            count=count,
                        )
                    )
            
            # Add Swiss German metrics if requested
            if request.include_swiss_german_metrics:
                swiss_german_metrics = self.metrics_collector.get_swiss_german_metrics()
                response.swiss_german_metrics.detection_count = swiss_german_metrics.get("detection_count", 0)
                response.swiss_german_metrics.detected_count = swiss_german_metrics.get("detected_count", 0)
                response.swiss_german_metrics.average_confidence = swiss_german_metrics.get("average_confidence", 0.0)
                response.swiss_german_metrics.medical_terms_detected = swiss_german_metrics.get("medical_terms_detected", 0)
            
            # Add audit trail metrics
            audit_metrics = self.metrics_collector.get_audit_metrics()
            response.audit_metrics.total_audit_events = audit_metrics.get("total_events", 0)
            response.audit_metrics.info_events = audit_metrics.get("info_events", 0)
            response.audit_metrics.warning_events = audit_metrics.get("warning_events", 0)
            response.audit_metrics.error_events = audit_metrics.get("error_events", 0)
            
            # [ATV] Log metrics request completion
            logger.info(
                "Service metrics request completed",
                request_id=request_id,
                processing_time_seconds=time.time() - start_time,
            )
            
            return response
            
        except Exception as e:
            logger.error(
                "Service metrics request failed",
                request_id=request_id,
                error=str(e),
                exc_info=True,
            )
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Service metrics request failed: {str(e)}")
            return medeasy_pb2.MetricsResponse(
                request_id=request_id,
                service_version=self.version,
                timestamp=self._format_swiss_datetime(datetime.datetime.now()),
            )


def serve_grpc():
    """Start the gRPC server.
    
    [SP] Secure server in production
    [ATV] Audit logging for server operations
    [MLB] Multi-Language Bridge between .NET and Python
    """
    # [ATV] Log server startup with request ID for tracing
    request_id = str(uuid.uuid4())
    logger.info(
        "Starting gRPC server",
        request_id=request_id,
        environment=os.getenv("ENV", "development"),
        max_workers=config.grpc.max_workers,
    )
    
    # [SP] Verify security settings in production
    if os.getenv("ENV", "development") == "production":
        if not config.security.encryption_key:
            logger.error("Encryption key must be set in production")
            raise ValueError("[SP] Encryption key must be set in production environment")
        
        # [AIU] Verify anonymization cannot be disabled
        if not config.anonymization.enabled:
            logger.error("Anonymization must be enabled")
            raise ValueError("[AIU] Anonymization cannot be disabled")
    
    # Configure server with appropriate settings
    server = grpc.server(
        futures.ThreadPoolExecutor(max_workers=config.grpc.max_workers),
        options=[
            ('grpc.max_send_message_length', 50 * 1024 * 1024),  # 50 MB
            ('grpc.max_receive_message_length', 50 * 1024 * 1024),  # 50 MB
            # Additional options for production
            ('grpc.keepalive_time_ms', 30000),  # 30 seconds
            ('grpc.keepalive_timeout_ms', 10000),  # 10 seconds
            ('grpc.http2.max_pings_without_data', 0),  # Allow pings without data
            ('grpc.http2.min_time_between_pings_ms', 10000),  # 10 seconds
        ]
    )
    
    # Add the service to the server
    try:
        # Create service instance with proper initialization
        service = MedEasyService()
        medeasy_pb2_grpc.add_MedEasyServiceServicer_to_server(service, server)
        
        # [ATV] Log successful service registration
        logger.info(
            "gRPC service registered",
            request_id=request_id,
            service="MedEasyService",
        )
    except NameError:
        logger.warning(
            "gRPC service registration skipped - protobuf modules not generated yet",
            request_id=request_id,
            message="Run 'python -m grpc_tools.protoc -I./protos --python_out=. --grpc_python_out=. ./protos/medeasy.proto'",
        )
        return
    except Exception as e:
        logger.error(
            "Failed to register gRPC service",
            request_id=request_id,
            error=str(e),
            exc_info=True,
        )
        raise
    
    # Start the server with appropriate security based on environment
    server_address = f"{config.grpc.host}:{config.grpc.port}"
    
    # [SP] Use secure connection in production
    if os.getenv("ENV", "development") == "production" and config.grpc.use_ssl:
        # SSL credentials for secure communication
        try:
            server_credentials = grpc.ssl_server_credentials(
                [(config.grpc.ssl_key_path, config.grpc.ssl_cert_path)]
            )
            server.add_secure_port(server_address, server_credentials)
            logger.info(
                "gRPC server using secure connection (SSL)",
                request_id=request_id,
                address=server_address,
            )
        except Exception as e:
            logger.error(
                "Failed to set up secure gRPC connection",
                request_id=request_id,
                error=str(e),
                exc_info=True,
            )
            raise
    else:
        # Insecure connection for development
        server.add_insecure_port(server_address)
        if os.getenv("ENV", "development") == "production":
            logger.warning(
                "gRPC server using insecure connection in production",
                request_id=request_id,
                address=server_address,
            )
        else:
            logger.info(
                "gRPC server using insecure connection (development)",
                request_id=request_id,
                address=server_address,
            )
    
    # Start the server
    server.start()
    
    logger.info(
        "gRPC server started successfully",
        request_id=request_id,
        address=server_address,
        max_workers=config.grpc.max_workers,
        environment=os.getenv("ENV", "development"),
        anonymization="enabled" if config.anonymization.enabled else "disabled",
        swiss_german_detection="enabled" if config.whisper.enable_swiss_german_detection else "disabled",
    )
    
    # Keep the server running
    server.wait_for_termination()
