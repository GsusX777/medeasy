# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Unit tests for the MedEasyService DetectSwissGerman method.

[KP100] Critical path test coverage
[SDH] Swiss German dialect handling
[MFD] Swiss medical terminology
[ATV] Audit trail for all operations
"""

import unittest
from unittest.mock import patch, MagicMock
import structlog
import pytest
import grpc
import time

import medeasy_pb2
from src.grpc_service import MedEasyService
from src.swiss.dialect_detector import SwissGermanDetectionResult, DialectMarker


class TestMedEasyServiceSwissGerman(unittest.TestCase):
    """Test suite for the MedEasyService DetectSwissGerman method."""

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

    def test_detect_swiss_german_positive(self):
        """Test Swiss German detection with positive result."""
        # Mock detector result
        dialect_markers = [
            DialectMarker(pattern="ha", standard="habe", matches=2, positions=[5, 20]),
            DialectMarker(pattern="chli", standard="klein", matches=1, positions=[10])
        ]
        mock_result = SwissGermanDetectionResult(
            is_swiss_german=True,
            confidence_score=0.85,
            dialect_markers=dialect_markers,
            swiss_medical_terms=["Spital", "Doktor"],
            processing_time_seconds=0.1
        )
        self.mock_swiss_german_detector.detect_swiss_german.return_value = mock_result
        
        # Create request
        request = medeasy_pb2.SwissGermanRequest(
            request_id="test-123",
            text="Grüezi, ich ha chli Chopfweh und bin zum Doktor im Spital gsi.",
            include_details=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.DetectSwissGerman(request, context)
        
        # Verify response
        self.assertEqual(response.request_id, "test-123")
        self.assertTrue(response.is_swiss_german)
        self.assertEqual(response.confidence_score, 0.85)
        self.assertEqual(len(response.dialect_markers), 2)
        self.assertEqual(len(response.swiss_medical_terms), 2)
        self.assertEqual(response.processing_time_seconds, 0.1)
        
        # Verify dialect markers
        self.assertEqual(response.dialect_markers[0].pattern, "ha")
        self.assertEqual(response.dialect_markers[0].standard_german, "habe")
        self.assertEqual(response.dialect_markers[0].match_count, 2)
        
        # Verify metrics were updated
        self.mock_metrics_collector.increment_request_count.assert_called_once()
        self.mock_metrics_collector.update_audit_metrics.assert_called_once_with("swiss_german_detection")
        self.mock_metrics_collector.update_swiss_german_metrics.assert_called_once_with(
            True, 0.85, 2
        )

    def test_detect_swiss_german_negative(self):
        """Test Swiss German detection with negative result."""
        # Mock detector result
        mock_result = SwissGermanDetectionResult(
            is_swiss_german=False,
            confidence_score=0.2,
            dialect_markers=[],
            swiss_medical_terms=[],
            processing_time_seconds=0.05
        )
        self.mock_swiss_german_detector.detect_swiss_german.return_value = mock_result
        
        # Create request
        request = medeasy_pb2.SwissGermanRequest(
            request_id="test-456",
            text="Guten Tag, ich habe Kopfschmerzen und war beim Arzt im Krankenhaus.",
            include_details=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.DetectSwissGerman(request, context)
        
        # Verify response
        self.assertEqual(response.request_id, "test-456")
        self.assertFalse(response.is_swiss_german)
        self.assertEqual(response.confidence_score, 0.2)
        self.assertEqual(len(response.dialect_markers), 0)
        self.assertEqual(len(response.swiss_medical_terms), 0)
        self.assertEqual(response.processing_time_seconds, 0.05)
        
        # Verify metrics were updated
        self.mock_metrics_collector.update_swiss_german_metrics.assert_called_once_with(
            False, 0.2, 0
        )

    def test_detect_swiss_german_without_details(self):
        """Test Swiss German detection without details."""
        # Mock detector result
        dialect_markers = [
            DialectMarker(pattern="ha", standard="habe", matches=2, positions=[5, 20]),
            DialectMarker(pattern="chli", standard="klein", matches=1, positions=[10])
        ]
        mock_result = SwissGermanDetectionResult(
            is_swiss_german=True,
            confidence_score=0.85,
            dialect_markers=dialect_markers,
            swiss_medical_terms=["Spital", "Doktor"],
            processing_time_seconds=0.1
        )
        self.mock_swiss_german_detector.detect_swiss_german.return_value = mock_result
        
        # Create request
        request = medeasy_pb2.SwissGermanRequest(
            request_id="test-789",
            text="Grüezi, ich ha chli Chopfweh und bin zum Doktor im Spital gsi.",
            include_details=False  # No details
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.DetectSwissGerman(request, context)
        
        # Verify response
        self.assertEqual(response.request_id, "test-789")
        self.assertTrue(response.is_swiss_german)
        self.assertEqual(response.confidence_score, 0.85)
        self.assertEqual(len(response.dialect_markers), 0)  # No details
        self.assertEqual(len(response.swiss_medical_terms), 0)  # No details
        self.assertEqual(response.processing_time_seconds, 0.1)

    def test_detect_swiss_german_empty_text(self):
        """Test Swiss German detection with empty text."""
        # Mock detector result
        mock_result = SwissGermanDetectionResult(
            is_swiss_german=False,
            confidence_score=0.0,
            dialect_markers=[],
            swiss_medical_terms=[],
            processing_time_seconds=0.01
        )
        self.mock_swiss_german_detector.detect_swiss_german.return_value = mock_result
        
        # Create request
        request = medeasy_pb2.SwissGermanRequest(
            request_id="test-101",
            text="",
            include_details=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.DetectSwissGerman(request, context)
        
        # Verify response
        self.assertEqual(response.request_id, "test-101")
        self.assertFalse(response.is_swiss_german)
        self.assertEqual(response.confidence_score, 0.0)
        self.assertEqual(len(response.dialect_markers), 0)
        self.assertEqual(len(response.swiss_medical_terms), 0)

    def test_detect_swiss_german_error_handling(self):
        """Test error handling in Swiss German detection."""
        # Mock detector to raise exception
        self.mock_swiss_german_detector.detect_swiss_german.side_effect = Exception("Test error")
        
        # Create request
        request = medeasy_pb2.SwissGermanRequest(
            request_id="test-202",
            text="Grüezi mitenand",
            include_details=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method and expect exception to be handled
        response = self.service.DetectSwissGerman(request, context)
        
        # Verify error response
        self.assertEqual(response.request_id, "test-202")
        self.assertFalse(response.is_swiss_german)  # Default value on error
        
        # Context should have error set
        context.set_code.assert_called_once_with(grpc.StatusCode.INTERNAL)
        context.set_details.assert_called_once()
        
        # Logger should have recorded the error
        self.logger.error.assert_called()

    def test_detect_swiss_german_medical_terms(self):
        """Test Swiss German detection with medical terms."""
        # Mock detector result with medical terms
        dialect_markers = [DialectMarker(pattern="ha", standard="habe", matches=1, positions=[5])]
        mock_result = SwissGermanDetectionResult(
            is_swiss_german=True,
            confidence_score=0.75,
            dialect_markers=dialect_markers,
            swiss_medical_terms=["Spital", "Doktor", "Tablette", "Chopfweh"],
            processing_time_seconds=0.1
        )
        self.mock_swiss_german_detector.detect_swiss_german.return_value = mock_result
        
        # Create request
        request = medeasy_pb2.SwissGermanRequest(
            request_id="test-303",
            text="Ich ha Chopfweh und bin zum Doktor im Spital gsi. Er het mir Tablette verschriebe.",
            include_details=True
        )
        
        # Create mock context
        context = MagicMock()
        
        # Call the method
        response = self.service.DetectSwissGerman(request, context)
        
        # Verify response
        self.assertEqual(response.request_id, "test-303")
        self.assertTrue(response.is_swiss_german)
        self.assertEqual(len(response.swiss_medical_terms), 4)
        
        # Verify medical terms
        medical_terms = list(response.swiss_medical_terms)
        self.assertIn("Spital", medical_terms)
        self.assertIn("Doktor", medical_terms)
        self.assertIn("Tablette", medical_terms)
        self.assertIn("Chopfweh", medical_terms)


if __name__ == '__main__':
    unittest.main()
