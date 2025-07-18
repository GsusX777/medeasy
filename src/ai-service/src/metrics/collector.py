# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Metrics collector for MedEasy AI service.

[ATV] Audit trail for all operations
[DSC] Swiss data protection compliance
[PK] Provider chain metrics
"""

import time
import threading
from typing import Dict, List, Optional, Any
import structlog

logger = structlog.get_logger()


class MetricsCollector:
    """
    Collects and provides metrics for the MedEasy AI service.
    
    [ATV] Audit trail for all operations
    [DSC] Swiss data protection compliance
    [PK] Provider chain metrics
    """
    
    def __init__(self):
        """Initialize metrics collector with thread-safe storage."""
        # Use thread locks for thread safety
        self._lock = threading.RLock()
        
        # Initialize metrics storage
        self._total_requests = 0
        self._start_time = time.time()
        
        # Provider metrics
        self._provider_metrics = {}
        
        # Anonymization metrics
        self._anonymization_metrics = {
            "total_entities": 0,
            "entities_needing_review": 0,
            "entities_reviewed": 0,
            "average_confidence": 0.0,
            "entity_types": {},
        }
        
        # Swiss German metrics
        self._swiss_german_metrics = {
            "detection_count": 0,
            "detected_count": 0,
            "average_confidence": 0.0,
            "medical_terms_detected": 0,
        }
        
        # Audit metrics
        self._audit_metrics = {
            "total_events": 0,
            "info_events": 0,
            "warning_events": 0,
            "error_events": 0,
        }
        
        logger.info("Metrics collector initialized")
    
    def increment_request_counter(self):
        """Increment the total request counter."""
        with self._lock:
            self._total_requests += 1
    
    def record_provider_request(self, provider_name: str, success: bool, latency_ms: float, is_cloud: bool):
        """
        Record a provider request.
        
        [PK] Provider chain metrics
        [CT] Cloud transparency
        
        Args:
            provider_name: Name of the provider
            success: Whether the request was successful
            latency_ms: Request latency in milliseconds
            is_cloud: Whether the provider is cloud-based
        """
        with self._lock:
            # Initialize provider metrics if not exists
            if provider_name not in self._provider_metrics:
                self._provider_metrics[provider_name] = {
                    "request_count": 0,
                    "success_count": 0,
                    "failure_count": 0,
                    "total_latency_ms": 0.0,
                    "average_latency_ms": 0.0,
                    "is_cloud_provider": is_cloud,
                }
            
            # Update metrics
            metrics = self._provider_metrics[provider_name]
            metrics["request_count"] += 1
            
            if success:
                metrics["success_count"] += 1
            else:
                metrics["failure_count"] += 1
            
            metrics["total_latency_ms"] += latency_ms
            metrics["average_latency_ms"] = metrics["total_latency_ms"] / metrics["request_count"]
    
    def record_anonymization_entity(self, entity_type: str, confidence: float, needs_review: bool):
        """
        Record an anonymization entity detection.
        
        [AIU] Anonymization is mandatory
        [ARQ] Anonymization review queue
        
        Args:
            entity_type: Type of entity detected
            confidence: Confidence score (0.0-1.0)
            needs_review: Whether the entity needs manual review
        """
        with self._lock:
            # Update total entities
            self._anonymization_metrics["total_entities"] += 1
            
            # Update entity types
            if entity_type not in self._anonymization_metrics["entity_types"]:
                self._anonymization_metrics["entity_types"][entity_type] = 0
            self._anonymization_metrics["entity_types"][entity_type] += 1
            
            # Update review queue
            if needs_review:
                self._anonymization_metrics["entities_needing_review"] += 1
            
            # Update average confidence
            total_confidence = (
                self._anonymization_metrics["average_confidence"] * 
                (self._anonymization_metrics["total_entities"] - 1) + 
                confidence
            )
            self._anonymization_metrics["average_confidence"] = (
                total_confidence / self._anonymization_metrics["total_entities"]
            )
    
    def record_entity_review(self, approved: bool):
        """
        Record an entity review decision.
        
        [ARQ] Anonymization review queue
        [ATV] Audit trail for all operations
        
        Args:
            approved: Whether the entity was approved
        """
        with self._lock:
            self._anonymization_metrics["entities_reviewed"] += 1
            if self._anonymization_metrics["entities_needing_review"] > 0:
                self._anonymization_metrics["entities_needing_review"] -= 1
    
    def increment_swiss_german_detection(self, is_detected: bool, confidence: float, medical_terms_count: int = 0):
        """
        Record a Swiss German detection.
        
        [SDH] Swiss German dialect handling
        [MFD] Swiss medical terminology
        
        Args:
            is_detected: Whether Swiss German was detected
            confidence: Confidence score (0.0-1.0)
            medical_terms_count: Number of Swiss medical terms detected
        """
        with self._lock:
            # Update detection count
            self._swiss_german_metrics["detection_count"] += 1
            
            # Update detected count if Swiss German was detected
            if is_detected:
                self._swiss_german_metrics["detected_count"] += 1
            
            # Update average confidence
            total_confidence = (
                self._swiss_german_metrics["average_confidence"] * 
                (self._swiss_german_metrics["detection_count"] - 1) + 
                confidence
            )
            self._swiss_german_metrics["average_confidence"] = (
                total_confidence / self._swiss_german_metrics["detection_count"]
            )
            
            # Update medical terms count
            self._swiss_german_metrics["medical_terms_detected"] += medical_terms_count
    
    def record_audit_event(self, level: str):
        """
        Record an audit event.
        
        [ATV] Audit trail for all operations
        
        Args:
            level: Log level (info, warning, error)
        """
        with self._lock:
            self._audit_metrics["total_events"] += 1
            
            if level.lower() == "info":
                self._audit_metrics["info_events"] += 1
            elif level.lower() == "warning":
                self._audit_metrics["warning_events"] += 1
            elif level.lower() == "error":
                self._audit_metrics["error_events"] += 1
    
    def get_total_requests(self) -> int:
        """Get the total number of requests processed."""
        with self._lock:
            return self._total_requests
    
    def get_provider_metrics(self) -> Dict[str, Dict[str, Any]]:
        """Get provider metrics."""
        with self._lock:
            # Return a deep copy to prevent modification
            return {k: v.copy() for k, v in self._provider_metrics.items()}
    
    def get_anonymization_metrics(self) -> Dict[str, Any]:
        """Get anonymization metrics."""
        with self._lock:
            # Return a deep copy to prevent modification
            result = self._anonymization_metrics.copy()
            result["entity_types"] = self._anonymization_metrics["entity_types"].copy()
            return result
    
    def get_swiss_german_metrics(self) -> Dict[str, Any]:
        """Get Swiss German metrics."""
        with self._lock:
            # Return a deep copy to prevent modification
            return self._swiss_german_metrics.copy()
    
    def get_audit_metrics(self) -> Dict[str, int]:
        """Get audit metrics."""
        with self._lock:
            # Return a deep copy to prevent modification
            return self._audit_metrics.copy()
    
    def reset_metrics(self):
        """Reset all metrics (for testing purposes only)."""
        with self._lock:
            self._total_requests = 0
            self._start_time = time.time()
            self._provider_metrics = {}
            self._anonymization_metrics = {
                "total_entities": 0,
                "entities_needing_review": 0,
                "entities_reviewed": 0,
                "average_confidence": 0.0,
                "entity_types": {},
            }
            self._swiss_german_metrics = {
                "detection_count": 0,
                "detected_count": 0,
                "average_confidence": 0.0,
                "medical_terms_detected": 0,
            }
            self._audit_metrics = {
                "total_events": 0,
                "info_events": 0,
                "warning_events": 0,
                "error_events": 0,
            }
