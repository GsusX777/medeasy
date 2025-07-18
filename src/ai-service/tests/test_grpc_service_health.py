# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Unit tests for the MedEasyService HealthCheck method.

[KP100] Critical path test coverage
[ATV] Audit trail for all operations
[SF] Swiss date format
[DSC] Swiss data protection compliance
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


class TestMedEasyServiceHealth(unittest.TestCase):
    """Test suite for the MedEasyService HealthCheck method."""

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
        
        # Mock component statuses
        self.service.component_status = {
            "whisper": {"status": "healthy", "last_check": time.time(), "details": "OK"},
            "pii_detector": {"status": "healthy", "last_check": time.time(), "details": "OK"},
            "provider_chain": {"status": "healthy", "last_check": time.time(), "details": "OK"},
            "swiss_german_detector": {"status": "healthy", "last_check": time.time(), "details": "OK"},
            "database": {"status": "healthy", "last_check": time.time(), "details": "OK"}
        }

    def test_health_check_basic(self):
        """Test basic health check without details."""
        # Create request
        request = medeasy_pb2.HealthRequest(
            request_id="test-123",
            include_details=False
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.HealthCheck(request, context)
        
        # Verify response
        self.assertEqual(response.request_id, "test-123")
        self.assertEqual(response.status, medeasy_pb2.Status.HEALTHY)
        self.assertEqual(len(response.components), 0)  # No details requested
        self.assertIn(".", response.timestamp)  # Swiss date format DD.MM.YYYY
        self.assertIsNotNone(response.environment)
        self.assertEqual(response.version, "1.0.0")
        
        # Verify metrics were updated
        self.mock_metrics_collector.increment_request_count.assert_called_once()
        self.mock_metrics_collector.update_audit_metrics.assert_called_once_with("health_check")

    def test_health_check_with_details(self):
        """Test health check with component details."""
        # Create request
        request = medeasy_pb2.HealthRequest(
            request_id="test-456",
            include_details=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.HealthCheck(request, context)
        
        # Verify response
        self.assertEqual(response.request_id, "test-456")
        self.assertEqual(response.status, medeasy_pb2.Status.HEALTHY)
        self.assertEqual(len(response.components), 5)  # All components
        
        # Verify component details
        component_names = [comp.name for comp in response.components]
        self.assertIn("whisper", component_names)
        self.assertIn("pii_detector", component_names)
        self.assertIn("provider_chain", component_names)
        self.assertIn("swiss_german_detector", component_names)
        self.assertIn("database", component_names)
        
        # All components should be healthy
        for comp in response.components:
            self.assertEqual(comp.status, medeasy_pb2.Status.HEALTHY)

    def test_health_check_degraded(self):
        """Test health check with degraded component."""
        # Set one component to degraded
        self.service.component_status["whisper"] = {
            "status": "degraded", 
            "last_check": time.time(), 
            "details": "High latency"
        }
        
        # Create request
        request = medeasy_pb2.HealthRequest(
            request_id="test-789",
            include_details=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.HealthCheck(request, context)
        
        # Verify response
        self.assertEqual(response.status, medeasy_pb2.Status.DEGRADED)
        
        # Find the whisper component
        whisper_comp = next(comp for comp in response.components if comp.name == "whisper")
        self.assertEqual(whisper_comp.status, medeasy_pb2.Status.DEGRADED)
        self.assertEqual(whisper_comp.details, "High latency")

    def test_health_check_unhealthy(self):
        """Test health check with unhealthy component."""
        # Set one component to unhealthy
        self.service.component_status["database"] = {
            "status": "unhealthy", 
            "last_check": time.time(), 
            "details": "Connection failed"
        }
        
        # Create request
        request = medeasy_pb2.HealthRequest(
            request_id="test-101",
            include_details=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.HealthCheck(request, context)
        
        # Verify response
        self.assertEqual(response.status, medeasy_pb2.Status.UNHEALTHY)
        
        # Find the database component
        db_comp = next(comp for comp in response.components if comp.name == "database")
        self.assertEqual(db_comp.status, medeasy_pb2.Status.UNHEALTHY)
        self.assertEqual(db_comp.details, "Connection failed")

    @patch('src.grpc_service.psutil.cpu_percent')
    @patch('src.grpc_service.psutil.virtual_memory')
    @patch('src.grpc_service.psutil.disk_usage')
    def test_health_check_system_resources(self, mock_disk, mock_memory, mock_cpu):
        """Test health check includes system resources."""
        # Mock system resource values
        mock_cpu.return_value = 75.0
        mock_memory.return_value = MagicMock(percent=80.0)
        mock_disk.return_value = MagicMock(percent=50.0)
        
        # Create request
        request = medeasy_pb2.HealthRequest(
            request_id="test-202",
            include_details=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.HealthCheck(request, context)
        
        # Verify system resource components
        component_names = [comp.name for comp in response.components]
        self.assertIn("system_cpu", component_names)
        self.assertIn("system_memory", component_names)
        self.assertIn("system_disk", component_names)
        
        # Find the CPU component
        cpu_comp = next(comp for comp in response.components if comp.name == "system_cpu")
        self.assertEqual(cpu_comp.status, medeasy_pb2.Status.DEGRADED)  # 75% CPU is degraded
        
        # Find the memory component
        mem_comp = next(comp for comp in response.components if comp.name == "system_memory")
        self.assertEqual(mem_comp.status, medeasy_pb2.Status.DEGRADED)  # 80% memory is degraded
        
        # Find the disk component
        disk_comp = next(comp for comp in response.components if comp.name == "system_disk")
        self.assertEqual(disk_comp.status, medeasy_pb2.Status.HEALTHY)  # 50% disk is healthy

    def test_health_check_error_handling(self):
        """Test health check error handling."""
        # Create request
        request = medeasy_pb2.HealthRequest(
            request_id="test-303",
            include_details=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Mock metrics collector to raise exception
        self.mock_metrics_collector.increment_request_count.side_effect = Exception("Test error")
        
        # Call the method
        response = self.service.HealthCheck(request, context)
        
        # Verify response still works despite the error
        self.assertEqual(response.request_id, "test-303")
        self.assertIsNotNone(response.timestamp)
        
        # Logger should have recorded the error
        self.logger.error.assert_called()

    def test_swiss_date_format(self):
        """Test Swiss date format in response."""
        # Create request
        request = medeasy_pb2.HealthRequest(
            request_id="test-404",
            include_details=False
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.HealthCheck(request, context)
        
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
        current_year = datetime.now().year
        self.assertEqual(year, current_year)


if __name__ == '__main__':
    unittest.main()
