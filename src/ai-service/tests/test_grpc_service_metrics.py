# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Unit tests for the MedEasyService GetServiceMetrics method.

[KP100] Critical path test coverage
[ATV] Audit trail for all operations
[DSC] Swiss data protection compliance
[SF] Swiss date format
[PK] Provider chain metrics
"""

import unittest
from unittest.mock import patch, MagicMock
import structlog
import pytest
import grpc
import time
from datetime import datetime

import medeasy_pb2
from src.grpc_service import MedEasyService


class TestMedEasyServiceMetrics(unittest.TestCase):
    """Test suite for the MedEasyService GetServiceMetrics method."""

    def setUp(self):
        """Set up test fixtures."""
        self.logger = structlog.get_logger()
        
        # Mock dependencies
        self.mock_pii_detector = MagicMock()
        self.mock_whisper = MagicMock()
        self.mock_provider_chain = MagicMock()
        self.mock_swiss_german_detector = MagicMock()
        self.mock_metrics_collector = MagicMock()
        
        # Create service instance with mocked dependencies
        self.service = MedEasyService(
            logger=self.logger,
            pii_detector=self.mock_pii_detector,
            whisper=self.mock_whisper,
            provider_chain=self.mock_provider_chain,
            swiss_german_detector=self.mock_swiss_german_detector,
            metrics_collector=self.mock_metrics_collector
        )
        
        # Mock metrics data
        self.mock_metrics = {
            "total_requests": 1000,
            "uptime_seconds": 3600.5,
            "timestamp": "08.07.2025 10:30:45",
            "provider_metrics": {
                "openai": {
                    "total_requests": 800,
                    "successful_requests": 780,
                    "failed_requests": 20,
                    "total_processing_time": 450.5,
                    "average_processing_time": 0.56
                },
                "anthropic": {
                    "total_requests": 150,
                    "successful_requests": 145,
                    "failed_requests": 5,
                    "total_processing_time": 90.2,
                    "average_processing_time": 0.60
                },
                "local": {
                    "total_requests": 50,
                    "successful_requests": 50,
                    "failed_requests": 0,
                    "total_processing_time": 25.0,
                    "average_processing_time": 0.50
                }
            },
            "anonymization_metrics": {
                "total_entities_detected": 5000,
                "total_entities_anonymized": 4800,
                "total_entities_reviewed": 200,
                "average_confidence": 0.85
            },
            "swiss_german_metrics": {
                "total_texts_analyzed": 500,
                "swiss_german_detected": 150,
                "average_confidence": 0.78,
                "total_dialect_markers": 750
            },
            "audit_metrics": {
                "total_events": 2500,
                "events_by_type": {
                    "transcription": 500,
                    "analysis": 800,
                    "anonymization_review": 200,
                    "health_check": 600,
                    "swiss_german_detection": 400
                }
            }
        }

    def test_get_service_metrics_all(self):
        """Test getting all service metrics."""
        # Mock metrics collector to return test metrics
        self.mock_metrics_collector.get_metrics.return_value = self.mock_metrics
        
        # Create request
        request = medeasy_pb2.MetricsRequest(
            request_id="test-123",
            include_provider_metrics=True,
            include_anonymization_metrics=True,
            include_swiss_german_metrics=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.GetServiceMetrics(request, context)
        
        # Verify response
        self.assertEqual(response.request_id, "test-123")
        self.assertEqual(response.service_version, "1.0.0")
        self.assertEqual(response.uptime_seconds, 3600.5)
        self.assertEqual(response.total_requests, 1000)
        self.assertIn(".", response.timestamp)  # Swiss date format DD.MM.YYYY
        
        # Verify provider metrics
        self.assertEqual(len(response.provider_metrics), 3)
        openai_metric = next(m for m in response.provider_metrics if m.provider_name == "openai")
        self.assertEqual(openai_metric.total_requests, 800)
        self.assertEqual(openai_metric.successful_requests, 780)
        self.assertEqual(openai_metric.failed_requests, 20)
        self.assertEqual(openai_metric.average_processing_time, 0.56)
        
        # Verify anonymization metrics
        self.assertEqual(response.anonymization_metrics.total_entities_detected, 5000)
        self.assertEqual(response.anonymization_metrics.total_entities_anonymized, 4800)
        self.assertEqual(response.anonymization_metrics.total_entities_reviewed, 200)
        self.assertEqual(response.anonymization_metrics.average_confidence, 0.85)
        
        # Verify Swiss German metrics
        self.assertEqual(response.swiss_german_metrics.total_texts_analyzed, 500)
        self.assertEqual(response.swiss_german_metrics.swiss_german_detected, 150)
        self.assertEqual(response.swiss_german_metrics.average_confidence, 0.78)
        self.assertEqual(response.swiss_german_metrics.total_dialect_markers, 750)
        
        # Verify audit metrics
        self.assertEqual(response.audit_metrics.total_events, 2500)
        self.assertEqual(len(response.audit_metrics.events_by_type), 5)
        self.assertEqual(response.audit_metrics.events_by_type["transcription"], 500)
        self.assertEqual(response.audit_metrics.events_by_type["analysis"], 800)
        
        # Verify metrics collector was called correctly
        self.mock_metrics_collector.get_metrics.assert_called_once_with(
            include_provider_metrics=True,
            include_anonymization_metrics=True,
            include_swiss_german_metrics=True
        )
        self.mock_metrics_collector.increment_request_count.assert_called_once()
        self.mock_metrics_collector.update_audit_metrics.assert_called_once_with("get_metrics")

    def test_get_service_metrics_selective(self):
        """Test getting selective service metrics."""
        # Create a copy of metrics with only provider metrics
        selective_metrics = {
            "total_requests": 1000,
            "uptime_seconds": 3600.5,
            "timestamp": "08.07.2025 10:30:45",
            "provider_metrics": self.mock_metrics["provider_metrics"],
            "audit_metrics": self.mock_metrics["audit_metrics"]
        }
        
        # Mock metrics collector to return selective metrics
        self.mock_metrics_collector.get_metrics.return_value = selective_metrics
        
        # Create request
        request = medeasy_pb2.MetricsRequest(
            request_id="test-456",
            include_provider_metrics=True,
            include_anonymization_metrics=False,
            include_swiss_german_metrics=False
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.GetServiceMetrics(request, context)
        
        # Verify response
        self.assertEqual(response.request_id, "test-456")
        self.assertEqual(response.total_requests, 1000)
        
        # Provider metrics should be included
        self.assertEqual(len(response.provider_metrics), 3)
        
        # Other metrics should not be included
        self.assertEqual(response.anonymization_metrics.total_entities_detected, 0)
        self.assertEqual(response.swiss_german_metrics.total_texts_analyzed, 0)
        
        # Audit metrics should always be included
        self.assertEqual(response.audit_metrics.total_events, 2500)
        
        # Verify metrics collector was called correctly
        self.mock_metrics_collector.get_metrics.assert_called_once_with(
            include_provider_metrics=True,
            include_anonymization_metrics=False,
            include_swiss_german_metrics=False
        )

    def test_get_service_metrics_empty(self):
        """Test getting metrics when collector returns empty data."""
        # Mock metrics collector to return minimal data
        empty_metrics = {
            "total_requests": 0,
            "uptime_seconds": 0.0,
            "timestamp": "08.07.2025 10:30:45",
            "audit_metrics": {
                "total_events": 0,
                "events_by_type": {}
            }
        }
        self.mock_metrics_collector.get_metrics.return_value = empty_metrics
        
        # Create request
        request = medeasy_pb2.MetricsRequest(
            request_id="test-789",
            include_provider_metrics=True,
            include_anonymization_metrics=True,
            include_swiss_german_metrics=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.GetServiceMetrics(request, context)
        
        # Verify response
        self.assertEqual(response.request_id, "test-789")
        self.assertEqual(response.total_requests, 0)
        self.assertEqual(response.uptime_seconds, 0.0)
        self.assertEqual(len(response.provider_metrics), 0)
        self.assertEqual(response.anonymization_metrics.total_entities_detected, 0)
        self.assertEqual(response.swiss_german_metrics.total_texts_analyzed, 0)
        self.assertEqual(response.audit_metrics.total_events, 0)

    def test_get_service_metrics_error_handling(self):
        """Test error handling in GetServiceMetrics."""
        # Mock metrics collector to raise exception
        self.mock_metrics_collector.get_metrics.side_effect = Exception("Test error")
        
        # Create request
        request = medeasy_pb2.MetricsRequest(
            request_id="test-101",
            include_provider_metrics=True,
            include_anonymization_metrics=True,
            include_swiss_german_metrics=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method and expect exception to be handled
        response = self.service.GetServiceMetrics(request, context)
        
        # Verify error response
        self.assertEqual(response.request_id, "test-101")
        self.assertEqual(response.total_requests, 0)  # Default value on error
        
        # Context should have error set
        context.set_code.assert_called_once_with(grpc.StatusCode.INTERNAL)
        context.set_details.assert_called_once()
        
        # Logger should have recorded the error
        self.logger.error.assert_called()

    def test_swiss_date_format(self):
        """Test Swiss date format in response."""
        # Mock metrics with timestamp
        metrics_with_timestamp = {
            "total_requests": 100,
            "uptime_seconds": 3600.0,
            "timestamp": "08.07.2025 10:30:45",
            "audit_metrics": {
                "total_events": 100,
                "events_by_type": {"test": 100}
            }
        }
        self.mock_metrics_collector.get_metrics.return_value = metrics_with_timestamp
        
        # Create request
        request = medeasy_pb2.MetricsRequest(
            request_id="test-202",
            include_provider_metrics=False,
            include_anonymization_metrics=False,
            include_swiss_german_metrics=False
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.GetServiceMetrics(request, context)
        
        # Verify Swiss date format (DD.MM.YYYY)
        date_parts = response.timestamp.split(' ')[0].split('.')
        self.assertEqual(len(date_parts), 3)
        
        # Day should be 1-31
        day = int(date_parts[0])
        self.assertTrue(1 <= day <= 31)
        
        # Month should be 1-12
        month = int(date_parts[1])
        self.assertTrue(1 <= month <= 12)
        
        # Year should be current year
        year = int(date_parts[2])
        self.assertEqual(year, 2025)  # From the mock data

    def test_provider_metrics_details(self):
        """Test provider metrics details in response."""
        # Mock metrics collector to return test metrics
        self.mock_metrics_collector.get_metrics.return_value = self.mock_metrics
        
        # Create request
        request = medeasy_pb2.MetricsRequest(
            request_id="test-303",
            include_provider_metrics=True,
            include_anonymization_metrics=False,
            include_swiss_german_metrics=False
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.GetServiceMetrics(request, context)
        
        # Verify provider metrics details
        provider_names = [m.provider_name for m in response.provider_metrics]
        self.assertIn("openai", provider_names)
        self.assertIn("anthropic", provider_names)
        self.assertIn("local", provider_names)
        
        # Check specific provider details
        local_metric = next(m for m in response.provider_metrics if m.provider_name == "local")
        self.assertEqual(local_metric.total_requests, 50)
        self.assertEqual(local_metric.successful_requests, 50)
        self.assertEqual(local_metric.failed_requests, 0)
        self.assertEqual(local_metric.success_rate, 1.0)  # 100% success rate
        self.assertEqual(local_metric.average_processing_time, 0.5)


if __name__ == '__main__':
    unittest.main()
