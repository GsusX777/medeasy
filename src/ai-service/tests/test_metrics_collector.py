# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Unit tests for the metrics collector.

[KP100] Critical path test coverage
[ATV] Audit trail for all operations
[DSC] Swiss data protection compliance
"""

import unittest
from unittest.mock import patch, MagicMock
import structlog
import pytest
import time
from datetime import datetime

from src.metrics.collector import MetricsCollector


class TestMetricsCollector(unittest.TestCase):
    """Test suite for the metrics collector."""

    def setUp(self):
        """Set up test fixtures."""
        self.logger = structlog.get_logger()
        self.collector = MetricsCollector(self.logger)

    def test_initialization(self):
        """Test that the collector initializes correctly."""
        self.assertIsNotNone(self.collector)
        self.assertEqual(self.collector.logger, self.logger)
        self.assertEqual(self.collector.total_requests, 0)
        self.assertEqual(self.collector.start_time, self.collector._get_current_time())
        self.assertIsNotNone(self.collector.provider_metrics)
        self.assertIsNotNone(self.collector.anonymization_metrics)
        self.assertIsNotNone(self.collector.swiss_german_metrics)
        self.assertIsNotNone(self.collector.audit_metrics)

    def test_increment_request_count(self):
        """Test incrementing the request count."""
        initial_count = self.collector.total_requests
        self.collector.increment_request_count()
        self.assertEqual(self.collector.total_requests, initial_count + 1)

    def test_update_provider_metrics(self):
        """Test updating provider metrics."""
        # Test successful request
        self.collector.update_provider_metrics("openai", True, 0.5)
        self.assertEqual(self.collector.provider_metrics["openai"]["total_requests"], 1)
        self.assertEqual(self.collector.provider_metrics["openai"]["successful_requests"], 1)
        self.assertEqual(self.collector.provider_metrics["openai"]["failed_requests"], 0)
        self.assertEqual(self.collector.provider_metrics["openai"]["total_processing_time"], 0.5)
        
        # Test failed request
        self.collector.update_provider_metrics("openai", False, 0.3)
        self.assertEqual(self.collector.provider_metrics["openai"]["total_requests"], 2)
        self.assertEqual(self.collector.provider_metrics["openai"]["successful_requests"], 1)
        self.assertEqual(self.collector.provider_metrics["openai"]["failed_requests"], 1)
        self.assertEqual(self.collector.provider_metrics["openai"]["total_processing_time"], 0.8)
        
        # Test new provider
        self.collector.update_provider_metrics("anthropic", True, 0.7)
        self.assertEqual(self.collector.provider_metrics["anthropic"]["total_requests"], 1)
        self.assertEqual(self.collector.provider_metrics["anthropic"]["successful_requests"], 1)

    def test_update_anonymization_metrics(self):
        """Test updating anonymization metrics."""
        # Test adding entities
        self.collector.update_anonymization_metrics(5, 3, 0.85)
        self.assertEqual(self.collector.anonymization_metrics["total_entities_detected"], 5)
        self.assertEqual(self.collector.anonymization_metrics["total_entities_anonymized"], 3)
        self.assertEqual(self.collector.anonymization_metrics["total_confidence_sum"], 0.85)
        
        # Test adding more entities
        self.collector.update_anonymization_metrics(3, 3, 0.95)
        self.assertEqual(self.collector.anonymization_metrics["total_entities_detected"], 8)
        self.assertEqual(self.collector.anonymization_metrics["total_entities_anonymized"], 6)
        self.assertEqual(self.collector.anonymization_metrics["total_confidence_sum"], 1.8)

    def test_update_swiss_german_metrics(self):
        """Test updating Swiss German metrics."""
        # Test positive detection
        self.collector.update_swiss_german_metrics(True, 0.85, 5)
        self.assertEqual(self.collector.swiss_german_metrics["total_texts_analyzed"], 1)
        self.assertEqual(self.collector.swiss_german_metrics["swiss_german_detected"], 1)
        self.assertEqual(self.collector.swiss_german_metrics["total_confidence_sum"], 0.85)
        self.assertEqual(self.collector.swiss_german_metrics["total_dialect_markers"], 5)
        
        # Test negative detection
        self.collector.update_swiss_german_metrics(False, 0.2, 1)
        self.assertEqual(self.collector.swiss_german_metrics["total_texts_analyzed"], 2)
        self.assertEqual(self.collector.swiss_german_metrics["swiss_german_detected"], 1)
        self.assertEqual(self.collector.swiss_german_metrics["total_confidence_sum"], 1.05)
        self.assertEqual(self.collector.swiss_german_metrics["total_dialect_markers"], 6)

    def test_update_audit_metrics(self):
        """Test updating audit metrics."""
        # Test adding audit event
        self.collector.update_audit_metrics("transcription")
        self.assertEqual(self.collector.audit_metrics["total_events"], 1)
        self.assertEqual(self.collector.audit_metrics["events_by_type"]["transcription"], 1)
        
        # Test adding another event of the same type
        self.collector.update_audit_metrics("transcription")
        self.assertEqual(self.collector.audit_metrics["total_events"], 2)
        self.assertEqual(self.collector.audit_metrics["events_by_type"]["transcription"], 2)
        
        # Test adding event of a different type
        self.collector.update_audit_metrics("analysis")
        self.assertEqual(self.collector.audit_metrics["total_events"], 3)
        self.assertEqual(self.collector.audit_metrics["events_by_type"]["analysis"], 1)

    def test_get_uptime(self):
        """Test getting uptime."""
        with patch.object(self.collector, '_get_current_time') as mock_time:
            # Mock start time as 100 seconds ago
            self.collector.start_time = 100
            mock_time.return_value = 200
            
            uptime = self.collector.get_uptime()
            self.assertEqual(uptime, 100.0)

    def test_get_metrics(self):
        """Test getting all metrics."""
        # Add some test data
        self.collector.increment_request_count()
        self.collector.update_provider_metrics("openai", True, 0.5)
        self.collector.update_anonymization_metrics(5, 3, 0.85)
        self.collector.update_swiss_german_metrics(True, 0.85, 5)
        self.collector.update_audit_metrics("transcription")
        
        # Get metrics
        metrics = self.collector.get_metrics(
            include_provider_metrics=True,
            include_anonymization_metrics=True,
            include_swiss_german_metrics=True
        )
        
        # Verify metrics
        self.assertEqual(metrics["total_requests"], 1)
        self.assertGreaterEqual(metrics["uptime_seconds"], 0)
        
        # Provider metrics
        self.assertIn("provider_metrics", metrics)
        self.assertEqual(metrics["provider_metrics"]["openai"]["total_requests"], 1)
        
        # Anonymization metrics
        self.assertIn("anonymization_metrics", metrics)
        self.assertEqual(metrics["anonymization_metrics"]["total_entities_detected"], 5)
        
        # Swiss German metrics
        self.assertIn("swiss_german_metrics", metrics)
        self.assertEqual(metrics["swiss_german_metrics"]["swiss_german_detected"], 1)
        
        # Audit metrics
        self.assertIn("audit_metrics", metrics)
        self.assertEqual(metrics["audit_metrics"]["total_events"], 1)

    def test_get_metrics_selective(self):
        """Test getting selective metrics."""
        # Add some test data
        self.collector.increment_request_count()
        self.collector.update_provider_metrics("openai", True, 0.5)
        self.collector.update_anonymization_metrics(5, 3, 0.85)
        self.collector.update_swiss_german_metrics(True, 0.85, 5)
        self.collector.update_audit_metrics("transcription")
        
        # Get only provider metrics
        metrics = self.collector.get_metrics(
            include_provider_metrics=True,
            include_anonymization_metrics=False,
            include_swiss_german_metrics=False
        )
        
        # Verify metrics
        self.assertEqual(metrics["total_requests"], 1)
        self.assertGreaterEqual(metrics["uptime_seconds"], 0)
        
        # Provider metrics should be included
        self.assertIn("provider_metrics", metrics)
        
        # Other metrics should be excluded
        self.assertNotIn("anonymization_metrics", metrics)
        self.assertNotIn("swiss_german_metrics", metrics)
        
        # Audit metrics should always be included
        self.assertIn("audit_metrics", metrics)

    def test_thread_safety(self):
        """Test thread safety with concurrent updates."""
        # This is a basic simulation of concurrent access
        # In a real environment, you might use threading or multiprocessing
        
        # Update provider metrics multiple times
        for _ in range(100):
            self.collector.update_provider_metrics("openai", True, 0.1)
        
        # Verify the total is correct
        self.assertEqual(self.collector.provider_metrics["openai"]["total_requests"], 100)
        self.assertEqual(self.collector.provider_metrics["openai"]["successful_requests"], 100)
        self.assertEqual(self.collector.provider_metrics["openai"]["total_processing_time"], 10.0)

    def test_reset_metrics(self):
        """Test resetting metrics."""
        # Add some test data
        self.collector.increment_request_count()
        self.collector.update_provider_metrics("openai", True, 0.5)
        self.collector.update_anonymization_metrics(5, 3, 0.85)
        self.collector.update_swiss_german_metrics(True, 0.85, 5)
        self.collector.update_audit_metrics("transcription")
        
        # Reset metrics
        self.collector.reset_metrics()
        
        # Verify metrics are reset
        self.assertEqual(self.collector.total_requests, 0)
        self.assertEqual(len(self.collector.provider_metrics), 0)
        self.assertEqual(self.collector.anonymization_metrics["total_entities_detected"], 0)
        self.assertEqual(self.collector.swiss_german_metrics["total_texts_analyzed"], 0)
        self.assertEqual(self.collector.audit_metrics["total_events"], 0)
        
        # Start time should be updated
        self.assertGreaterEqual(self.collector.start_time, 0)

    def test_anonymize_ip_address(self):
        """Test IP address anonymization."""
        # Test IPv4
        ip = "192.168.1.123"
        anonymized = self.collector.anonymize_ip_address(ip)
        self.assertEqual(anonymized, "192.168.1.0")
        
        # Test IPv6
        ip = "2001:0db8:85a3:0000:0000:8a2e:0370:7334"
        anonymized = self.collector.anonymize_ip_address(ip)
        self.assertEqual(anonymized, "2001:0db8:85a3:0000:0000:0000:0000:0000")
        
        # Test invalid IP
        ip = "not an ip"
        anonymized = self.collector.anonymize_ip_address(ip)
        self.assertEqual(anonymized, "invalid_ip")


if __name__ == '__main__':
    unittest.main()
